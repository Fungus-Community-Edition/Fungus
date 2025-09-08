using UnityEngine;

namespace Amanita.VScripting
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
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(ControllerColliderHitVariable))]
        public IVariable<ControllerColliderHit> controllerColliderHitRef;

        public ControllerColliderHitData() : base(default) { }

        public ControllerColliderHitData(ControllerColliderHit startVal) : base(startVal)
        {
        }

        public override IVariable VarRef
        {
            get { return controllerColliderHitRef; }
            set
            {
                if (value == null) { controllerColliderHitRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    controllerColliderHitRef = value as ControllerColliderHitVariable;
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new System.InvalidCastException(errorMessage);
                }

            }
        }

    }
}