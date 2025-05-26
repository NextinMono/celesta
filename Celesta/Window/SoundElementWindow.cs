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
    public class SoundElementWindow : Singleton<SoundElementWindow>, IWindow
    {
        public void OnReset(IProgramProject in_Renderer)
        {
        }
        void RightClickCue()
        {
            if (ImGui.Selectable("Add"))
            {
                CelestaProject.Instance.AddSoundElement();
            }
            if (ImGui.Selectable("Remove"))
            {
                var cue = EditorWindow.Instance.GetCurrentSE();
                CelestaProject.Instance.config.workFile.SoundElementNodes.Remove(cue);
            }
        }
            CsbProject csbProject => CelestaProject.Instance.config.workFile;

        public void Render(IProgramProject in_Renderer)
        {
            var posEditor = new Vector2(ViewportWindow.WindowSize.X, Celesta.MenuBarWindow.menuBarHeight);
            var sizeEditor = new Vector2(ViewportWindow.WindowSize.X * 2.5f, ViewportWindow.WindowSize.Y * 2);
            var renderer = (CelestaProject)in_Renderer;
            ImGui.SetNextWindowPos(posEditor + new Vector2(sizeEditor.X, ViewportWindow.WindowSize.Y));
            ImGui.SetNextWindowSize(ViewportWindow.WindowSize);
            if (ImGui.Begin("Sound Element", ImGuiWindowFlags.NoResize))
            {
                if (ImGui.BeginListBox("##selist", new Vector2(-1, -1)))
                {
                    if(csbProject != null)
                    {
                        for (int i = 0; i < csbProject.SoundElementNodes.Count; i++)
                        {
                            SoundElement soundElement = csbProject.SoundElementNodes[i];
                            if (ImConverse.VisibilityNodeSimple(soundElement.Path.Split('/')[^1], RightClickCue, in_Icon: NodeIconResource.SoundElement))
                            {
                                EditorWindow.Instance.SelectSE(i);
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
