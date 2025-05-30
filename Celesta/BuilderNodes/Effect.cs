using System;

namespace Celesta.BuilderNodes
{
    public class Effect
    {
        public EffectChannel left;
        public EffectChannel right;
        public EffectChannel rearLeft;
        public EffectChannel rearRight;
        public EffectChannel center;
        public EffectChannel lfe;
        public EffectChannel extra1;
        public EffectChannel extra2;

        public Effect()
        {
            left = new EffectChannel(0, 255);
            right = new EffectChannel(0, 255);
            rearLeft = new EffectChannel(0, 255);
            rearRight = new EffectChannel(0, 255);
            center = new EffectChannel(0, 255);
            lfe = new EffectChannel(0, 255);
            extra1 = new EffectChannel(0, 255);
            extra2 = new EffectChannel(0, 255);

        }
        public void SetLevels(ushort[] in_Levels)
        {
            left.level = in_Levels[0];
            right.level = in_Levels[1];
            rearLeft.level = in_Levels[2];
            rearRight.level = in_Levels[3];
            center.level = in_Levels[4];
            lfe.level = in_Levels[5];
            extra1.level = in_Levels[6];
            extra2.level = in_Levels[7];
        }
        public void SetGains(ushort[] in_Gains)
        {
            left.gain = in_Gains[0];
            right.gain = in_Gains[1];
            rearLeft.gain = in_Gains[2];
            rearRight.gain = in_Gains[3];
            center.gain = in_Gains[4];
            lfe.gain = in_Gains[5];
            extra1.gain = in_Gains[6];
            extra2.gain = in_Gains[7];
        }
        public Effect(ushort[] in_Levels, ushort[] in_Gains)
        {
            if (in_Levels.Length != in_Gains.Length)
            {
                throw new ArgumentException("Level and Gain arrays do not match.");
            }

            left = new EffectChannel(in_Levels[0], in_Gains[0]);
            right = new EffectChannel(in_Levels[1], in_Gains[1]);
            rearLeft = new EffectChannel(in_Levels[2], in_Gains[2]);
            rearRight = new EffectChannel(in_Levels[3], in_Gains[3]);
            center = new EffectChannel(in_Levels[4], in_Gains[4]);
            lfe = new EffectChannel(in_Levels[5], in_Gains[5]);
            extra1 = new EffectChannel(in_Levels[6], in_Gains[6]);
            extra2 = new EffectChannel(in_Levels[7], in_Gains[7]);
        }
    }
}
