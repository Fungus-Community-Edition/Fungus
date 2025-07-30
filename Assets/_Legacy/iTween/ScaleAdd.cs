using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Amanita.VScripting
{
    /// <summary>
    /// Changes a game object's scale by a specified offset over time.
    /// </summary>
    [CommandInfo("iTween", 
                 "Scale Add", 
                 "Changes a game object's scale by a specified offset over time.")]
    [AddComponentMenu("")]
    [System.Obsolete("Deprecated, consider using the LeanTween based Tween command instead.")]
    [ExecuteInEditMode]
    public class ScaleAdd : iTweenCommand
    {
        [Tooltip("A scale offset in space the GameObject will animate to")]
        [SerializeField] protected Vector3Data _offset;

        #region Public members

        public override void DoTween()
        {
            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("name", _tweenName.Value);
            tweenParams.Add("amount", _offset.Value);
            tweenParams.Add("time", _duration.Value);
            tweenParams.Add("easetype", easeType);
            tweenParams.Add("looptype", loopType);
            tweenParams.Add("oncomplete", "OniTweenComplete");
            tweenParams.Add("oncompletetarget", gameObject);
            tweenParams.Add("oncompleteparams", this);
            iTween.ScaleAdd(_targetObject.Value, tweenParams);
        }

        public override bool HasReference(Variable variable)
        {
            return _offset.vector3Ref == variable ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("offset")] public Vector3 offsetOLD;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (offsetOLD != default(Vector3))
            {
                _offset.Value = offsetOLD;
                offsetOLD = default(Vector3);
            }
        }

        #endregion
    }
}