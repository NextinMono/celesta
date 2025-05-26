using Celesta;
using Celesta.BuilderNodes;
using Celesta.CsbTypes;
using Celesta.Project;
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
                if (item.Path == in_Name)
                    return item;
            }
            return null;
        }
        public static AisacNode GetAisacByName(string in_Name)
        {
            foreach (var item in CelestaProject.Instance.config.workFile.AisacNodes)
            {
                if (item.Path == in_Name)
                    return item;
            }
            return null;
        }
        public static EasyAisacNode GetEasyAisacByName(string in_Name)
        {
            foreach (var item in CelestaProject.Instance.config.workFile.UniqueAisacNodes)
            {
                foreach(var item2 in item.AisacNodes)
                {
                    if (item2.Path == in_Name)
                        return item;
                }
            }
            return null;
        }
        public static SynthNode GetSynth(this CueNode node)
        {
            return GetSynthByName(node.SynthReference);
        }
        public static AisacNode GetAisac(this SynthNode node)
        {
            return GetAisacByName(node.AisacReference);
        }
        public static EasyAisacNode GetEasyAisac(this SynthNode node)
        {
            return GetEasyAisacByName(node.AisacReference);
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
