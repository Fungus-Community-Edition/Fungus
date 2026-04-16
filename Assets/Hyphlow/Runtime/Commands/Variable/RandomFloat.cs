using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sets an float variable to a random value in the defined range.
    /// </summary>
    [CommandInfo("Variable", 
                 "Random Float", 
                 "Sets an float or double variable to a random value in the defined range.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class RandomFloat : Command 
    {
        [Tooltip("The variable that will get its value set. Can be a float or a double.")]
        [ContentTypeConstraint(typeof(float), typeof(double))]
        [SerializeField] protected VariableReference _variable;

        [Tooltip("Minimum value for random range")]
        [FormerlySerializedAs("minValue")]
        [SerializeField] protected FloatData _minValue;

        [Tooltip("Maximum value for random range")]
        [FormerlySerializedAs("maxValue")]
        [SerializeField] protected FloatData _maxValue;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_minValue);
            _variableDataCache.Add(_maxValue);
        }

        #region Public members

        public override void OnEnter()
        {
            if (variable != null)
            {
                variable.Value = Random.Range(_minValue.Value, _maxValue.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            string result = "Error: Variable not selected";
            if (_variable.Variable != null)
            {
                result = $"Set {_variable.Variable.Key} between {_minValue.Value} and {_maxValue.Value}";
            }

            return result;
        }

        public override bool HasReference(Variable variable)
        {
            return (variable == this.variable) || 
                ReferenceEquals(_minValue.VarRef, variable) || 
                ReferenceEquals(_maxValue.VarRef, variable);
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        #endregion

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (variable != null)
            {
                _variable.Variable = variable;
                variable = null;
            }
        }

        [VariableProperty(typeof(FloatVariable))]
        [HideInInspector]
        [SerializeField] protected FloatVariable variable;
    }
}
