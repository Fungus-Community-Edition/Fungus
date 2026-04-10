using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Type of log message. Maps directly to Unity's log types.
    /// </summary>
    public enum DebugLogType
    {
        /// <summary> Informative log message. </summary>
        Info,
        /// <summary> Warning log message. </summary>
        Warning,
        /// <summary> Error log message. </summary>
        Error
    }

    /// <summary>
    /// Writes a log message to the debug console.
    /// </summary>
    [CommandInfo("Scripting", 
                 "Debug Log", 
                 "Writes a log message to the debug console.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class DebugLog : Command 
    {
        [Tooltip("Display type of debug log info")]
        [SerializeField] protected DebugLogType logType;

        [Tooltip("Text to write to the debug log. Supports variable substitution, e.g. {$Myvar}")]
        [SerializeField] protected StringDataMulti logMessage = new StringDataMulti();

        #region Public members

        public override void OnEnter ()
        {
            var flowchart = GetFlowchart();
            string message = logMessage.Value;

            if (flowchart != null)
            {
                message = flowchart.SubstituteVariables(message);
            }

            switch (logType)
            {
            case DebugLogType.Info:
                Debug.Log(message);
                break;
            case DebugLogType.Warning:
                Debug.LogWarning(message);
                break;
            case DebugLogType.Error:
                Debug.LogError(message);
                break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return logMessage.GetDescription();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(logMessage.VarRef, variable) || base.HasReference(variable);
        }

        #endregion

        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();
            var f = GetFlowchart();
            if (f == null)
            {
                return;
            }

            f.DetermineSubstituteVariables(logMessage.Value, referencedVariables);
        }
#endif
        #endregion Editor caches

    }
}
