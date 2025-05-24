using Celesta.Serialization;

namespace Celesta.CsbTypes
{
    public class CueNode
    {
        public string Name;
        public string UserComment;
        public uint Id;
        public byte Flags;
        public string SynthReference;

        public CueNode(SerializationCueTable in_Cue)
        {
            Name = in_Cue.Name;
            UserComment = in_Cue.UserData;
            Id = in_Cue.Id;
            Flags = in_Cue.Flags;
            SynthReference = in_Cue.SynthPath;
        }
        public CueNode()
        {
            Name = "NewCue";
        }
    }
}
