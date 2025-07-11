


using UnityEngine;
using Amanita.DentedPixel;

namespace Amanita.VScripting
{
    /// <summary>
    /// Impulse style shake of an object's rotation, using LeanTween internally.
    /// </summary>
    [CommandInfo("LeanTween",
                 "Shake Rotation",
                 "")]
    [AddComponentMenu("Impulse style shake of an object's rotation, using LeanTween internally.")]
    public class ShakeRotationLean : ShakeLean
    {
        public override LTDescr ExecuteTween()
        {
            if (isLocal)
                return LeanTweenHelpers.ShakeEulerLocal(
                    _targetObject.Value.transform,
                    _axisScale.Value,
                    _axisSpeedRange.Value,
                    _duration.Value);
            else
                return LeanTweenHelpers.ShakeEuler(
                    _targetObject.Value.transform,
                    _axisScale.Value,
                    _axisSpeedRange.Value,
                    _duration.Value);
        }
    }
}
