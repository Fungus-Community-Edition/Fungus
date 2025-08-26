using System;
using UnityEngine;

namespace Amanita.VScripting
{
    
    /// <summary>
    /// Base class for a more lightweight reimplementation of Fungus Variables.
    /// </summary>
    [System.Serializable]
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

        public Muscariable() { }

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
            get { return value; }
            set
            {
                if (!CanHoldAsValue(value))
                {
                    string errorMessage = $"Variable {Key} cannot hold {value} as a value.";
                    throw new System.ArgumentException(errorMessage, "value");
                }

                this.value = value;
            }
        }

        [SerializeField]
        protected System.Object value;

        protected virtual bool CanHoldAsValue(System.Object obj)
        {
            bool result;

            if (obj == null)
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
    }

    [Serializable]
    public abstract class Muscariable<T> : Muscariable, IVariable<T>, IEquatable<T>, IEquatable<IVariable<T>>
    {
        public override Type ContentType { get { return typeof(T); } }

        public virtual new T Value
        {
            get { return valOfType; }
            set
            {
                // We call base.Value here so that when this instance is being
                // cast as a non-generic Muscariable, clients can still access the right value
                base.Value = valOfType = value;
                InvokeOnValueChanged();
            }
        }

        [SerializeField] protected T valOfType;

        protected virtual void InvokeOnValueChanged()
        {
            OnValueChanged?.Invoke(valOfType);
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
            return this.Value.Equals(otherVar.Value);
        }

    }

    public interface IVariable : IHasKey
    {
        void Init();
        new string Key { get; set; }
        object Value { get; set; }
        VariableScope Scope { get; }
        int ItemID { get; set; }

        /// <summary>
        /// The type of the value that this is meant to represent. It's like how Fungus
        /// FloatVariables represent float, Fungus StringVariables represent strings,
        /// so on so forth.
        /// </summary>
        Type ContentType { get; }
        bool IsComparisonSupported();

        /// <summary>
        /// Used by Ifs, While, and the like. Child classes required to declare and implement comparisons.
        /// </summary>
        bool Evaluate(CompareOperator compareOperator, object value);

        void Apply(SetOperator setOperator, object value);
    }

    public interface IVariable<T> : IVariable, IEquatable<T>
    {
        new T Value { get; set; }
        void Apply(SetOperator setOperator, T value);
    }

    [Serializable]
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

    [System.Serializable]
    [Muscariable("Primitive", typeof(string), "String")]
    public class StringMuscariable : Muscariable<string>
    {
        public static StringMuscariable operator +(StringMuscariable a, StringMuscariable b)
            => new StringMuscariable { Value = a.Value + b.Value };

        public static bool operator ==(StringMuscariable a, StringMuscariable b)
            => a.Value == b.Value;

        public static bool operator !=(StringMuscariable a, StringMuscariable b)
            => a.Value != b.Value;

        public override bool Equals(object obj)
        {
            var other = obj as StringMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

    }

    public interface IHasKey
    {
        string Key { get; }
    }

}