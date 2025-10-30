using Amanita.Myceliaudio;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class MyceliaudioSaveData : SaveData
    {
        [SerializeField] protected PlayAudioArgs playAudioArgs = PlayAudioArgs.Null;
        [SerializeField] protected VolumeSettings volumeSettings = new VolumeSettings();
        [SerializeField] protected IDictionary<int, int> bgmIndexes = new Dictionary<int, int>();
        // ^The keys are the BGM track numbers, the values are the asset indexes in ShadowDatabase

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

        /// <summary>
        /// The keys are the BGM track numbers in AudioSystem, the values are the
        /// AudioClip asset indexes in ShadowDatabase.
        /// </summary>
        public virtual IDictionary<int, int> BgmIndexes
        {
            get { return new Dictionary<int, int>(bgmIndexes); }
            set
            {
                bgmIndexes.Clear();
                foreach (var kvp in value)
                {
                    bgmIndexes[kvp.Key] = kvp.Value;
                }
            }
        }

        public virtual void AddBgmIndex(int trackNumber, int assetIndex)
        {
            bgmIndexes[trackNumber] = assetIndex;
        }

        public virtual void RemoveBgmIndex(int trackNumber)
        {
            if (bgmIndexes.ContainsKey(trackNumber))
            {
                bgmIndexes.Remove(trackNumber);
            }
        }

        public virtual int GetBgmIndex(int trackNumber)
        {
            if (bgmIndexes.ContainsKey(trackNumber))
            {
                return bgmIndexes[trackNumber];
            }
            else
            {
                return -1;
            }
        }

        public MyceliaudioSaveData()
        {
            
        }
    }
}