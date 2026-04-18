using System;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [Serializable]
    [VariableInfo("Physics/ThreeD", "ColliderThreeD", typeof(Collider))]
    [MovedFrom(true, "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core")]
    public class ColliderThreeDMuscariable : Muscariable<Collider>
    {
        // We use SerializeField here (instead of SerializeReference) due to the way
        // Unity manages UnityEngine.Object types. SerializeReference is for other
        // types that:
        // Aren't easy to serialize (like when you have a member var going off an interface)
        // Aren't in the same family tree as UnityEngine.Object
        [SerializeField] protected Collider colliderRef;

        [SerializeField] protected GameObject hostGO; // For ref recovery

        // Optional shadow data for Undo safety & runtime restore
        [SerializeField] protected Vector3 cachedCenter;
        [SerializeField] protected Vector3 cachedSize;
        [SerializeField] protected bool cachedIsTrigger;

        public ColliderThreeDMuscariable() { }

        public override Collider Value
        {
            get => colliderRef;
            set
            {
                base.Value = colliderRef = value;
                hostGO = value != null ? value.gameObject : null;
                CacheColliderData(value);
            }
        }

        protected void CacheColliderData(Collider colToCache)
        {
            if (colToCache is BoxCollider box)
            {
                cachedCenter = box.center;
                cachedSize = box.size;
            }
            else if (colToCache is SphereCollider sphere)
            {
                cachedCenter = sphere.center;
                cachedSize = Vector3.one * (sphere.radius * 2f);
            }
            else
            {
                cachedCenter = Vector3.zero;
                cachedSize = Vector3.zero;
            }

            cachedIsTrigger = colToCache != null && colToCache.isTrigger;
        }

        public void RefreshReferenceIfNeeded()
        {
            if (colliderRef == null && hostGO != null)
                colliderRef = hostGO.GetComponent<Collider>();
        }

        public override void OnReset()
        {
            base.OnReset();
            colliderRef = null;
            hostGO = null;
            cachedCenter = Vector3.zero;
            cachedSize = Vector3.zero;
            cachedIsTrigger = false;
        }

        public override void Apply(SetOperator setOperator, Collider toApply)
        {
            switch (setOperator)
            {
                case SetOperator.Assign:
                    Value = toApply;
                    break;
                default:
                    Debug.LogError($"SetOperator {setOperator} not valid for {ContentType.Name}");
                    break;
            }
        }

        public override bool Evaluate(CompareOperator op, Collider toCompareTo)
        {
            switch (op)
            {
                case CompareOperator.Equals:
                    return Value.Equals(toCompareTo);
                case CompareOperator.NotEquals:
                    return Value.Equals(toCompareTo);
                default:
                    Debug.LogError($"CompareOperator {op} not supported for {ContentType.Name}");
                    return false;
            }
        }
    }
    
    [Serializable]
    [VariableInfo("Physics/TwoD", "ColliderTwoD", typeof(Collider2D))]
    [MovedFrom(true, "AtMycelia.Hyphlow",
        "AtMycelia.Amanita.Core")]
    public class ColliderTwoDMuscariable : Muscariable<Collider2D>
    {
        [SerializeField] protected Collider2D colliderRef;
        [SerializeField] protected GameObject hostGO;

        [SerializeField] protected Vector2 cachedOffset;
        [SerializeField] protected Vector2 cachedSize;
        [SerializeField] protected bool cachedIsTrigger;

        public ColliderTwoDMuscariable() { }

        public override Collider2D Value
        {
            get => colliderRef;
            set
            {
                base.Value = colliderRef = value;

                if (value != null)
                {
                    hostGO = value.gameObject;
                }
                else
                {
                    hostGO = null;
                }

                CacheColliderData(value);
            }
        }

        protected void CacheColliderData(Collider2D colToCache)
        {
            if (colToCache is BoxCollider2D box)
            {
                cachedOffset = box.offset;
                cachedSize = box.size;
            }
            else
            {
                cachedOffset = Vector2.zero;
                cachedSize = Vector2.zero;
            }
            cachedIsTrigger = colToCache != null && colToCache.isTrigger;
        }

        public void RefreshReferenceIfNeeded()
        {
            if (colliderRef == null && hostGO != null)
                colliderRef = hostGO.GetComponent<Collider2D>();
        }

        public override void OnReset()
        {
            base.OnReset();
            colliderRef = null;
            hostGO = null;
            cachedOffset = Vector2.zero;
            cachedSize = Vector2.zero;
            cachedIsTrigger = false;
        }

        public override void Apply(SetOperator setOperator, Collider2D toApply)
        {
            switch (setOperator)
            {
                case SetOperator.Assign:
                    Value = toApply;
                    break;
                default:
                    Debug.LogError($"SetOperator {setOperator} not valid for {ContentType.Name}");
                    break;
            }
        }

        public override bool Evaluate(CompareOperator op, Collider2D toCompareTo)
        {
            switch (op)
            {
                case CompareOperator.Equals:
                    return Value == toCompareTo;
                case CompareOperator.NotEquals:
                    return Value != toCompareTo;
                default:
                    Debug.LogError($"CompareOperator {op} not supported for {ContentType.Name}");
                    return false;
            }
        }

    }

    [Serializable]
    [VariableInfo("Physics/ThreeD", "RigidbodyThreeD", typeof(Rigidbody))]
    public class RigidbodyThreeDMuscariable : Muscariable<Rigidbody>
    {
        [SerializeField] protected Rigidbody rigidbodyRef;
        [SerializeField] protected GameObject hostGO;
        public RigidbodyThreeDMuscariable() { }
        public override Rigidbody Value
        {
            get => rigidbodyRef;
            set
            {
                base.Value = rigidbodyRef = value;
                if (value != null)
                {
                    hostGO = value.gameObject;
                }
                else
                {
                    hostGO = null;
                }
            }
        }
        public void RefreshReferenceIfNeeded()
        {
            if (rigidbodyRef == null && hostGO != null)
                rigidbodyRef = hostGO.GetComponent<Rigidbody>();
        }
        public override void OnReset()
        {
            base.OnReset();
            rigidbodyRef = null;
            hostGO = null;
        }
        public override void Apply(SetOperator setOperator, Rigidbody toApply)
        {
            switch (setOperator)
            {
                case SetOperator.Assign:
                    Value = toApply;
                    break;
                default:
                    Debug.LogError($"SetOperator {setOperator} not valid for {ContentType.Name}");
                    break;
            }
        }
        public override bool Evaluate(CompareOperator op, Rigidbody toCompareTo)
        {
            switch (op)
            {
                case CompareOperator.Equals:
                    return Value == toCompareTo;
                case CompareOperator.NotEquals:
                    return Value != toCompareTo;
                default:
                    Debug.LogError($"CompareOperator {op} not supported for {ContentType.Name}");
                    return false;
            }
        }
    }

    [Serializable]
    [VariableInfo("Physics/TwoD", "RigidbodyTwoD", typeof(Rigidbody2D))]
    public class RigidbodyTwoDMuscariable : Muscariable<Rigidbody2D>
    {
        [SerializeField] protected Rigidbody2D rigidbodyRef;
        [SerializeField] protected GameObject hostGO;
        public RigidbodyTwoDMuscariable() { }
        public override Rigidbody2D Value
        {
            get => rigidbodyRef;
            set
            {
                base.Value = rigidbodyRef = value;
                if (value != null)
                {
                    hostGO = value.gameObject;
                }
                else
                {
                    hostGO = null;
                }
            }
        }
        public void RefreshReferenceIfNeeded()
        {
            if (rigidbodyRef == null && hostGO != null)
                rigidbodyRef = hostGO.GetComponent<Rigidbody2D>();
        }
        public override void OnReset()
        {
            base.OnReset();
            rigidbodyRef = null;
            hostGO = null;
        }
        public override void Apply(SetOperator setOperator, Rigidbody2D toApply)
        {
            switch (setOperator)
            {
                case SetOperator.Assign:
                    Value = toApply;
                    break;
                default:
                    Debug.LogError($"SetOperator {setOperator} not valid for {ContentType.Name}");
                    break;
            }
        }
        public override bool Evaluate(CompareOperator op, Rigidbody2D toCompareTo)
        {
            switch (op)
            {
                case CompareOperator.Equals:
                    return Value == toCompareTo;
                case CompareOperator.NotEquals:
                    return Value != toCompareTo;
                default:
                    Debug.LogError($"CompareOperator {op} not supported for {ContentType.Name}");
                    return false;
            }
        }
    }


}