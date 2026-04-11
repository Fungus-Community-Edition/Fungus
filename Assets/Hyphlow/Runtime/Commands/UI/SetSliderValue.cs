using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sets or Gets the value property of a slider object.
    /// </summary>
    [CommandInfo("UI",
                 "Set or Get Slider Value",
                 "Sets or Gets the value property of a slider object")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetSliderValue : Command 
    {
        [Tooltip("Target slider object to set the value on")]
        [SerializeField] protected Slider slider;

        [Tooltip("Float value to set the slider value to.")]
        [SerializeField] protected FloatData value;

        protected BaseVariableProperty.GetSet getOrSet = BaseVariableProperty.GetSet.Set;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(value);
        }

        #region Public members

        public override void OnEnter() 
        {
            if (slider != null)
            {
                switch (getOrSet)
                {
                case BaseVariableProperty.GetSet.Get:
                    value.Value = slider.value;
                    break;
                case BaseVariableProperty.GetSet.Set:
                    slider.value = value.Value;
                    break;
                default:
                    break;
                }
            }

            Continue();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override string GetSummary()
        {
            if (slider == null)
            {
                return "Error: Slider object not selected";
            }

            return  getOrSet == BaseVariableProperty.GetSet.Set ? 
                slider.name + " = " + value.GetDescription() :
                value.GetDescription() + " = " + slider.name;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(value.VarRef, variable) || base.HasReference(variable);
        }

        #endregion
    }
}
