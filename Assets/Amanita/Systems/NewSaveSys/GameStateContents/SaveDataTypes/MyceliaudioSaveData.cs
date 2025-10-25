using Amanita.Myceliaudio;
using System.Linq;
using UnityEngine;

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
            
        }
    }
}