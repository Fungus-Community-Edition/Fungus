using System;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    // To reduce the boilerplate in IVariableData implementors such as AnimatorData and FloatData
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public abstract class VariableData : IVariableData, IRefreshable, ISerializationCallbackReceiver
    {
        [SerializeField]
        [FormerlySerializedAs("backingVarRef")]
        protected VariableReference _backingVarRef = new VariableReference();

        public IVariableSource VarOwner
        {
            get
            {
                return _backingVarRef.VarOwner;
            }
            set
            {
                _backingVarRef.VarOwner = value;
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
                return _backingVarRef.Variable;
            }
            set
            {
                bool alreadyAssigned = ReferenceEquals(value, _backingVarRef.Variable);
                if (alreadyAssigned)
                {
                    return;
                }

                if (value == null) // We want to treat null-assignments as switching to literal mode
                {
                    _backingVarRef.Variable = null;
                    return;
                }

                bool validType = CanHoldAsVar(value);
                if (!validType)
                {
                    string errorMessage = $"VariableData: Cannot hold {value} as a variable. I am working with a" +
                        $"ContentType of {ContentType.Name}.";
                    throw new InvalidCastException(errorMessage);
                }
                _backingVarRef.Variable = value;
            }
        }

        /// <summary>
        /// If this is false, this is representing a literal value.
        /// </summary>
        public virtual bool RepresentingVar
        {
            get
            {
                IVariable varRef = VarRef;
                if (varRef == null)
                {
                    return false;
                }

                if (varRef.ItemId == Muscariable.InvalidId)
                {
                    if (!string.IsNullOrEmpty(varRef.Key) || varRef.Owner != null)
                    {
                        Debug.LogWarning($"VariableData: Variable reference {varRef.Key} owned by {varRef.Owner} has invalid ID. Treating as literal value.");
                    }
                    return false;
                }

                if (string.IsNullOrEmpty(varRef.Key))
                {
                    return false;
                }

                return true;
            }
        }

        protected virtual bool CanHoldAsVar(IVariable variable)
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
                _backingVarRef.Variable = null;
                _backingVarRef.VarOwner = null;
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
            _backingVarRef.Refresh();
        }

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
                result = TypeUtils.TypesCompatible(ContentType, obj.GetType());
            }

            return result;
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += DoBackwardsCompatibility;
#endif
        }

        protected virtual void DoBackwardsCompatibility()
        {
            if (LegacyVarRef != null)
            {
                _backingVarRef.Variable = LegacyVarRef;
                LegacyVarRef = null;
            }
        }

        protected virtual Variable LegacyVarRef { get; set; }
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
            _value = default;
            VarRef = null;
        }

        public VariableData(TValue startVal = default)
        {
            _value = startVal;
            VarRef = null;
        }

        public override Type ContentType => typeof(TValue);

        public virtual TValue Value
        {
            get
            {
                _backingVarRef.Refresh();
                if (RepresentingVar)
                {
                    return ConvertToValue(VarRef.BoxedValue);
                }
                
                return _value;
                
            }
            set
            {
                if (RepresentingVar)
                {
                    VarRef.BoxedValue = value;
                }
                else
                {
                    this._value = value;
                    VarRef = null;
                }
            }
        }

        public override object BoxedValue
        {
            get
            {
                _backingVarRef.Refresh();
                if (RepresentingVar)
                {
                    return VarRef.BoxedValue;
                }
                else
                {
                    return _value;
                }
            }
            set
            {
                bool canBeAssigned = CanHoldAsValue(value);
                if (!canBeAssigned)
                {
                    string errorMessage = $"VariableData of value type {typeof(TValue).Name} cannot hold " +
                        $"a value of type {value.GetType().Name}. Assignment aborted.";
                    throw new InvalidCastException(errorMessage);
                }

                TValue whatToAssign = ConvertToValue(value);

                if (RepresentingVar)
                {
                    VarRef.BoxedValue = whatToAssign;
                }
                else
                {
                    this._value = whatToAssign;
                    VarRef = null;
                }
            }
        }

        protected virtual TValue ConvertToValue(object value)
        {
            if (ReferenceEquals(value, null))
            {
                return default;
            }

            if (value is TValue typedValue)
            {
                return typedValue;
            }

            var targetType = typeof(TValue);
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying.IsEnum)
            {
                if (value is string enumString)
                {
                    return (TValue)Enum.Parse(underlying, enumString);
                }

                object enumValue = Enum.ToObject(underlying, value);
                return (TValue)enumValue;
            }

            if (value is IConvertible)
            {
                object changedValue = Convert.ChangeType(value, underlying);
                return (TValue)changedValue;
            }

            return (TValue)value;
        }

        public virtual TValue LiteralValue
        {
            get
            {
                return _value;
            }
            set
            {
                this._value = value;
            }
        }
        [SerializeField]
        [FormerlySerializedAs("value")]
        protected TValue _value = default;

        public override string GetDescription()
        {
            string result = "null"; // <- This is valid for reference types

            if (!RepresentingVar && _value != null)
            {
                result = _value.ToString();
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
            this._value = otherVarData._value;
        }

        public override string ToString()
        {
            if (BoxedValue == null)
            {
                return $"valueless {this.GetType().Name}";
            }
            else
            {
                return BoxedValue.ToString();
            }
        }

        protected override void DoBackwardsCompatibility()
        {
            base.DoBackwardsCompatibility();

            if (!ShouldMigrateLegacyLiteral())
            {
                return;
            }

            LiteralValue = LegacyLiteralVal;
            LegacyLiteralVal = default;
        }

        private bool ShouldMigrateLegacyLiteral()
        {
            if (LegacyLiteralVal == null)
            {
                return false;
            }
            bool sameAsDefault = LegacyLiteralVal.Equals(default(TValue));
            return !sameAsDefault;
        }

        protected virtual TValue LegacyLiteralVal { get; set; }
    }

}