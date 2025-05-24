using Celesta.Settings;
using HekonrayBase;
using HekonrayBase.Base;
using Hexa.NET.ImGui;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using TeamSpettro.SettingsSystem;

namespace Celesta
{
    public class SettingsWindow : Singleton<SettingsWindow>, IWindow
    {
        public static bool Enabled = false;
        public bool themeIsDark = SettingsManager.GetBool("IsDarkThemeEnabled");

        int nAudioWavePlayer = -1;
        public static int bufferSize = 4096;

        public NAudioWavePlayer GetAudioPlayerType()
        {
            if (nAudioWavePlayer == -1)
                nAudioWavePlayer = SettingsManager.GetInt("AudioPlayer");
            return (NAudioWavePlayer)nAudioWavePlayer;
        }

        public void OnReset(IProgramProject in_Renderer)
        {
        }

        public static string AddQuotesIfRequired(string in_Path)
        {
            return !string.IsNullOrWhiteSpace(in_Path) ?
                in_Path.Contains(" ") && (!in_Path.StartsWith("\"") && !in_Path.EndsWith("\"")) ?
                    "\"" + in_Path + "\"" : in_Path :
                    string.Empty;
        }
        public static void ExecuteAsAdmin(string in_FileName)
        {
            //Reason for this try-catch statement is because
            //if the user cancels the UAC prompt,
            //an exception will be thrown
            try
            {
                in_FileName = AddQuotesIfRequired(in_FileName);
                Process proc = new Process();
                proc.StartInfo.FileName = in_FileName;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
                proc.Start();
            }
            catch (Exception e)
            {

            }
        }
        public void Render(IProgramProject in_Renderer)
        {
            if (Enabled)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("Settings", ref Enabled))
                {
                    int currentTheme = themeIsDark ? 1 : 0;
                    int currentPlayer = (int)nAudioWavePlayer;
                    if (ImGui.Combo("Theme", ref currentTheme, ["Light", "Dark"], 2))
                    {
                        themeIsDark = currentTheme == 1;
                        SettingsManager.SetBool("IsDarkThemeEnabled", themeIsDark);
                        ImGuiThemeManager.SetTheme(themeIsDark);
                    }
                    if (ImGui.Combo("Audio Player", ref currentPlayer, ["WaveOut", "WasapiOut", "DirectSoundOut", "AsioOut"], 2))
                    {
                        nAudioWavePlayer = currentPlayer;
                        SettingsManager.SetInt("AudioPlayer", currentPlayer);
                    }

                    if (ImGui.Button("Associate extensions"))
                    {
                        ExecuteAsAdmin(@Path.Combine(@Program.Path, "FileTypeRegisterService.exe"));
                    }
                    ImGui.End();
                }
            }
        }
    }
}