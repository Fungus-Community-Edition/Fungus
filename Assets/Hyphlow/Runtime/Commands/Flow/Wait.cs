using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Waits for period of time before executing the next command in the block.
    /// </summary>
    [CommandInfo("Flow", 
                 "Wait", 
                 "Waits for period of time before executing the next command in the block.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Wait : Command
    {
        [Tooltip("Duration to wait for")]
        [SerializeField] protected FloatData _duration = new FloatData(1);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_duration);
        }

        protected virtual void OnWaitComplete()
        {
            Continue();
        }

        #region Public members

        public override void OnEnter()
        {
            Invoke (nameof(OnWaitComplete), _duration.Value);
        }

        public override string GetSummary()
        {
            string result = $"{_duration.Value} second(s)";
            IVariable varRef = _duration.VarRef;
            bool durationIsVar = varRef != null;
            
            if (durationIsVar)
            {
                result = $"{varRef.Key} ({_duration.Value}) second(s)";
            }

            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_duration.VarRef, variable) || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("duration")] public float durationOLD;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (durationOLD != default(float))
            {
                _duration.Value = durationOLD;
                durationOLD = default(float);
            }
        }

        #endregion
    }
}
