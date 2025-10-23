using Amanita.Myceliaudio;
using System.Linq;
using UnityEngine;
using Amanita.FSExt;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class MyceliaudioSaveData : SaveData
    {
        [SerializeField] protected PlayAudioArgs playAudioArgs = PlayAudioArgs.Null;
        [SerializeField] protected VolumeSettings volumeSettings = new VolumeSettings();
        [SerializeField] protected string pathToMainBGM = string.Empty;
        // ^Whatever's playing in BGMusic Track 0, if anything.

        public virtual PlayAudioArgs PlayAudioArgs
        {
            get { return playAudioArgs; }
            set { playAudioArgs = value; }
        }

        public virtual VolumeSettings VolumeSettings
        {
            get { return volumeSettings; }
            set { volumeSettings = value; }
        }

        public MyceliaudioSaveData()
        {
            // We automatically check the current state of Myceliaudio and then register it
            AudioSystem audioSys = AudioSystem.S;
            volumeSettings = audioSys.GetVolumeSettings();
            bool currentlyPlaying = audioSys.GetIsPlaying(TrackGroup.BGMusic, 0);
            AudioClip mainBGM = audioSys.GetBaseMainClip(TrackGroup.BGMusic, 0);

            if (currentlyPlaying)
            {
                SavePlayAudioArgs();
                void SavePlayAudioArgs()
                {
                    PlayAudioArgs = new PlayAudioArgs()
                    {
                        MainClip = mainBGM,
                        TrackGroup = TrackGroup.BGMusic,
                        Track = 0,
                        Loop = audioSys.IsLoopingMain(TrackGroup.BGMusic, 0),
                        LoopStartPoint = audioSys.GetLoopStartPoint(TrackGroup.BGMusic, 0),
                        LoopEndPoint = audioSys.GetLoopEndPoint(TrackGroup.BGMusic, 0),
                        OneShot = false
                    };

                    AudioClip[] allAudioClips = Resources.FindObjectsOfTypeAll<AudioClip>();
                    AudioClip clipPlaying = audioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);
                    bool clipIsProjectAsset = allAudioClips.Contains(clipPlaying);

                    if (clipIsProjectAsset)
                    {
                        playAudioArgs.MainClip = clipPlaying;
                    }
                }

            }

        }

        public override SaveDataUnit Serialized()
        {
            string json = Serializer.ToJson(this, true);
            SaveDataUnit serializedSaveData = new SaveDataUnit(TypeName, json);
            return serializedSaveData;
        }
    }
}