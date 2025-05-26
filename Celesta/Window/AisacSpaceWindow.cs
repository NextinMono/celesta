using Celesta;
using Celesta.BuilderNodes;
using Celesta.Project;
using Celesta.CsbTypes;
using HekonrayBase;
using HekonrayBase.Base;
using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Hexa.NET.Utilities.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celesta
{
    public class AisacSpaceWindow : Singleton<AisacSpaceWindow>, IWindow
    {
        private bool editModeAisac;
        private bool showAllTemp;
        public float aisacprogres;
        CsbProject csbProject => CelestaProject.Instance.config.workFile;
        public void OnReset(IProgramProject in_Renderer)
        {
        }


        void DrawAisacSlider(EasyAisacNode node)
        {
            float prog = node.Progress;
            if(ImConverse.VisibilityNodeSimpleSlider(node.AisacName, ref prog,1, in_Icon: NodeIconResource.Aisac))
            {
                EditorWindow.Instance.SelectAisac(node);
            }    
            //ImGui.SliderFloat(node.aisacName, ref prog, 0, 1);
            node.SetProgress(prog);
        }
        public void Render(IProgramProject in_Renderer)
        {
            var posEditor = new Vector2(ViewportWindow.WindowSize.X, Celesta.MenuBarWindow.menuBarHeight);
            var sizeEditor = new Vector2(ViewportWindow.WindowSize.X * 2.5f, ViewportWindow.WindowSize.Y * 2);
            var renderer = (CelestaProject)in_Renderer;
            ImGui.SetNextWindowPos(posEditor + new Vector2(sizeEditor.X, 0));
            ImGui.SetNextWindowSize(ViewportWindow.WindowSize);
            if (ImGui.Begin("Aisac", ImGuiWindowFlags.NoResize))
            {
                ImGui.Checkbox("Edit Mode", ref editModeAisac);
                ImGui.SameLine();
                ImGui.Checkbox("Show All (temp)", ref showAllTemp);

                if (ImGui.BeginListBox("##aisaclist", new Vector2(-1, -1)))
                {
                    if (renderer.config.workFile != null)
                    {

                        if (!showAllTemp)
                        {
                            var currentSynth = EditorWindow.Instance.GetCurrentCueNode();
                            if (currentSynth != null)
                            {
                                var synth = CsbCommon.GetSynth(currentSynth);
                                List<string> aisacsAlreadyShown = new List<string>();
                                foreach (var child in synth.Children)
                                {
                                    var cSynth = CsbCommon.GetSynthByName(child);
                                    if (cSynth.AisacReference != null)
                                    {
                                        var aisacName = CsbCommon.GetAisacByName(cSynth.AisacReference);
                                        if (!aisacsAlreadyShown.Contains(aisacName.AisacName))
                                        {
                                            aisacsAlreadyShown.Add(aisacName.AisacName);
                                            DrawAisacSlider(csbProject.GetAisacNodesByCommonName(aisacName.AisacName));
                                        }
                                    }
                                }
                                if (synth.AisacReference != null)
                                {
                                    var aisacName = CsbCommon.GetAisacByName(synth.AisacReference);
                                    if (!aisacsAlreadyShown.Contains(aisacName.AisacName))
                                    {
                                        aisacsAlreadyShown.Add(aisacName.AisacName);
                                        DrawAisacSlider(csbProject.GetAisacNodesByCommonName(aisacName.AisacName));
                                    }
                                }
                            }

                        }
                        else
                        {
                            for (int i = 0; i < renderer.config.workFile.UniqueAisacNodes.Count; i++)
                            {
                                EasyAisacNode item = renderer.config.workFile.UniqueAisacNodes[i];
                                DrawAisacSlider(item);
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
