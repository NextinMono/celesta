using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

//using Celesta.Audio;
using Celesta.Project;
using Celesta.BuilderNodes;
using Celesta.Serialization;

using SonicAudioLib.IO;
using SonicAudioLib.CriMw.Serialization;
using SonicAudioLib.Archives;
using Celesta.CsbTypes;
using System.Threading.Tasks;
using SonicAudioLib.CriMw;

namespace Celesta.Importer
{
    public static class CsbFileManager
    {
        private static CriAudioFile FindAudioFromSoundelement(CsbProject proj, string name)
        {
            foreach (var audio in proj.AudioFiles)
            {
                if (audio.Name == name)
                    return audio;
            }
            return null;
        }
        static SynthNode GetSynthByName(CsbProject in_Proj, string in_Name)
        {
            foreach (var item in in_Proj.SynthNodes)
            {
                if (item.Path == in_Name)
                    return item;
            }
            return null;
        }
        public static void Export(CsbProject project, string outputFileName)
        {
            CriCpkArchive cpkArchive = new CriCpkArchive();

            DirectoryInfo outputDirectory = new DirectoryInfo(Path.GetDirectoryName(outputFileName));

            List<SerializationCueSheetTable> cueSheetTables = new List<SerializationCueSheetTable>();

            SerializationVersionInfoTable versionInfoTable = new SerializationVersionInfoTable();
            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(new List<SerializationVersionInfoTable>() { versionInfoTable }, CriTableWriterSettings.AdxSettings),
                Name = "INFO",
                TableType = 7,
            });

            // Serialize cues.
            List<SerializationCueTable> cueTables = new List<SerializationCueTable>();
            foreach (CueNode cueNode in project.CueNodes)
            {
                cueTables.Add(new SerializationCueTable
                {
                    Name = cueNode.Name,
                    Id = cueNode.Id,
                    UserData = cueNode.UserComment,
                    Flags = cueNode.Flags,
                    SynthPath = cueNode.SynthReference,
                });
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(cueTables, CriTableWriterSettings.AdxSettings),
                Name = "CUE",
                TableType = 1,
            });

            // Serialize synth tables.
            List<SerializationSynthTable> synthTables = new List<SerializationSynthTable>();
            foreach (SynthNode synthNode in project.SynthNodes)
            {
                SerializationSynthTable synthTable = new SerializationSynthTable
                {
                    SynthName = synthNode.Path,
                    SynthType = (byte)synthNode.Type,
                    ComplexType = (byte)synthNode.PlaybackType,
                    Volume = synthNode.Volume,
                    Pitch = synthNode.Pitch,
                    DelayTime = synthNode.DelayTime,
                    SControl = synthNode.SControl,
                    EgDelay = synthNode.EnvelopeGenerator.Delay,
                    EgAttack = synthNode.EnvelopeGenerator.Delay,
                    EgHold = synthNode.EnvelopeGenerator.Delay,
                    EgDecay = synthNode.EnvelopeGenerator.Delay,
                    EgRelease = synthNode.EnvelopeGenerator.Delay,
                    EgSustain = synthNode.EnvelopeGenerator.Delay,
                    FType = synthNode.FilterType,
                    FCof1 = synthNode.FilterCutoff1,
                    FCof2 = synthNode.FilterCutoff2,
                    FReso = synthNode.FilterReso,
                    FReleaseOffset = synthNode.FilterReleaseOffset,
                    DryOName = synthNode.DryOName,
                    Mtxrtr = synthNode.Mtxrtr,
                    Dry0 = synthNode.DrySendLevels.left.level,
                    Dry1 = synthNode.DrySendLevels.right.level,
                    Dry2 = synthNode.DrySendLevels.rearLeft.level,
                    Dry3 = synthNode.DrySendLevels.rearRight.level,
                    Dry4 = synthNode.DrySendLevels.center.level,
                    Dry5 = synthNode.DrySendLevels.lfe.level,
                    Dry6 = synthNode.DrySendLevels.extra1.level,
                    Dry7 = synthNode.DrySendLevels.extra2.level,
                    WetOName = synthNode.WetOName,
                    Wet0 = synthNode.WetLevels.left.level,
                    Wet1 = synthNode.WetLevels.right.level,
                    Wet2 = synthNode.WetLevels.rearLeft.level,
                    Wet3 = synthNode.WetLevels.rearRight.level,
                    Wet4 = synthNode.WetLevels.center.level,
                    Wet5 = synthNode.WetLevels.lfe.level,
                    Wet6 = synthNode.WetLevels.extra1.level,
                    Wet7 = synthNode.WetLevels.extra2.level,
                    Wcnct0 = synthNode.Wcnct0,
                    Wcnct1 = synthNode.Wcnct1,
                    Wcnct2 = synthNode.Wcnct2,
                    Wcnct3 = synthNode.Wcnct3,
                    Wcnct4 = synthNode.Wcnct4,
                    Wcnct5 = synthNode.Wcnct5,
                    Wcnct6 = synthNode.Wcnct6,
                    Wcnct7 = synthNode.Wcnct7,
                    VoiceLimitGroupName = synthNode.VoiceLimitGroupReference,
                    VoiceLimitType = synthNode.VoiceLimitType,
                    VoiceLimitPriority = synthNode.VoiceLimitPriority,
                    VoiceLimitPhTime = synthNode.VoiceLimitProhibitionTime,
                    VoiceLimitPcdlt = synthNode.VoiceLimitPcdlt,
                    Pan3dVolumeOffset = synthNode.Pan3D.VolumeOffset,
                    Pan3dVolumeGain = synthNode.Pan3D.VolumeGain,
                    Pan3dAngleOffset = synthNode.Pan3D.AngleOffset,
                    Pan3dAngleGain = synthNode.Pan3D.AngleGain,
                    Pan3dDistanceOffset = synthNode.Pan3D.DistanceOffset,
                    Pan3dDistanceGain = synthNode.Pan3D.DistanceGain,
                    Dry0g = (byte)synthNode.DrySendLevels.left.gain,
                    Dry1g = (byte)synthNode.DrySendLevels.right.gain,
                    Dry2g = (byte)synthNode.DrySendLevels.rearLeft.gain,
                    Dry3g = (byte)synthNode.DrySendLevels.rearRight.gain,
                    Dry4g = (byte)synthNode.DrySendLevels.center.gain,
                    Dry5g = (byte)synthNode.DrySendLevels.lfe.gain,
                    Dry6g = (byte)synthNode.DrySendLevels.extra1.gain,
                    Dry7g = (byte)synthNode.DrySendLevels.extra2.gain,
                    Wet0g = (byte)synthNode.WetLevels.left.gain,
                    Wet1g = (byte)synthNode.WetLevels.right.gain,
                    Wet2g = (byte)synthNode.WetLevels.rearLeft.gain,
                    Wet3g = (byte)synthNode.WetLevels.rearRight.gain,
                    Wet4g = (byte)synthNode.WetLevels.center.gain,
                    Wet5g = (byte)synthNode.WetLevels.lfe.gain,
                    Wet6g = (byte)synthNode.WetLevels.extra1.gain,
                    Wet7g = (byte)synthNode.WetLevels.extra2.gain,
                    F1Type = synthNode.Filter1Type,
                    F1CofOffset = synthNode.Filter1CutoffOffset,
                    F1CofGain = synthNode.Filter1CutoffGain,
                    F1ResoOffset = synthNode.Filter1ResoOffset,
                    F1ResoGain = synthNode.Filter1ResoGain,
                    F2Type = synthNode.Filter2Type,
                    F2CofLowOffset = synthNode.Filter2CutoffLowerOffset,
                    F2CofLowGain = synthNode.Filter2CutoffLowerGain,
                    F2CofHighOffset = synthNode.Filter2CutoffHigherOffset,
                    F2CofHighGain = synthNode.Filter2CutoffHigherGain,
                    Probability = synthNode.PlaybackProbability,
                    NumberLmtChildren = synthNode.NLmtChildren,
                    Repeat = synthNode.Repeat,
                    ComboTime = synthNode.ComboTime,
                    ComboLoopBack = synthNode.ComboLoopBack,
                };

                if (synthNode.Type == BuilderSynthType.Single)
                {
                    synthTable.LinkName = synthNode.SoundElementReference;
                }

                else if (synthNode.Type == BuilderSynthType.WithChildren)
                {
                    foreach (string trackReference in synthNode.Children)
                    {
                        synthTable.LinkName += trackReference + (char)0x0A;
                    }
                }

                if (!string.IsNullOrEmpty(synthNode.AisacReference))
                {
                    AisacNode aisacNode = project.AisacNodes.First(aisac => aisac.Path == synthNode.AisacReference);
                    synthTable.AisacSetName = aisacNode.AisacName + "::" + aisacNode.Path + (char)0x0A;
                }

                synthTables.Add(synthTable);
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(synthTables, CriTableWriterSettings.AdxSettings),
                Name = "SYNTH",
                TableType = 2,
            });

            List<FileInfo> junks = new List<FileInfo>();

            // Serialize the sound elements.
            List<SerializationSoundElementTable> soundElementTables = new List<SerializationSoundElementTable>();
            foreach (SoundElement soundElementNode in project.SoundElementNodes)
            {
                CriAaxArchive aaxArchive = new CriAaxArchive();

                if (!string.IsNullOrEmpty(soundElementNode.Intro))
                {
                    aaxArchive.Add(new CriAaxEntry
                    {
                        Flag = CriAaxEntryFlag.Intro,
                        //FilePath = new FileInfo(Path.Combine(project.AudioDirectory.FullName, soundElementNode.Intro)),
                        FileData = FindAudioFromSoundelement(project, soundElementNode.Intro).EncodeAudioDataToAdxStream()
                    });

                    aaxArchive.SetModeFromExtension(soundElementNode.Intro);
                }

                if (!string.IsNullOrEmpty(soundElementNode.Loop))
                {
                    aaxArchive.Add(new CriAaxEntry
                    {
                        Flag = CriAaxEntryFlag.Loop,
                        //FilePath = new FileInfo(Path.Combine(project.AudioDirectory.FullName, soundElementNode.Loop)),
                        FileData = FindAudioFromSoundelement(project, soundElementNode.Loop).EncodeAudioDataToAdxStream()
                    });

                    aaxArchive.SetModeFromExtension(soundElementNode.Loop);
                }

                byte[] data = new byte[0];

                if (soundElementNode.Streaming)
                {
                    CriCpkEntry entry = new CriCpkEntry();
                    entry.Name = Path.GetFileName(soundElementNode.Path);
                    entry.DirectoryName = Path.GetDirectoryName(soundElementNode.Path);
                    entry.Id = (uint)cpkArchive.Count;
                    entry.UpdateDateTime = DateTime.Now;
                    entry.FileData = FindAudioFromSoundelement(project, soundElementNode.Path).EncodeAudioDataToAdxStream();
                    entry.FilePath = new FileInfo(Path.GetTempFileName());
                    cpkArchive.Add(entry);

                    aaxArchive.Save(entry.FilePath.FullName);
                    junks.Add(entry.FilePath);
                }
                else
                {
                    data = aaxArchive.Save();
                }

                soundElementTables.Add(new SerializationSoundElementTable
                {
                    Name = soundElementNode.Path,
                    Data = data,
                    FormatType = (byte)aaxArchive.Mode,
                    SoundFrequency = soundElementNode.SampleRate,
                    NumberChannels = soundElementNode.ChannelCount,
                    Streaming = soundElementNode.Streaming,
                    NumberSamples = soundElementNode.SampleCount,
                });
                aaxArchive = null;
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(soundElementTables, CriTableWriterSettings.AdxSettings),
                Name = "SOUND_ELEMENT",
                TableType = 4,
            });

            // Serialize the aisacs.
            List<SerializationAisacTable> aisacTables = new List<SerializationAisacTable>();
            foreach (AisacNode aisacNode in project.AisacNodes)
            {
                List<SerializationAisacGraphTable> graphTables = new List<SerializationAisacGraphTable>();
                foreach (BuilderAisacGraphNode graphNode in aisacNode.Graphs)
                {
                    List<SerializationAisacPointTable> pointTables = new List<SerializationAisacPointTable>();
                    foreach (BuilderAisacPointNode pointNode in graphNode.Points)
                    {
                        pointTables.Add(new SerializationAisacPointTable
                        {
                            In = pointNode.X,
                            Out = pointNode.Y,
                        });
                    }

                    graphTables.Add(new SerializationAisacGraphTable
                    {
                        Points = CriTableSerializer.Serialize(pointTables, CriTableWriterSettings.AdxSettings),
                        Type = graphNode.Type,
                        InMax = graphNode.MaximumX,
                        InMin = graphNode.MinimumX,
                        OutMax = graphNode.MaximumY,
                        OutMin = graphNode.MinimumY,
                    });
                }

                aisacTables.Add(new SerializationAisacTable
                {
                    Graph = CriTableSerializer.Serialize(graphTables, CriTableWriterSettings.AdxSettings),
                    Name = aisacNode.AisacName,
                    PathName = aisacNode.Path,
                    Type = aisacNode.Type,
                    RandomRange = aisacNode.RandomRange,
                });
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(aisacTables, CriTableWriterSettings.AdxSettings),
                Name = "ISAAC",
                TableType = 5,
            });

            // Serialize the voice limit groups.
            List<SerializationVoiceLimitGroupTable> voiceLimitGroupTables = new List<SerializationVoiceLimitGroupTable>();
            foreach (BuilderVoiceLimitGroupNode voiceLimitGroupNode in project.VoiceLimitGroupNodes)
            {
                voiceLimitGroupTables.Add(new SerializationVoiceLimitGroupTable
                {
                    VoiceLimitGroupName = voiceLimitGroupNode.Path,
                    VoiceLimitGroupNum = voiceLimitGroupNode.MaxAmountOfInstances,
                });
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(voiceLimitGroupTables, CriTableWriterSettings.AdxSettings),
                Name = "VOICE_LIMIT_GROUP",
                TableType = 6,
            });

            // Finally, serialize the CSB file.
            CriTableSerializer.Serialize(outputFileName, cueSheetTables, CriTableWriterSettings.AdxSettings, SettingsWindow.bufferSize);

            if (cpkArchive.Count > 0)
            {
                cpkArchive.Save(Path.ChangeExtension(outputFileName, "cpk"), SettingsWindow.bufferSize);
            }

            foreach (FileInfo junk in junks)
            {
                junk.Delete();
            }
        }

        // Structure of CSB file nodes:
        // SynthNode list (e.g Synth/cue/subsynth, Synth/cue)
        //   -synth nodes also contain aisac node names, and one aisac node might control
        //    multiple synths (e.g boost aisac node controlling bgm and the boost bgm
        // CueNode list (e.g sonic_normal, v_homing, etc.)
        // Aisac Nodes (e.g boost)
        public static CsbProject Import(string path)
        {
            CsbProject project = new CsbProject();
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
            List<SerializationCueSheetTable> cueSheets = null;
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
                soundElementNode.Path = soundElementTable.Name;
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
                    Path = voiceLimitGroupTable.VoiceLimitGroupName,
                    MaxAmountOfInstances = voiceLimitGroupTable.VoiceLimitGroupNum,
                });
            }

            // Deserialize Aisac tables
            foreach (SerializationAisacTable aisacTable in aisacTables)
            {
                project.AisacNodes.Add(new AisacNode(aisacTable));
            }

            foreach (var aisacNodes in project.AisacNodes)
            {
                var e = project.UniqueAisacNodes.FirstOrDefault(x => x.AisacName == aisacNodes.AisacName);
                if (e != null)
                {
                    e.AisacNodes.Add(aisacNodes);
                }
                else
                {
                    project.UniqueAisacNodes.Add(new EasyAisacNode());
                    project.UniqueAisacNodes[^1].AisacName = aisacNodes.AisacName;
                    project.UniqueAisacNodes[^1].AisacNodes.Add(aisacNodes);
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
                    foreach (var child in synthNode.Children)
                    {
                        var childSynth = GetSynthByName(project, child);
                        childSynth.Parent = synthNode.Path;
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
                project.AudioFiles = extractor.Run();
                project.AudioExtractorTime.Stop();
            });
            return project;
        }
    }
}
