using Amanita.FSExt;
using Amanita.Myceliaudio;
using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "MyceliaudioSaveCodec",
        menuName = "Amanita/SaveSys/Codecs/MyceliaudioSaveCodec")]
    public class MyceliaudioSaveCodec : SaveCodec<AudioSystem, MyceliaudioSaveData>, IMainSaveCodec
    {
        //[SerializeField] protected int[] bgmChannels = new int[] { 0, 1, 2, 3 };
        // TODO: Support multiple BGM channels

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
            AudioClip mainBGM = audioSys.GetBaseMainClip(TrackGroup.BGMusic, 0);
            PlayAudioArgs playAudioArgs = PlayAudioArgs.Null;

            if (currentlyPlaying)
            {
                SavePlayAudioArgs();
                void SavePlayAudioArgs()
                {
                    playAudioArgs = new PlayAudioArgs()
                    {
                        MainClip = mainBGM,
                        TrackGroup = TrackGroup.BGMusic,
                        Track = 0,
                        Loop = audioSys.IsLoopingMain(TrackGroup.BGMusic, 0),
                        LoopStartPoint = audioSys.GetLoopStartPoint(TrackGroup.BGMusic, 0),
                        LoopEndPoint = audioSys.GetLoopEndPoint(TrackGroup.BGMusic, 0),
                        OneShot = false
                    };

                    // TODO: Account for when the main and intro clips were split off an asset
                    AudioClip[] allAudioClips = Resources.FindObjectsOfTypeAll<AudioClip>();
                    AudioClip clipPlaying = audioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);
                    bool clipIsProjectAsset = allAudioClips.Contains(clipPlaying);

                    if (clipIsProjectAsset)
                    {
                        playAudioArgs.MainClip = clipPlaying;
                    }
                }

            }

            MyceliaudioSaveData saveData = new MyceliaudioSaveData()
            {
                PlayAudioArgs = playAudioArgs,
                VolumeSettings = volumeSettings
            };

            return saveData;
        }

        public IList<SaveData> FindAndCreateAll(Action<IList<SaveData>> onComplete = null)
        {
            IList<SaveData> result = new SaveData[] { EncodeToSave(AudioSystem.S) };
            onComplete?.Invoke(result);
            return result;
        }
    }
}