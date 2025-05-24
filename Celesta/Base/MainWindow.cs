using Hexa.NET.ImGui;
using System.IO;
using System;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using Celesta.Settings;
using TeamSpettro.SettingsSystem;
using HekonrayBase;
using System.Runtime.InteropServices;
using System.Numerics;
using Hexa.NET.ImGui.Utilities;
using IconFonts;
using Celesta;

namespace Celesta
{
    public class MainWindow : HekonrayMainWindow
    {
        private IntPtr m_IniName;
        public string appName = "Celesta";
        public static float ScreenDpi;
        public CelestaProject CelestaProject => (CelestaProject)Project;
        public static ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse;

        public MainWindow(Version in_OpenGlVersion, Vector2Int in_WindowSize) : base(in_OpenGlVersion, in_WindowSize)
        {
            Title = appName;
        }
        public override void SetupFonts(ImGuiFontBuilder in_Builder)
        {
            ScreenDpi = GetDpiScaling();
            unsafe
            {
            in_Builder
                .SetOption(config => { config.FontBuilderFlags |= (uint)ImGuiFreeTypeBuilderFlags.LoadColor; })
                .AddFontFromFileTTF(Path.Combine(Application.ResourcesDirectory, "RobotoVariable.ttf"), 16 * ScreenDpi)
                .AddFontFromFileTTF(Path.Combine(Application.ResourcesDirectory, "NotoSansJP-Regular.ttf"), 18 * ScreenDpi, ImGui.GetIO().Fonts.GetGlyphRangesJapanese())
                .AddFontFromFileTTF(Path.Combine(Application.ResourcesDirectory, FontAwesome6.FontIconFileNameFAS), 16 * ScreenDpi, [0x1, 0x1FFFF])
                .Build();
            }
        }
        public override void OnLoad()
        {
            OnActionWithArgs = LoadFromArgs;
            TeamSpettro.Resources.Initialize(Path.Combine(Program.Path, "config.json"));
            CelestaProject.Instance.Setup(this);
            Project = CelestaProject.Instance;
            base.OnLoad();

            ImGuiThemeManager.SetTheme(SettingsManager.GetBool("IsDarkThemeEnabled", false));
            // Example #10000 for why ImGui.NET is kinda bad
            // This is to avoid having imgui.ini files in every folder that the program accesses
            unsafe
            {
                m_IniName = Marshal.StringToHGlobalAnsi(Path.Combine(Program.Path, "imgui.ini"));
                ImGuiIOPtr io = ImGui.GetIO();
                io.IniFilename = (byte*)m_IniName;
            }
            //    converseProject.windowList.Add(MenuBarWindow.Instance);
            //    converseProject.windowList.Add(FcoViewerWindow.Instance);
            //    converseProject.windowList.Add(SettingsWindow.Instance);
            Windows.Add(ModalHandler.Instance);
            Windows.Add(MenuBarWindow.Instance);
            Windows.Add(ViewportWindow.Instance);
            Windows.Add(CueSpaceWindow.Instance);
            Windows.Add(SynthSpaceWindow.Instance);
            Windows.Add(AisacSpaceWindow.Instance);
            Windows.Add(EditorWindow.Instance);
            Windows.Add(SettingsWindow.Instance);
            Windows.Add(SoundElementWindow.Instance);
            SettingsWindow.Instance.OnReset(null);
        }

        private void LoadFromArgs(string[] in_Args)
        {
        }
        public override void OnRenderImGuiFrame()
        {
            if (ShouldRender())
            {
                base.OnRenderImGuiFrame();

                //float deltaTime = (float)(GetDeltaTime());
                //co.Render(KunaiProject.WorkProjectCsd, (float)deltaTime);

                
            }
        }
    }
}
