using System;
using UnityEngine;
using baseObj = System.Object;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// A VariableData class that can hold any type of variable data. It does this by holding a 
    /// reference to an IVariableData, which can be swapped out at runtime to change the type 
    /// of variable data being held.
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
                logMessage = $"Could not find appropriate IVariableData for the " +
                    $"{varType.Name} content type";
                Debug.LogError(logMessage);
                return;
            }

            data = toSet;
        }

        public void SetFor(Type contentType)
        {
            if (contentType == null)
            {
                Debug.LogWarning("Cannot set AnyVariableData for a null content type.");
                return;
            }

            if (data != null && contentType.Equals(data.ContentType))
            {
                return;
            }

            IVariableData toSet = VariableDataTypeRegistry.CreateForContentType(contentType);
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