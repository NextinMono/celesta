using Celesta;
using Celesta.BuilderNodes;
using Celesta.Project;
using Celesta.CsbTypes;
using HekonrayBase;
using HekonrayBase.Base;
using Hexa.NET.ImGui;
using System;
using System.Numerics;
using Hexa.NET.ImPlot;
using Hexa.NET.Utilities.Text;
using System.Collections.Generic;
namespace Celesta
{
    public class EditorWindow : Singleton<EditorWindow>, IWindow
    {
        private int cueNodeSelection, seSelection;
        private EasyAisacNode aisacSelection;
        private SynthNode synthSelection;
        CsbProject csbProject => CelestaProject.Instance.config.workFile;
        public void SelectSE(int in_Index)
        {
            seSelection = in_Index;
            cueNodeSelection = -1;
            synthSelection = null;
            aisacSelection = null;
        }
        public void SelectCue(int in_Index)
        {
            cueNodeSelection = in_Index;
            synthSelection = null;
            seSelection = -1;
            aisacSelection = null;
        }
        public void SelectSynth(SynthNode in_Synth)
        {
            cueNodeSelection = -1;
            synthSelection = in_Synth;
            seSelection = -1;
            aisacSelection = null;
        }
        public void SelectAisac(EasyAisacNode in_Index)
        {
            cueNodeSelection = -1;
            synthSelection = null;
            seSelection = -1;
            aisacSelection = in_Index;
        }
        public CueNode GetCurrentCueNode()
        {
            if (cueNodeSelection == -1)
                return null;
            return csbProject.CueNodes[Math.Clamp(cueNodeSelection, 0, csbProject.CueNodes.Count-1)];
        }
        public SoundElement GetCurrentSE()
        {
            if(seSelection == -1) return null;
            return csbProject.SoundElementNodes[Math.Clamp(seSelection, 0, csbProject.SoundElementNodes.Count - 1)];
        }
        public EasyAisacNode GetCurrentAisac()
        {
            return aisacSelection;
        }
        public SynthNode GetCurrentSynth()
        {
            return synthSelection;
        }
        public void OnReset(IProgramProject in_Renderer)
        {
            cueNodeSelection = -1;
            seSelection = -1;
            synthSelection = null;
        }

        public bool CollapsingHeaderIcon(string in_label, SIconData in_IconData, ImGuiTreeNodeFlags in_Flags = ImGuiTreeNodeFlags.None)
        {
            var color = in_IconData.Color;
            color.W = 0.3f;
            ImGui.PushStyleColor(ImGuiCol.Header, ImGui.ColorConvertFloat4ToU32(color));
            bool ret = ImGui.CollapsingHeader($"{in_IconData.Icon} {in_label}", in_Flags);
            ImGui.PopStyleColor();
            return ret;
        }
        //public bool CollapsingHeaderIcon(string in_Label, SIconData in_IconData)
        //{
        //    ImGui.BeginGroup();
        //    var region = ImGui.GetContentRegionAvail().X;
        //    var cursorPos = ImGui.GetCursorPos();
        //    bool returnval = ImGui.CollapsingHeader($"##{in_Label}");
        //    ImGui.SetCursorPosX(cursorPos.X += region / 10);
        //    ImGui.SetCursorPosY(cursorPos.Y);
        //    ImGui.Text(in_IconData.Icon);
        //}

        public static void PushMultiItemsWidths(int components, float widthFull = 0.0f)
        {
            if (widthFull <= 0.0f)
                widthFull = ImGui.CalcItemWidth();

            float itemInnerSpacingX = ImGui.GetStyle().ItemInnerSpacing.X;
            float totalSpacing = itemInnerSpacingX * (components - 1);
            float itemWidth = (widthFull - totalSpacing) / components;

            for (int i = 0; i < components; i++)
                ImGui.PushItemWidth(itemWidth);
        }
        public bool InputChannel(string in_Label, ref int value1, ref int value2, bool clamp = true, int max = 255)
        {
            ImGui.BeginGroup();
            ImGui.PushID(in_Label);

            bool value_changed = false;
            var g = ImGui.GetCurrentContext();
            PushMultiItemsWidths(2, ImGui.CalcItemWidth());

            ImGui.PushID(1);
            //if (i > 0)
            //    ImGui.SameLine(0, g.Style.ItemInnerSpacing.X);
            value_changed |= ImGui.InputInt($"##{in_Label}1", ref value1);
            ImGui.PopID();
            ImGui.PopItemWidth();
            if(clamp)
            value1 = Math.Clamp(value1, 0, ushort.MaxValue);

            ImGui.PushID(2);
            //if (i > 0)
            ImGui.SameLine(0, g.Style.ItemInnerSpacing.X);
            value_changed |= ImGui.SliderInt($"##{in_Label}2", ref value2, 0, max);
            ImGui.PopID();
            ImGui.PopItemWidth();
            //for (int i = 0; i < 2; i++)
            //{
            //    ImGui.PushID(i);
            //    if (i > 0)
            //        ImGui.SameLine(0, g.Style.ItemInnerSpacing.X);
            //    value_changed |= ImGui.InputInt("");
            //    ImGui.PopID();
            //    ImGui.PopItemWidth();
            //}
            ImGui.PopID();


            ImGui.SameLine(0.0f, g.Style.ItemInnerSpacing.X);
            ImGui.Text(in_Label);


            ImGui.EndGroup();
            return value_changed;
        }
        public bool InputImport(string in_Label, ref string in_FileName, ref bool in_IsImporting)
        {
            ImGui.BeginGroup();
            ImGui.PushID(in_Label);

            bool value_changed = false;
            var g = ImGui.GetCurrentContext();

            float itemInnerSpacingX = ImGui.GetStyle().ItemInnerSpacing.X;
            float totalSpacing = itemInnerSpacingX * (2 - 1);
            float itemWidth = (ImGui.CalcItemWidth() - totalSpacing);

            ImGui.PushItemWidth(itemWidth / 5);
            ImGui.PushItemWidth(itemWidth);

            ImGui.PushID(1);
            //if (i > 0)
            //    ImGui.SameLine(0, g.Style.ItemInnerSpacing.X);
            value_changed |= ImGui.InputText($"##{in_Label}1", ref in_FileName, 16384);
            ImGui.PopID();
            ImGui.PopItemWidth();

            ImGui.PushID(2);
            //if (i > 0)
            ImGui.SameLine(0, g.Style.ItemInnerSpacing.X);
            in_IsImporting = ImGui.Button($"...");
            ImGui.PopID();
            ImGui.PopItemWidth();
            //for (int i = 0; i < 2; i++)
            //{
            //    ImGui.PushID(i);
            //    if (i > 0)
            //        ImGui.SameLine(0, g.Style.ItemInnerSpacing.X);
            //    value_changed |= ImGui.InputInt("");
            //    ImGui.PopID();
            //    ImGui.PopItemWidth();
            //}
            ImGui.PopID();


            ImGui.SameLine(0.0f, g.Style.ItemInnerSpacing.X);
            ImGui.Text(in_Label);


            ImGui.EndGroup();
            return value_changed;
        }

        private void DrawGraph(string in_Name, BuilderAisacGraphNode in_Graph, int synth)
        {
            unsafe
            {
                int type = in_Graph.Type;
                ImGui.PushID($"{in_Name}_type");
                ImGui.InputInt("Type", ref type);
                ImGui.PopID();
                in_Graph.Type = (byte)type;

                //ImPlot.PushStyleVar(ImPlotStyleVar., ImPlotAxisFlags.LockMin);
                if (ImPlot.BeginPlot(in_Name, new System.Numerics.Vector2(-1, 200)))
                {
                    ImPlotAxis* xAxis = ImPlot.GetCurrentPlot().XAxis(0);
                    xAxis->Flags |= ImPlotAxisFlags.LockMin | ImPlotAxisFlags.LockMax;
                    xAxis->SetMin(0);
                    xAxis->SetMax(110, true);

                    //WHYYYYY
                    ImPlotAxis* yAxis = ImPlot.GetCurrentPlot().YAxis(0);
                    yAxis->Flags |= ImPlotAxisFlags.LockMin | ImPlotAxisFlags.LockMax;
                    yAxis->SetMin(0);
                    yAxis->SetMax(110, true);

                    int divisionFactor = 100;
                    const int bufferSize = 256;
                    byte* buffer = stackalloc byte[bufferSize];
                    StrBuilder sb = new(buffer, bufferSize);
                    sb.Append($"{in_Name}anim");
                    sb.End();

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
                                if (AudioManager.IsPlaying)
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
        public void Render(IProgramProject in_Renderer)
        {
            var posEditor = new Vector2(ViewportWindow.WindowSize.X, Celesta.MenuBarWindow.menuBarHeight);
            var sizeEditor = new Vector2(ViewportWindow.WindowSize.X * 2.5f, ViewportWindow.WindowSize.Y * 2);

            ImGui.SetNextWindowPos(posEditor);
            ImGui.SetNextWindowSize(sizeEditor);
            var renderer = (CelestaProject)in_Renderer;
            if (ImGui.Begin("Editor", ImGuiWindowFlags.NoResize))
            {
                if (ImGui.BeginListBox("##editor", new Vector2(-1, -1)))
                {
                    if (renderer.config.workFile != null)
                    {
                        CueNode currentCue = GetCurrentCueNode();
                        SoundElement currentSE = GetCurrentSE();
                        SynthNode currentSynth = GetCurrentSynth();
                        EasyAisacNode currentAisac = GetCurrentAisac();

                        if (currentCue != null)
                        {
                            currentSynth = currentCue.GetSynth();
                            if (CollapsingHeaderIcon("Cue Settings", NodeIconResource.Cue, ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                int id = (int)currentCue.Id;
                                string name = currentCue.Name;
                                int flag = currentCue.Flags;
                                ImGui.InputInt("ID", ref id);
                                ImGui.InputText("Name", ref name, 512);
                                ImGui.InputInt("Flags", ref flag);
                                if (ImGui.BeginCombo("Synth", currentSynth.SynthName))
                                {
                                    for (int n = 0; n < csbProject.SynthNodes.Count; n++)
                                    {
                                        bool isChild = !string.IsNullOrEmpty(csbProject.SynthNodes[n].Parent);
                                        string icon = (csbProject.SynthNodes[n].Children.Count > 0 ? NodeIconResource.SynthGroup.Icon : NodeIconResource.Synth.Icon) + " ";
                                        if (isChild)
                                            ImGui.Indent();
                                        if (ImGui.Selectable(icon + csbProject.SynthNodes[n].SynthName))
                                        {
                                            currentCue.SynthReference = csbProject.SynthNodes[n].Path;
                                        }
                                        if (isChild)
                                            ImGui.Unindent();
                                    }
                                    ImGui.EndCombo();
                                }
                                currentCue.Id = (uint)Math.Clamp(id, 0, uint.MaxValue);
                                currentCue.Name = name;
                                currentCue.Flags = (byte)flag;
                            }
                        }
                        if (currentSynth != null)
                        {
                            currentAisac = currentSynth.GetEasyAisac();
                            if (CollapsingHeaderIcon($"Synth Settings",NodeIconResource.Synth, ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                string name = currentSynth.SynthName;
                                int csbVolume = currentSynth.Volume;
                                int wetMix = currentSynth.WetLevels.center.level;
                                var panAngle = (int)currentSynth.Pan3D.AngleOffset;
                                if (csbVolume > 1250)
                                {
                                    ImConverse.Notice("The volume of this synth is higher than 100%, playback volume will be lowered in the tool.", ImConverseNotice.Warning);
                                }
                                if (wetMix > 0)
                                {
                                    ImConverse.Notice("The reverb is not accurate, and the effect might not be applied in-game.", ImConverseNotice.Info);
                                }

                                ImGui.PushID("##synthname");
                                if(ImGui.InputText("Name", ref name, 512))
                                {
                                    csbProject.Rename(currentSynth, name);
                                    currentSynth.SynthName = name;
                                }
                                ImGui.PopID();
                                ImGui.SeparatorText("Quick Access");
                                if (ImConverse.PercentKnob("Volume", ref csbVolume, 1000))
                                {
                                    currentSynth.Volume = (short)csbVolume;
                                }
                                ImGui.SameLine();
                                ImConverse.AngleKnob("Pan3D Angle", ref panAngle, new Vector4(0, 0.6f, 1, 1));
                                ImGui.SameLine();
                                ImConverse.PercentKnob("Wet Mix", ref wetMix, 255, new Vector4(0.6f, 0, 1, 1));
                                currentSynth.WetLevels.center.level = (ushort)(wetMix);
                                currentSynth.Pan3D.AngleOffset = (short)panAngle;

                                ImGui.SeparatorText("");
                                if (ImGui.BeginCombo("Sound Element", string.IsNullOrEmpty(currentSynth.SoundElementReference) ? "Empty" : currentSynth.SoundElementReference))
                                {
                                    if(ImGui.Selectable("Empty"))
                                    {
                                        currentSynth.SoundElementReference = "";
                                    }
                                    for (int n = 0; n < csbProject.SoundElementNodes.Count; n++)
                                    {
                                        
                                        string icon = NodeIconResource.SoundElement.Icon + " ";
                                        
                                        if (ImGui.Selectable(icon + csbProject.SoundElementNodes[n].SEName))
                                        {
                                            currentSynth.SoundElementReference = csbProject.SoundElementNodes[n].Path;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }
                                //if (ImGui.InputFloat("Volume", ref vol, 0, 1000))
                                //{
                                //    if(sounds.TryGetValue(currentSynth, out SoundPlayer sound))
                                //        sound.SetVolumeCsb(vol);
                                //}
                                currentSynth.Volume = (short)csbVolume;
                                if (ImGui.TreeNodeEx("Dry Send Filter"))
                                {
                                    ImGui.PushID("dry");
                                    var dryName = currentSynth.DryOName;
                                    var sendLevels = currentSynth.DrySendLevels;
                                    //L/R = Front Left/Right, LS/RS = Side Left/Right, LFE = sub, EX1/2 = IDK!
                                    int dryL = sendLevels.left.level;
                                    int dryLG = sendLevels.left.gain;
                                    int dryR = sendLevels.right.level;
                                    int dryRG = sendLevels.right.gain;
                                    int dryLS = sendLevels.rearLeft.level;
                                    int dryLSG = sendLevels.rearLeft.gain;
                                    int dryRS = sendLevels.rearRight.level;
                                    int dryRSG = sendLevels.rearRight.gain;
                                    int dryC = sendLevels.center.level;
                                    int dryCG = sendLevels.center.gain;
                                    int dryLFE = sendLevels.lfe.level;
                                    int dryLFEG = sendLevels.lfe.gain;
                                    int dryEX1 = sendLevels.extra1.level;
                                    int dryEX1G = sendLevels.extra1.gain;
                                    int dryEX2 = sendLevels.extra2.level;
                                    int dryEX2G = sendLevels.extra2.gain;
                                    ImGui.InputText("DryOName", ref dryName, 512);
                                    InputChannel("Left", ref dryL, ref dryLG);
                                    InputChannel("Right", ref dryR, ref dryRG);
                                    InputChannel("Side Left", ref dryLS, ref dryLSG);
                                    InputChannel("Side Right", ref dryRS, ref dryRSG);
                                    InputChannel("Center", ref dryC, ref dryCG);
                                    InputChannel("LFE", ref dryLFE, ref dryLFEG);
                                    InputChannel("Extra 1", ref dryEX1, ref dryEX1G);
                                    InputChannel("Extra 2", ref dryEX2, ref dryEX2G);
                                    currentSynth.DrySendLevels.SetLevels([(ushort)dryL, (ushort)dryR, (ushort)dryLS, (ushort)dryRS, (ushort)dryC, (ushort)dryLFE, (ushort)dryEX1, (ushort)dryEX2]);
                                    currentSynth.DrySendLevels.SetGains([(ushort)dryLG, (ushort)dryRG, (ushort)dryLSG, (ushort)dryRSG, (ushort)dryCG, (ushort)dryLFEG, (ushort)dryEX1G, (ushort)dryEX2G]);
                                    currentSynth.DryOName = dryName;
                                    ImGui.PopID();
                                    ImGui.TreePop();
                                }
                                if (ImGui.TreeNodeEx("Wet Filter"))
                                {
                                    ImGui.PushID("wet");
                                    var dryName = currentSynth.WetOName;
                                    var sendLevels = currentSynth.WetLevels;
                                    //L/R = Front Left/Right, LS/RS = Side Left/Right, LFE = sub, EX1/2 = IDK!
                                    int dryL = sendLevels.left.level;
                                    int dryLG = sendLevels.left.gain;
                                    int dryR = sendLevels.right.level;
                                    int dryRG = sendLevels.right.gain;
                                    int dryLS = sendLevels.rearLeft.level;
                                    int dryLSG = sendLevels.rearLeft.gain;
                                    int dryRS = sendLevels.rearRight.level;
                                    int dryRSG = sendLevels.rearRight.gain;
                                    int dryC = sendLevels.center.level;
                                    int dryCG = sendLevels.center.gain;
                                    int dryLFE = sendLevels.lfe.level;
                                    int dryLFEG = sendLevels.lfe.gain;
                                    int dryEX1 = sendLevels.extra1.level;
                                    int dryEX1G = sendLevels.extra1.gain;
                                    int dryEX2 = sendLevels.extra2.level;
                                    int dryEX2G = sendLevels.extra2.gain;
                                    ImGui.InputText("WetOName", ref dryName, 512);
                                    InputChannel("Left", ref dryL, ref dryLG);
                                    InputChannel("Right", ref dryR, ref dryRG);
                                    InputChannel("Side Left", ref dryLS, ref dryLSG);
                                    InputChannel("Side Right", ref dryRS, ref dryRSG);
                                    InputChannel("Center", ref dryC, ref dryCG);
                                    InputChannel("LFE", ref dryLFE, ref dryLFEG);
                                    InputChannel("Extra 1", ref dryEX1, ref dryEX1G);
                                    InputChannel("Extra 2", ref dryEX2, ref dryEX2G);
                                    currentSynth.WetLevels.SetLevels([(ushort)dryL, (ushort)dryR, (ushort)dryLS, (ushort)dryRS, (ushort)dryC, (ushort)dryLFE, (ushort)dryEX1, (ushort)dryEX2]);
                                    currentSynth.WetLevels.SetGains([(ushort)dryLG, (ushort)dryRG, (ushort)dryLSG, (ushort)dryRSG, (ushort)dryCG, (ushort)dryLFEG, (ushort)dryEX1G, (ushort)dryEX2G]);
                                    currentSynth.WetOName = dryName;
                                    ImGui.PopID();
                                    ImGui.TreePop();
                                }
                                if (ImGui.TreeNodeEx("Pan3D Settings"))
                                {
                                    var vOff    = (int)currentSynth.Pan3D.VolumeOffset;
                                    var vGain   = (int)currentSynth.Pan3D.VolumeGain;
                                    var aOff    = (int)currentSynth.Pan3D.AngleOffset;
                                    var aGain   = (int)currentSynth.Pan3D.AngleGain;
                                    var dOff = (int)currentSynth.Pan3D.DistanceOffset;
                                    var dGain   = (int)currentSynth.Pan3D.DistanceGain;
                                    InputChannel("Volume", ref vOff, ref vGain, false, 1000);
                                    InputChannel("Angle", ref aOff, ref aGain, false, 1000);
                                    InputChannel("Distance", ref dOff, ref dGain, false, 1000);
                                    currentSynth.Pan3D.VolumeOffset     = (short)vOff;
                                    currentSynth.Pan3D.VolumeGain       = (short)vGain;
                                    currentSynth.Pan3D.AngleOffset      = (short)aOff;
                                    currentSynth.Pan3D.AngleGain        = (short)aGain;
                                    currentSynth.Pan3D.DistanceOffset   = (short)dOff;
                                    currentSynth.Pan3D.DistanceGain     = (short)dGain;

                                    ImGui.TreePop();
                                }
                                if (ImGui.TreeNodeEx("Envelope Generator"))
                                {
                                    var attack = (int)currentSynth.EgAttack;
                                    var decay = (int)currentSynth.EgDecay;
                                    var delay = (int)currentSynth.EgDelay;
                                    var hold = (int)currentSynth.EgHold;
                                    var release = (int)currentSynth.EgRelease;
                                    var sustain = (int)currentSynth.EgSustain;
                                    ImConverse.Notice("This filter is not applied in the tool.", ImConverseNotice.Info);
                                    ImGui.InputInt("Attack", ref attack);
                                    ImGui.InputInt("Decay", ref decay);
                                    ImGui.InputInt("Delay", ref delay);
                                    ImGui.InputInt("Hold", ref hold);
                                    ImGui.InputInt("Release", ref release);
                                    ImGui.InputInt("Sustain", ref sustain);

                                    currentSynth.EgAttack = (ushort)attack;
                                    currentSynth.EgDecay = (ushort)decay;
                                    currentSynth.EgDelay = (ushort)delay;
                                    currentSynth.EgHold = (ushort)hold;
                                    currentSynth.EgRelease = (ushort)release;
                                    currentSynth.EgSustain = (ushort)sustain;
                                    ImGui.TreePop();
                                }
                                if (ImGui.TreeNodeEx("Filter 0"))
                                {
                                    var fCut1 = (int)currentSynth.FilterCutoff1;
                                    var fCut2 = (int)currentSynth.FilterCutoff2;
                                    var fRelO = (int)currentSynth.FilterReleaseOffset;
                                    var fReso = (int)currentSynth.FilterReso;
                                    var fType = (int)currentSynth.FilterType;
                                    ImConverse.Notice("This filter is not applied in the tool.", ImConverseNotice.Info);
                                    ImGui.InputInt("Cutoff 1", ref fCut1);
                                    ImGui.InputInt("Cutoff 2", ref fCut2);
                                    ImGui.InputInt("Release Offset", ref fRelO);
                                    ImGui.InputInt("Reso", ref fReso);
                                    ImGui.InputInt("Type", ref fType);
                                    currentSynth.FilterCutoff1 = (ushort)fCut1;
                                    currentSynth.FilterCutoff2 = (ushort)fCut2;
                                    currentSynth.FilterReleaseOffset = (byte)fRelO;
                                    currentSynth.FilterReso = (ushort)fReso;
                                    currentSynth.FilterType = (byte)fType;
                                    ImGui.TreePop();
                                }
                                if (ImGui.TreeNodeEx("Filter 1"))
                                {
                                    var fCutGain =      (int)currentSynth.Filter1CutoffGain;
                                    var fCutOffset =    (int)currentSynth.Filter1CutoffOffset;
                                    var fResoGain =     (int)currentSynth.Filter1ResoGain;
                                    var fResoOffset =   (int)currentSynth.Filter1ResoOffset;
                                    var fType =         (int)currentSynth.Filter1Type;
                                    ImConverse.Notice("This filter is not applied in the tool.", ImConverseNotice.Info);
                                    InputChannel("Cutoff", ref fCutOffset, ref fCutGain, true, 1000);
                                    InputChannel("Reso", ref fResoOffset, ref fResoGain, true, 1000);
                                    ImGui.InputInt("Type", ref fType);
                                    currentSynth.Filter1CutoffGain = (ushort)fCutGain;
                                    currentSynth.Filter1CutoffOffset = (ushort)fCutOffset;
                                    currentSynth.Filter1ResoGain = (ushort)fResoGain;
                                    currentSynth.Filter1ResoOffset = (ushort)fResoOffset;
                                    currentSynth.Filter1Type = (byte)fType;
                                    ImGui.TreePop();
                                }
                                if (ImGui.TreeNodeEx("Filter 2"))
                                {
                                    var fCutGain = (int)currentSynth.Filter2CutoffHigherGain;
                                    var fCutOffset = (int)currentSynth.Filter2CutoffHigherOffset;
                                    var fResoGain = (int)currentSynth.Filter2CutoffLowerGain;
                                    var fResoOffset = (int)currentSynth.Filter2CutoffLowerOffset;
                                    var fType = (int)currentSynth.Filter2Type;
                                    ImConverse.Notice("This filter is not applied in the tool.", ImConverseNotice.Info);
                                    InputChannel("High Cutoff", ref fCutOffset, ref fCutGain, true, 1000);
                                    InputChannel("Low Cutoff", ref fResoOffset, ref fResoGain, true, 1000);
                                    ImGui.InputInt("Type", ref fType);
                                    currentSynth.Filter2CutoffHigherGain = (ushort)fCutGain;
                                    currentSynth.Filter2CutoffHigherOffset = (ushort)fCutOffset;
                                    currentSynth.Filter2CutoffLowerGain = (ushort)fResoGain;
                                    currentSynth.Filter2CutoffLowerOffset = (ushort)fResoOffset;
                                    currentSynth.Filter1Type = (byte)fType;
                                    ImGui.TreePop();
                                }
                                if (ImGui.TreeNodeEx("Unknown"))
                                {
                                    var fUnk1 = currentSynth.Mtxrtr;
                                    var fUnk2 = (int)currentSynth.NLmtChildren;
                                    var fUnk3 = (int)currentSynth.SControl;

                                    var fUnk4 = currentSynth.Wcnct0;
                                    var fUnk5 = currentSynth.Wcnct1;
                                    var fUnk6 = currentSynth.Wcnct2;
                                    var fUnk7 = currentSynth.Wcnct3;
                                    var fUnk8 = currentSynth.Wcnct4;
                                    var fUnk9 = currentSynth.Wcnct5;
                                    var fUnk10 = currentSynth.Wcnct6;
                                    var fUnk11 = currentSynth.Wcnct7;

                                    ImGui.InputText("Mtxrtr", ref fUnk1, 512);
                                    ImGui.InputInt("NLmtChildren", ref fUnk2);
                                    ImGui.InputInt("SControl", ref fUnk3);
                                    ImGui.InputText("Wcnct0", ref fUnk4, 512);
                                    ImGui.InputText("Wcnct1", ref fUnk5, 512);
                                    ImGui.InputText("Wcnct2", ref fUnk6, 512);
                                    ImGui.InputText("Wcnct3", ref fUnk7, 512);
                                    ImGui.InputText("Wcnct4", ref fUnk8, 512);
                                    ImGui.InputText("Wcnct5", ref fUnk9, 512);
                                    ImGui.InputText("Wcnct6", ref fUnk10, 512);
                                    ImGui.InputText("Wcnct7", ref fUnk11, 512);
                                    ImGui.TreePop();

                                    currentSynth.Mtxrtr = fUnk1;
                                    currentSynth.NLmtChildren = (byte)fUnk2;
                                    currentSynth.SControl = (byte)fUnk3;
                                    currentSynth.Wcnct0 = fUnk4;
                                    currentSynth.Wcnct1 = fUnk5;
                                    currentSynth.Wcnct2 = fUnk6;
                                    currentSynth.Wcnct3 = fUnk7;
                                    currentSynth.Wcnct4 = fUnk8;
                                    currentSynth.Wcnct5 = fUnk9;
                                    currentSynth.Wcnct6 = fUnk10;
                                    currentSynth.Wcnct7 = fUnk11;
                                }
                            }
                        }
                        if(currentAisac != null)
                        {
                            if (CollapsingHeaderIcon($"Aisac Node Settings", NodeIconResource.Aisac, ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                string aisacname = currentAisac.AisacName;
                                ImGui.InputText("Name", ref aisacname, 256);
                                ImGui.SeparatorText("Graphs");
                                for (int a = 0; a < currentAisac.AisacNodes.Count; a++)
                                {
                                    if (ImGui.TreeNodeEx(currentAisac.AisacNodes[a].Path))
                                    {
                                        string name = currentAisac.AisacNodes[a].AisacName;
                                        ImGui.InputText("Name", ref name, 512);
                                        for (int b = 0; b < currentAisac.AisacNodes[a].Graphs.Count; b++)
                                        {
                                            int snyth = 0;
                                            for (int h = 0; h < renderer.config.workFile.SynthNodes.Count; h++)
                                            {
                                                SynthNode synth = renderer.config.workFile.SynthNodes[h];
                                                if (synth.AisacReference == currentAisac.AisacNodes[a].Path)
                                                {
                                                    snyth = h;
                                                    break;
                                                }
                                            }
                                            DrawGraph($"##curves_{b}", currentAisac.AisacNodes[a].Graphs[b], snyth);
                                        }
                                        ImGui.TreePop();
                                    }
                                }
                            }
                            //for (int i = 0; i < renderer.config.workFile.AisacNodes.Count; i++)
                            //{
                            //    AisacNode item = renderer.config.workFile.AisacNodes[i];
                            //    if (ImGui.CollapsingHeader(item.Path))
                            //    {
                            //        for (int a = 0; a < item.Graphs.Count; a++)
                            //        {
                            //            int snyth = 0;
                            //            for (int h = 0; h < renderer.config.workFile.SynthNodes.Count; h++)
                            //            {
                            //                SynthNode synth = renderer.config.workFile.SynthNodes[h];
                            //                if (synth.AisacReference == item.Path)
                            //                {
                            //                    snyth = h;
                            //                    break;
                            //                }
                            //            }
                            //            DrawGraph($"##curves{i}_{a}", item.Graphs[a], snyth);
                            //        }
                            //    }
                            //}
                        }
                        if(currentSE != null)
                        {
                            if (CollapsingHeaderIcon($"Sound Element Settings", NodeIconResource.SoundElement, ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                var name = currentSE.SEName;
                                if(ImGui.InputText("Name", ref name, 4096))
                                {
                                    csbProject.Rename(currentSE, name);
                                }
                                ImGui.SeparatorText("Files");
                                var inputintro = string.IsNullOrEmpty(currentSE.Intro) ? "" : currentSE.Intro;
                                var inputloop = string.IsNullOrEmpty(currentSE.Loop) ? "" : currentSE.Loop;
                                bool isIntroBrowse = false;
                                bool isLoopBrowse = false;
                                if(InputImport("Intro File", ref inputintro, ref isIntroBrowse))                                
                                   currentSE.Intro = inputintro;
                                
                                if(InputImport("Loop File", ref inputloop, ref isLoopBrowse))
                                    currentSE.Loop = inputloop;

                                if (isIntroBrowse)
                                {
                                    currentSE.Intro = LoadNewAudio(currentSE.Intro);                                    
                                }
                                if (isLoopBrowse)
                                {
                                    currentSE.Loop = LoadNewAudio(currentSE.Intro);
                                }
                            }
                        }
                    }
                    ImGui.EndListBox();
                }
            }
            ImGui.End();
        }

        private string LoadNewAudio(string in_CurrentFile)
        {
            var testdial = NativeFileDialogSharp.Dialog.FileOpen("adx");
            if (testdial.IsOk)
            {
                var fname = csbProject.AddAudioFile(testdial.Path);
                if (!string.IsNullOrEmpty(fname))
                {
                    return fname;
                }
            }
            return in_CurrentFile;
        }
    }
}