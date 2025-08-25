using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Scope types for Variables.
    /// </summary>
    public enum VariableScope
    {
        /// <summary> Can only be accessed by commands in the same Flowchart. </summary>
        Private,
        /// <summary> Can be accessed from any command in any Flowchart. </summary>
        Public,
        /// <summary> Creates and/or references a global variable of that name, all variables of this name and scope share the same underlying fungus variable and exist for the duration of the instance of Unity.</summary>
        Global,
    }

    /// <summary>
    /// Abstract base class for variables.
    /// </summary>
    [RequireComponent(typeof(Flowchart))]
    [System.Serializable]
    public abstract class Variable : MonoBehaviour, IVariable
    {
        [SerializeField] protected VariableScope scope;

        [SerializeField] protected string key = "";

        [HideInInspector]
        [SerializeField] private int itemID = InvalidID;

        public static readonly int InvalidID = -1;

        // Non-global variables each belong to a particular Flowchart. Thus, rather
        // than a unique string ID, it's best for them to get an int that their
        // Flowcharts assign them.
        public int ItemID
        {
            get => itemID;
            set => itemID = value;
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
        public virtual VariableScope Scope { get { return scope; } set { scope = value; } }

        /// <summary>
        /// String identifier for the variable.
        /// </summary>
        public virtual string Key { get { return key; } set { key = value; } }

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


        public abstract System.Type ContentType { get; }

        public virtual object Value
        {
            get { return baseVal; }
            set { baseVal = value; }
        }

        protected object baseVal;
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
    }

    /// <summary>
    /// Generic concrete base class for variables.
    /// </summary>
    public abstract class VariableBase<T> : Variable, IVariable<T>
    {

        //caching mechanism for global static variables
        private VariableBase<T> _globalStaicRef;
        private VariableBase<T> globalStaicRef
        {
            get
            {

                if (_globalStaicRef != null)
                {
                    return _globalStaicRef;
                }
                else if (Application.isPlaying && AmanitaManager.S != null)
                {
                    return _globalStaicRef = AmanitaManager.S.GlobalVariables.GetOrAddVariable(Key, value, this.GetType());
                }
                else
                {
                    return null;
                }
            }
        }

        public override System.Type ContentType => typeof(T);

        [SerializeField] protected T value;
        public virtual new T Value
        {
            get
            {
                return this.value;
                //if (scope != VariableScope.Global || !Application.isPlaying)
                //{
                //    return this.value;
                //}
                //else
                //{ 
                //    return globalStaicRef.value;
                //}
            }
            set
            {
                if (scope != VariableScope.Global || !Application.isPlaying)
                {
                    this.value = value;
                    baseVal = value;
                }
                else
                {
                    globalStaicRef.Value = value;
                }
            }
        }

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            this.value = (T)value;
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

                Init((T)startValue);
                initted = true;
            }
            catch (System.Exception ex)
            {
                string errorMessage = $"Cannot initialize {nameof(T)} variable {this.key} with {startValue}";
                throw new System.ArgumentException(errorMessage, ex);
            }
        }

        protected bool initted = false;

        protected virtual void Init(T startVal)
        {
            this.startValue = startVal;
            base.Value = startValue;
        }

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

        //Apply to get from base system.object to T
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
            if (value == null)
            {
                if (other == null)
                {
                    result = true;
                }
            }
            else
            {
                result = value.Equals(other);
            }
            return result;
        }
    }
}
