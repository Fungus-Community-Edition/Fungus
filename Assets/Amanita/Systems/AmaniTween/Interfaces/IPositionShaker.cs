using UnityEngine;

namespace Amanita.Tweening
{
    public interface IPositionShaker
    {
        ITweenHandle ShakePosition(Transform target, Vector3 axis, Vector3 force, float duration);
    }
}
