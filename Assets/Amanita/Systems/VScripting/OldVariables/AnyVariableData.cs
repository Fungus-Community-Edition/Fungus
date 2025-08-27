using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    [System.Serializable]
    public partial class AnyVariableData : VariableData, ISerializationCallbackReceiver
    {
        [SerializeReference] // Allows polymorphic serialization of IVariableData
        protected IVariableData data;

        public override System.Object Value
        {
            get
            { 
                Debug.Log($"AnyVariableData.Value called. data: {data}, type: {data?.GetType().Name}, value: {data?.Value}");
                if (ReferenceEquals(data, null))
                {
                    return null;
                }
                return data.Value;
            }
            set
            {
                if (ReferenceEquals(data, null))
                {
                    return;
                }

                if (ReferenceEquals(value, null))
                {
                    data.Value = null;
                    return;
                }

                Type valueType = value.GetType();
                if (data.ContentType.Equals(valueType))
                {
                    data.Value = value;
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

        public virtual void SetFor<T>() where T : IVariable
        {
            SetFor(typeof(T));
        }

        public virtual void OnBeforeSerialize() { }

        public virtual void OnAfterDeserialize()
        {
        }

        public virtual void SetFor(Type varType)
        {
            // Chances are that at this time, the dict has been emptied due to how Unity doesn't
            // play nice with dictionaries.
            string logMessage;
            if (varType == null)
            {
                logMessage = "Cannot set AnyVariableData for a null var type.";
                Debug.LogError(logMessage);
                return;
            }

            bool alreadySetToThatType = data != null && data.VarRef != null && data.VarRef.GetType() == varType;
            if (alreadySetToThatType)
            {
                return;
            }

            IVariableData toSet = VariableDataRegistry.CreateForVar(varType);
            
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
                SetFor(value.GetType());

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