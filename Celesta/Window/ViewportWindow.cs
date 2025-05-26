using Hexa.NET.ImGui;
using System.Numerics;
using HekonrayBase.Base;
using HekonrayBase;
using Celesta;
using IconFonts;
using System.Linq;
namespace Celesta
{
    public class ViewportWindow : Singleton<ViewportWindow>, IWindow
    {
        public static Vector2 WindowSize;

        public void Render(IProgramProject in_Renderer)
        {
            var size = ImGui.GetWindowViewport().Size.X / 4.5f;
            float footSizeY = 37.5f * MainWindow.ScreenDpi;
            var renderer = (CelestaProject)in_Renderer;
            Vector2 viewportMin = new Vector2(0, MenuBarWindow.menuBarHeight);
            Vector2 wndSize = new Vector2(size, ((ImGui.GetWindowViewport().Size.Y - MenuBarWindow.menuBarHeight - footSizeY) / 2));
            WindowSize = wndSize;


            if (renderer.config.workFile != null)
            {
                //Show a little warning message when the audio is still extracting after 0.25s
                if (!renderer.config.workFile.AudioExtractor.IsCompleted && renderer.config.workFile.AudioExtractorTime.ElapsedMilliseconds > 250)
                {
                    string msg = "Loading audio into memory, please wait...";
                    ImConverse.CenterWindow(new Vector2(wndSize.X, ImGui.GetFontSize() * 4.5f));
                    ImGui.OpenPopup("Loading Audio");
                    bool open = true;
                    if (ImGui.BeginPopupModal("Loading Audio", ref open, ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize))
                    {
                        ImConverse.Notice(msg, ImConverseNotice.Info);
                        ImGui.EndPopup();
                    }
                }
            }

            Vector2 footSize = new Vector2(ImGui.GetWindowViewport().Size.X, footSizeY);
            ImGui.SetNextWindowPos(new Vector2(0, renderer.screenSize.Y - footSize.Y));
            ImGui.SetNextWindowSize(footSize);

            if (ImGui.Begin("Footer", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                //Size of biggest button + some extra space (so it looks nicer)
                var btnSize = ImGui.CalcTextSize($"{FontAwesome6.CirclePlay} Resume --");
                btnSize.X *= 1.18f;
                btnSize.Y = footSizeY * 0.62f;

                //Play button
                if (ImGui.Button($"{FontAwesome6.Play} Play", btnSize))
                    AudioManager.PlaySound();
                ImGui.SameLine();
                //Pause/Resume
                if (ImGui.Button(!AudioManager.IsPaused ? $"{FontAwesome6.Pause} Pause" : $"{FontAwesome6.CirclePlay} Resume", btnSize))
                    AudioManager.PauseToggleSounds();
                ImGui.SameLine();
                //Stop button
                if (ImGui.Button($"{FontAwesome6.Stop} Stop", btnSize))
                    AudioManager.StopSound();

                ImGui.SameLine();
                ImConverse.VerticalSeparator(footSizeY);
                ImGui.SameLine();

                //Waveform of all current played audio
                var waveform = WaveformCaptureProvider.LatestWaveform;
                //Scale waveform info to visual range (0.8f because some sounds are quiet)
                float[] display = waveform.Select(s => s * 0.8f).ToArray();

                ImGui.PushStyleColor(ImGuiCol.PlotLines, ImGui.ColorConvertFloat4ToU32(ColorResource.Celeste));
                ImGui.PlotLines("##Waveform", ref display[0], display.Length, 0, "", -0.5f, 0.5f, new System.Numerics.Vector2(100, -1));
                ImGui.PopStyleColor();
                ImGui.SameLine();

                //Active voices
                ImGui.Text($"Sounds Playing: {AudioManager.sounds.Count}");
                ImGui.SameLine();
                ImGui.Dummy(new Vector2(5, 0));
                ImGui.SameLine();
                ImConverse.VerticalSeparator();
                foreach (var sound in AudioManager.sounds)
                {
                    ImGui.SameLine();
                    ImGui.Text($"[{sound.Key.Path}: {(int)(sound.Value.Volume * 100)}%%]");
                }
            }
            ImGui.End();
            AudioManager.Update();
        }
        public void OnReset(IProgramProject in_Renderer)
        {
        }
    }
}