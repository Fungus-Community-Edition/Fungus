


using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Amanita.VScripting
{
    /// <summary>
    /// Object variable type.
    /// </summary>
    [VariableInfo("Other", "Object", "UnityObject")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class ObjectVariable : VariableBase<UnityObject>
    {
    }

    /// <summary>
    /// Container for an Object variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(UnityObject), typeof(ObjectVariable))]
    public class ObjectData : VariableData<UnityObject, IVariable<UnityObject>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(ObjectVariable))]
        public ObjectVariable objectRef;

        public ObjectData() : base(default) { }
        public ObjectData(UnityObject startVal = null) : base(startVal) { }
        
        public static implicit operator UnityObject(ObjectData objectData)
        {
            return objectData.Value;
        }

        public override IVariable VarRef
        {
            get { return objectRef; }
            set
            {
                if (value == null) { objectRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    objectRef = value as ObjectVariable;
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