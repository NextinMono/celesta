namespace Celesta.BuilderNodes
{
    public struct EnvelopeGenerator
    {
        public EnvelopeGenerator(ushort egDelay, ushort egAttack, ushort egHold, ushort egDecay, ushort egRelease, ushort egSustain)
        {
            Delay = egDelay;
            Attack = egAttack;
            Hold = egHold;
            Decay = egDecay;
            Release = egRelease;
            Sustain = egSustain;
        }

        public ushort Delay { get; set; }
        public ushort Attack { get; set; }
        public ushort Hold { get; set; }
        public ushort Decay { get; set; }
        public ushort Release { get; set; }
        public ushort Sustain { get; set; }
    }
}
