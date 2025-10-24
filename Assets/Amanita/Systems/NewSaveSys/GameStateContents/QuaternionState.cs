using System;
using UnityEngine;

namespace Amanita.SaveSys
{
    [Serializable]
    public struct QuaternionState : IEquatable<QuaternionState>, IEquatable<Quaternion>
    {
        public float x, y, z, w;

        public static QuaternionState From(Quaternion quat)
        {
            return new QuaternionState { x = quat.x, y = quat.y, z = quat.z, w = quat.w };
        }

        public readonly Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

        public static implicit operator Quaternion(QuaternionState other)
        {
            return other.ToQuaternion();
        }

        public static implicit operator QuaternionState(Quaternion quat)
        {
            return From(quat);
        }

        public readonly bool Equals(QuaternionState other)
        {
            return x == other.x &&
                y == other.y &&
                z == other.z &&
                w == other.w;
        }

        public readonly bool Equals(Quaternion other)
        {
            return x == other.x &&
                y == other.y &&
                z == other.z &&
                w == other.w;
        }

        public override string ToString()
        {
            return $"Quaternion({x}, {y}, {z}, {w})";
        }
    }

}