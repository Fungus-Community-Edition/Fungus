using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public interface ILineDrawer
    {
        Color Color { get; set; }
        void DrawLine(Vector2 start, Vector2 end);
    }
}