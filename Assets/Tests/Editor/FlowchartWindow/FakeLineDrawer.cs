using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class FakeLineDrawer : ILineDrawer
    {
        public Color Color { get; set; }
        public List<(Vector2, Vector2)> LinesDrawn = new();

        public void DrawLine(Vector2 a, Vector2 b)
        {
            LinesDrawn.Add((a, b));
        }
    }
}