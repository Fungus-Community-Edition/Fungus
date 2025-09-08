using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    /// <summary>
    /// Object variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "UnityObject", typeof(UnityObj))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class ObjectVariable : VariableBase<UnityObj>
    {
    }

    /// <summary>
    /// Container for an Object variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(UnityObj), typeof(ObjectVariable))]
    public class ObjectData : VariableData<UnityObj>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(ObjectVariable))]
        public IVariable<UnityObj> objectRef;

        public ObjectData() : base(default) { }
        public ObjectData(UnityObj startVal = null) : base(startVal) { }
        
        public static implicit operator UnityObj(ObjectData objectData)
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