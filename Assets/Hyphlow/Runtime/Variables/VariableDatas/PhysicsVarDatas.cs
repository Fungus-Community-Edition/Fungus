using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Container for a Collider variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Collider), typeof(ColliderVariable))]
    [MovedFrom("AtMycelia.Hyphlow")]
    public class ColliderData : VariableData<Collider>
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

        public ColliderData() : base(default) { }

        public ColliderData(Collider startVal) : base(startVal)
        {
        }

    }

    /// <summary>
    /// Container for a Collider2D variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Collider2D), typeof(Collider2DVariable))]
    [MovedFrom("AtMycelia.Hyphlow")]
    public class Collider2DData : VariableData<Collider2D>
    {
        [SerializeField]
        public ColliderVariable collider2DRef;

        [SerializeField]
        [HideInInspector]
        public Collider2D collider2DVal;

        public Collider2DData() : base(default) { }

        public Collider2DData(Collider2D startVal) : base(startVal)
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
}