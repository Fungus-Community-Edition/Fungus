using UnityEngine;

namespace AtMycelia.Myceliaudio.Utils
{
    [System.Obsolete("Better to use QuickPlayAudio. Does the same thing as this Component and more.")]
    public class PlayMusicOnStart : MonoBehaviour
    {
        [SerializeField] protected AudioClip _clip;
        [Tooltip("In milliseconds")]
        [SerializeField] protected double _loopStartPoint = 0f, _loopEndPoint = 0f;

        protected virtual void Awake()
        {
            if (_clip == null)
            {
                Debug.LogWarning($"{this.gameObject}'s PlayMusicOnStart has no clip to play.");
                return;
            }

            PlayAudioArgs playMusic = new PlayAudioArgs()
            {
                MainClip = _clip,
                TrackGroup = TrackGroup.BGMusic,
                Loop = true,
                LoopStartPoint = _loopStartPoint,
                LoopEndPoint = _loopEndPoint
            };

            AudioSystem.S.Play(playMusic);
        }
    }
}