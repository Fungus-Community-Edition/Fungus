using UnityEngine;
using Amanita.DentedPixel;

namespace Amanita.VScripting
{
    /// <summary>
    /// Impulse style shake of an object's scale, using LeanTween internally.
    /// </summary>
    [CommandInfo("LeanTween",
                 "Shake Scale",
                 "Impulse style shake of an object's scale, using LeanTween internally.")]
    [AddComponentMenu("")]
    public class ShakeScaleLean : ShakeLean
    {
        public override LTDescr ExecuteTween()
        {
            return LeanTweenHelpers.ShakeScale(
                _targetObject.Value.transform,
                _axisScale.Value,
                _axisSpeedRange.Value,
                _duration.Value);
        }
    }
}
