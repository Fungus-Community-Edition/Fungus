using System;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    // To reduce the boilerplate in IVariableData implementors such as AnimatorData and FloatData
    public abstract class VariableData : IVariableData
    {
        // ^Used to find the owner of the variable (Flowchart or VariableSourceAsset)
        [SerializeField] protected byte storedItemId = 0;
        // ^Used to find the variable within its owner
        [SerializeField] protected Flowchart owningFc;
        [SerializeField] protected VariableSourceAsset owningVsa;
        // ^We can't trust Unity's serialization when it comes to polymorphic references, so we store
        // Flowchart and VSA references separately.
        // Note that for each instance of VariableData, only one or neither of these should be set. Also,
        // the owning fc and owning vsa are for owners of the vars (not owners for this particular VariableData).

        protected virtual Variable LegacyVarRef { get; set; } // For backward compatibility

        public IVariableSource VarOwner
        {
            get
            {
                owner ??= owningFc;
                owner ??= owningVsa;
                return owner;
            }
            set
            {
                owner = value;
                owningFc = owner as Flowchart;
                owningVsa = owner as VariableSourceAsset;
            }
        }

        protected IVariableSource owner;

        public virtual Flowchart OwningFc
        {
            get => owningFc;
        }

        public virtual VariableSourceAsset OwningVsa
        {
            get => owningVsa;
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
                IVariable result = null;
                // Subclasses may have var refs for legacy stuff, and thus we need to check LegacyVarRef here
                // (despite how we try to keep it in sync with regular varRef).
                if (LegacyVarRef != null) // Let's not worry about stored ID. Remember, this is for legacy support.
                {
                    varRef = LegacyVarRef;
                }
                if (varRef != null && varRef.ItemId == storedItemId)
                {
                    result = varRef;
                }
                else if (VarOwner != null)
                {
                    // We'll need to look it up again
                    result = FindVariableBasedOnItemId();
                    UpdateBackingFieldsBasedOn(result);
                }
                return result;
            }
            set
            {
                bool alreadyAssigned = ReferenceEquals(value, varRef);
                if (alreadyAssigned)
                {
                    return;
                }
                UpdateBackingFieldsBasedOn(value);
            }
        }

        private IVariable FindVariableBasedOnItemId()
        {
            IVariable result = null;
            if (storedItemId == Variable.InvalidID)
            {
                return result;
            }
            var foundVar = VarOwner.GetVariable(storedItemId);
            bool foundValidVar = foundVar != null;
            bool foundCorrectType = foundVar != null && ContentType.IsAssignableFrom(foundVar.ContentType);
            if (foundValidVar && foundCorrectType)
            {
                varRef = foundVar;
                LegacyVarRef = foundVar as Variable;
                result = varRef;
            }
            else if (!foundCorrectType)
            {
                string errorMessage = $"VariableData: Found variable with ID {storedItemId} in Flowchart " +
                    $"{owningFc.name}, but its type ({foundVar.ContentType.Name}) is not assignable to " +
                    $"the expected type: {ContentType.Name}.";
                Debug.LogError(errorMessage);
            }
            return result;
        }

        protected virtual void UpdateBackingFieldsBasedOn(IVariable variable)
        {
            if (variable == null)
            {
                storedItemId = Variable.InvalidID;
                owningFc = null;
                owningVsa = null;
                varRef = null;
                LegacyVarRef = null;
                return;
            }

            bool correctType = ContentType.IsAssignableFrom(variable.ContentType);
            if (!correctType)
            {
                string errorMessage = $"VariableData: Cannot assign variable of ContentType {variable.ContentType.Name} " +
                    $"to VariableData of ContentType {ContentType.Name}.";
                throw new InvalidCastException(errorMessage);
            }
            storedItemId = variable.ItemId;
            VarOwner = variable.Owner;
            varRef = variable;
            LegacyVarRef = variable as Variable;
        }

        protected IVariable varRef; // This should NOT be serialized directly
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
            if (storedItemId == Variable.InvalidID || varRef != null)
            {
                return;
            }
            FindVariableBasedOnItemId();
        }

        public virtual void SetContentsTo(IVariableData otherVarData)
        {
            if (otherVarData is VariableData otherVarDataCasted)
            {
                this.VarOwner = otherVarDataCasted.VarOwner;
                this.storedItemId = otherVarDataCasted.storedItemId;
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
                // varRef should be set to the same as LegacyVarRef when appropriate, and thus we
                // don't need to check the two separately here.
                // Also, we are using the property VarRef here to make sure we sustain
                // the right reference no matter at what point this Value prop is accessed.
                bool shouldRefetchFromOwner = VarRef == null &&
                    storedItemId != Variable.InvalidID &&
                    VarOwner != null;
                if (shouldRefetchFromOwner)
                {
                    VarRef = owner.GetVariable(storedItemId);
                }

                if (VarRef != null)
                {
                    return (TValue)VarRef.BoxedValue;
                }
                
                return value;
                
            }
            set
            {
                if (VarRef != null)
                {
                    VarRef.BoxedValue = value;
                }
                else
                {
                    this.value = value;
                    storedItemId = Variable.InvalidID;
                }

            }
        }

        public override object BoxedValue
        {
            get
            {
                if (VarRef != null)
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
                try
                {
                    whatToAssign = (TValue)value;
                }
                catch
                {
                    Debug.LogWarning($"VariableData of value type {typeof(TValue).Name} could not box value " +
                        $"of type {value.GetType().Name} to type {typeof(TValue).Name}");
                }

                if (VarRef != null)
                {
                    VarRef.BoxedValue = whatToAssign;
                }
                else
                {
                    this.value = (TValue)whatToAssign;
                    storedItemId = Variable.InvalidID;
                }
            }
        }

        [SerializeField] protected TValue value = default;

        public override string GetDescription()
        {
            string result = "null"; // <- This is valid for reference types

            if (VarRef == null && value != null)
            {
                result = value.ToString();
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
            this.VarRef = otherVarData.VarRef;
            this.value = otherVarData.value;
        }


    }

}