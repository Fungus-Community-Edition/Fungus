using System;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Base class for a more lightweight reimplementation of Fungus Variables.
    /// </summary>
    [Serializable]
    public abstract class Muscariable : IVariable
    {
        [SerializeField] protected VariableScope scope = VariableScope.Private;
        [SerializeField] protected string key = string.Empty;
        [HideInInspector]
        [SerializeField] protected int itemID = InvalidID;

        public static readonly int InvalidID = 0;

        public virtual VariableScope Scope
        {
            get => scope;
            set => scope = value;
        }

        public virtual string Key
        {
            get => key;
            set => key = value;
        }

        public virtual int ItemID
        {
            get => itemID;
            set => itemID = value;
        }

        public Muscariable() : base() { }

        public Muscariable (IVariable otherVar)
        {
            key = otherVar.Key;
            scope = otherVar.Scope;
            itemID = otherVar.ItemID;
            value = otherVar.Value;
        }

        public Muscariable(string key, int itemID, VariableScope scope)
        {
            this.key = key;
            this.itemID = itemID;
            this.scope = scope;
        }

        public virtual System.Type ContentType => typeof(Type);
        // ^So clients can see the type even through this non-generic interface

        public virtual System.Object Value
        {
            get { return this.value; }
            set
            {
                bool sameAsAssignedVal = !ReferenceEquals(value, null) && value.Equals(this.value); 
                // ^For some reason, == won't work here
                if (sameAsAssignedVal) 
                {
                    return;
                }
                if (!CanHoldAsValue(value))
                {
                    string errorMessage = $"Variable {Key} cannot hold {value} as a value.";
                    throw new System.ArgumentException(errorMessage, "value");
                }

                object prevValue = this.value;
                object filtered = FilterForValueSet(value);
                this.value = filtered;
                OnBaseValueSet(prevValue);
            }
        }

        protected System.Object value;

        protected virtual object FilterForValueSet(object valueToConvert)
        {
            return valueToConvert;
        }

        protected virtual bool CanHoldAsValue(System.Object obj)
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

        public virtual void OnReset()
        {

        }

        public virtual void Init()
        {
            string errorMessage = string.Empty;
            if (string.IsNullOrEmpty(Key))
            {
                errorMessage += "Variable needs a valid key before Init. ";
            }

            if (itemID == InvalidID)
            {
                errorMessage += "Variable needs a valid ID before Init.";
            }

            // For unique IDs, we'll let client code worry about that.

            if (errorMessage.Length > 0)
            {
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Used by SetVariable. Child classes required to declare and implement operators.
        /// </summary>
        public virtual void Apply(SetOperator setOperator, object toApply)
        {
            value = toApply;
        }

        /// <summary>
        /// Used by Ifs, While, and the like. Child classes required to declare and implement comparisons.
        /// </summary>
        public abstract bool Evaluate(CompareOperator compareOperator, object toCompareTo);

        /// <summary>
        /// Does the underlying type provide support for +-*/
        /// </summary>
        public virtual bool IsArithmeticSupported { get; } = false;

        /// <summary>
        /// Does the underlying type provide support for < <= > >=
        /// </summary>
        public virtual bool IsRelationalSupported { get; } = false;

        // Unlike the orig implementation, we are NOT required to be on Flowcharts. But we
        // have this in case client (especially editor) code cares about whether we are or not
        public virtual Flowchart ParentFlowchart { get; set; }

        public virtual bool IsComparisonSupported() => false;

        /// <summary>
        /// A callback for right after the base value is set. The previous value,
        /// as it sounds, is the value the base had right before being set
        /// to the new one.
        /// </summary>
        protected virtual void OnBaseValueSet(object previousValue)
        {

        }

        /// <summary>
        /// When you expect the value to be a value type (as opposed to a ref type), use this rather than 
        /// directly casting to that specific value type. One quirk of C# is that when casting a
        /// System.Object, it only works if said System.Object is of the type you're casting to.
        /// </summary>
        public TVal GetValueAs<TVal>()
        {
            object val = Value;
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
            set { _owner = value; }
        }
        [SerializeField] protected IVariableSource _owner;
    }

    [Serializable]
    public abstract class Muscariable<T> : Muscariable, IVariable<T>, IEquatable<T>, IEquatable<IVariable<T>>
    {
        [SerializeField] protected new T value;

        // We have these constructors to make sure that the base value starts out synced 
        // with the strongly typed one
        public Muscariable() : base()
        {
            value = default;
            base.value = value;
        }

        public Muscariable(T startVal) : this()
        {
            value = startVal;
            base.value = startVal;
        }

        public static implicit operator T(Muscariable<T> genericMuscari)
        {
            return genericMuscari.Value;
        }

        public override Type ContentType { get { return typeof(T); } }

        public virtual new T Value
        {
            get { return value; }
            set
            {
                if (value != null && value.Equals(this.value))
                {
                    return;
                }

                // We call base.Value here so that when this instance is being
                // cast as a non-generic Muscariable, clients can still access the right value
                T prev = this.value;
                base.Value = value;
                OnGenericValueSet(prev);
                InvokeOnValueChanged();
            }
        }

        protected virtual void InvokeOnValueChanged()
        {
            OnValueChanged?.Invoke(value);
        }

        public event Action<T> OnValueChanged = delegate { };

        public override void Apply(SetOperator setOperator, object toApply)
        {
            if (!this.CanHoldAsValue(toApply))
            {
                string errorMessage = $"Cannot apply {toApply} to {ContentType.Name} variable {Key}.";
                throw new System.Exception(errorMessage);
            }

            Apply(setOperator, (T)toApply);
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
                    throw new System.ArgumentException(errorMessage);
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

        protected override void OnBaseValueSet(object previousValue)
        {
            // We don't care about the prev val here. We're just making sure that
            // the generic field stays in sync with the base field when appropriate.
            // Say, when this instance's Value property is set through a base class.
            value = (T)base.value;
        }

        protected virtual void OnGenericValueSet(T previousValue)
        {

        }

    }

    [Serializable]
    [VariableInfo("NoShow", "", typeof(object), showInMenu: false)]
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
            var other = obj as GenericMuscariable;
            if (other is null) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

    }

    

}