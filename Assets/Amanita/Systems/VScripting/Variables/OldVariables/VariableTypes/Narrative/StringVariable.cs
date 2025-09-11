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
    public class StringData : VariableData<string>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(StringVariable))]
        public IVariable<string> stringRef;

        public StringData() : base(default) { }

        public StringData(string startVal) : base(startVal)
        {
        }

        public static implicit operator string(StringData spriteData)
        {
            return spriteData.Value;
        }

        public override void Refresh()
        {
            varRef ??= stringRef;
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
                    result = valOfType;
                }

                // To make sure we never return a null value
                if (result == null)
                {
                    result = "";
                    if (VarRef != null)
                    {
                        VarRef.Value = result;
                    }
                    base.Value = valOfType = result;
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
                    valOfType = value;
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