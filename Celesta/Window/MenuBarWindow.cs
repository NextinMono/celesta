using Hexa.NET.ImGui;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using HekonrayBase.Base;
using HekonrayBase;
using Celesta.Importer;

namespace Celesta
{
    public class MenuBarWindow : Singleton<MenuBarWindow>, IWindow
    {
        public static float menuBarHeight = 32;
        private readonly string fco = "fco";
        private readonly string fte = "fte";
        //https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
        
        public void OnReset(IProgramProject in_Renderer)
        {

        }
        void SaveFileAction(CelestaProject in_Renderer)
        {
            CsbFileManager.Export(CelestaProject.Instance.config.workFile, CelestaProject.Instance.config.workFilePath);
        }
        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (CelestaProject)in_Renderer;
            if (ImGui.BeginMainMenuBar())
            {
                menuBarHeight = ImGui.GetWindowSize().Y;

                if (ImGui.BeginMenu($"File"))
                {
                    if (ImGui.MenuItem("Open"))
                    {
                        var testdial = NativeFileDialogSharp.Dialog.FileOpen("csb");
                        if (testdial.IsOk)
                        {
                            renderer.LoadFile(testdial.Path);
                        }
                    }
                    if (ImGui.MenuItem("Open folder"))
                    {
                        var testdial = NativeFileDialogSharp.Dialog.FolderPicker(fco);
                        if (testdial.IsOk)
                        {
                        }
                    }
                    
                    ImGui.Separator();
                    if (ImGui.MenuItem("Save", "Ctrl + S"))
                    {
                        SaveFileAction(renderer);
                    }
                    if (ImGui.MenuItem("Save As...", "Ctrl + Alt + S"))
                    {
                        SaveFileAsAction(renderer);
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Exit"))
                    {
                        Environment.Exit(0);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    
                    if (ImGui.MenuItem("Settings", SettingsWindow.Enabled))
                    {
                        SettingsWindow.Enabled = !SettingsWindow.Enabled;
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Help"))
                {
                    if (ImGui.MenuItem("How to use Converse"))
                    {
                        OpenUrl("https://wiki.hedgedocs.com/index.php/How_to_use_Converse");
                    }
                    if (ImGui.MenuItem("Report a bug"))
                    {
                        OpenUrl("https://github.com/NextinMono/converse/issues/new");
                    }

                    ImGui.EndMenu();
                }


            }
            if (UpdateChecker.UpdateAvailable)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0, 0.7f, 1, 1)));
                var size = ImGui.CalcTextSize("Update Available!").X;
                ImGui.SetCursorPosX(ImGui.GetWindowSize().X - size - ImGui.GetStyle().ItemSpacing.X * 2);
                if (ImGui.Selectable("Update Available!"))
                {
                    OpenUrl("https://github.com/NextinMono/converse/releases/latest");
                }
                ImGui.PopStyleColor();
            }
            ProcessShortcuts(renderer);
            ImGui.EndMainMenuBar();
        }

        private void SaveFileAsAction(CelestaProject in_Renderer)
        {
            var testdial = NativeFileDialogSharp.Dialog.FileSave(fco);
            if (testdial.IsOk)
            {
            };
        }

        private void ProcessShortcuts(CelestaProject in_Renderer)
        {
            if (ImGui.IsKeyDown(ImGuiKey.ModCtrl))
            {
                if (ImGui.IsKeyPressed(ImGuiKey.S))
                {
                    if (ImGui.IsKeyDown(ImGuiKey.ModAlt))
                    {
                        SaveFileAsAction(in_Renderer);
                    }
                    else
                    {
                        SaveFileAction(in_Renderer);
                    }
                }
            };
        }
    }
}
