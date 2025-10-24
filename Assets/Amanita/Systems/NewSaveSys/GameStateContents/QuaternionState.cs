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
            bool sameX = x.Equals(other.x);
            bool sameY = y.Equals(other.y);
            bool sameZ = z.Equals(other.z);
            bool sameW = w.Equals(other.w);
            return sameX && sameY && sameZ && sameW;
        }

        public readonly bool Equals(Quaternion other)
        {
            bool sameX = x.Equals(other.x);
            bool sameY = y.Equals(other.y);
            bool sameZ = z.Equals(other.z);
            bool sameW = w.Equals(other.w);
            return sameX && sameY && sameZ && sameW;
        }

        public override readonly string ToString()
        {
            return $"Quaternion({x:R}, {y:R}, {z:R}, {w:R})";
        }
    }

}