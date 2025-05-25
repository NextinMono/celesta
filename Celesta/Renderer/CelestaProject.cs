using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using HekonrayBase;
using System.Numerics;
using HekonrayBase.Base;
using Celesta.Importer;
using Celesta.Project;
using Celesta.CsbTypes;
using Celesta.BuilderNodes;
using System.Linq;

namespace Celesta
{
    public class CelestaProject : Singleton<CelestaProject>, IProgramProject
    {
        public struct SViewportData
        {
            public int csdRenderTextureHandle;
            public Vector2Int framebufferSize;
            public int renderbufferHandle;
            public int framebufferHandle;
        }
        public struct SProjectConfig
        {
            public CsbProject workFile;
            public string workFilePath;
            public string tablePath;
            public bool playingAnimations;
            public bool showQuads;
            public double time;
            public SProjectConfig()
            {
            }
        }
        public SProjectConfig config;
        private SViewportData viewportData;
        public bool isFileLoaded = false;
        public MainWindow window;
        public Vector2 screenSize => new Vector2(window.WindowSize.X, window.WindowSize.Y);
        public CelestaProject() { }
        public void Setup(MainWindow in_Window)
        {
            window = in_Window;
            viewportData = new SViewportData();
            config = new SProjectConfig();
        }
        private void SendResetSignal()
        {
            window.ResetWindows(this);
        }
        public void Reset()
        {
            isFileLoaded = false;
        }
        public void ShowMessageBoxCross(string title, string message, int logType = 0)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Windows.MessageBoxImage image = System.Windows.MessageBoxImage.Information;
                switch(logType)
                {
                    case 0:
                        image = System.Windows.MessageBoxImage.Information;
                        break;
                    case 1:
                        image = System.Windows.MessageBoxImage.Warning;
                        break;
                    case 2:
                        image = System.Windows.MessageBoxImage.Error;
                        break;
                }
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, image);
            }
        }
       
        private void AfterLoadFile()
        {
            SendResetSignal();
            isFileLoaded = true;

            ////Gens FCO, load All table automatically since it only uses that
            //if (config.fcoFile[0].file.Header.Version != 0)
            //{
            //    string path = Path.Combine(Program.Path, "Resources", "Tables", "bb", "All.json");
            //    ImportTranslationTable(path);
            //}
            //if (GetFcoFiles().Count > 0)
            //{
            //    if (GetFcoFiles().Count > 1)
            //        window.Title = window.appName + $" - [{config.ftePath}]";
            //    else
            //        window.Title = window.appName + $" - [{config.fcoFile[0].path}]";
            //}
            //else
            //    window.Title = window.appName;
        }
        public int GetViewportImageHandle()
        {
            return viewportData.csdRenderTextureHandle;
        }

        internal void LoadFile(string path)
        {
            AudioManager.StopSound();
            config.workFile?.Dispose();
            config.workFile = null;
            config.workFile = CsbImporter.Import(path);
            config.workFilePath = path;
            AfterLoadFile();
        }

        internal CueNode AddCue()
        {
            var node = new CueNode();
            uint id = 0;
            
            foreach (var cues in config.workFile.CueNodes)
            {
                if (id < cues.Id)
                    id = cues.Id;
            }
            node.Id = id + 1;
            node.SynthReference = CelestaProject.Instance.config.workFile.SynthNodes[0].Name;
            node.Name += id;
            config.workFile.CueNodes.Add(node);
            return node;
        }

        internal SoundElement AddSoundElement()
        {
            var node = new SoundElement();
            config.workFile.Rename(node, $"NewSoundElement{config.workFile.SoundElementNodes.Count - 1}");
            config.workFile.SoundElementNodes.Add(node);
            return node;
        }

        internal SynthNode AddSynth()
        {
            var synth = new SynthNode();
            synth.Name = $"Synth/NewSynth{config.workFile.SynthNodes.Count - 1}";
            config.workFile.SynthNodes.Add(synth);
            return synth;
        }

        internal void AddSynthChild(SynthNode synth, SynthNode synthChild)
        {
            var split = synthChild.Name.Split('/').ToList();
            split.Insert(1, synth.SynthName);
            synthChild.Name = string.Join('/', split);
            synthChild.Parent = synth.Name;
            synth.Children.Add(synthChild.Name);
        }
    }
}