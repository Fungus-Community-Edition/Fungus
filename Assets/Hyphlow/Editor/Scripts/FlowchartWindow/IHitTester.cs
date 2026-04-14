using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public interface IHitTester
    {
        Block TopmostBlockOverlapping(Vector2 mousePosition);
    }
}