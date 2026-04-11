using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class HandlesLineDrawer : ILineDrawer
    {
        public Color Color
        {
            get => Handles.color;
            set => Handles.color = value;
        }

        public void DrawLine(Vector2 start, Vector2 end)
        {
            Handles.DrawLine(start, end);
        }
    }
}