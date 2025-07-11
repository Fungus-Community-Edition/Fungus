using UnityEngine;

namespace Amanita.VScripting
{
    [System.Serializable]
    [Muscariable("PhysicsThreeD", typeof(Collider), "ColliderThreeD")]
    public class ColliderMuscariableThreeD : Muscariable<Collider>
    {
        public static bool operator ==(ColliderMuscariableThreeD a, ColliderMuscariableThreeD b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(ColliderMuscariableThreeD a, ColliderMuscariableThreeD b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ColliderMuscariableThreeD;
            if (other == null) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    [System.Serializable]
    [Muscariable("PhysicsTwoD", typeof(Collider2D), "ColliderTwoD")]
    public class ColliderMuscariableTwoD : Muscariable<Collider2D>
    {
        public static bool operator ==(ColliderMuscariableTwoD a, ColliderMuscariableTwoD b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(ColliderMuscariableTwoD a, ColliderMuscariableTwoD b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ColliderMuscariableTwoD;
            if (other == null) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }



}