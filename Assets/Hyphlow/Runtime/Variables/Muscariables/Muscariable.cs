using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Base class for a more lightweight reimplementation of Fungus Variables.
    /// </summary>
    [Serializable]
    [MovedFrom(true, 
        "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core", "Muscariable")]
    public abstract class Muscariable : IVariable, IEquatable<Muscariable>, ISerializationCallbackReceiver
    {
        [SerializeField]
        protected VariableScope _scope = VariableScope.Private;
        [SerializeField]
        protected string _key = string.Empty;
        [HideInInspector]
        [SerializeField] protected byte _itemId = InvalidId; 
        // ^Default to invalid ID to avoid accidental collisions with valid variables. See VariableDataCache for more.

        public static readonly byte InvalidId = 0;

        #region Legacy stuff
        [SerializeField]
        protected VariableScope scope = VariableScope.Private;
        [SerializeField]
        protected string key = string.Empty;
        [HideInInspector]
        [SerializeField] protected byte itemID = InvalidId;

        #endregion

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            ApplyLegacyDataOnAfterDeserialize();
        }

        protected virtual void ApplyLegacyDataOnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(key))
            {
                _key = key;
            }

            if (itemID != InvalidId)
            {
                _itemId = itemID;
            }

            if (scope != default)
            {
                _scope = scope;
            }

            key = string.Empty;
            itemID = InvalidId;
            scope = default;
        }

        public virtual VariableScope Scope
        {
            get => _scope;
            set => _scope = value;
        }

        public virtual string Key
        {
            get => _key;
            set => _key = value;
        }

        public virtual byte ItemId
        {
            get => _itemId;
            set => _itemId = value;
        }

        public Muscariable() : base() { }

        // We want to check for semantic equality mainly
        public static bool operator == (Muscariable left, Muscariable right)
        {
            if (ReferenceEquals(left, right)) return true; // In case both are null or same ref
            bool sameValue = !ReferenceEquals(left, null) && left.Equals(right);
            return sameValue;
        }

        public static bool operator != (Muscariable left, Muscariable right)
        {
            if (ReferenceEquals(left, right)) return false; // In case both are null or same ref
            bool sameValue = !ReferenceEquals(left, null) && left.Equals(right);
            return !sameValue;
        }

        public override bool Equals(object obj)
        {
            if (obj is Muscariable other)
            {
                return Equals(other);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified Muscariable is (semantically) equal to the current Muscariable.
        /// </summary>
        public virtual bool Equals(Muscariable other)
        {
            bool result = other != null && this.BoxedValue?.Equals(other.BoxedValue) == true;
            return result;
        }

        public Muscariable (IVariable otherVar)
        {
            _key = otherVar.Key;
            _scope = otherVar.Scope;
            _itemId = otherVar.ItemId;
            BoxedValue = otherVar.BoxedValue;
        }

        public Muscariable(string key, byte itemID, VariableScope scope)
        {
            this._key = key;
            this._itemId = itemID;
            this._scope = scope;
        }

        public abstract Type ContentType { get; }
        // ^So clients can see the type even through this non-generic interface

        public abstract object BoxedValue
        {
            get;
            set;
        }

        protected virtual object FilterForValueSet(object valueToConvert)
        {
            return valueToConvert;
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
                result = TypeUtils.TypesCompatible(obj.GetType(), ContentType);
            }

            return result;
        }

        public virtual void Init(object startValue = default)
        {
            _startValue = startValue;
        }

        private object _startValue;

        public virtual void OnReset()
        {
            // Optional override by child classes
        }

        /// <summary>
        /// Used by SetVariable. Child classes required to declare and implement operators.
        /// </summary>
        public abstract void Apply(SetOperator setOperator, object toApply);

        /// <summary>
        /// Used by Ifs, While, and the like. Child classes required to declare and implement comparisons.
        /// </summary>
        public abstract bool Evaluate(CompareOperator compareOperator, object toCompareTo);

        /// <summary>
        /// Does the underlying type provide support for +-*/
        /// </summary>
        public virtual bool IsArithmeticSupported(SetOperator setOperator)
        {
            bool result = setOperator == SetOperator.Assign;
            return result;
        }

        /// <summary>
        /// Does the underlying type provide support for < <= > >=
        /// </summary>
        public virtual bool IsRelationalSupported { get; } = false;

        // Unlike the orig implementation, we are NOT required to be on Flowcharts. But we
        // have this in case client (especially editor) code cares about whether we are or not
        public virtual Flowchart ParentFlowchart { get; set; }

        public virtual bool IsComparisonSupported() => false;

        /// <summary>
        /// When you expect the value to be a value type (as opposed to a ref type), use this rather than 
        /// directly casting to that specific value type. One quirk of C# is that when casting a
        /// object, it only works if said object is of the type you're casting to.
        /// </summary>
        public TVal GetValueAs<TVal>()
        {
            object val = BoxedValue;
            if (val == null)
            {
                return default;
            }

            var targetType = typeof(TVal);
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // If already the right runtime type
            if (underlying.IsInstanceOfType(val))
            {
                return (TVal)val;
            }

            // Enums
            if (underlying.IsEnum)
            {
                if (val is string enumStr)
                {
                    return (TVal)Enum.Parse(underlying, enumStr);
                }
                return (TVal)Enum.ToObject(underlying, val);
            }

            // Use IConvertible / Convert.ChangeType for primitives
            if (val is IConvertible)
            {
                object changed = Convert.ChangeType(val, underlying);
                return (TVal)changed;
            }

            // Last resort - try direct cast (may throw)
            return (TVal)val;
        }

        public virtual IVariableSource Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
            }
        }
        protected IVariableSource _owner;

        public abstract Muscariable Clone();

        protected virtual void TriggerOnValueChanged()
        {
            OnValueChanged.Invoke(this);
        }
        public event Action<Muscariable> OnValueChanged = delegate { };

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string result = $"{this.GetType().Name} w/ val: {BoxedValue})";
            return result;
        }

    }

    [Serializable]
    [MovedFrom(true, 
        "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core")]
    public abstract class Muscariable<T> : Muscariable, IVariable<T>, IEquatable<T>, IEquatable<IVariable<T>>
    {
        [SerializeField]
        protected T value;

        [SerializeField]
        protected T _startValue;

        [SerializeField]
        protected T _value;

        [SerializeField]
        protected T startValue;

        protected override void ApplyLegacyDataOnAfterDeserialize()
        {
            base.ApplyLegacyDataOnAfterDeserialize();

            bool origValueIsDefault = EqualityComparer<T>.Default.Equals(value, default) || value == null;
            bool currentValueIsDefault = EqualityComparer<T>.Default.Equals(_value, default) || _value == null;
            // ^The == null is to account for fake Unity nulls
            if (!origValueIsDefault && currentValueIsDefault)
            {
                _value = value;
            }

            bool origStartValueIsDefault = EqualityComparer<T>.Default.Equals(startValue, default) || startValue == null;
            bool currentStartValueIsDefault = EqualityComparer<T>.Default.Equals(_startValue, default) || _startValue == null;
            if (!origStartValueIsDefault && currentStartValueIsDefault)
            {
                _startValue = startValue;
            }

            value = startValue = default;
        }

        // We have these constructors to make sure that the base value starts out synced 
        // with the strongly typed one
        public Muscariable() : base()
        {
            _value = _startValue = default;
        }

        public Muscariable(T startVal) : this()
        {
            _value = _startValue = startVal;
        }

        public virtual void Init(T startValue = default)
        {
            this._startValue = startValue; 
            this.Value = startValue;
        }

        public static implicit operator T(Muscariable<T> genericMuscari)
        {
            return genericMuscari.Value;
        }

        public override Type ContentType { get { return typeof(T); } }

        public virtual T Value
        {
            get { return _value; }
            set
            {
                bool sameVal = value != null && value.Equals(this._value);
                if (sameVal)
                {
                    return;
                }

                this._value = (T)value; 
                // ^Need to cast here for the sake of numeric types. Can't do an "as" cast with those.
                TriggerOnValueChanged();
            }
        }

        public override object BoxedValue
        {
            get { return _value; }
            set
            {
                if (!this.CanHoldAsValue(value))
                {
                    string errorMessage = $"Cannot set {ContentType.Name} variable {Key} to value " +
                        $"of type {value.GetType().Name}.";
                    throw new ArgumentException(errorMessage);
                }
                object filteredValue = this.FilterForValueSet(value);
                this._value = ConvertToValue(filteredValue);
                TriggerOnValueChanged();
            }
        }

        protected virtual T ConvertToValue(object value)
        {
            if (ReferenceEquals(value, null))
            {
                return default;
            }

            if (value is T typedValue)
            {
                return typedValue;
            }

            var targetType = typeof(T);
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying.IsEnum)
            {
                if (value is string enumString)
                {
                    return (T)Enum.Parse(underlying, enumString);
                }

                object enumValue = Enum.ToObject(underlying, value);
                return (T)enumValue;
            }

            if (value is IConvertible)
            {
                object changedValue = Convert.ChangeType(value, underlying);
                return (T)changedValue;
            }

            return (T)value;
        }

        public override void Apply(SetOperator setOperator, object toApply)
        {
            if (!this.CanHoldAsValue(toApply))
            {
                string errorMessage = $"Cannot apply {toApply} to {ContentType.Name} variable {Key}.";
                throw new Exception(errorMessage);
            }

            Apply(setOperator, ConvertToValue(toApply));
        }

        public virtual void Apply(SetOperator setOperator, T toApply)
        {
            switch (setOperator)
            {
                case SetOperator.Assign:
                    this.Value = toApply;
                    break;
                default:
                    Debug.LogError($"The {setOperator} set operator is not valid for {ContentType.Name} variable {Key}.");
                    break;
            }

        }

        public override bool Evaluate(CompareOperator op, object value)
        {
            bool result = false;
            if (value is T || value == null)
            {
                result = Evaluate(op, (T)value);
            }
            else if (value is Muscariable<T> varOfType)
            {
                result = Evaluate(op, varOfType.Value);
            }
            else
            {
                Debug.LogError("Cannot do Evaluate on variable, as object type: " + value.GetType().Name + " is incompatible with " + typeof(T).Name);
            }

            return result;
        }

        public virtual bool Evaluate(CompareOperator op, T toCompareTo)
        {
            bool result;
            switch (op)
            {
                case CompareOperator.Equals:
                    result = this.Value.Equals(toCompareTo); break;
                case CompareOperator.NotEquals:
                    result = !this.Value.Equals(toCompareTo); break;
                default:
                    string errorMessage = $"Muscariable<{typeof(T).Name}> {Key} not compatible with CompareOperator {op}";
                    throw new ArgumentException(errorMessage);
            }

            return result;
        }

        public virtual bool Equals(T other)
        {
            return this.Value.Equals(other);
        }

        public virtual bool Equals(IVariable<T> otherVar)
        {
            return ValEquals(otherVar) && this.Key == otherVar.Key;
        }

        public virtual bool ValEquals(T other)
        {
            return this.Value.Equals(other);
        }

        public virtual bool ValEquals(IVariable<T> otherVar)
        {
            return otherVar != null && this.Value.Equals(otherVar.Value);
        }

        public override Muscariable Clone()
        {
            Muscariable result = VariableFactory.CreateByContentType(typeof(T), this);
            return result;
        }

        public override void OnReset()
        {
            _value = _startValue;
            TriggerOnValueChanged();
        }

    }

    [Serializable]
    [VariableInfo("NoShow", 
        "", 
        typeof(object), 
        showInMenu: false)]
    public class GenericMuscariable : Muscariable<object>
    {
        // Keep defaults: Assign supported; Equals/NotEquals from base are fine.
        // You can extend later for numeric T to support + - * / or relational ops.

        public static bool operator ==(GenericMuscariable a, GenericMuscariable b)
            => a.Value == b.Value;

        public static bool operator !=(GenericMuscariable a, GenericMuscariable b)
            => a.Value != b.Value;

        public override bool Equals(object obj)
        {
            if (obj is not GenericMuscariable other) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }


    }

}