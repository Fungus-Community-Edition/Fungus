using UnityEngine;

namespace AtMycelia.AmaniTween
{
    public interface IPositionShaker
    {
        ITweenHandle ShakePosition(Transform target, Vector3 axis, Vector3 force, float duration, bool isLocalSpace = true);
    }
}
