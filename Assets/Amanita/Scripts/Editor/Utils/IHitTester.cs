using UnityEngine;

namespace Amanita.EditorUtils
{
    public interface IHitTester
    {
        Block TopmostBlockOverlapping(Vector2 mousePosition);
    }
}