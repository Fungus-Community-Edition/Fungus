using UnityEngine;
using Amanita.VScripting;

namespace Amanita.EditorUtils
{
    public interface IHitTester
    {
        Block TopmostBlockOverlapping(Vector2 mousePosition);
    }
}