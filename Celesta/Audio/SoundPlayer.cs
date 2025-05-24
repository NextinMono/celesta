using NAudio.Wave;
using System;
using NAudio.Wave.SampleProviders;
namespace Celesta
{
    public class PauseProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        public bool IsPaused { get; private set; }

        public PauseProvider(ISampleProvider source)
        {
            _source = source;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (IsPaused)
            {
                // Output silence if paused
                Array.Clear(buffer, offset, count);
                return count; 
            }

            int read = _source.Read(buffer, offset, count);
            if (read == 0)
                IsPaused = true;

            return read;
        }

        public WaveFormat WaveFormat => _source.WaveFormat;

        public void Stop() => IsPaused = true;
        public void Resume() => IsPaused = false;
    }
    public class SoundPlayer
    {
        private StereoPanProvider pan;
        private ReverbDelayEffectProvider reverb;
        private VolumeSampleProvider volumeProvider;
        private PauseProvider stop;
        public ISampleProvider soundProvider;
        public EOptions Flags;
        [Flags]
        public enum EOptions
        {
            eOptions_None = 0x0,
            eOptions_IsSynthPlay = 0x1
        }
        public SoundPlayer(ISampleProvider insound, float invol)
        {
            if (insound.WaveFormat.Channels < 2)
            {
                // Turn into stereo if the source is mono by just duplicating the channel
                insound = new MonoToStereoSampleProvider(insound);
                ((MonoToStereoSampleProvider)insound).LeftVolume = 1;
                ((MonoToStereoSampleProvider)insound).RightVolume = 1.0f;
            }
            pan = new StereoPanProvider(insound);
            reverb = new ReverbDelayEffectProvider(pan);
            volumeProvider = new VolumeSampleProvider(reverb);
            Volume = (float)invol;
            soundProvider = volumeProvider;
            // Mixer requires sounds to be all of the same sample rate
            if (volumeProvider.WaveFormat.SampleRate != 44100)
                soundProvider = new WdlResamplingSampleProvider(volumeProvider, 44100);

            soundProvider = stop = new PauseProvider(soundProvider);
        }
        public void Pause() => stop.Stop();
        public void Play() => stop.Resume();
        public bool IsPaused() => stop.IsPaused;
        public float ReverbWetMix
        {
            get { return reverb.WetMix; }
            set { reverb.WetMix = value; }
        }
        public float PanAngle
        {
            get
            {
                return pan.PanAngleDegrees;
            }
            set
            {
                pan.PanAngleDegrees = value;
            }
        }
        public float Volume
        {
            get
            {
                return volumeProvider.Volume;
            }
            set
            {
                volumeProvider.Volume = Math.Clamp(value, 0, 1.25f);
            }
        }
        public void SetVolumeCsb(float in_Volume)
        {
            Volume = in_Volume / 1000.0f;
        }
    }
}