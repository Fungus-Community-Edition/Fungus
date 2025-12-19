using System;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    // To reduce the boilerplate in IVariableData implementors such as AnimatorData and FloatData
    public abstract class VariableData : IVariableData
    {
        [SerializeField] protected VariableReference backingVarRef = new VariableReference();
        protected virtual Variable LegacyVarRef
        {
            get => backingVarRef.Variable as Variable;
            set => backingVarRef.Variable = value;
        }

        public IVariableSource VarOwner
        {
            get
            {
                return backingVarRef.VarOwner;
            }
            set
            {
                backingVarRef.VarOwner = value;
            }
        }

        public abstract Type ContentType { get; }
        public abstract object BoxedValue
        {
            get;
            set;
        }

        public virtual IVariable VarRef
        {
            get
            {
                return backingVarRef.Variable;
            }
            set
            {
                bool alreadyAssigned = ReferenceEquals(value, backingVarRef.Variable);
                if (alreadyAssigned)
                {
                    return;
                }

                if (value == null) // We want to treat null-assignments as switching to literal mode
                {
                    backingVarRef.Variable = null;
                    return;
                }

                bool validType = CanHoldAsVar(value);
                if (!validType)
                {
                    string errorMessage = $"VariableData: Cannot hold {value} as a variable. I am working with a" +
                        $"ContentType of {ContentType.Name}.";
                    throw new InvalidCastException(errorMessage);
                }
                backingVarRef.Variable = value;
            }
        }

        /// <summary>
        /// If this is false, this is representing a literal value.
        /// </summary>
        public virtual bool RepresentingVar => VarRef != null;

        private bool CanHoldAsVar(IVariable variable)
        {
            bool result;
            if (variable == null)
            {
                result = ContentType.IsClass;
            }
            else
            {
                 result = CanHoldAsValue(variable.BoxedValue);
            }
            return result;
        }

        protected virtual void UpdateBackingFieldsBasedOn(IVariable variable)
        {
            if (variable == null)
            {
                backingVarRef.VarOwner = null;
                backingVarRef.Variable = null;
                LegacyVarRef = null;
                return;
            }

            bool correctType = CanHoldAsVar(variable);
            if (!correctType)
            {
                string errorMessage = $"VariableData: Cannot assign variable of ContentType {variable.ContentType.Name} " +
                    $"to VariableData of ContentType {ContentType.Name}.";
                throw new InvalidCastException(errorMessage);
            }
            VarRef = variable;
        }

        public abstract string GetDescription();

        public virtual IVariableData GetCopy()
        {
            Type thisType = GetType();

            IVariableData theCopy = (IVariableData)Activator.CreateInstance(thisType);
            theCopy.SetContentsTo(this);
            return theCopy;
        }

        public virtual void Refresh()
        {
            backingVarRef.Refresh();
        }

        public virtual void SetContentsTo(IVariableData otherVarData)
        {
            if (otherVarData is VariableData otherVarDataCasted)
            {
                this.VarOwner = otherVarDataCasted.VarOwner;
            }

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
        IVariableSource VarOwner { get; set; }

    }

    public abstract class VariableData<TValue> : VariableData
    {
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
                backingVarRef.Refresh();
                if (RepresentingVar)
                {
                    return (TValue)VarRef.BoxedValue;
                }
                
                return value;
                
            }
            set
            {
                if (RepresentingVar)
                {
                    VarRef.BoxedValue = value;
                }
                else
                {
                    this.value = value;
                    VarRef = null;
                }
            }
        }

        public override object BoxedValue
        {
            get
            {
                if (RepresentingVar)
                {
                    return VarRef.BoxedValue;
                }
                else
                {
                    return value;
                }
            }
            set
            {
                object whatToAssign = null;
                bool canBeAssigned = CanHoldAsValue(value);
                if (!canBeAssigned)
                {
                    string errorMessage = $"VariableData of value type {typeof(TValue).Name} cannot hold " +
                        $"a value of type {value.GetType().Name}. Assignment aborted.";
                    throw new InvalidCastException(errorMessage);
                }

                whatToAssign = (TValue)value;

                if (RepresentingVar)
                {
                    VarRef.BoxedValue = whatToAssign;
                }
                else
                {
                    this.value = (TValue)whatToAssign;
                    VarRef = null;
                }
            }
        }

        [SerializeField] protected TValue value = default;

        public override string GetDescription()
        {
            string result = "null"; // <- This is valid for reference types

            if (!RepresentingVar && value != null)
            {
                result = value.ToString();
            }
            else if (RepresentingVar)
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
            this.VarRef = otherVarData.VarRef;
            this.value = otherVarData.value;
        }
    }

}