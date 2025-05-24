namespace Celesta
{
    using IconFonts;

    public static class NodeIconResource
    {
        private static SIconData soundResource = new SIconData(FontAwesome6.File, ColorResource.File);
        private static SIconData synth = new SIconData(FontAwesome6.Music, ColorResource.Group);
        private static SIconData synthGroup = new SIconData(FontAwesome6.CompactDisc, ColorResource.Group);
        private static SIconData aisac = new SIconData(FontAwesome6.Circle, ColorResource.File);
        private static SIconData cue = new SIconData(FontAwesome6.VolumeHigh, ColorResource.Celeste);

        public static SIconData SoundElement => soundResource;
        public static SIconData Synth => synth;
        public static SIconData SynthGroup => synthGroup;
        public static SIconData Aisac => aisac;
        public static SIconData Cue => cue;
    }
}