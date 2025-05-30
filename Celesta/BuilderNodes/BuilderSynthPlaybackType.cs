using System.ComponentModel;

namespace Celesta.BuilderNodes
{
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
}
