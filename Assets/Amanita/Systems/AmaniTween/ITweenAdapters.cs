using UnityEngine;
using UnityEngine.UI;

namespace Amanita.Tweening
{
    public interface ITransformTweenAdapter
    {
        ITweenHandle MoveTo(Transform target, Vector3 position, float duration);
        ITweenHandle ScaleTo(Transform target, Vector3 scale, float duration);
        ITweenHandle RotateTo(Transform target, Quaternion rotation, float duration);
    }

    public interface IGraphicTweenAdapter
    {
        ITweenHandle ShiftColorTo(Graphic target, Color endVal, float duration);
        ITweenHandle FadeTo(Graphic target, float endVal, float duration);
        ITweenHandle ShiftColorTo(SpriteRenderer target, Color endVal, float duration);
        ITweenHandle FadeTo(SpriteRenderer target, float endVal, float duration);
    }

    public interface IAudioSourceTweenAdapter
    {
        /// <summary>
        /// On a scale of 0 for silent to 1 for max.
        ITweenHandle ShiftVolumeTo(AudioSource target, float targVal, float duration);

        /// <summary>
        /// On a scale of 0 for silent to 100 for max.
        /// </summary>
        ITweenHandle ShiftVolume02To(AudioSource target, float targVal, float duration);

        /// <summary>
        /// On a scale of -3 for min to 3 for max. Note that the default pitch here is 1.
        /// </summary>
        ITweenHandle ShiftPitchTo(AudioSource target, float targVal, float duration);

        /// <summary>
        /// On a scale of 0-300 for min to 300 for max. Note that the default pitch here is 100.
        /// </summary>
        ITweenHandle ShiftPitch02To(AudioSource target, float targVal, float duration);
    }

}