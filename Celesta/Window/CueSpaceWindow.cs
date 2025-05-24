using Celesta;
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
    public class CueSpaceWindow : Singleton<CueSpaceWindow>, IWindow
    {
        public void OnReset(IProgramProject in_Renderer)
        {
        }
        void RightClickCue()
        {
            if(ImGui.Selectable("Add"))
            {
                CelestaProject.Instance.AddCue();
            }
            if(ImGui.Selectable("Remove"))
            {
                var cue = EditorWindow.Instance.GetCurrentCueNode();
                CelestaProject.Instance.config.workFile.CueNodes.Remove(cue);
            }
        }
        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (CelestaProject)in_Renderer;
            ImGui.SetNextWindowPos(new Vector2(0, Celesta.MenuBarWindow.menuBarHeight));
            ImGui.SetNextWindowSize(ViewportWindow.WindowSize);
            if (ImGui.Begin("Cues", ImGuiWindowFlags.NoResize))
            {
                bool isContextMenuCueOpen = false;
                var e = ImGui.BeginListBox("##cuelist", new Vector2(-1, -1));
               
                if (e)
                {
                    if (renderer.config.workFile != null)
                    {
                        for (int i = 0; i < renderer.config.workFile.CueNodes.Count; i++)
                        {
                            CueNode item = renderer.config.workFile.CueNodes[i];
                            if (ImConverse.VisibilityNodeSimple($"{item.Name} (ID:{item.Id})", RightClickCue, in_Icon: NodeIconResource.Cue, in_ShowArrow: false))
                            {
                                EditorWindow.Instance.SelectCue(i);
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
