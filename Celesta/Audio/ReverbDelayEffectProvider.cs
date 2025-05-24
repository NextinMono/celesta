using NAudio.Wave;
namespace Celesta
{
    public class ReverbDelayEffectProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly float[] delayBuffer;
        private int delayIndex;
        private readonly int sampleRate;
        private readonly int channels;
        private int delaySamples;

        // Dictates the "percentage" of the reverb, 0 = no reverb, 1 = full reverb
        public float WetMix { get; set; } = 0.05f;

        // How loud the reverb is
        public float Volume { get; set; } = 0.3f;

        // How long the reverb lasts
        public float Feedback { get; set; } = 0.6f;
        public int DelayMilliseconds { get; set; } = 100;

        public ReverbDelayEffectProvider(ISampleProvider source)
        {
            this.source = source;
            this.sampleRate = source.WaveFormat.SampleRate;
            this.channels = source.WaveFormat.Channels;
            this.delaySamples = (DelayMilliseconds * sampleRate / 1000) * channels;

            this.delayBuffer = new float[delaySamples * 2];
            this.delayIndex = 0;

            WaveFormat = source.WaveFormat;
        }

        public WaveFormat WaveFormat { get; }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = source.Read(buffer, offset, count);

            for (int n = 0; n < samplesRead; n++)
            {
                float inputSample = buffer[offset + n];
                float delayedSample = delayBuffer[delayIndex];
                buffer[offset + n] = (inputSample + delayedSample * (Volume * WetMix));
                delayBuffer[delayIndex] = inputSample + delayedSample * Feedback;
                delayIndex++;
                if (delayIndex >= delayBuffer.Length)
                    delayIndex = 0;
            }

            return samplesRead;
        }
    }
}