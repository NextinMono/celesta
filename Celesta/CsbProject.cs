using System.Collections.Generic;
using System.IO;
using Celesta.BuilderNodes;
using System.Xml.Serialization;
using Celesta.CsbTypes;
using SonicAudioLib.IO;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Celesta.Project
{
    public class EasyAisacNode : ICloneable
    {
        public string AisacName;
        public List<AisacNode> AisacNodes = new List<AisacNode>();
        public float Progress;

        public void SetProgress(float in_Prog)
        {
            foreach (var node in AisacNodes)
            {
                node.Progress = in_Prog;
            }
            Progress = in_Prog;
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
    public class CsbProject : IDisposable
    {
        private string name;
        private DirectoryInfo directory;

        public List<CriAudioFile> audioFiles;
        private List<EasyAisacNode> easyNodes = new List<EasyAisacNode>();
        public List<CueNode> CueNodes = new List<CueNode>();
        private List<SynthNode> synthNodes = new List<SynthNode>();
        private List<SoundElement> soundElementNodes = new List<SoundElement>();
        private List<AisacNode> aisacNodes = new List<AisacNode>();
        private List<BuilderVoiceLimitGroupNode> voiceLimitGroupNodes = new List<BuilderVoiceLimitGroupNode>();

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        [XmlIgnore]
        public DirectoryInfo Directory
        {
            get
            {
                return directory;
            }

            set
            {
                directory = value;
            }
        }

        [XmlIgnore]
        public DirectoryInfo AudioDirectory
        {
            get
            {
                return new DirectoryInfo(Path.Combine(directory.FullName, "Audio"));
            }
        }

        [XmlIgnore]
        public DirectoryInfo BinaryDirectory
        {
            get
            {
                return new DirectoryInfo(Path.Combine(directory.FullName, "Binary"));
            }
        }

        [XmlIgnore]
        public FileInfo ProjectFile
        {
            get
            {
                return new FileInfo(Path.Combine(directory.FullName, $"{name}.csbproject"));
            }
        }

        [XmlArray("SynthNodes"), XmlArrayItem(typeof(SynthNode))]
        public List<SynthNode> SynthNodes
        {
            get
            {
                return synthNodes;
            }
            set
            {
                synthNodes = value;
            }
        }

        [XmlArray("SoundElementNodes"), XmlArrayItem(typeof(SoundElement))]
        public List<SoundElement> SoundElementNodes
        {
            get
            {
                return soundElementNodes;
            }
        }

        [XmlArray("AisacNodes"), XmlArrayItem(typeof(AisacNode))]
        public List<AisacNode> AisacNodes
        {
            get
            {
                return aisacNodes;
            }
        }

        [XmlArray("VoiceLimitGroupNodes"), XmlArrayItem(typeof(BuilderVoiceLimitGroupNode))]
        public List<BuilderVoiceLimitGroupNode> VoiceLimitGroupNodes
        {
            get
            {
                return voiceLimitGroupNodes;
            }
        }

        public List<EasyAisacNode> UniqueAisacNodes
        {
            get
            {
                return easyNodes;
            }
        }

        public Task AudioExtractor { get; internal set; }
        public Stopwatch AudioExtractorTime = new Stopwatch();

        public EasyAisacNode GetAisacNodesByCommonName(string name)
        {
            foreach(var node in easyNodes)
            {
                if (node.AisacName == name)
                    return node;
            }
            return null;
        }

       

        public static CsbProject Load(string projectFile)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CsbProject));

            CsbProject csbProject = null;
            using (Stream source = File.OpenRead(projectFile))
            {
                csbProject = (CsbProject)serializer.Deserialize(source);
            }

            csbProject.Directory = new DirectoryInfo(Path.GetDirectoryName(projectFile));
            return csbProject;
        }

        public string AddAudio(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            string name = Path.GetFileName(path);
            string nameNoExtension = Path.GetFileNameWithoutExtension(name);
            string outputPath = Path.Combine(AudioDirectory.FullName, name);

            if (path != outputPath)
            {
                string uniqueName = nameNoExtension;

                int index = -1;
                while (File.Exists(Path.Combine(AudioDirectory.FullName, $"{uniqueName}.adx")))
                {
                    uniqueName = $"{nameNoExtension}_{++index}";
                }

                outputPath = Path.Combine(AudioDirectory.FullName, $"{uniqueName}.adx");
                File.Copy(path, outputPath, true);

                name = $"{uniqueName}.adx";
            }

            return name;
        }

        public CriAudioFile GetAudioFile(string name)
        {
            foreach(var audiofile in audioFiles)
            {
                if (audiofile.Name == name)
                    return audiofile;
            }
            return null;
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CsbProject));

            using (Stream destination = ProjectFile.Create())
            {
                serializer.Serialize(destination, this);
            }
        }

        public void SaveAs(string outputDirectory)
        {
            DirectoryInfo oldAudioDirectory = AudioDirectory;

            name = Path.GetFileNameWithoutExtension(outputDirectory);
            directory = new DirectoryInfo(Path.GetDirectoryName(outputDirectory));

            Create();
            Save();

            if (oldAudioDirectory.Exists)
            {
                foreach (FileInfo audioFile in oldAudioDirectory.EnumerateFiles())
                {
                    audioFile.CopyTo(Path.Combine(AudioDirectory.FullName, audioFile.Name), true);
                }
            }
        }

        public void Create()
        {
            directory.Create();
            AudioDirectory.Create();
        }
        public void Rename(SoundElement currentSynth, string name)
        {
            var oldPath = currentSynth.Path;
            currentSynth.SEName = name;

            foreach (var cue in SynthNodes)
            {
                if (cue.SoundElementReference == oldPath)
                    cue.SoundElementReference = currentSynth.Path;
            }
        }
        public void Rename(SynthNode currentSynth, string name)
        {
            var oldPath = currentSynth.Path;
            currentSynth.SynthName = name;

            foreach(var cue in CueNodes)
            {
                if (cue.SynthReference == oldPath)
                    cue.SynthReference = currentSynth.Path;
            }
            foreach(var synth in SynthNodes)
            {
                if (synth.Path.Contains(oldPath))
                    synth.Path.Replace(oldPath, currentSynth.Path);
            }
        }

        internal string AddAudioFile(string @inputintro)
        {
            if(File.Exists(inputintro))
            {
                var sound = new CriAudioFile(Path.GetFileName(inputintro).Replace('.', '_'), File.OpenRead(inputintro));
                audioFiles.Add(sound);
                return sound.Name;
            }
            return null;
        }

        internal float GetProgress(string aisacName)
        {
            foreach(var uNode in AisacNodes)
            {
                if (uNode.AisacName == aisacName) return uNode.Progress;
            }
            return 0;
        }

        public void Dispose()
        {
            for (int i = 0; i < audioFiles.Count; i++)
            {
                audioFiles[i].Dispose();
            }
            audioFiles.Clear();
        }

        public CsbProject(string in_Path) : base()
        {
            name = Path.GetFileNameWithoutExtension(in_Path);
            directory = System.IO.Directory.GetParent(in_Path);
            //name = MainForm.Settings.ProjectsName;
            //directory = new DirectoryInfo(Path.Combine(MainForm.Settings.ProjectsDirectory, name));
        }
        public CsbProject()
        {
            //name = MainForm.Settings.ProjectsName;
            //directory = new DirectoryInfo(Path.Combine(MainForm.Settings.ProjectsDirectory, name));
        }
    }
}
