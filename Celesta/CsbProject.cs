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
        //Used for the UI to show the extraction process
        public Task AudioExtractor { get; internal set; }
        public Stopwatch AudioExtractorTime = new Stopwatch();

        public List<CriAudioFile> AudioFiles;
        public List<EasyAisacNode> UniqueAisacNodes = new List<EasyAisacNode>();
        public List<CueNode> CueNodes = new List<CueNode>();
        public List<SynthNode> SynthNodes = new List<SynthNode>();
        public List<SoundElement> SoundElementNodes = new List<SoundElement>();
        public List<AisacNode> AisacNodes = new List<AisacNode>();
        public List<BuilderVoiceLimitGroupNode> VoiceLimitGroupNodes = new List<BuilderVoiceLimitGroupNode>();

        public EasyAisacNode GetAisacNodesByCommonName(string name)
        {
            foreach (var node in UniqueAisacNodes)
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
            return csbProject;
        }
        public CriAudioFile GetAudioFile(string name)
        {
            foreach (var audiofile in AudioFiles)
            {
                if (audiofile.Name == name)
                    return audiofile;
            }
            return null;
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

            foreach (var cue in CueNodes)
            {
                if (cue.SynthReference == oldPath)
                    cue.SynthReference = currentSynth.Path;
            }
            foreach (var synth in SynthNodes)
            {
                if (synth.Path.Contains(oldPath))
                    synth.Path.Replace(oldPath, currentSynth.Path);
            }
        }
        internal string AddAudioFile(string @inputintro)
        {
            if (File.Exists(inputintro))
            {
                var sound = new CriAudioFile(Path.GetFileName(inputintro).Replace('.', '_'), File.OpenRead(inputintro));
                AudioFiles.Add(sound);
                return sound.Name;
            }
            return null;
        }
        internal float GetProgress(string aisacName)
        {
            foreach (var uNode in AisacNodes)
            {
                if (uNode.AisacName == aisacName) return uNode.Progress;
            }
            return 0;
        }
        public void Dispose()
        {
            for (int i = 0; i < AudioFiles.Count; i++)
            {
                AudioFiles[i].Dispose();
            }
            AudioFiles.Clear();
        }
    }
}