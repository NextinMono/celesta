using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

//using Celesta.Audio;
using Celesta.Project;
using Celesta.BuilderNodes;
using Celesta.Serialization;

using SonicAudioLib.IO;
using SonicAudioLib.CriMw.Serialization;
using SonicAudioLib.Archives;

using System.Windows.Forms;
using Celesta.CsbTypes;
using System.Threading.Tasks;

namespace Celesta.Importer
{
    public static class CsbImporter
    {
        static SynthNode GetSynthByName(CsbProject in_Proj, string in_Name)
        {
            foreach (var item in in_Proj.SynthNodes)
            {
                if (item.Name == in_Name)
                    return item;
            }
            return null;
        }

        // Structure of CSB file nodes:
        // SynthNode list (e.g Synth/cue/subsynth, Synth/cue)
        //   -synth nodes also contain aisac node names, and one aisac node might control
        //    multiple synths (e.g boost aisac node controlling bgm and the boost bgm
        // CueNode list (e.g sonic_normal, v_homing, etc.)
        // Aisac Nodes (e.g boost)
        public static CsbProject Import(string path)
        {
            CsbProject project = new CsbProject(path);
            var extractor = new AudioFileProcessor();
            var buffSizeSettings = 4096;
            var enableThreadingSettings = true;
            var maxThreadSettings = 4;
            extractor.BufferSize = buffSizeSettings;
            extractor.EnableThreading = enableThreadingSettings;
            extractor.MaxThreads = maxThreadSettings;

            // Find the CPK first
            string cpkPath = Path.ChangeExtension(path, "cpk");
            bool exists = File.Exists(cpkPath);

            CriCpkArchive cpkArchive = new CriCpkArchive();

            // First, deserialize the main tables
            List<SerializationCueSheetTable> cueSheets  = null;
            cueSheets = CriTableSerializer.Deserialize<SerializationCueSheetTable>(path, buffSizeSettings);

            /* Deserialize all the tables we need to import.
             * None = 0,
             * Cue = 1,
             * Synth = 2,
             * SoundElement = 4,
             * Aisac = 5,
             * VoiceLimitGroup = 6,
             * VersionInfo = 7,
             */

            List<SerializationCueTable> cueTables = CriTableSerializer.Deserialize<SerializationCueTable>(cueSheets.FirstOrDefault(table => table.TableType == 1).TableData);
            List<SerializationSynthTable> synthTables = CriTableSerializer.Deserialize<SerializationSynthTable>(cueSheets.FirstOrDefault(table => table.TableType == 2).TableData);
            List<SerializationSoundElementTable> soundElementTables = CriTableSerializer.Deserialize<SerializationSoundElementTable>(cueSheets.FirstOrDefault(table => table.TableType == 4).TableData);
            List<SerializationAisacTable> aisacTables = CriTableSerializer.Deserialize<SerializationAisacTable>(cueSheets.FirstOrDefault(table => table.TableType == 5).TableData);
            
            // voice limit groups appeared in the later versions, so check if it exists.
            List<SerializationVoiceLimitGroupTable> voiceLimitGroupTables = new List<SerializationVoiceLimitGroupTable>();

            if (cueSheets.Exists(table => table.TableType == 6))
            {
                voiceLimitGroupTables = CriTableSerializer.Deserialize<SerializationVoiceLimitGroupTable>(cueSheets.FirstOrDefault(table => table.TableType == 6).TableData);
            }

            // Deserialize Sound Element tables

            // BUT BEFORE THAT, see if there's any sound element with Streamed on
            if (soundElementTables.Exists(soundElementTable => soundElementTable.Streaming))
            {
                if (!exists)
                {
                    throw new Exception("Cannot find CPK file for this CSB file. Please ensure that the CPK file is in the directory where the CSB file is, and has the same name as the CSB file, but with .CPK extension.");
                }

                cpkArchive.Load(cpkPath);
            }

            foreach (SerializationSoundElementTable soundElementTable in soundElementTables)
            {
                SoundElement soundElementNode = new SoundElement();
                soundElementNode.Name = soundElementTable.Name;
                soundElementNode.ChannelCount = soundElementTable.NumberChannels;
                soundElementNode.SampleRate = soundElementTable.SoundFrequency;
                soundElementNode.Streaming = soundElementTable.Streaming;
                soundElementNode.SampleCount = soundElementTable.NumberSamples;

                CriAaxArchive aaxArchive = new CriAaxArchive();

                CriCpkEntry cpkEntry = cpkArchive.GetByPath(soundElementTable.Name);
                if (soundElementNode.Streaming && cpkEntry != null)
                {
                    using (Stream source = File.OpenRead(cpkPath))
                    using (Stream entrySource = cpkEntry.Open(source))
                    {
                        aaxArchive.Read(entrySource);
                    }
                }

                else if (soundElementNode.Streaming && cpkEntry == null)
                {
                    soundElementNode.Intro = soundElementNode.Loop = string.Empty;
                    soundElementNode.SampleRate = soundElementNode.SampleCount = soundElementNode.ChannelCount = 0;
                }

                else
                {
                    aaxArchive.Load(soundElementTable.Data);
                }

                foreach (CriAaxEntry entry in aaxArchive)
                {
                    string outputFileName = soundElementTable.Name.Replace('/', '_');
                    if (entry.Flag == CriAaxEntryFlag.Intro)
                    {
                        outputFileName += $"_Intro{aaxArchive.GetModeExtension()}";
                        soundElementNode.Intro = Path.GetFileName(outputFileName);
                    }

                    else if (entry.Flag == CriAaxEntryFlag.Loop)
                    {
                        outputFileName += $"_Loop{aaxArchive.GetModeExtension()}";
                        soundElementNode.Loop = Path.GetFileName(outputFileName);
                    }

                    if (soundElementNode.Streaming)
                    {
                        extractor.Add(cpkPath, outputFileName, cpkEntry.Position + entry.Position, entry.Length);
                    }

                    else
                    {
                        extractor.Add(soundElementTable.Data, outputFileName, entry.Position, entry.Length);
                    }
                }

                project.SoundElementNodes.Add(soundElementNode);
            }

            // Deserialize Voice Limit Group tables
            foreach (SerializationVoiceLimitGroupTable voiceLimitGroupTable in voiceLimitGroupTables)
            {
                project.VoiceLimitGroupNodes.Add(new BuilderVoiceLimitGroupNode
                {
                    Name = voiceLimitGroupTable.VoiceLimitGroupName,
                    MaxAmountOfInstances = voiceLimitGroupTable.VoiceLimitGroupNum,
                });
            }

            // Deserialize Aisac tables
            foreach (SerializationAisacTable aisacTable in aisacTables)
            {                
                project.AisacNodes.Add(new BuilderAisacNode(aisacTable));
            }

            foreach(var aisacNodes in project.AisacNodes)
            {
                var e = project.UniqueAisacNodes.FirstOrDefault(x => x.aisacName == aisacNodes.AisacName);
                if(e != null)
                {
                    e.builderAisacNodes.Add(aisacNodes);
                }
                else
                {
                    project.UniqueAisacNodes.Add(new EasyAisacNode());
                    project.UniqueAisacNodes[^1].aisacName = aisacNodes.AisacName;
                    project.UniqueAisacNodes[^1].builderAisacNodes.Add(aisacNodes);
                }
            }

            // Deserialize Synth tables
            foreach (SerializationSynthTable synthTable in synthTables)
            {                
                project.SynthNodes.Add(new SynthNode(synthTable));
            }

            // Convert the cue tables
            foreach (SerializationCueTable cueTable in cueTables)
            {
                project.CueNodes.Add(new CueNode(cueTable));
            }

            // Fix links
            for (int i = 0; i < synthTables.Count; i++)
            {
                SerializationSynthTable synthTable = synthTables[i];
                SynthNode synthNode = project.SynthNodes[i];

                if (synthNode.Type == BuilderSynthType.Single)
                {
                    synthNode.SoundElementReference = synthTable.LinkName;
                }

                // Polyphonic
                else if (synthNode.Type == BuilderSynthType.WithChildren)
                {
                    synthNode.Children = synthTable.LinkName.Split(new char[] { (char)0x0A }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach(var child in synthNode.Children)
                    {
                        var childSynth = GetSynthByName(project, child);
                        childSynth.Parent = synthNode.Name;
                    }

                }

                if (!string.IsNullOrEmpty(synthTable.AisacSetName))
                {
                    string[] aisacs = synthTable.AisacSetName.Split(new char[] { (char)0x0A }, StringSplitOptions.RemoveEmptyEntries);
                    string[] name = aisacs[0].Split(new string[] { "::" }, StringSplitOptions.None);
                    synthNode.AisacReference = name[1]; // will add support for multiple aisacs (I'm actually not even sure if csbs support multiple aisacs...)
                }

                if (!string.IsNullOrEmpty(synthTable.VoiceLimitGroupName))
                {
                    synthNode.VoiceLimitGroupReference = synthTable.VoiceLimitGroupName;
                }
            }
                        
            // Extract everything
            project.AudioExtractor = Task.Factory.StartNew(() => 
            {
                project.AudioExtractorTime.Start();
                project.audioFiles = extractor.Run();
                project.AudioExtractorTime.Stop();
            });
            return project;
        }
    }
}
