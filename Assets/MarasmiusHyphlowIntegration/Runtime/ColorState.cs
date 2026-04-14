using System;
using UnityEngine;

namespace AtMycelia.Amanita.SaveSys
{
    [Serializable]
    public struct ColorState : IEquatable<ColorState>, IEquatable<Color>, IEquatable<Color32>
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static implicit operator Color(ColorState other)
        {
            return other.ToColor();
        }

        public static implicit operator ColorState(Color col)
        {
            return From(col);
        }

        public static implicit operator Color32(ColorState other)
        {
            return other.ToColor();
        }

        public static ColorState From(Color color)
        {
            return new ColorState(color);
        }

        public ColorState(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public readonly Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        public readonly bool Equals(ColorState other)
        {
            return r == other.r && g == other.g && b == other.b && a == other.a;
        }

        public readonly bool Equals(Color other)
        {
            return r == other.r && g == other.g && b == other.b && a == other.a;
        }

        public readonly bool Equals(Color32 other)
        {
            return Mathf.Approximately(r, other.r / 255f) &&
                   Mathf.Approximately(g, other.g / 255f) &&
                   Mathf.Approximately(b, other.b / 255f) &&
                   Mathf.Approximately(a, other.a / 255f);
        }
    }
}