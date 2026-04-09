using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Abstract base class for TweenUI commands.
    /// </summary>
    [MovedFrom("AtMycelia.Amanita.VScripting.Commands")]
    public abstract class TweenUI : Command 
    {
        [Tooltip("List of objects to be affected by the tween")]
        [SerializeField] protected List<GameObject> targetObjects = new List<GameObject>();

        //[Tooltip("Type of tween easing to apply")]
        //[SerializeField] protected LeanTweenType tweenType = LeanTweenType.easeOutQuad;

        [Tooltip("Whether to wait until this Command completes before continuing execution")]
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(true);
        
        [Tooltip("Time for the tween to complete")]
        [SerializeField] protected FloatData duration = new FloatData(1f);

        protected virtual void Awake()
        {
            ValidateTweeners();
        }

        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();
            _variableDataCache.Add(waitUntilFinished);
            _variableDataCache.Add(duration);
        }

        protected abstract void ValidateTweeners();

        protected virtual void ApplyTween()
        {
            ApplyToEachValidTarget();
            void ApplyToEachValidTarget()
            {
                for (int i = 0; i < targetObjects.Count; i++)
                {
                    var targetObject = targetObjects[i];
                    if (targetObject == null)
                    {
                        continue;
                    }
                    ApplyTweenToSingle(targetObject);
                }
            }

            if (waitUntilFinished)
            {
                //LeanTween.value(gameObject, 0f, 1f, duration).setOnComplete(OnComplete);
                Invoke(nameof(OnComplete), duration);
            }
        }

        protected abstract void ApplyTweenToSingle(GameObject go);

        protected virtual void OnComplete()
        {
            Continue();
        }

        protected virtual string GetSummaryValue()
        {
            return "";
        }

        #region Public members

        public override void OnEnter()
        {
            if (targetObjects.Count == 0)
            {
                Continue();
                return;
            }
            
            ApplyTween();

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override void OnCommandAdded(Block parentBlock)
        {
            // Add an empty slot by default. Saves an unnecessary user click.
            if (targetObjects.Count == 0)
            {
                targetObjects.Add(null);
            }
        }

        public override string GetSummary()
        {
            if (targetObjects.Count == 0)
            {
                return "Error: No targetObjects selected";
            }
            else if (targetObjects.Count == 1)
            {
                if (targetObjects[0] == null)
                {
                    return "Error: No targetObjects selected";
                }
                return targetObjects[0].name + " = " + GetSummaryValue();
            }
            
            string namesOfGameObjects = "";
            for (int i = 0; i < targetObjects.Count; i++)
            {
                var go = targetObjects[i];
                if (go == null)
                {
                    continue;
                }
                if (namesOfGameObjects == "")
                {
                    namesOfGameObjects += go.name;
                }
                else
                {
                    namesOfGameObjects += ", " + go.name;
                }
            }
            
            return namesOfGameObjects + " = " + GetSummaryValue();
        }
        
        public override Color GetButtonColor()
        {
            return new Color32(180, 250, 250, 255);
        }

        public override bool IsReorderableArray(string propertyName)
        {
            if (propertyName == "targetObjects")
            {
                return true;
            }

            return false;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(waitUntilFinished.VarRef, variable) || 
                ReferenceEquals(duration.VarRef, variable) || base.HasReference(variable);
        }

        #endregion
    }
}