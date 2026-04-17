using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Collision variable type.
    /// </summary>
    [VariableInfo("Physics/ThreeD", "Collision", typeof(Collision), IsPreviewedOnly = true)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class CollisionVariable : VariableBase<UnityEngine.Collision>
    { }

    [System.Serializable]
    [VariableData(typeof(Collision), typeof(CollisionVariable))]
    public class CollisionData : VariableData<Collision>
    {
        public CollisionData() : base(default) { }

        public CollisionData(Collision startVal) : base(startVal)
        {
        }


    }
}