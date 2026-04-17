using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Container for a Collider variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Collider), typeof(ColliderVariable))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ColliderThreeDData : VariableData<Collider>
    {
        [SerializeField]
        public ColliderVariable colliderRef;

        [SerializeField]
        [HideInInspector]
        public Collider colliderVal;

        protected override Variable LegacyVarRef
        {
            get => colliderRef;
            set => colliderRef = value as ColliderVariable;
        }

        public override Collider LiteralValue
        {
            get => colliderVal;
            set => colliderVal = value;
        }

        public ColliderThreeDData() : base(default) { }

        public ColliderThreeDData(Collider startVal) : base(startVal)
        {
        }

    }

    /// <summary>
    /// Container for a Collider2D variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Collider2D), typeof(Collider2DVariable))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ColliderTwoDData : VariableData<Collider2D>
    {
        [SerializeField]
        public ColliderVariable collider2DRef;

        [SerializeField]
        [HideInInspector]
        public Collider2D collider2DVal;

        public ColliderTwoDData() : base(default) { }

        public ColliderTwoDData(Collider2D startVal) : base(startVal)
        {
        }

        protected override Variable LegacyVarRef
        {
            get => collider2DRef;
            set => collider2DRef = value as ColliderVariable;
        }
        public override Collider2D LiteralValue
        {
            get => collider2DVal;
            set => collider2DVal = value;
        }

    }

    /// <summary>
    /// Container for a Rigidbody variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Rigidbody), typeof(RigidbodyVariable))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class RigidbodyThreeDData : VariableData<Rigidbody>
    {
        [SerializeField]
        public RigidbodyVariable rigidbodyRef;

        [SerializeField]
        [HideInInspector]
        public Rigidbody rigidbodyVal;

        protected override Variable LegacyVarRef
        {
            get => rigidbodyRef;
            set => rigidbodyRef = value as RigidbodyVariable;
        }

        public override Rigidbody LiteralValue
        {
            get => rigidbodyVal;
            set => rigidbodyVal = value;
        }

        public RigidbodyThreeDData() : base(default) { }

        public RigidbodyThreeDData(Rigidbody startVal) : base(startVal)
        {
        }

    }

    /// <summary>
    /// Container for a Rigidbody2D variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Rigidbody2D), typeof(Rigidbody2DVariable))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class RigidbodyTwoDData : VariableData<Rigidbody2D>
    {
        [SerializeField]
        public Rigidbody2DVariable rigidbody2DRef;

        [SerializeField]
        [HideInInspector]
        public Rigidbody2D rigidbody2DVal;

        public RigidbodyTwoDData() : base(default) { }

        public RigidbodyTwoDData(Rigidbody2D startVal) : base(startVal)
        {
        }

        protected override Variable LegacyVarRef
        {
            get => rigidbody2DRef;
            set => rigidbody2DRef = value as Rigidbody2DVariable;
        }

        public override Rigidbody2D LiteralValue
        {
            get => rigidbody2DVal;
            set => rigidbody2DVal = value;
        }

    }


}