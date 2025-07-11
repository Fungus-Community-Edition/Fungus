


using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Object variable type.
    /// </summary>
    [VariableInfo("Other", "Object")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class ObjectVariable : VariableBase<Object>
    {
    }

    /// <summary>
    /// Container for an Object variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct ObjectData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(ObjectVariable))]
        public ObjectVariable objectRef;
        
        [SerializeField]
        public Object objectVal;

        public ObjectData(Object v)
        {
            objectVal = v;
            objectRef = null;
        }
        
        public static implicit operator Object(ObjectData objectData)
        {
            return objectData.Value;
        }

        public Object Value
        {
            get { return (objectRef == null) ? objectVal : objectRef.Value; }
            set { if (objectRef == null) { objectVal = value; } else { objectRef.Value = value; } }
        }

        public string GetDescription()
        {
            if (objectRef == null)
            {
                return objectVal != null ? objectVal.ToString() : "Null";
            }
            else
            {
                return objectRef.Key;
            }
        }
    }
}