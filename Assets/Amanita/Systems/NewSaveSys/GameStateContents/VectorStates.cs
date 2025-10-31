using System;
using UnityEngine;

namespace Amanita.SaveSys
{
    [Serializable]
    public struct Vector2State : IEquatable<Vector2State>, IEquatable<Vector2>, IEquatable<Vector3>
    {
        public float x, y;

        public static Vector2State From(Vector2 vec)
        {
            return new Vector2State { x = vec.x, y = vec.y };
        }

        public readonly Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        public static implicit operator Vector2(Vector2State other)
        {
            return other.ToVector2();
        }

        public static implicit operator Vector2State(Vector2 vec)
        {
            return From(vec);
        }

        public static implicit operator Vector3(Vector2State other)
        {
            return new Vector3(other.x, other.y, 0);
        }

        public readonly bool Equals(Vector2State other)
        {
            return x == other.x &&
                y == other.y;
        }

        public readonly bool Equals(Vector2 other)
        {
            return x == other.x &&
                y == other.y;
        }

        public readonly bool Equals(Vector3 other)
        {
            return x == other.x &&
                y == other.y;
        }
    }

    [Serializable]
    public struct Vector3State : IEquatable<Vector3State>, IEquatable<Vector3>, IEquatable<Vector2>
    {
        public float x, y, z;

        public static Vector3State From(Vector3 vec)
        {
            return new Vector3State { x = vec.x, y = vec.y, z = vec.z };
        }

        public readonly Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static implicit operator Vector3(Vector3State other)
        {
            return other.ToVector3();
        }

        public static implicit operator Vector3State(Vector3 vec)
        {
            return From(vec);
        }

        public static implicit operator Vector2(Vector3State other)
        {
            return new Vector2(other.x, other.y);
        }

        public readonly bool Equals(Vector3State other)
        {
            return x == other.x &&
                y == other.y &&
                z == other.z;
        }

        public readonly bool Equals(Vector3 other)
        {
            return x == other.x &&
                y == other.y &&
                z == other.z;
        }

        public readonly bool Equals(Vector2 other)
        {
            return x == other.x &&
                y == other.y;
        }
    }

}