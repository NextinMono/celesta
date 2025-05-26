using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using Celesta.Serialization;

namespace Celesta.BuilderNodes
{
    public enum BuilderSynthType
    {
        Single,
        WithChildren,
    }

    public enum BuilderSynthPlaybackType
    {
        [Browsable(false)]
        Normal = -1,
        Polyphonic = 0,
        RandomNoRepeat = 1,
        Sequential = 2,
        Random = 3,
        SequentialNoLoop = 6,
    }
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
    public class Pan3D
    {
        public Pan3D(short pan3dVolumeOffset, short pan3dVolumeGain, short pan3dAngleOffset, short pan3dAngleGain, short pan3dDistanceOffset, short pan3dDistanceGain)
        {
            VolumeOffset = pan3dVolumeOffset;
            VolumeGain = pan3dVolumeGain;
            AngleOffset = pan3dAngleOffset;
            AngleGain = pan3dAngleGain;
            DistanceOffset = pan3dDistanceOffset;
            DistanceGain = pan3dDistanceGain;
        }

        [Category("Pan 3D"), DisplayName("Pan 3D Volume Offset")]
        public short VolumeOffset { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Volume Gain")]
        public short VolumeGain { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Angle Offset")]
        public short AngleOffset { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Angle Gain")]
        public short AngleGain { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Distance Offset")]
        public short DistanceOffset { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Distance Gain")]
        public short DistanceGain { get; set; }


    }
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

    public class SynthNode : BuilderBaseNode
    {
        private BuilderSynthPlaybackType playbackType = BuilderSynthPlaybackType.Polyphonic;
        private int previousChild = -1;
        private int nextChild = -1;
        private byte playbackProbability = 100;
        private SerializationSynthTable synthTable;
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

        [ReadOnly(true)]
        [Category("General"), DisplayName("Sound Element Reference Path")]
        [Description("Full reference path of sound element node. This is going to be empty if the node is a track.")]
        public string SoundElementReference { get; set; }

        [Browsable(false)]
        public List<string> Children { get; set; } = new List<string>();

        [ReadOnly(true)]
        [Category("General"), DisplayName("Aisac Reference Path")]
        [Description("Full reference path of aisac node.")]
        public string AisacReference { get; set; }

        [Category("General")]
        [Description("Volume of this synth. 1000 equals to 100%.")]
        public short Volume { get; set; }

        [Category("General")]
        [Description("Pitch of this synth. Use positive/negative values to make it higher/lower pitched.")]
        public short Pitch { get; set; }

        [Category("General"), DisplayName("Delay Time")]
        [Description("How much time it takes to play this synth. The time is in milliseconds.")]
        public uint DelayTime { get; set; }

        [Category("Unknown"), DisplayName("S Control")]
        public byte SControl { get; set; }

        [Category("EG"), DisplayName("EG Delay")]
        public ushort EgDelay { get; set; }

        [Category("EG"), DisplayName("EG Attack")]
        public ushort EgAttack { get; set; }

        [Category("EG"), DisplayName("EG Hold")]
        public ushort EgHold { get; set; }

        [Category("EG"), DisplayName("EG Decay")]
        public ushort EgDecay { get; set; }

        [Category("EG"), DisplayName("EG Release")]
        public ushort EgRelease { get; set; }

        [Category("EG"), DisplayName("EG Sustain")]
        public ushort EgSustain { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter Type")]
        public byte FilterType { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter Cutoff 1")]
        public ushort FilterCutoff1 { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter Cutoff 2")]
        public ushort FilterCutoff2 { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter RESO (Unknown)")]
        public ushort FilterReso { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter Release Offset")]
        public byte FilterReleaseOffset { get; set; }

        [Category("Dryness"), DisplayName("Dry O Name (Unknown)")]
        public string DryOName { get; set; }

        [Category("Unknown")]
        public string Mtxrtr { get; set; }

        public Effect DrySendLevels { get; set; }

        [Category("Wetness"), DisplayName("Wet O Name (Unknown)")]
        public string WetOName { get; set; }

        public Effect WetLevels { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 0")]
        public string Wcnct0 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 1")]
        public string Wcnct1 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 2")]
        public string Wcnct2 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 3")]
        public string Wcnct3 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 4")]
        public string Wcnct4 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 5")]
        public string Wcnct5 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 6")]
        public string Wcnct6 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 7")]
        public string Wcnct7 { get; set; }

        [ReadOnly(true)]
        [Category("Voice Limit"), DisplayName("Voice Limit Group Reference")]
        public string VoiceLimitGroupReference { get; set; }

        [Category("Voice Limit"), DisplayName("Voice Limit Type")]
        public byte VoiceLimitType { get; set; }

        [Category("Voice Limit"), DisplayName("Voice Limit Priority")]
        public byte VoiceLimitPriority { get; set; }

        [Category("Voice Limit"), DisplayName("Voice Limit Prohibition Time")]
        public ushort VoiceLimitProhibitionTime { get; set; }

        [Category("Voice Limit"), DisplayName("Voice Limit Pcdlt (Unknown)")]
        public sbyte VoiceLimitPcdlt { get; set; }

        public Pan3D Pan3D;



        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 Type")]
        public byte Filter1Type { get; set; }

        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 Cutoff Offset")]
        public ushort Filter1CutoffOffset { get; set; }

        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 Cutoff Gain")]
        public ushort Filter1CutoffGain { get; set; }

        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 RESO (Unknown) Offset")]
        public ushort Filter1ResoOffset { get; set; }

        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 RESO (Unknown) Gain")]
        public ushort Filter1ResoGain { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Type")]
        public byte Filter2Type { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Cutoff Lower Offset")]
        public ushort Filter2CutoffLowerOffset { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Cutoff Lower Gain")]
        public ushort Filter2CutoffLowerGain { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Cutoff Higher Offset")]
        public ushort Filter2CutoffHigherOffset { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Cutoff Higher Gain")]
        public ushort Filter2CutoffHigherGain { get; set; }

        [Category("General"), DisplayName("Playback Probability")]
        [Description("Probability of this synth being played. Lower values make it less probable to play. Max is 100.")]
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

        [Category("Unknown"), DisplayName("N LMT Children")]
        public byte NLmtChildren { get; set; }

        [Category("General"), DisplayName("Repeat")]
        public byte Repeat { get; set; }

        [Category("General"), DisplayName("Combo Time")]
        public uint ComboTime { get; set; }

        [Category("General"), DisplayName("Combo Loop Back")]
        public byte ComboLoopBack { get; set; }

        [Browsable(false)]
        public bool PlayThisTurn
        {
            get
            {
                return Random.Shared.Next(100) <= PlaybackProbability;
            }
        }

        [Browsable(false)]
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

        [Browsable(false)]
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

            EgSustain = 1000;

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
            EgDelay = synthTable.EgDelay;
            EgAttack = synthTable.EgAttack;
            EgHold = synthTable.EgHold;
            EgDecay = synthTable.EgDecay;
            EgRelease = synthTable.EgRelease;
            EgSustain = synthTable.EgSustain;
            FilterType = synthTable.FType;
            FilterCutoff1 = synthTable.FCof1;
            FilterCutoff2 = synthTable.FCof2;
            FilterReso = synthTable.FReso;
            FilterReleaseOffset = synthTable.FReleaseOffset;
            DryOName = synthTable.DryOName;
            Mtxrtr = synthTable.Mtxrtr;
            ushort[] dryLvl = [synthTable.Dry0, synthTable.Dry1, synthTable.Dry2, synthTable.Dry3, synthTable.Dry4, synthTable.Dry5, synthTable.Dry6, synthTable.Dry7];
            ushort[] dryGain = [synthTable.Dry0g, synthTable.Dry1g, synthTable.Dry2g, synthTable.Dry3g, synthTable.Dry4g, synthTable.Dry5g, synthTable.Dry6g, synthTable.Dry7g];
            DrySendLevels = new Effect(dryLvl, dryGain);
            WetOName = synthTable.WetOName;
            ushort[] wetLvl = [synthTable.Wet0, synthTable.Wet1, synthTable.Wet2, synthTable.Wet3, synthTable.Wet4, synthTable.Wet5, synthTable.Wet6, synthTable.Wet7];
            ushort[] wetGain = [synthTable.Wet0, synthTable.Wet1g, synthTable.Wet2g, synthTable.Wet3g, synthTable.Wet4g, synthTable.Wet5g, synthTable.Wet6g, synthTable.Wet7g];
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
