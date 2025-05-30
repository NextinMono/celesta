using System;
using System.ComponentModel;

namespace Celesta.BuilderNodes
{
    public abstract class BuilderBaseNode : ICloneable
    {
        public string Path { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
