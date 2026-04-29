using System;
using UnityEngine;
using UnityEngine.Serialization;
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
        [SerializeReference]
        [FormerlySerializedAs("data")]
        protected IVariableData _data; 
        // ^Represents the actual data being held, which can change dynamically

        public override baseObj BoxedValue
        {
            get
            {
                if (_data == null)
                {
                    return null;
                }
                return _data.BoxedValue;
            }
            set
            {
                if (_data == null)
                {
                    return;
                }

                if (value == null)
                {
                    _data.BoxedValue = null;
                    return;
                }

                Type valueType = value.GetType();
                if (_data.ContentType.Equals(valueType))
                {
                    _data.BoxedValue = value;
                }
                else
                {
                    string errorMessage = $"AnyVariableData cannot accept a {valueType.Name}.";
                    throw new InvalidCastException(errorMessage);
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

            _data = toSet;
        }

        public void SetFor(Type contentType)
        {
            if (contentType == null)
            {
                Debug.LogWarning("Cannot set AnyVariableData for a null content type.");
                return;
            }

            if (_data != null && contentType.Equals(_data.ContentType))
            {
                return;
            }

            IVariableData toSet = VariableDataTypeRegistry.CreateForContentType(contentType);
            _data = toSet;
        }

        public override string GetDescription() => _data?.GetDescription() ?? "Null";

        public override IVariable VarRef
        {
            get
            {
                return _data?.VarRef;
            }
            set
            {
                if (ReferenceEquals(value, null))
                {
                    _backingVarRef.Variable = null;
                    _data.VarRef = null;
                    return;
                }

                // Adapt the data to the type of the var
                SetFor(value.GetType(), value.ContentType);

                _data.VarRef = _backingVarRef.Variable = value;
            }
        }

        public override Type ContentType => _data?.ContentType;

        public bool HasReference(Variable var)
        {
            bool result = false;
            if (_data is not null)
            {
                result = ReferenceEquals(_data.VarRef, var);
            }
            return result;
        }

        public T GetValue<T>(bool logMessageOnFail = false)
        {
            Type tType = typeof(T);
            if (_data is null)
            {
                string logMessage = $"Cannot get value of type {tType.Name} from AnyVariableData " +
                    $"because it has no data.";
                Debug.LogError(logMessage);
                return default;
            }

            bool validType = tType.IsAssignableFrom(_data.ContentType);
            if (!validType)
            {
                if (logMessageOnFail)
                {
                    var logMessage = $"Cannot get value of type {tType.Name} from AnyVariableData " +
                    $"because it holds data of type {_data.ContentType.Name}.";
                    Debug.LogError(logMessage);
                }
                return default;
            }

            return (T)_data.BoxedValue;
        }
    }

}