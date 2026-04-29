using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Abstract base class for variables.
    /// </summary>
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    [RequireComponent(typeof(Flowchart))]
    [Serializable]
    [ExecuteInEditMode]
    public abstract class Variable : MonoBehaviour, IVariable
    {
        [SerializeField]
        [FormerlySerializedAs("scope")]
        protected VariableScope _scope;

        [SerializeField]
        [FormerlySerializedAs("key")]
        protected string _key = "";

        [HideInInspector]
        [SerializeField]
        [FormerlySerializedAs("itemID")]
        private byte _itemId = InvalidID;

        [HideInInspector]
        [FormerlySerializedAs("itemID")]
        [SerializeField] private int oldItemID = 0;

        public static readonly byte InvalidID = 0;

        public virtual bool IsScalar() => false;

        // Non-global variables each belong to a particular Flowchart. Thus, rather
        // than a unique string ID, it's best for them to get an int that their
        // Flowcharts assign them.
        public byte ItemId
        {
            get => _itemId;
            set => _itemId = value;
        }

        #region Public members

        public virtual void Init()
        {
            Init(GetValue());
        }

        public abstract void Init(System.Object startValue);

        /// <summary>
        /// Visibility scope for the variable.
        /// </summary>
        public virtual VariableScope Scope { get { return _scope; } set { _scope = value; } }

        /// <summary>
        /// String identifier for the variable.
        /// </summary>
        public virtual string Key
        {
            get { return _key; } 
            set
            {
                _key = value;
            }
        }

        /// <summary>
        /// Callback to reset the variable if the Flowchart is reset.
        /// </summary>
        public abstract void OnReset();

        /// <summary>
        /// Used by SetVariable, child classes required to declare and implement operators.
        /// </summary>
        /// <param name="setOperator"></param>
        /// <param name="value"></param>
        public abstract void Apply(SetOperator setOperator, object value);

        /// <summary>
        /// Used by Ifs, While, and the like. Child classes required to declare and implement comparisons.
        /// </summary>
        /// <param name="compareOperator"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool Evaluate(CompareOperator compareOperator, object value);

        /// <summary>
        /// Does the underlying type provide support for +-*/
        /// </summary>
        public virtual bool IsArithmeticSupported(SetOperator setOperator) { return false; }

        /// <summary>
        /// Does the underlying type provide support for < <= > >=
        /// </summary>
        public virtual bool IsComparisonSupported() { return false; }
        
        /// <summary>
        /// Boxed or referenced value of type defined within inherited types.
        /// Not recommended for direct use, primarily intended for use in editor code.
        /// </summary>
        public abstract object GetValue();


        public abstract Type ContentType { get; }

        public virtual object Value
        {
            get { return baseVal; }
            set
            {
                object prevValue = baseVal;
                object filtered = FilteredForValueSet(value);
                baseVal = filtered;
                OnBaseValueSet(prevValue);
            }
        }

        protected object baseVal;

        protected virtual object FilteredForValueSet(object valueToConvert)
        {
            return valueToConvert;
        }

        protected virtual void OnBaseValueSet(object prevValue)
        {

        }

        /// <summary>
        /// Set value in inherited types via Boxed value.
        /// Not recommended for direct use, primarily intended for use in editor code.
        /// </summary>
        public abstract void SetValue(object value);

        //we are required to be on a flowchart so we provide this as a helper
        public virtual Flowchart GetFlowchart()
        {
            return GetComponent<Flowchart>();
        }
        #endregion

        public abstract object BoxedValue { get; set; }

        public virtual IVariableSource Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                string errorMessage = $"Cannot set the owner of a legacy variable";
                Debug.LogError(errorMessage);
            }
        }

        public virtual bool IsRelationalSupported => false;

        protected virtual void OnValidate()
        {
            _owner ??= GetComponent<Flowchart>();
        }

        [SerializeField] [HideInInspector] protected Flowchart _owner;

        protected virtual void OnEnable()
        {
            // Backwards compatibility: migrate old int ItemID to uint
            if (oldItemID != InvalidID)
            {
                _itemId = (byte)oldItemID;
                oldItemID = 0;
            }

        }

        protected virtual void Awake()
        {
            _owner ??= GetComponent<Flowchart>();
        }

    }

    /// <summary>
    /// Generic concrete base class for variables.
    /// </summary>
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public abstract class VariableBase<T> : Variable, IVariable<T>
    {
        public override Type ContentType => typeof(T);

        [SerializeField]
        [FormerlySerializedAs("value")]
        protected T _value;

        private bool ShouldBlockValueAssignment()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return EditorUtility.IsPersistent(this) || PrefabUtility.IsPartOfPrefabAsset(this);
            }
#endif
            return false;
        }

        // Explicit IVariable implementation for object-typed access
        object IVariable.BoxedValue
        {
            get => _value; // boxes T correctly (works for structs like Vector2)
            set
            {
                if (ShouldBlockValueAssignment())
                {
                    return;
                }

                object prevValue = _value;
                if (value == null)
                {
                    this._value = default;
                    VariableSignals.PostValueChange(this, prevValue);
                    return;
                }

                Type valueType = value.GetType();
                if (TypeUtils.TypesCompatible(ContentType, valueType))
                {
                    this._value = ConvertTo(value);
                    VariableSignals.PostValueChange(this, prevValue);
                    return;
                }

                throw new InvalidCastException($"Cannot assign value of type {value?.GetType().Name ?? "null"} " +
                    $"to {GetType().Name}.");
            }
        }

        public override object BoxedValue
        {
            get => _value;
            set
            {
                if (ShouldBlockValueAssignment())
                {
                    return;
                }

                if (value == null)
                {
                    this._value = default;
                    return;
                }

                Type valueType = value.GetType();
                if (TypeUtils.TypesCompatible(ContentType, valueType))
                {
                    this._value = ConvertTo(value);
                    return;
                }

                throw new InvalidCastException($"Cannot assign value of type {value?.GetType().Name ?? "null"} to {typeof(T).Name}.");
            }
        }

        // Preserve the typed Value required by IVariable<T>
        public virtual new T Value
        {
            get
            {
                return this._value;
            }
            set
            {
                if (ShouldBlockValueAssignment())
                {
                    return;
                }

                object prevValue = this._value;
                this._value = value;
                baseVal = value;
                VariableSignals.PostValueChange(this, prevValue);
            }
        }

        public virtual void Init(T startValue)
        {
            this.startValue = startValue;
            this._value = startValue;
            baseVal = startValue;
        }

        protected override void OnBaseValueSet(object prevValue)
        {
            base.OnBaseValueSet(prevValue);
            // Use a safe conversion path instead of direct unboxing cast to handle legacy boxed numerics (e.g. boxed double -> float)
            this._value = ConvertTo(baseVal);
        }

        public override object GetValue()
        {
            return _value;
        }

        public override void SetValue(object value)
        {
            // Use conversion helper so callers setting with boxed primitives (double) can be converted to T (float) safely.
            this._value = ConvertTo(value);
        }

        protected T startValue;

        public override void OnReset()
        {
            Value = startValue;
        }
        
        public override string ToString()
        {
            if (Value != null)
                return Value.ToString();
            else
                return "Null";
        }
        
        public override void Init(object startValue)
        {
            if (initted)
            {
                return;
            }

            try
            {
                if (startValue == null && Value == null)
                {
                    return;
                }

                baseVal = default(T);
                // Use conversion helper to handle boxed numbers that don't match T's exact CLR type.
                Init(ConvertTo(startValue));
                initted = true;
            }
            catch (System.Exception ex)
            {
                string errorMessage = $"Cannot initialize {nameof(T)} variable {this._key} with {startValue}";
                throw new System.ArgumentException(errorMessage, ex);
            }
        }

        protected bool initted = false;

        //Apply to get from base system.object to T
        public override void Apply(SetOperator op, object value)
        {
            if(value is T || value == null)
            {
                Apply(op, (T)value);
            }
            else if(value is VariableBase<T>)
            {
                var vbg = value as VariableBase<T>;
                Apply(op, vbg.Value);
            }
            else
            {
                Debug.LogError("Cannot do Apply on variable, as object type: " + value.GetType().Name + " is incompatible with " + typeof(T).Name);
            }
        }

        public virtual void Apply(SetOperator setOperator, T value)
        {
            switch (setOperator)
            {
            case SetOperator.Assign:
                Value = value;
                break;
            default:
                Debug.LogError("The " + setOperator.ToString() + " set operator is not valid.");
                break;
            }
        }

        // Apply to get from base system.object to T
        public override bool Evaluate(CompareOperator op, object value)
        {
            if (value is T || value == null)
            {
                return Evaluate(op, (T)value);
            }
            else if (value is VariableBase<T>)
            {
                var vbg = value as VariableBase<T>;
                return Evaluate(op, vbg.Value);
            }
            else
            {
                Debug.LogError("Cannot do Evaluate on variable, as object type: " + value.GetType().Name + " is incompatible with " + typeof(T).Name);
            }

            return false;
        }

        public virtual bool Evaluate(CompareOperator compareOperator, T value)
        {
            bool condition = false;

            switch (compareOperator)
            {
            case CompareOperator.Equals:
                condition = Equals(Value, value);// Value.Equals(value);
                break;
            case CompareOperator.NotEquals:
                condition = !Equals(Value, value);
                break;
            default:
                Debug.LogError("The " + compareOperator.ToString() + " comparison operator is not valid.");
                break;
            }

            return condition;
        }

        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return setOperator == SetOperator.Assign || base.IsArithmeticSupported(setOperator);
        }

        public bool Equals(T other)
        {
            bool result = false;
            if (_value == null)
            {
                if (other == null)
                {
                    result = true;
                }
            }
            else
            {
                result = _value.Equals(other);
            }
            return result;
        }

        // Helper: attempts safe conversion from boxed object to T.
        // Covers common boxed numeric -> numeric conversions (e.g. boxed double -> float).
        private static T ConvertTo(object src)
        {
            if (src == null)
            {
                return default;
            }

            // If already the right type, just return it.
            if (src is T direct)
            {
                return direct;
            }

            var targetType = typeof(T);

            try
            {
                // Handle nullable<> by converting to underlying type then wrapping
                var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

                // Enum handling
                if (underlying.IsEnum)
                {
                    if (src is string s)
                    {
                        return (T)Enum.Parse(underlying, s);
                    }
                    return (T)Enum.ToObject(underlying, src);
                }

                // If source implements IConvertible, use Convert.ChangeType
                if (src is IConvertible)
                {
                    object changed = System.Convert.ChangeType(src, underlying);
                    return (T)changed;
                }

                // Last resort: try direct cast (will throw if incompatible)
                return (T)src;
            }
            catch (Exception)
            {
                // Preserve original exception behavior for callers expecting a cast failure.
                throw;
            }
        }
        
    }
}
