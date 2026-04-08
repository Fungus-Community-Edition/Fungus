using UnityEngine;
using AtMycelia.Amanita.VScripting;

namespace AtMycelia.Amanita.EditorUtils
{
    public interface IHitTester
    {
        Block TopmostBlockOverlapping(Vector2 mousePosition);
    }
}