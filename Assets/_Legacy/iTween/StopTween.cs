using UnityEngine;
using UnityEngine.Serialization;

namespace Amanita.VScripting
{
    /// <summary>
    /// Stops an active iTween by name.
    /// </summary>
    [CommandInfo("iTween", 
                 "Stop Tween", 
                 "Stops an active iTween by name.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class StopTween : Command
    {
        [Tooltip("Stop and destroy any Tweens in current scene with the supplied name")]
        [SerializeField] protected StringData _tweenName;

        #region Public members

        public override void OnEnter()
        {
            iTween.StopByName(_tweenName.Value);
            Continue();
        }

        public override bool HasReference(Variable variable)
        {
            return _tweenName.stringRef == variable || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("tweenName")] public string tweenNameOLD = "";

        protected virtual void OnEnable()
        {
            if (tweenNameOLD != "")
            {
                _tweenName.Value = tweenNameOLD;
                tweenNameOLD = "";
            }
        }

        #endregion
    }
}