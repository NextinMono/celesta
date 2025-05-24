using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;

namespace Celesta.BuilderNodes
{
    public abstract class BuilderBaseNode : ICloneable
    {
        [Category("General"), ReadOnly(true)]
        [Description("Name of this node.")]
        public string Name { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
