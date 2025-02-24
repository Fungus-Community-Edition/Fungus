namespace Myceliaudio
{
    public static class AudioMath
    {
        /// <summary>
        /// Converting between 0-1 scales and 0-100 scales.
        /// </summary>
        public static float VolumeConversion { get; private set; } = 100f;

        public static float MinVol { get; private set; } = 0f;

        public static float MaxVol { get; set; } = 100f;

        /// <summary>
        /// Normalized for Audio Sources
        /// </summary>
        public static float MaxVolNormalized { get {  return MaxVol / VolumeConversion; } }

        public static float MinPitch { get; private set; } = 0f;

        public static float MaxPitch { get; private set; } = 200f;

        public static float MaxPitchNormalized { get { return MaxPitch / VolumeConversion; } }
    }
}