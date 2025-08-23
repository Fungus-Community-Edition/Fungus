using System;
using UnityEngine;

namespace Amanita.VScripting
{
    // To reduce the boilerplate in IVariableData implementors such as AnimatorData and FloatData

    public abstract class VariableData : IVariableData
    { 
        public abstract Type ContentType { get; }

        public object Value
        {
            get
            {
                if (VarRef != null)
                {
                    return VarRef.Value;
                }
                else
                {
                    return valObj;
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
                    valObj = value;
                }
            }
        }

        [SerializeReference, SerializeField] protected object valObj;
        public abstract IVariable VarRef { get; set; }

        public abstract string GetDescription();
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
    }

    public abstract class VariableData<TValue, TVar> : VariableData where TVar : IVariable<TValue>
    {
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

        public new TValue Value
        {
            get
            {
                if (VarRef != null)
                {
                    return (TValue)VarRef.Value;
                }
                else
                {
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
                    base.Value = value;
                    _valOfType = value;
                }
            }
        }

        [SerializeReference, SerializeField] protected TValue _valOfType;

        public override string GetDescription()
        {
            string result = "null"; // <- This is valid for reference types

            if (VarRef == null && _valOfType != null)
            {
                result = _valOfType.ToString();
            }
            else
            {
                result = VarRef.Key;
            }

            return result;
        }

    }

    
}