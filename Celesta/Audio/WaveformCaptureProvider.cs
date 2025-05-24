using NAudio.Wave;
using System;
namespace Celesta
{
    public class WaveformCaptureProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly float[] ringBuffer;
        private int ringOffset;
        DirectSoundOut player;
        public static float[] LatestWaveform { get; private set; } = new float[1];
        public WaveformCaptureProvider(ISampleProvider source, int bufferSize = 1024)
        {
            this.source = source;
            this.ringBuffer = new float[bufferSize];
            LatestWaveform = new float[bufferSize];
            this.ringOffset = 0;
            WaveFormat = source.WaveFormat;
        }

        public WaveFormat WaveFormat { get; }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);

            // Capture waveform from first channel
            for (int i = 0; i < read; i += WaveFormat.Channels)
            {
                // Left channel
                float sample = buffer[offset + i];
                ringBuffer[ringOffset++] = sample;

                if (ringOffset >= ringBuffer.Length)
                    ringOffset = 0;
            }
            // Make a copy for drawing
            Array.Copy(ringBuffer, 0, LatestWaveform, 0, ringBuffer.Length);
            return read;
        }
    }
}