using Amanita.FSExt;
using Amanita.Myceliaudio;
using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Lorekeeper;
using Amanita.Utils;

namespace Amanita.SaveSys
{
    [SaveSysDisplayName("Myceliaudio Codec (Amanita Default)")]
    public class MyceliaudioSaveCodec : SaveCodec<AudioSystem, MyceliaudioSaveData>, IMainSaveCodec
    {
        // TODO: Support multiple BGM channels
        //[SerializeField] protected int[] bgmChannels = new int[] { 0, 1, 2, 3 };

        public virtual void PreInstallInit()
        {
            allAudioClips = ShadowDB.GetAssetsOfType<AudioClip>(AssetType.AudioClip);
            // ^So we don't have to fetch them every time we want to encode.
        }

        protected IList<AudioClip> allAudioClips;

        public override bool CanHandle(string typeName)
        {
            return typeName == typeof(AudioSystem).FullName || typeName == typeof(MyceliaudioSaveData).Name;
        }

        public override MyceliaudioSaveData Decode(string rawText)
        {
            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                MyceliaudioSaveData result = serializer.FromJson<MyceliaudioSaveData>(rawText);
                return result;
            }
        }

        public override MyceliaudioSaveData EncodeToSave(AudioSystem from)
        {
            AudioSystem audioSys = AudioSystem.S;
            var volumeSettings = audioSys.GetVolumeSettings();

            // For now, we only support saving BGMusic Track 0
            bool currentlyPlaying = audioSys.GetIsPlaying(TrackGroup.BGMusic, 0);
            AudioClip mainBgm = audioSys.GetBaseMainClip(TrackGroup.BGMusic, 0);
            PlayAudioArgs playAudioArgs = PlayAudioArgs.Null;
            int assetIndex = -1;

            if (currentlyPlaying)
            {
                SavePlayAudioArgs();
                void SavePlayAudioArgs()
                {
                    playAudioArgs = new PlayAudioArgs()
                    {
                        MainClip = mainBgm,
                        TrackGroup = TrackGroup.BGMusic,
                        Track = 0,
                        Loop = audioSys.IsLoopingMain(TrackGroup.BGMusic, 0),
                        LoopStartPoint = audioSys.GetLoopStartPoint(TrackGroup.BGMusic, 0),
                        LoopEndPoint = audioSys.GetLoopEndPoint(TrackGroup.BGMusic, 0),
                        OneShot = false
                    };

                    assetIndex = allAudioClips.IndexOf(mainBgm);
                    bool clipIsProjectAsset = assetIndex >= 0;
                    // ^Since for all we know, the clip playing could've been split from
                    // one of the assets on disk, in which case we can't save that reference.
                    if (clipIsProjectAsset)
                    {
                        playAudioArgs.MainClip = mainBgm;
                    }
                    else
                    {
                        // In this case, the clip was likely split off from an asset. Let's find the original.
                        AudioClip originalClip = FindOriginalAsset();
                        AudioClip FindOriginalAsset()
                        {
                            AudioClip clip = null;
                            string baseName = mainBgm.name;
                            string suffixToRemove = AudioClipSplitter.LoopClipNameSuffix;
                            // ^Since we only care about the loop part of the clip, and thus we'll
                            // remove that suffix to find the original asset name.
                            string realAssetName = baseName.Substring(0, baseName.Length - suffixToRemove.Length);
                            clip = allAudioClips.FirstOrDefault(
                                ac => ac.name == realAssetName);
                            return clip;
                        }
                        
                        if (originalClip != null)
                        {
                            assetIndex = allAudioClips.IndexOf(originalClip);
                            playAudioArgs.MainClip = originalClip;
                        }
                        else
                        {
                            Debug.LogWarning($"Could not find the original asset for the clip playing: {mainBgm.name}. " +
                                $"It may have been dynamically generated and cannot be saved.");
                            assetIndex = -1;
                        }

                    }
                
                }

            }

            MyceliaudioSaveData saveData = new MyceliaudioSaveData()
            {
                PlayAudioArgs = playAudioArgs,
                VolumeSettings = volumeSettings
            };
            saveData.AddBgmIndex(0, assetIndex);

            return saveData;
        }

        protected ShadowDatabase ShadowDB => AmanitaManager.ShadowDB;

        public IList<SaveData> FindAndCreateAll(Action<IList<SaveData>> onComplete = null)
        {
            IList<SaveData> result = null;
            UnityThreadUtil.RunOnMainThread(() =>
            {
                IList<SaveData> result = new SaveData[] { EncodeToSave(AudioSystem.S) };
                onComplete?.Invoke(result);
            });
            
            return result;
        }
    }
}