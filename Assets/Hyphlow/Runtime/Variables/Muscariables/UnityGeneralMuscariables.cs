using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityObject = UnityEngine.Object;

namespace AtMycelia.Hyphlow
{
    [System.Serializable]
    [VariableInfo("UnityGeneral", "GameObject", typeof(GameObject))]
    [MovedFrom(true, 
        "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core", 
        "GameObjectMuscariable")]
    public class GameObjectMuscariable : Muscariable<GameObject>
    {
        // The Evaluate func by default only handles Equals and NotEquals. Thus, we
        // won't have to override it for this class.

        public GameObjectMuscariable() : base() { }

        public virtual string GOName
        {
            get
            {
                if (Value == null)
                    return string.Empty;

                return Value.name;
            }
            set
            {
                if (!IsDestroyed)
                {
                    Value.name = value;
                }
                else
                {
                    string errorMessage = "Cannot change the name of a GameObject through a GameObjectVariable that has no GO assigned.";
                    Debug.LogError(errorMessage);
                }
            }
        }

        public bool IsDestroyed => Value == null;

        public static bool operator ==(GameObjectMuscariable a, GameObjectMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(GameObjectMuscariable a, GameObjectMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as GameObjectMuscariable;
            if (other == null) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }


    [System.Serializable]
    [VariableInfo("UnityGeneral", "Transform", typeof(Transform))]
    [MovedFrom(true, 
        "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core", 
        "TransformMuscariable")]
    public class TransformMuscariable : Muscariable<Transform>
    {
        public TransformMuscariable() : base() { }

        public static bool operator ==(TransformMuscariable a, TransformMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(TransformMuscariable a, TransformMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TransformMuscariable;
            if (other == null) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    // This is sort of a handle-everything else type, just like the original ObjectVariable in Fungus
    // (which also only accepted Unity objects in particular, not just any System.object)
    [System.Serializable]
    [VariableInfo("UnityGeneral", "UnityObject", typeof(UnityObject))]
    [MovedFrom(true, "AtMycelia.Hyphlow",
        "AtMycelia.Amanita.Core")]
    public class UnityObjectMuscariable : Muscariable<UnityObject>
    {
        public UnityObjectMuscariable() : base() { }

        public static bool operator ==(UnityObjectMuscariable a, UnityObjectMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(UnityObjectMuscariable a, UnityObjectMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as UnityObjectMuscariable;
            if (other == null) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }
}