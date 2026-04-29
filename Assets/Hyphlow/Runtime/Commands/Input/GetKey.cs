using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Store Input.GetKey in a variable. Supports an optional Negative key input. A negative 
    /// value will be overridden by a positive one, they do not add.
    /// </summary>
    [CommandInfo("Input",
                 "GetKey",
                 "Store Input.GetKey in a variable. Supports an optional Negative key input. A " +
                "negative value will be overridden by a positive one, they do not add.")]
    [AddComponentMenu("")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class GetKey : Command
    {
        [SerializeField]
        [FormerlySerializedAs("keyCode")]
        protected KeyCode _keyCode = KeyCode.None;

        [Tooltip("Optional, secondary or negative keycode. For booleans will also set to true, " +
            "for int and float will set to -1.")]
        [SerializeField]
        [FormerlySerializedAs("keyCodeNegative")]
        protected KeyCode _keyCodeNegative = KeyCode.None;

        [SerializeField]
        [FormerlySerializedAs("keyCodeName")]
        [Tooltip("Only used if KeyCode is KeyCode.None, expects a name of the key to use.")]
        protected StringData _keyCodeName = new StringData(string.Empty);

        [SerializeField]
        [FormerlySerializedAs("keyCodeNameNegative")]
        [Tooltip("Optional, secondary or negative keycode. For booleans will also set to true, " +
            "for int and float will set to -1. Only used if KeyCode is KeyCode.None, expects " +
            "a name of the key to use.")]
        protected StringData _keyCodeNameNegative = new StringData(string.Empty);

        public enum InputKeyQueryType
        {
            Down,
            Up,
            State
        }

        [Tooltip("Do we want an Input.GetKeyDown, GetKeyUp or GetKey")]
        [FormerlySerializedAs("keyQueryType")]
        [SerializeField]
        protected InputKeyQueryType keyQueryType = InputKeyQueryType.State;

        [Tooltip("Will store true or false or 0 or 1 depending on type. Sets true or -1 for negative key values.")]
        [SerializeField]
        [ContentTypeConstraint(typeof(int), typeof(float), typeof(bool))]
        protected VariableReference _outValue = new VariableReference();

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_keyCodeName);
            _variableDataCache.Add(_keyCodeNameNegative);
        }

        public override void OnEnter()
        {
            if (_outValue.Variable == null)
            {
                Debug.LogError($"GetKey Command on Flowchart {this.name}, Block {ParentBlock.BlockName} at " +
                    $"index {CommandIndex} has no variable set to store the result.", this);
                Continue();
                return;
            }

            int valToSet = 0;

            if (_keyCodeNegative != KeyCode.None)
            {
                DoKeyCode(_keyCodeNegative, -1, ref valToSet);
            }
            else if (!string.IsNullOrEmpty(_keyCodeNameNegative))
            {
                DoKeyName(_keyCodeNameNegative, -1, ref valToSet);
            }

            if (_keyCode != KeyCode.None)
            {
                DoKeyCode(_keyCode, 1, ref valToSet);
            }
            else if (!string.IsNullOrEmpty(_keyCodeName))
            {
                DoKeyName(_keyCodeName, 1, ref valToSet);
            }

            _outValue.SetValue(valToSet);
            Debug.Log($"Filling out value {valToSet} in variable {_outValue.VarKey} for GetKey command on " +
                $"Flowchart {this.name}, Block {ParentBlock.BlockName} at index " +
                $"{CommandIndex}. The out value is: {_outValue.Variable.BoxedValue}", this);

            Continue();
        }

        private void DoKeyCode(KeyCode key, int trueVal, ref int valToSet)
        {
            switch (keyQueryType)
            {
                case InputKeyQueryType.Down:
                    if (Input.GetKeyDown(key))
                    {
                        valToSet = trueVal;
                    }
                    break;
                case InputKeyQueryType.Up:
                    if (Input.GetKeyUp(key))
                    {
                        valToSet = trueVal;
                    }
                    break;
                case InputKeyQueryType.State:
                    if (Input.GetKey(key))
                    {
                        valToSet = trueVal;
                    }
                    break;
                default:
                    break;
            }
        }

        private void DoKeyName(string key, int trueVal, ref int valToSet)
        {
            switch (keyQueryType)
            {
                case InputKeyQueryType.Down:
                    if (Input.GetKeyDown(key))
                    {
                        valToSet = trueVal;
                    }
                    break;
                case InputKeyQueryType.Up:
                    if (Input.GetKeyUp(key))
                    {
                        valToSet = trueVal;
                    }
                    break;
                case InputKeyQueryType.State:
                    if (Input.GetKey(key))
                    {
                        valToSet = trueVal;
                    }
                    break;
                default:
                    break;
            }
        }

        

        public override string GetSummary()
        {
            if (_outValue.Variable == null)
            {
                return "Error: no outvalue set";
            }

            string keyCodeStr = _keyCode != KeyCode.None ? _keyCode.ToString() : _keyCodeName;
            string result = $"{keyCodeStr} in {_outValue.VarKey}";
            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            bool result = base.HasReference(variable) || 
                ReferenceEquals(_keyCodeName.VarRef, variable) ||
                ReferenceEquals(_outValue.Variable, variable) ||
                ReferenceEquals(_keyCodeNameNegative.VarRef, variable);

            return result;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_oldOutValue != null)
            {
                _outValue.Variable = _oldOutValue;
                _oldOutValue = null;
            }

        }

        [HideInInspector]
        [FormerlySerializedAs("outValue")]
        [VariableProperty(typeof(FloatVariable), typeof(BooleanVariable), typeof(IntegerVariable))]
        protected Variable _oldOutValue;
    }
}
