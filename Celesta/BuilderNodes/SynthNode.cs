using System;
using System.Collections.Generic;
using System.ComponentModel;
using Celesta.Serialization;

namespace Celesta.BuilderNodes
{
    public class SynthNode : BuilderBaseNode
    {
        private BuilderSynthPlaybackType playbackType = BuilderSynthPlaybackType.Polyphonic;
        private int previousChild = -1;
        private int nextChild = -1;
        private byte playbackProbability = 100;
        public string SynthName
        {
            get
            {
                return Path.Split('/')[^1];
            }
            set
            {
                var split = Path.Split('/');
                split[^1] = value;
                Path = string.Join('/', split);

            }
        }

        [Browsable(false)]
        public BuilderSynthType Type { get; set; }

        [Category("General"), DisplayName("Playback Type")]
        [Description("Playback type of this synth.")]
        public BuilderSynthPlaybackType PlaybackType
        {
            get
            {
                if (Type == BuilderSynthType.Single)
                {
                    return BuilderSynthPlaybackType.Normal;
                }

                return playbackType;
            }

            set
            {
                if (Type == BuilderSynthType.Single || value == BuilderSynthPlaybackType.Normal)
                {
                    return;
                }

                playbackType = value;
            }
        }
        public string SoundElementReference { get; set; }
        public List<string> Children { get; set; } = new List<string>();
        public string AisacReference { get; set; }
        public short Volume { get; set; }
        public short Pitch { get; set; }

        // "How much time it takes to play this synth. The time is in milliseconds."
        public uint DelayTime { get; set; }
        public byte SControl { get; set; }

        public byte FilterType { get; set; }
        public ushort FilterCutoff1 { get; set; }
        public ushort FilterCutoff2 { get; set; }
        public ushort FilterReso { get; set; }
        public byte FilterReleaseOffset { get; set; }
        public string Mtxrtr { get; set; }
        public string DryOName { get; set; }
        public string WetOName { get; set; }
        public EnvelopeGenerator EnvelopeGenerator;
        public Effect DrySendLevels { get; set; }
        public Effect WetLevels { get; set; }
        public string Wcnct0 { get; set; }
        public string Wcnct1 { get; set; }
        public string Wcnct2 { get; set; }
        public string Wcnct3 { get; set; }
        public string Wcnct4 { get; set; }
        public string Wcnct5 { get; set; }
        public string Wcnct6 { get; set; }
        public string Wcnct7 { get; set; }
        public string VoiceLimitGroupReference { get; set; }
        public byte VoiceLimitType { get; set; }
        public byte VoiceLimitPriority { get; set; }
        public ushort VoiceLimitProhibitionTime { get; set; }
        public sbyte VoiceLimitPcdlt { get; set; }
        public Pan3D Pan3D;
        public byte Filter1Type { get; set; }
        public ushort Filter1CutoffOffset { get; set; }
        public ushort Filter1CutoffGain { get; set; }
        public ushort Filter1ResoOffset { get; set; }
        public ushort Filter1ResoGain { get; set; }
        public byte Filter2Type { get; set; }
        public ushort Filter2CutoffLowerOffset { get; set; }
        public ushort Filter2CutoffLowerGain { get; set; }
        public ushort Filter2CutoffHigherOffset { get; set; }
        public ushort Filter2CutoffHigherGain { get; set; }

        //"Probability of this synth being played. Lower values make it less probable to play. Max is 100."
        public byte PlaybackProbability
        {
            get
            {
                if (Type == BuilderSynthType.WithChildren)
                {
                    return 0;
                }

                return playbackProbability;
            }

            set
            {
                playbackProbability = value > 100 ? (byte)100 : value;
            }
        }
        public byte NLmtChildren { get; set; }
        public byte Repeat { get; set; }
        public uint ComboTime { get; set; }
        public byte ComboLoopBack { get; set; }
        public bool PlayThisTurn
        {
            get
            {
                return Random.Shared.Next(100) <= PlaybackProbability;
            }
        }
        public int RandomChildNode
        {
            get
            {
                if (playbackType == BuilderSynthPlaybackType.RandomNoRepeat)
                {
                    int randomChild = Random.Shared.Next(Children.Count);

                    while (randomChild == previousChild)
                    {
                        randomChild = Random.Shared.Next(Children.Count);
                    }

                    previousChild = randomChild;
                    return randomChild;
                }

                return Random.Shared.Next(Children.Count);
            }
        }
        public int NextChildNode
        {
            get
            {
                if (nextChild + 1 == Children.Count)
                {
                    nextChild = -1;
                }

                return ++nextChild;
            }
        }

        public string Parent { get; internal set; }

        public SynthNode()
        {
            Children = new List<string>();
            WetLevels = new Effect();
            DrySendLevels = new Effect();

            Volume = 1000;

            EnvelopeGenerator.Sustain = 1000;

            Pan3D = new Pan3D(0, 255, 0, 255, 0, 255);
            Filter2CutoffHigherGain = 0;
            Filter2CutoffLowerGain = 0;
            DryOName = "";
            WetOName = "";
            Mtxrtr = "";
            Wcnct0 = "";
            Wcnct1 = "";
            Wcnct2 = "";
            Wcnct3 = "";
            Wcnct4 = "";
            Wcnct5 = "";
            Wcnct6 = "";
            Wcnct7 = "";
        }

        public SynthNode(SerializationSynthTable synthTable)
        {
            Path = synthTable.SynthName;
            Type = (BuilderSynthType)synthTable.SynthType;
            PlaybackType = (BuilderSynthPlaybackType)synthTable.ComplexType;
            Volume = synthTable.Volume;
            Pitch = synthTable.Pitch;
            DelayTime = synthTable.DelayTime;
            SControl = synthTable.SControl;
            EnvelopeGenerator = new EnvelopeGenerator(synthTable.EgDelay,
                                                      synthTable.EgAttack,
                                                      synthTable.EgHold,
                                                      synthTable.EgDecay,
                                                      synthTable.EgRelease,
                                                      synthTable.EgSustain);
            FilterType = synthTable.FType;
            FilterCutoff1 = synthTable.FCof1;
            FilterCutoff2 = synthTable.FCof2;
            FilterReso = synthTable.FReso;
            FilterReleaseOffset = synthTable.FReleaseOffset;
            DryOName = synthTable.DryOName;
            Mtxrtr = synthTable.Mtxrtr;
            ushort[] dryLvl =
            [
                synthTable.Dry0,
                synthTable.Dry1,
                synthTable.Dry2,
                synthTable.Dry3,
                synthTable.Dry4,
                synthTable.Dry5,
                synthTable.Dry6,
                synthTable.Dry7
            ];
            ushort[] dryGain =
            [
                synthTable.Dry0g,
                synthTable.Dry1g,
                synthTable.Dry2g,
                synthTable.Dry3g,
                synthTable.Dry4g,
                synthTable.Dry5g,
                synthTable.Dry6g,
                synthTable.Dry7g
            ];
            DrySendLevels = new Effect(dryLvl, dryGain);
            WetOName = synthTable.WetOName;
            ushort[] wetLvl =
            [
                synthTable.Wet0,
                synthTable.Wet1,
                synthTable.Wet2,
                synthTable.Wet3,
                synthTable.Wet4,
                synthTable.Wet5,
                synthTable.Wet6,
                synthTable.Wet7
            ];
            ushort[] wetGain =
            [
                synthTable.Wet0,
                synthTable.Wet1g,
                synthTable.Wet2g,
                synthTable.Wet3g,
                synthTable.Wet4g,
                synthTable.Wet5g,
                synthTable.Wet6g,
                synthTable.Wet7g
            ];
            WetLevels = new Effect(wetLvl, wetGain);
            Wcnct0 = synthTable.Wcnct0;
            Wcnct1 = synthTable.Wcnct1;
            Wcnct2 = synthTable.Wcnct2;
            Wcnct3 = synthTable.Wcnct3;
            Wcnct4 = synthTable.Wcnct4;
            Wcnct5 = synthTable.Wcnct5;
            Wcnct6 = synthTable.Wcnct6;
            Wcnct7 = synthTable.Wcnct7;
            VoiceLimitType = synthTable.VoiceLimitType;
            VoiceLimitPriority = synthTable.VoiceLimitPriority;
            VoiceLimitProhibitionTime = synthTable.VoiceLimitPhTime;
            VoiceLimitPcdlt = synthTable.VoiceLimitPcdlt;

            Pan3D = new Pan3D(synthTable.Pan3dVolumeOffset,
                              synthTable.Pan3dVolumeGain,
                              synthTable.Pan3dAngleOffset,
                              synthTable.Pan3dAngleGain,
                              synthTable.Pan3dDistanceOffset,
                              synthTable.Pan3dDistanceGain);

            Filter1Type = synthTable.F1Type;
            Filter1CutoffOffset = synthTable.F1CofOffset;
            Filter1CutoffGain = synthTable.F1CofGain;
            Filter1ResoOffset = synthTable.F1ResoOffset;
            Filter1ResoGain = synthTable.F1ResoGain;
            Filter2Type = synthTable.F2Type;
            Filter2CutoffLowerOffset = synthTable.F2CofLowOffset;
            Filter2CutoffLowerGain = synthTable.F2CofLowGain;
            Filter2CutoffHigherOffset = synthTable.F2CofHighOffset;
            Filter2CutoffHigherGain = synthTable.F2CofHighGain;
            PlaybackProbability = synthTable.Probability;
            NLmtChildren = synthTable.NumberLmtChildren;
            Repeat = synthTable.Repeat;
            ComboTime = synthTable.ComboTime;
            ComboLoopBack = synthTable.ComboLoopBack;
        }
    }
}
