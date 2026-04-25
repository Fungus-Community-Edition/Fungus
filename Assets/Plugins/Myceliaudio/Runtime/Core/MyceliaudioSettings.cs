using UnityEngine;

namespace AtMycelia.Myceliaudio
{
    [System.Serializable]
    public class MyceliaudioSettings
    {
        [SerializeField]
        protected VolumeSettings _volume = new VolumeSettings(50, 100, 100, 100);

        public virtual VolumeSettings Volume { get { return _volume; } }
    }
}