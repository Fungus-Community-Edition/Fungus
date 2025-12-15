using UnityEngine;
using UnityEngine.Serialization;
using Amanita.LocalizationSys;

namespace Amanita.VScripting
{
    /// <summary>
    /// Set the active language for the scene. A Localization object with a localization file must be present in the scene.
    /// </summary>
    [CommandInfo("Narrative", 
                 "Set Language", 
                 "Set the active language for the scene. A Localization object with a localization file must be present in the scene.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class SetLanguage : Command
    {
        [Tooltip("Code of the language to set. e.g. ES, DE, JA")]
        [SerializeField] protected StringData _languageCode = new StringData(); 

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(_languageCode);
        }

        #region Public members

        public static string mostRecentLanguage = "";

        public override void OnEnter()
        {
        #if UNITY_6000
            Localization localization = GameObject.FindFirstObjectByType<Localization>();
        #else
            Localization localization = GameObject.FindObjectOfType<Localization>();
        #endif
            if (localization != null)
            {
                localization.SetActiveLanguage(_languageCode.Value, true);

                // Cache the most recently set language code so we can continue to 
                // use the same language in subsequent scenes.
                mostRecentLanguage = _languageCode.Value;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return _languageCode.Value;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Narrative;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_languageCode.VarRef, variable) || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("languageCode")] public string languageCodeOLD = "";

        protected override void OnEnable()
        {
            base.OnEnable();
            if (languageCodeOLD != "")
            {
                _languageCode.Value = languageCodeOLD;
                languageCodeOLD = "";
            }
        }

        #endregion
    }
}
