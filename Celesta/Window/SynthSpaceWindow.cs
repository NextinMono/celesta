using Celesta;
using Celesta.BuilderNodes;
using Celesta.CsbTypes;
using HekonrayBase;
using HekonrayBase.Base;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celesta
{
    public class SynthSpaceWindow : Singleton<SynthSpaceWindow>, IWindow
    {
        public void OnReset(IProgramProject in_Renderer)
        {
        }
        private List<SynthNode> GetSynthChildren(SynthNode item)
        {
            List<SynthNode> list = new List<SynthNode>();
            foreach (var child in item.Children)
            {
                list.Add(CsbCommon.GetSynthByName(child));
            }
            return list;
        }
        void RightClickCue(SynthNode synth)
        {
            if (ImGui.Selectable("Add"))
            {
                CelestaProject.Instance.AddSynth();
            }
            if(string.IsNullOrEmpty(synth.Parent))
            {
                if(ImGui.Selectable("Add Child"))
                {
                    var synthChild = CelestaProject.Instance.AddSynth();
                    CelestaProject.Instance.AddSynthChild(synth, synthChild);
                }
            }
            if (ImGui.Selectable("Remove"))
            {
                foreach (var child in synth.Children)
                {
                    CelestaProject.Instance.config.workFile.SynthNodes.Remove(CsbCommon.GetSynthByName(child));
                }
                CelestaProject.Instance.config.workFile.SynthNodes.Remove(synth);
                if (!string.IsNullOrEmpty(synth.Parent))
                {
                    var parent = CsbCommon.GetSynthByName(synth.Parent);
                    if (parent != null)
                        parent.Children.Remove(synth.Name);
                }

            }
        }
        /// <summary>
        /// PLEASE REMAKE THIS!!!
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="synth"></param>
        /// <param name="children"></param>
        void DrawSynthSelectable(CelestaProject renderer, SynthNode synth, List<SynthNode> children)
        {
            bool isSelected = false;
            bool isOpen = false;
            if (children.Count > 0)
                isOpen = ImConverse.VisibilityNode(synth.SynthName, ref isSelected, delegate { RightClickCue(synth); }, in_Icon: NodeIconResource.SynthGroup);
            else
                isSelected = ImConverse.VisibilityNodeSimple(synth.SynthName, delegate { RightClickCue(synth); }, in_Icon: NodeIconResource.Synth);

            if (isSelected)
            {
                EditorWindow.Instance.SelectSynth(synth);
            }
            if (isOpen)
            {
                ImGui.Indent();
                foreach (var child in children)
                {
                    DrawSynthSelectable(renderer, child, GetSynthChildren(child));
                }
                ImGui.Unindent();
                ImGui.TreePop();
            }
        }
        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (CelestaProject)in_Renderer;
            ImGui.SetNextWindowPos(new Vector2(0, ViewportWindow.WindowSize.Y + Celesta.MenuBarWindow.menuBarHeight));
            ImGui.SetNextWindowSize(ViewportWindow.WindowSize);
            if (ImGui.Begin("Synths", ImGuiWindowFlags.NoResize))
            {
                if (ImGui.BeginListBox("##synthlist", new Vector2(-1, -1)))
                {
                    if (renderer.config.workFile != null)
                    {
                        for (int i = 0; i < renderer.config.workFile.SynthNodes.Count; i++)
                        {
                            SynthNode item = renderer.config.workFile.SynthNodes[i];
                            if (item.Parent == null)
                            {
                                var children = GetSynthChildren(item);
                                DrawSynthSelectable(renderer, item, children);
                            }
                        }
                    }
                    ImGui.EndListBox();
                }
            }
            ImGui.End();
        }
    }
}
