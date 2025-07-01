using UnityEngine;

namespace Amanita.Myceliaudio
{
    public interface IPlayAudioContext
    {
        AudioClip IntroClip { get; set; }
        AudioClip MainClip { get; set; }
        TrackGroup TrackGroup { get; }
        int Track { get; }
        bool Loop { get; }
        double LoopStartPoint { get; }
        double LoopEndPoint { get; }
        bool HasEndPointBeforeEndOfClip { get; }
        bool OneShot { get; }

    }
}