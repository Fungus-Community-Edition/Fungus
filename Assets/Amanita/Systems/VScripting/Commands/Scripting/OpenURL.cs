using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Opens the specified URL in the browser.
    /// </summary>
    [CommandInfo("Scripting",
                 "Open URL",
                 "Opens the specified URL in the browser.")]
    public class OpenURL : Command
    {
        [Tooltip("URL to open in the browser")]
        [SerializeField] protected StringData url = new StringData();

        #region Public members

        public override void OnEnter()
        {
            Application.OpenURL(url.Value);

            Continue();
        }

        public override string GetSummary()
        {
            return url.Value;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(url.VarRef, variable) || base.HasReference(variable);
        }

        #endregion
    }
}
