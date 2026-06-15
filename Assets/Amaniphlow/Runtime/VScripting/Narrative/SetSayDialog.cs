using UnityEngine;
using AtMycelia.Amanita.DialogueSys;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow.Legacy
{
    /// <summary>
    /// Sets a custom say dialog to use when displaying story text.
    /// </summary>
    [CommandInfo("Narrative", 
                 "Set Say Dialog", 
                 "Sets a custom say dialog to use when displaying story text")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amaniphlow.Legacy")]
    public class SetSayDialog : Command, ISerializationCallbackReceiver
    {
        [Tooltip("The Say Dialog to use for displaying Say story text")]
        [SerializeField] protected GameObjectData sayDialog = new GameObjectData();

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(sayDialog);
        }

        public override void OnEnter()
        {
            if (!InputIsValid(out SayDialog sdToSet))
            {
                Debug.LogError($"Set Say Dialog Command on {gameObject.name} has invalid Say" +
                    $"Dialog input.", this);
                Continue();
                return;
            }

            SayDialogManager.S.MainSayDialog = sdToSet;
            
            Continue();
        }

        protected virtual bool InputIsValid(out SayDialog dialogFound)
        {
            dialogFound = null;
            if (sayDialog == null || sayDialog.Value == null)
            {
                return false;
            }
            sayDialog.TryGetComponent(out dialogFound);
            return dialogFound != null;
        }

        public override string GetSummary()
        {
            SayDialog dialog = 
                (sayDialog != null && sayDialog.Value != null) ? 
                sayDialog.Value.GetComponent<SayDialog>() 
                : null;
            bool inputValid = dialog != null;
            if (!inputValid)
            {
                return "Error: No say dialog selected";
            }

            return sayDialog.Value.name;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Narrative;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (oldSayDialog != null)
            {
                sayDialog.Value = oldSayDialog.gameObject;
                oldSayDialog = null;
            }
        }

        [HideInInspector]
        [FormerlySerializedAs("sayDialog")]
        [SerializeField] protected SayDialog oldSayDialog;
    }
}