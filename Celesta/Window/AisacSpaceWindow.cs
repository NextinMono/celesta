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
        public void OnReset(IProgramProject in_Renderer)
        {
        }
        public float aisacprogres;

        CsbProject csbProject => CelestaProject.Instance.config.workFile;

        private void DrawGraph(string in_Name, BuilderAisacGraphNode in_Graph, int synth)
        {
            unsafe
            {
                int type = in_Graph.Type;
                ImGui.PushID($"{in_Name}_type");
                ImGui.InputInt("Type", ref type);
                ImGui.PopID();
                in_Graph.Type = (byte)type;
                if (ImPlot.BeginPlot(in_Name, new System.Numerics.Vector2(-1, -1)))
                {
                    int divisionFactor = 100;
                    const int bufferSize = 256;
                    byte* buffer = stackalloc byte[bufferSize];
                    StrBuilder sb = new(buffer, bufferSize);
                    sb.Append($"{in_Name}anim");
                    sb.End();
                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, 10000 / divisionFactor);
                    ImPlot.SetupAxisLimits(ImAxis.Y1, 0, 10000 / divisionFactor);

                    //ms_Points.Clear();
                    //Line for the anim time
                    //if (ImPlot.DragLineX(0, &time, new Vector4(1, 1, 1, 1), 1))
                    //{
                    //    in_Renderer.Config.Time = (float)(time / selectedScene.Value.Value.FrameRate);
                    //}
                    List<float> pointsX = new List<float>();
                    List<float> pointsY = new List<float>();
                    //Animation keyframe points
                    for (int i = 0; i < in_Graph.Points.Count; i++)
                    {
                        ImPlotPoint point = new ImPlotPoint(in_Graph.Points[i].X / divisionFactor, in_Graph.Points[i].Y / divisionFactor);

                        bool isClicked = false;
                        if (ImPlot.DragPoint(i, &point.X, &point.Y, new Vector4(0, 0.6f, 1, 1), 8, ImPlotDragToolFlags.None, &isClicked))
                        {
                            if (point.X <= 0)
                                point.X = 0;
                            if (point.Y <= 0)
                            {
                                point.Y = 0;
                                if(AudioManager.IsPlaying)
                                    AudioManager.sounds[csbProject.SynthNodes[synth]].Volume = 0;
                            }
                            else
                            {
                                if (point.Y > 50000 / divisionFactor)
                                    point.Y = 50000 / divisionFactor;
                                if (AudioManager.IsPlaying)
                                    AudioManager.sounds[csbProject.SynthNodes[synth]].SetVolumeCsb((float)(point.Y));
                            }
                            in_Graph.Points[i].X = (ushort)(point.X * divisionFactor);
                            in_Graph.Points[i].Y = (ushort)(point.Y * divisionFactor);
                        }
                        pointsX.Add((float)point.X);
                        pointsY.Add((float)point.Y);
                        //if (isClicked)
                        //    in_Renderer.SelectionData.KeyframeSelected = in_Renderer.SelectionData.TrackAnimation.Frames[i];
                    }

                    fixed (float* test = pointsX.ToArray())
                    {
                        fixed (float* test2 = pointsY.ToArray())
                        {
                            ImPlot.SetNextLineStyle(new Vector4(0, 0.6f, 1, 1), 2.0f);
                            ImPlot.PlotLine($"##h1", test, test2, pointsX.Count);
                        }
                    }
                    ImPlot.EndPlot();
                }
            }
        }
        void DrawAisacSlider(EasyAisacNode node)
        {
            float prog = node.progress;
            ImGui.SliderFloat(node.aisacName, ref prog, 0, 1);
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
                        if (editModeAisac)
                        {
                            for (int i = 0; i < renderer.config.workFile.AisacNodes.Count; i++)
                            {
                                BuilderAisacNode item = renderer.config.workFile.AisacNodes[i];
                                if (ImGui.CollapsingHeader(item.Name))
                                {

                                    for (int a = 0; a < item.Graphs.Count; a++)
                                    {
                                        var minX = item.Graphs[a].MinimumX;
                                        var minY = item.Graphs[a].MinimumY;
                                        var maxX = item.Graphs[a].MaximumX;
                                        var maxY = item.Graphs[a].MaximumY;
                                        ImGui.Text($"MinX {minX}\nMinY{minY}\nMaxX{maxX}\nMaxY{maxY}");
                                        int snyth = 0;
                                        for (int h = 0; h < renderer.config.workFile.SynthNodes.Count; h++)
                                        {
                                            SynthNode synth = renderer.config.workFile.SynthNodes[h];
                                            if (synth.AisacReference == item.Name)
                                            {
                                                snyth = h;
                                                break;
                                            }
                                        }
                                        DrawGraph($"##curves{i}_{a}", item.Graphs[a], snyth);
                                    }

                                }
                            }
                        }
                        else
                        {
                            if(!showAllTemp)
                            {
                                var currentSynth = EditorWindow.Instance.GetCurrentCueNode();
                                if (currentSynth != null)
                                {
                                    var synth = CsbCommon.GetSynth(currentSynth);
                                    List<string> aisacsAlreadyShown = new List<string>();
                                    foreach(var child in synth.Children)
                                    {
                                        var cSynth = CsbCommon.GetSynthByName(child);
                                        if(cSynth.AisacReference != null)
                                        {
                                            var aisacName = CsbCommon.GetAisacByName(cSynth.AisacReference);
                                            if (!aisacsAlreadyShown.Contains(aisacName.AisacName))
                                            {
                                                aisacsAlreadyShown.Add(aisacName.AisacName);
                                                DrawAisacSlider(csbProject.GetAisacNodesByCommonName(aisacName.AisacName));
                                            }
                                        }
                                    }
                                    if(synth.AisacReference != null)
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
                    }
                    ImGui.EndListBox();
                }
            }
            ImGui.End();
        }
    }
}
