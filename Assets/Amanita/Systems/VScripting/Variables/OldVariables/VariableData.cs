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
                    return VarRef.Value;
                }
                else
                {
                    return _valObj;
                }
            }
            set
            {
                if (VarRef != null)
                {
                    VarRef.Value = value;
                }
                else
                {
                    _valObj = value;
                }
            }
        }

        [SerializeReference, SerializeField] protected object _valObj;
        public abstract IVariable VarRef { get; set; }

        public abstract string GetDescription();

        public virtual void SetContentsTo(IVariableData otherVarData)
        {
            this._valObj = (otherVarData as VariableData)._valObj;
        }

        public virtual IVariableData GetCopy()
        {
            Type thisType = GetType();

            IVariableData theCopy = (IVariableData)Activator.CreateInstance(thisType);
            theCopy.SetContentsTo(this);
            return theCopy;
        }
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
        protected IVariable<TValue> _varRef;

        public static implicit operator TValue(VariableData<TValue> someData)
        {
            return someData.Value;
        }

        public VariableData()
        {
            _valOfType = default;
            VarRef = null;
        }

        public VariableData(TValue startVal = default)
        {
            _valOfType = startVal;
            VarRef = null;
        }

        public override Type ContentType => typeof(TValue);

        public virtual new TValue Value
        {
            get
            {
                if (VarRef != null)
                {
                    //Debug.Log($"{GetType().Name}.Value get: {VarRef.Value} (hash: {GetHashCode()})");
                    return (TValue)VarRef.Value;
                }
                else
                {
                    //Debug.Log($"{GetType().Name}.Value get: {_valOfType} (hash: {GetHashCode()})");
                    return _valOfType;
                }
            }
            set
            {
                if (VarRef != null)
                {
                    VarRef.Value = value;
                }
                else
                {
                    //Debug.Log($"{GetType().Name}.Value set: {value} (hash: {GetHashCode()})");
                    base.Value = value;
                    _valOfType = value;
                }
            }
        }

        [SerializeReference, SerializeField] protected TValue _valOfType = default;

        public override string GetDescription()
        {
            string result = "null"; // <- This is valid for reference types

            if (VarRef == null && _valOfType != null)
            {
                result = _valOfType.ToString();
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
            this._valObj = this._valOfType = otherVarData._valOfType;
            this.VarRef = otherVarData.VarRef;
        }

        public override IVariable VarRef
        {
            get { return _varRef; }
            set
            {
                if (value == null) { _varRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    _varRef = value as IVariable<TValue>;
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