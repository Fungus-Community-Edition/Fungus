using UnityEngine;
using Amanita.DentedPixel;

namespace Amanita.VScripting
{
    /// <summary>
    /// Impulse style shake of an object's position, using LeanTween internally.
    /// </summary>
    [CommandInfo("LeanTween",
                 "Shake Position",
                 "Impulse style shake of an object's position, using LeanTween internally.")]
    [AddComponentMenu("")]
    public class ShakePositionLean : ShakeLean
    {
        public override LTDescr ExecuteTween()
        {
            if (isLocal)
                return LeanTweenHelpers.ShakePositionLocal(
                    _targetObject.Value.transform,
                    _axisScale.Value,
                    _axisSpeedRange.Value,
                    _duration.Value);
            else
                return LeanTweenHelpers.ShakePosition(
                    _targetObject.Value.transform,
                    _axisScale.Value,
                    _axisSpeedRange.Value,
                    _duration.Value);
        }
    }
}
