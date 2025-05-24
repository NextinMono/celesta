using Celesta;
using Celesta.BuilderNodes;
using Celesta.CsbTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celesta
{
    public static class CsbCommon
    {
        public static SynthNode GetSynthByName(string in_Name)
        {
            foreach (var item in CelestaProject.Instance.config.workFile.SynthNodes)
            {
                if (item.Name == in_Name)
                    return item;
            }
            return null;
        }
        public static BuilderAisacNode GetAisacByName(string in_Name)
        {
            foreach (var item in CelestaProject.Instance.config.workFile.AisacNodes)
            {
                if (item.Name == in_Name)
                    return item;
            }
            return null;
        }
        public static SynthNode GetSynth(this CueNode node)
        {
            return GetSynthByName(node.SynthReference);
        }
        public static int FindKeyframe(this BuilderAisacGraphNode in_List, float in_Frame)
        {
            int min = 0;
            int max = in_List.Points.Count - 1;

            while (min <= max)
            {
                int index = (min + max) / 2;

                if (in_Frame < in_List.Points[index].X)
                    max = index - 1;
                else
                    min = index + 1;
            }

            return min;
        }
    }
}
