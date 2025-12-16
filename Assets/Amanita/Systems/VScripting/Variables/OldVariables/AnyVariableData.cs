using System;
using UnityEngine;
using baseObj = System.Object;

namespace Amanita.VScripting
{
    /// <summary>
    /// Collection of every Fungus VariableData type, used in commands that are designed to
    /// support any and all types. Those command just have a AnyVariableData anyVar or
    /// an AnyVariableAndDataPair anyVarDataPair to encapsulate the more unpleasant parts.
    ///
    /// New types created need to be added to the list below and also to AllVariableTypes and
    /// AnyVariableAndDataPair
    /// 
    /// Note; when using this in a command ensure that RefreshVariableCache is also handled for
    /// string var substitution.
    /// </summary>
    [Serializable]
    public partial class AnyVariableData : VariableData
    {
        [SerializeReference] protected IVariableData data; 
        // ^Represents the actual data being held, which can change dynamically

        public override baseObj BoxedValue
        {
            get
            {
                string valStr = "none";
                if (data != null)
                {
                    valStr = data.BoxedValue != null ? data.BoxedValue.ToString() : "null";
                }

                if (ReferenceEquals(data, null))
                {
                    return null;
                }
                return data.BoxedValue;
            }
            set
            {
                if (ReferenceEquals(data, null))
                {
                    return;
                }

                if (ReferenceEquals(value, null))
                {
                    data.BoxedValue = null;
                    return;
                }

                Type valueType = value.GetType();
                if (data.ContentType.Equals(valueType))
                {
                    data.BoxedValue = value;
                }
                else
                {
                    string errorMessage = $"AnyVariableData cannot accept a {valueType.Name}.";
                    throw new System.InvalidCastException(errorMessage);
                }

            }
        }

        public virtual void Init()
        {
        }

        public virtual void SetFor<TVarType, TContentType>()
        {
            SetFor(typeof(TVarType), typeof(TContentType));
        }

        public virtual void SetFor(Type varType, Type contentType)
        {
            // Chances are that at this time, the dict has been emptied due to how Unity doesn't
            // play nice with dictionaries.
            string logMessage;
            if (varType == null)
            {
                logMessage = "Cannot set AnyVariableData for a null var type.";
                Debug.LogWarning(logMessage);
                return;
            }

            bool alreadySetToThatType = contentType.Equals(this.ContentType);
            if (alreadySetToThatType)
            {
                return;
            }

            IVariableData toSet = VariableDataTypeRegistry.CreateForVar(varType); //
            
            if (toSet == null)
            {
                logMessage = $"Could not find appropriate IVariableData for the {varType.Name} content type";
                Debug.LogError(logMessage);
                return;
            }

            data = toSet;
        }

        public override string GetDescription() => data?.GetDescription() ?? "Null";

        public override IVariable VarRef
        {
            get
            {
                return data?.VarRef;
            }
            set
            {
                if (ReferenceEquals(value, null))
                {
                    data.VarRef = null;
                    return;
                }

                // Adapt the data to the type of the var
                SetFor(value.GetType(), value.ContentType);

                data.VarRef = value;
            }
        }

        public override Type ContentType => data?.ContentType;

        public bool HasReference(Variable var)
        {
            bool result = false;
            if (data is not null)
            {
                result = ReferenceEquals(data.VarRef, var);
            }
            return result;
        }

    }

}