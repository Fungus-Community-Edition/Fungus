


using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// String variable type.
    /// </summary>
    [VariableInfo("", "String", typeof(string))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class StringVariable : VariableBase<string>
    {
    }

    /// <summary>
    /// Container for a string variable reference or constant value.
    /// Appears as a single line property in the inspector.
    /// For a multi-line property, use StringDataMulti.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(string), typeof(StringVariable))]
    public class StringData : VariableData<string, IVariable<string>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(StringVariable))]
        public StringVariable stringRef;

        public StringData() : base(default) { }

        public StringData(string startVal) : base(startVal)
        {
        }

        public static implicit operator string(StringData spriteData)
        {
            return spriteData.Value;
        }

        public override IVariable VarRef
        {
            get { return stringRef; }
            set
            {
                if (value == null) { stringRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    stringRef = value as StringVariable;
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new System.InvalidCastException(errorMessage);
                }

            }
        }

        public override string Value
        {
            get
            {
                string result;
                if (VarRef != null)
                {
                    result = (string)VarRef.Value;
                }
                else
                {
                    result = _valOfType;
                }

                // To make sure we never return a null value
                if (result == null)
                {
                    result = "";
                    if (VarRef != null)
                    {
                        VarRef.Value = result;
                    }
                    base.Value = _valOfType = result;
                }

                return result;
            }
            set
            {
                if (VarRef != null)
                {
                    VarRef.Value = value;
                }
                else
                {
                    base.Value = value;
                    _valOfType = value;
                }
            }
        }
    }

    /// <summary>
    /// Container for a string variable reference or constant value.
    /// Appears as a multi-line property in the inspector.
    /// For a single-line property, use StringData.
    /// </summary>
    [System.Serializable]
    public class StringDataMulti : StringData
    {
        public StringDataMulti() : base(default) { }

        public StringDataMulti(string startVal) : base(startVal)
        {
        }

        public static implicit operator string(StringDataMulti spriteData)
        {
            return spriteData.Value;
        }

    }
        
}