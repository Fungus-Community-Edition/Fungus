namespace AtMycelia.Myceliaudio
{
    [System.Serializable]
    public class VolumeSettings
    {
        public float master;
        public float bgMusic;
        public float soundFX;
        public float voice;

        public VolumeSettings(float master = 50, float bgMusic = 100, float soundFx = 100, float voice = 100)
        {
            this.master = master;
            this.bgMusic = bgMusic;
            this.soundFX = soundFx;
            this.voice = voice;
        }
    }
}