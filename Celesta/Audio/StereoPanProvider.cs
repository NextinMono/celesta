using NAudio.Wave;
using System;
namespace Celesta
{
    public class StereoPanProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        // Angle in degrees: 0 = front, 90 = right, -90 = left, 180 = back
        private float panAngleDegrees;

        public WaveFormat WaveFormat { get; }

        public float PanAngleDegrees
        {
            get => panAngleDegrees;
            set
            {
                if (value > 180f) value -= 360f;
                else if (value < -180f) value += 360f;
                panAngleDegrees = value;
            }
        }
        public StereoPanProvider(ISampleProvider source)
        {
            if (source.WaveFormat.Channels != 2)
                throw new ArgumentException("Source must be stereo");

            this.source = source;
            panAngleDegrees = 0f;
            WaveFormat = source.WaveFormat;
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = source.Read(buffer, offset, count);

            // Apply panning per stereo frame (2 samples per frame)
            for (int n = 0; n < samplesRead; n += 2)
            {
                // Compute stereo gains based on pan angle
                GetStereoGains(panAngleDegrees, out float gainL, out float gainR);

                int iL = offset + n;
                int iR = offset + n + 1;

                // Apply gain to left and right channels
                buffer[iL] *= gainL;
                buffer[iR] *= gainR;
            }
            return samplesRead;
        }
        private void GetStereoGains(float angleDeg, out float gainL, out float gainR)
        {
            // Normalize angle to [-180,180]
            if (angleDeg > 180f) angleDeg -= 360f;
            else if (angleDeg < -180f) angleDeg += 360f;

            float angleRad = angleDeg * (float)Math.PI / 180f;
            float pan = (float)Math.Sin(angleRad);
            gainL = (float)Math.Sqrt((1f - pan) / 2f);
            gainR = (float)Math.Sqrt((1f + pan) / 2f);
        }
    }
}