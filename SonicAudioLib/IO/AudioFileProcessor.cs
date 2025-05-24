using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VGAudio; 
using SonicAudioLib.Archives;
using VGAudio.Containers.Adx;
using VGAudio.Formats.Pcm16;
using VGAudio.Containers.Wave;
using NAudio.Wave;
using VGAudio.Formats;
using VGAudio.Containers;

namespace SonicAudioLib.IO
{
    public class CriAudioFile : IDisposable
    {
        public WaveStream WaveStream { get; private set; }
        public byte[] WavData { get; private set; }
        public int LoopStartSample { get; private set; }
        public int LoopEndSample { get; private set; }
        public bool ShouldLoop { get; private set; }
        public string Name { get; private set; }
        public bool IsEmpty { get; private set; }
        private AudioData audioData;
        public MemoryStream EncodeAudioDataToAdxStream()
        {
            var adxWriter = new AdxWriter();
            byte[] adxBytes = adxWriter.GetFile(audioData);
            return new MemoryStream(adxBytes, writable: false);
        }

        public void Dispose()
        {
            WaveStream?.Dispose();
            WavData = null;
            audioData = null;
        }

        public CriAudioFile(string name, Stream adxStream)
        {
            Name = name;
            AdxReader reader = new AdxReader();
            AdxStructure meta = null;

            try
            {
                audioData = reader.Read(adxStream);
                adxStream.Position = 0;
                meta = reader.ReadMetadata(adxStream);
                ShouldLoop = meta.Looping;

                //Convert to wav for NAudio
                WavData = new VGAudio.Containers.Wave.WaveWriter().GetFile(audioData);
                using(var mem = new MemoryStream(WavData))
                    WaveStream = new WaveFileReader(new MemoryStream(WavData));

                if (ShouldLoop)
                {
                    LoopStartSample = meta.LoopStartSample;
                    LoopEndSample = meta.LoopEndSample;
                }
                else
                {
                    LoopStartSample = 0;
                    LoopEndSample = (int)(WaveStream.Length / WaveStream.WaveFormat.BlockAlign);
                }
                IsEmpty = false;
            }
            catch (Exception ex)
            {
                //Funnily enough, 06 also has unfinished adx files, some audio files have no samples at all.
                //Since VGAudio does not handle this, the sound will get replaced with pure silence.
                Console.WriteLine($"Malformed ADX file, replacing with silence. [{name}]");
                IsEmpty = true;
            }
            adxStream.Dispose();
        }
    }
    public class AudioFileProcessor
    {
        public enum LoggingType
        {
            Progress,
            Message,
        }

        private class Item
        {
            public object Source { get; set; }
            public string DestinationFileName { get; set; }
            public long Position { get; set; }
            public long Length { get; set; }
        }

        private List<Item> items = new List<Item>();

        public int BufferSize { get; set; }
        public bool EnableThreading { get; set; }
        public int MaxThreads { get; set; }

        public event ProgressChanged ProgressChanged;
        public List<string> OutputStreams { get; } = new List<string>();

        public void Add(object source, string destinationFileName, long position, long length)
        {
            items.Add(new Item { Source = source, DestinationFileName = destinationFileName, Position = position, Length = length, });
        }

        public List<CriAudioFile> Run()
        {
            double progress = 0.0;
            double factor = 100.0 / items.Count;
            object lockTarget = new object();

            List<CriAudioFile> readers = new List<CriAudioFile>();
            Action<Item> action = item =>
            {
                if (ProgressChanged != null)
                {
                    lock (lockTarget)
                    {
                        progress += factor;
                        ProgressChanged(this, new ProgressChangedEventArgs(progress));
                    }
                }

                //Open the appropriate input stream depending on the item's source type:
                //- string => file path
                //- byte[] => memory stream
                //- Stream => use as-is
                using (Stream source =
                    item.Source is string fileName ? new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize) :
                    item.Source is byte[] byteArray ? new MemoryStream(byteArray) :
                    item.Source is Stream stream ? stream :
                    throw new ArgumentException("Unknown source in item", nameof(item.Source))
                )
                {
                    using (MemoryStream destination = new MemoryStream())
                    {
                        DataStream.CopyPartTo(source, destination, item.Position, item.Length, BufferSize);
                        destination.Position = 0;

                        if (!OutputStreams.Contains(item.DestinationFileName))
                        {
                            lock (OutputStreams)
                            {
                                readers.Add(new CriAudioFile(item.DestinationFileName, destination));
                                OutputStreams.Add(item.DestinationFileName);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Duplicate? [{item.DestinationFileName}]");
                        }
                    }
                }
            };
            if (EnableThreading)
            {
                Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = MaxThreads }, action);
            }
            else
            {
                items.ForEach(action);
            }
            items.Clear();
            return readers;
        }


        public AudioFileProcessor()
        {
            BufferSize = 4096;
            EnableThreading = true;
            MaxThreads = 4;
        }
    }
}
