namespace Celesta.BuilderNodes
{
    public class EffectChannel
    {
        public ushort level;
        public ushort gain;
        public EffectChannel(ushort in_Level, ushort in_Gain)
        {
            level = in_Level;
            gain = in_Gain;
        }
    }
}
