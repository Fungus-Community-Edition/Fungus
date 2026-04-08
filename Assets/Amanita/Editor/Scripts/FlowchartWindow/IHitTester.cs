using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.Amanita.EditorUtils
{
    public interface IHitTester
    {
        Block TopmostBlockOverlapping(Vector2 mousePosition);
    }
}