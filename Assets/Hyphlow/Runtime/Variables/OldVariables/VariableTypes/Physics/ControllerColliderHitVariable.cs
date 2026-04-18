using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// ControllerColliderHit variable type.
    /// </summary>
    [VariableInfo("Physics", "ControllerColliderHit", typeof(ControllerColliderHit), IsPreviewedOnly = true)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class ControllerColliderHitVariable : VariableBase<UnityEngine.ControllerColliderHit>
    { }

    [System.Serializable]
    [VariableData(typeof(ControllerColliderHit), typeof(ControllerColliderHitVariable))]
    public class ControllerColliderHitData : VariableData<ControllerColliderHit>
    {
        public ControllerColliderHitData() : base(default) { }

        public ControllerColliderHitData(ControllerColliderHit startVal) : base(startVal)
        {
        }

    }
}