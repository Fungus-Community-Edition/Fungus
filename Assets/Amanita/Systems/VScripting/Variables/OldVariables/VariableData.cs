using System;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    // To reduce the boilerplate in IVariableData implementors such as AnimatorData and FloatData

    public abstract class VariableData : IVariableData
    { 
        public abstract Type ContentType { get; }
        public abstract object BoxedValue
        {
            get;
            set;
        }

        public abstract IVariable VarRef { get; set; }

        public abstract string GetDescription();

        public virtual IVariableData GetCopy()
        {
            Type thisType = GetType();

            IVariableData theCopy = (IVariableData)Activator.CreateInstance(thisType);
            theCopy.SetContentsTo(this);
            return theCopy;
        }

        public virtual void Refresh() { }

        public virtual void SetContentsTo(IVariableData otherVarData)
        {
            this.VarRef = otherVarData.VarRef;
        }

        protected virtual bool CanHoldAsValue(object obj)
        {
            bool result;

            if (ReferenceEquals(obj, null))
            {
                result = ContentType.IsClass;
            }
            else
            {
                result = ContentType.IsAssignableFrom(obj.GetType());
            }

            return result;
        }
    }

    public interface IVariableData
    {
        Type ContentType { get; }

        object BoxedValue { get; set; }

        /// <summary>
        /// Returns a human-readable description for UI/debug.
        /// </summary>
        string GetDescription();

        IVariable VarRef { get; set; } // To be a more generic way to access stuff like animatorRef, floatRef, etc
        void SetContentsTo(IVariableData otherVarData);

        IVariableData GetCopy();

    }

    public abstract class VariableData<TValue> : VariableData
    {
        [SerializeReference]
        protected IVariable varRef; // Should be a muscariable IVariable<TValue>//

        protected virtual Variable LegacyVarRef { get; set; } // For backward compatibility

        public static implicit operator TValue(VariableData<TValue> someData)
        {
            someData.Refresh();
            return someData.Value;
        }

        public VariableData()
        {
            value = default;
            VarRef = null;
        }

        public VariableData(TValue startVal = default)
        {
            value = startVal;
            VarRef = null;
        }

        public override Type ContentType => typeof(TValue);
        
        public virtual TValue Value
        {
            get
            {
                if (LegacyVarRef != null)
                {
                    return (TValue)LegacyVarRef.BoxedValue;
                }
                else if (VarRef != null)
                {
                    return (TValue)VarRef.BoxedValue;
                }
                else
                {
                    return value;
                }
            }
            set
            {
                if (LegacyVarRef != null)
                {
                    LegacyVarRef.BoxedValue = value;
                }
                else if (VarRef != null)
                {
                    VarRef.BoxedValue = value;
                }
                else
                {
                    this.value = value;
                }
            }
        }

        public override object BoxedValue
        {
            get => value;
            set
            {
                try
                {
                    this.value = (TValue)value;
                }
                catch
                {
                    Debug.LogWarning($"VariableData of value type {typeof(TValue).Name} could not box value " +
                        $"of type {value.GetType().Name} to type {typeof(TValue).Name}");
                }
            }
        }

        [SerializeReference, SerializeField] protected TValue value = default;

        public override string GetDescription()
        {
            string result = "null"; // <- This is valid for reference types

            if (VarRef == null && value != null)
            {
                result = value.ToString();//
            }
            else if (VarRef != null)
            {
                result = VarRef.Key;
            }

            result = $"'{result}'";
            return result;
        }

        public override void SetContentsTo(IVariableData otherVarData)
        {
            var ourType = this.GetType();
            var theirType = otherVarData.GetType();
            if (ourType.Equals(theirType))
            {
                SetContentsTo(otherVarData as VariableData<TValue>);
            }
        }

        public virtual void SetContentsTo(VariableData<TValue> otherVarData)
        {
            this.VarRef = otherVarData.VarRef;
            this.value = otherVarData.value;
        }

        public override IVariable VarRef
        {
            get
            {
                if (LegacyVarRef != null)
                {
                    return LegacyVarRef;
                }
                return varRef;
            }
            set
            {
                if (value == null) { varRef = null; return; }

                if (this.ContentType.IsAssignableFrom(value.ContentType)) // We want to allow polymorphism
                {
                    if (value is UnityObj)
                    {
                        LegacyVarRef = (Variable)value;
                    }
                    else
                    {
                        varRef = (IVariable<TValue>)value;
                    }
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new InvalidCastException(errorMessage);
                }

            }
        }


    }

}