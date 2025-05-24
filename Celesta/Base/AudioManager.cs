using Celesta;
using Celesta.Audio;
using Celesta.BuilderNodes;
using Celesta.Project;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celesta
{
    public static class AudioManager
    {
        private static IWavePlayer sound;
        public static Dictionary<SynthNode, SoundPlayer> sounds = new Dictionary<SynthNode, SoundPlayer>();
        static CsbProject csbProject => CelestaProject.Instance.config.workFile;
        private static MixingSampleProvider providerMixer;
        public static bool IsPaused = false;
        public static bool IsPlaying
        {
            get
            {
                return sounds.Count > 0;
            }
        }
        public static void Update()
        {
            foreach (var soundP in sounds)
            {
                soundP.Value.PanAngle = soundP.Key.Pan3D.AngleOffset;
                soundP.Value.ReverbWetMix = GetAbsoluteWetMix(soundP.Key);
            }
            ApplyAisacNodes();
        }
        private static float GetSingle(BuilderAisacGraphNode in_List, float in_Frame)
        {
            if (in_List.Points.Count == 0)
                return -1;

            if (in_Frame >= in_List.MaximumX)
                return in_List.Points[^1].Y;
            float frameX = (int)(in_Frame * 10000);
            int index = in_List.FindKeyframe(frameX);

            if (index == 0)
                return in_List.Points[index].Y;

            var keyframe = in_List.Points[index - 1];
            if(index >= in_List.Points.Count)
            {
                Console.WriteLine($"Keyframe index went over keyframe list. Remember to change this stupid method. {in_List.Points.Count}, {index}");
                index = Math.Clamp(index, 0, in_List.Points.Count - 1);
            }
            var nextKeyframe = in_List.Points[index];

            float factor;

            if (nextKeyframe.X - keyframe.X > 0)
                factor = (frameX - keyframe.X) / (nextKeyframe.X - keyframe.X);
            else
                factor = 0.0f;


            return (1.0f - factor) * keyframe.Y + nextKeyframe.Y * factor;
        }
        private static void ApplyAisacNodes()
        {
            if (csbProject == null)
                return;
            var uniqueNodes = csbProject.UniqueAisacNodes;
            if (sounds.Count != 0)
            {
                foreach (var sound in sounds)
                {
                    if (sound.Value.Flags.HasFlag(SoundPlayer.EOptions.eOptions_IsSynthPlay))
                        continue;

                    var e = csbProject.AisacNodes.FirstOrDefault(x => x.Name == sound.Key.AisacReference);
                    if(e != null)
                    {
                        foreach (var graph in e.Graphs)
                        {
                            if (graph.Type == 0)
                            {                                
                                //Apply aisac node value only if the sound is playing and only if all the sounds
                                //that are necessary for the aisac node are playing
                                 sound.Value.SetVolumeCsb((GetAbsoluteVolume(sound.Key) * 1000) * GetSingle(graph, e.Progress) / 10000);
                            }
                        }
                    }
                }
                //
                //
                //foreach (var uniqueNode in csbProject.UniqueAisacNodes)
                //{
                //    foreach (var aisacNode in uniqueNode.builderAisacNodes)
                //    {
                //        foreach (var graph in aisacNode.Graphs)
                //        {
                //            if (graph.Type == 0)
                //            {
                //                SynthNode snyth = null;
                //                //Find synths based on referenec to aisac node
                //                for (int i = 0; i < csbProject.SynthNodes.Count; i++)
                //                {
                //                    SynthNode synth = csbProject.SynthNodes[i];
                //                    if (synth.AisacReference == aisacNode.Name)
                //                    {
                //                        snyth = synth;
                //                        break;
                //                    }
                //                }
                //                sounds.TryGetValue(snyth, out SoundPlayer soundPlayer);
                //                //Apply aisac node value only if the sound is playing and only if all the sounds
                //                //that are necessary for the aisac node are playing
                //                if (soundPlayer != null && !soundPlayer.Flags.HasFlag(SoundPlayer.EOptions.eOptions_IsSynthPlay))
                //                    soundPlayer.SetVolumeCsb((GetAbsoluteVolume(snyth) * 1000) * GetSingle(graph, uniqueNode.progress) / 10000);
                //            }
                //        }
                //    }
                //}
            }
        }

        private static SynthNode GetSoundToPlay(ref SoundPlayer.EOptions in_Options)
        {
            if (EditorWindow.Instance.GetCurrentSynth() != null)
            {
                in_Options &= SoundPlayer.EOptions.eOptions_IsSynthPlay;
                return EditorWindow.Instance.GetCurrentSynth();
            }
            else
                if (EditorWindow.Instance.GetCurrentCueNode() != null)
                return EditorWindow.Instance.GetCurrentCueNode().GetSynth();
            return null;
        }
        public static void PlaySound()
        {
            IsPaused = false;
            StopSound();
            SoundPlayer.EOptions soundPlayerOpt = SoundPlayer.EOptions.eOptions_None;
            SynthNode synthToPlay = GetSoundToPlay(ref soundPlayerOpt);
            if (synthToPlay == null)
                return;
            AddSynthSound(synthToPlay, soundPlayerOpt);

            switch (SettingsWindow.Instance.GetAudioPlayerType())
            {
                case NAudioWavePlayer.WasapiOut:
                    sound = new WasapiOut();
                    break;

                case NAudioWavePlayer.WaveOut:
                case NAudioWavePlayer.DirectSoundOut:
                    sound = new DirectSoundOut();
                    break;

                case NAudioWavePlayer.AsioOut:
                    sound = new AsioOut();
                    break;
            }
            sound.Init(new WaveformCaptureProvider(providerMixer));
            sound.Play();

            //if (cueTree.Focused && cueTree.SelectedNode != null && cueTree.SelectedNode.Tag is BuilderCueNode cueNode)
            //{
            //TreeNode synthNode = synthTree.FindNodeByFullPath(cueNode.SynthReference);
            //AddSynthSound(synthNode);


            //}

            //else if (soundElementTree.Focused && soundElementTree.SelectedNode != null && soundElementTree.SelectedNode.Tag is BuilderSoundElementNode)
            //{
            //    AddSoundElementSound(soundElementTree.SelectedNode, 1, 0, 0);
            //}
            //
            //else if (synthTree.Focused && synthTree.SelectedNode != null && synthTree.SelectedNode.Tag is BuilderSynthNode)
            //{
            //    AddSynthSound(synthTree.SelectedNode);
            //}
        }
        public static void PauseResumeSounds(bool in_Resume = false)
        {
            foreach (var sond in sounds)
            {
                if (!in_Resume)
                    sond.Value.Pause();
                else
                    sond.Value.Play();
            }
        }
        public static void PauseToggleSounds()
        {
            IsPaused = !IsPaused;
            foreach (var sond in sounds)
            {
                if (sond.Value.IsPaused())
                    sond.Value.Play();
                else
                    sond.Value.Pause();
            }
        }
        public static void StopSound()
        {
            if (sound != null)
            {
                sound.Stop();
                sound.Dispose();
            }
            providerMixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
            sounds.Clear();
            //foreach(var sound in sounds)
            //{
            //    sound.Value.Kill();
            //}
            //sounds.Clear();
        }
        static ISampleProvider AdjustInput(ISampleProvider provider)
        {
            if (provider.WaveFormat.Channels == 1)
                provider = new MonoToStereoSampleProvider(provider);

            if (provider.WaveFormat.SampleRate != 44100)
                provider = new WdlResamplingSampleProvider(provider, 44100);
            providerMixer.AddMixerInput(provider);
            return provider;
        }
        private static float GetAbsoluteVolume(SynthNode synthTreeNode)
        {
            float volume = 1000.0f;

            while (synthTreeNode != null)
            {
                volume = (float)((volume * synthTreeNode.Volume) / 1000.0);
                synthTreeNode = CsbCommon.GetSynthByName(synthTreeNode.Parent);
            }

            return volume / 1000.0f;
        }
        private static float GetAbsoluteWetMix(SynthNode synthTreeNode)
        {
            float volume = 0;

            while (synthTreeNode != null)
            {
                volume = (volume += synthTreeNode.WetLevels.center.level);
                synthTreeNode = CsbCommon.GetSynthByName(synthTreeNode.Parent);
            }

            return volume / 255.0f;
        }
        private static double GetAbsolutePitch(SynthNode synthTreeNode)
        {
            double pitch = 0;

            while (synthTreeNode != null)
            {
                pitch += synthTreeNode.Pitch;
                synthTreeNode = CsbCommon.GetSynthByName(synthTreeNode.Parent);
            }
            return pitch / 1500.0;
        }
        private static int GetAbsoluteDelayTime(SynthNode synthTreeNode)
        {
            int delayTime = 0;
            while (synthTreeNode != null)
            {
                delayTime += (int)synthTreeNode.DelayTime;
                synthTreeNode = CsbCommon.GetSynthByName(synthTreeNode.Parent);
            }
            return delayTime;
        }
        static SoundElement GetSoundElementNode(string in_Name)
        {
            foreach (var se in csbProject.SoundElementNodes)
            {
                if (se.Name == in_Name)
                    return se;
            }
            return null;
        }

        private static void AddSynthSound(SynthNode synthNode, SoundPlayer.EOptions in_Options)
        {
            if (synthNode.Type == BuilderSynthType.WithChildren)
            {
                switch (synthNode.PlaybackType)
                {
                    case BuilderSynthPlaybackType.RandomNoRepeat:
                    case BuilderSynthPlaybackType.Random:
                        {
                            SynthNode childSynthNode = CsbCommon.GetSynthByName(synthNode.Children[synthNode.RandomChildNode]);
                            PlaySynthSound(childSynthNode, in_Options);
                            break;
                        }
                    case BuilderSynthPlaybackType.Sequential:
                        {
                            SynthNode childSynthNode = CsbCommon.GetSynthByName(synthNode.Children[synthNode.NextChildNode]);
                            PlaySynthSound(childSynthNode, in_Options);
                            break;
                        }
                    default:
                        {
                            foreach (var childNode in synthNode.Children)
                            {
                                SynthNode childSynthNode = CsbCommon.GetSynthByName(childNode);
                                PlaySynthSound(childSynthNode, in_Options);
                            }
                            break;
                        }
                }
            }
            else if (synthNode.Type == BuilderSynthType.Single)
            {
                if (!string.IsNullOrEmpty(synthNode.SoundElementReference) && synthNode.PlayThisTurn)
                {
                    AddSoundElementSound(synthNode, GetSoundElementNode(synthNode.SoundElementReference), GetAbsoluteVolume(synthNode), GetAbsolutePitch(synthNode), GetAbsoluteDelayTime(synthNode), in_Options);
                }
            }
        }

        private static void PlaySynthSound(SynthNode childSynthNode, SoundPlayer.EOptions in_Options)
        {
            if (childSynthNode.Type == BuilderSynthType.Single && !string.IsNullOrEmpty(childSynthNode.SoundElementReference) && childSynthNode.PlayThisTurn)
            {
                AddSoundElementSound(childSynthNode, GetSoundElementNode(childSynthNode.SoundElementReference), GetAbsoluteVolume(childSynthNode), GetAbsolutePitch(childSynthNode), GetAbsoluteDelayTime(childSynthNode), in_Options);
            }
            else if (childSynthNode.Type == BuilderSynthType.WithChildren)
            {
                AddSynthSound(childSynthNode, in_Options);
            }
        }
        private static void AddSoundElementSound(SynthNode synth, SoundElement soundElementNode, double volume, double pitch, int delayTime, SoundPlayer.EOptions in_Options)
        {
            WaveStream waveStream = null;

            volume = Math.Clamp(volume, 0, 1);
            bool isIntroPresent = !string.IsNullOrEmpty(soundElementNode.Intro);
            bool isLoopPresent = !string.IsNullOrEmpty(soundElementNode.Loop);
            if (isIntroPresent && !isLoopPresent)
            {
                var reader = new LoopingWaveStream(csbProject.GetAudioFile(soundElementNode.Intro));
                reader.DisableLoop();

                waveStream = new ExtendedWaveStream(reader)
                {
                    DelayTime = delayTime,
                    Volume = volume,
                    Pitch = pitch,
                };
            }
            else if (!isIntroPresent && isLoopPresent)
            {
                var reader = new LoopingWaveStream(csbProject.GetAudioFile(soundElementNode.Loop));
                reader.EnableLoop();

                waveStream = new ExtendedWaveStream(reader)
                {
                    DelayTime = delayTime,
                    Volume = volume,
                    Pitch = pitch,
                    ForceLoop = true,
                };
            }
            else if (isIntroPresent && isLoopPresent)
            {
                var intro = new LoopingWaveStream(csbProject.GetAudioFile(soundElementNode.Intro));
                intro.DisableLoop();

                var loop = new LoopingWaveStream(csbProject.GetAudioFile(soundElementNode.Loop));
                loop.EnableLoop();

                waveStream = new ExtendedWaveStream(intro, loop)
                {
                    DelayTime = delayTime,
                    Volume = volume,
                    Pitch = pitch,
                    ForceLoop = true,
                };
            }

            if (waveStream != null)
            {
                SoundPlayer soundPlayer = new SoundPlayer(waveStream.ToSampleProvider(), (float)volume);
                soundPlayer.Flags = in_Options;
                providerMixer.AddMixerInput(soundPlayer.soundProvider);
                sounds.Add(synth, soundPlayer);
            }
        }
    }
}
