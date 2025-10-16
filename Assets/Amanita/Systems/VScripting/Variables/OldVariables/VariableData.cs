using System;
using UnityEngine;

namespace Amanita.VScripting
{
    // To reduce the boilerplate in IVariableData implementors such as AnimatorData and FloatData

    public abstract class VariableData : IVariableData
    { 
        public abstract Type ContentType { get; }
        public virtual object Value
        {
            get
            {
                if (VarRef != null)
                {
                    return VarRef.BoxedValue;
                }
                else
                {
                    return valObj;
                }
            }
            set
            {
                var prevValue = Value;
                if (VarRef != null)
                {
                    VarRef.BoxedValue = value;
                }
                else
                {
                    valObj = value;
                }

                OnBaseValueSet(prevValue);
            }
        }

        [SerializeReference, SerializeField] protected object valObj;
        public abstract IVariable VarRef { get; set; }

        protected virtual void OnBaseValueSet(object prevValue)
        {

        }

        public abstract string GetDescription();

        public virtual void SetContentsTo(IVariableData otherVarData)
        {
            this.valObj = (otherVarData as VariableData).valObj;
        }

        public virtual IVariableData GetCopy()
        {
            Type thisType = GetType();

            IVariableData theCopy = (IVariableData)Activator.CreateInstance(thisType);
            theCopy.SetContentsTo(this);
            return theCopy;
        }

        
        public virtual void Refresh() { }
    }

    public interface IVariableData
    {
        Type ContentType { get; }

        object Value { get; set; }

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
        [SerializeField, SerializeReference]
        protected IVariable varRef;

        public static implicit operator TValue(VariableData<TValue> someData)
        {
            someData.Refresh();
            return someData.Value;
        }

        public VariableData()
        {
            valOfType = default;
            VarRef = null;
        }

        public VariableData(TValue startVal = default)
        {
            valOfType = startVal;
            VarRef = null;
        }

        public override Type ContentType => typeof(TValue);

        public virtual new TValue Value
        {
            get
            {
                if (VarRef != null)
                {
                    return (TValue)VarRef.BoxedValue;
                }
                else
                {
                    return valOfType;
                }
            }
            set
            {
                if (VarRef != null)
                {
                    VarRef.BoxedValue = value;
                }
                else
                {
                    base.Value = value;
                    valOfType = value;
                }
            }
        }

        [SerializeReference, SerializeField] protected TValue valOfType = default;

        public override string GetDescription()
        {
            string result = "null"; // <- This is valid for reference types

            if (VarRef == null && valOfType != null)
            {
                result = valOfType.ToString();
            }
            else if (VarRef != null)
            {
                result = VarRef.Key;
            }

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
            this.valObj = this.valOfType = otherVarData.valOfType;
            this.VarRef = otherVarData.VarRef;
        }

        protected override void OnBaseValueSet(object prevValue)
        {
            valOfType = (TValue)valObj;
        }

        public override IVariable VarRef
        {
            get { return varRef; }
            set
            {
                if (value == null) { varRef = null; return; }

                if (this.ContentType.IsAssignableFrom(value.ContentType)) // We want to allow polymorphism
                {
                    varRef = (IVariable<TValue>)value;
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