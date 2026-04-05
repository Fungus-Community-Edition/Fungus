using UnityEngine;
using AtMycelia.Amanita.VScripting;

namespace AtMycelia.Amanita.Tweening.VScripting
{
    [CommandInfo("Animation",
        "Scale",
        "Scales a component's transform to a target scale over time.")]
    public class ScaleTransform : BaseTweenCommand
    {
        [Tooltip("The Component with the transform to scale.")]
        [SerializeField] protected ComponentData _toScale = new ComponentData();
        [Tooltip("The target scale to use. Only applies if Tween Relativity is set to Absolute.")]
        [SerializeField] protected Vector3Data _absoluteScale = new Vector3Data();
        [Tooltip("The amount to scale by. Only applies if Tween Relativity is set to Relative.")]
        [SerializeField] protected Vector3Data _scaleByAmount = new Vector3Data();
        [Tooltip("The scale the tween will start at. Only applies if ToFrom is set to From.")]
        [SerializeField] protected Vector3Data _scaleFromValue = new Vector3Data();

        protected override bool AreTargetsValid()
        {
            bool result = _toScale != null && _toScale.BoxedValue != null;
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();
            Vector3 startScale = DecideStartScale();
            Vector3 endScale = DecideTargetScale(startScale);
            Transform tForm = _toScale.Value.transform;
            tForm.localScale = startScale;

            _ourTween = _tweener.ScaleTo(tForm, endScale, _duration);
            return _ourTween;
        }

        protected virtual Vector3 DecideStartScale()
        {
            Vector3 startScale = Vector3.one;

            if (_startMode == StartFromMode.FromValue)
            {
                startScale = _scaleFromValue.Value;
            }
            else
            {
                startScale = _toScale.Value.transform.localScale;
            }

            return startScale;
        }

        /// <summary>
        /// Determines the target scale for the tween based on the Tween Relativity
        /// setting and the ToFrom setting. The startScale arg is there for when
        /// we need to decide the result relative to it. Of course, it gets ignored
        /// when Tween Relativity is Absolute.
        /// </summary>
        protected virtual Vector3 DecideTargetScale(Vector3 startScale)
        {
            Vector3 targetScale = Vector3.one;

            if (_relativity == TweenRelativity.Absolute)
            {
                targetScale = _absoluteScale.Value;
            }
            else
            {
                targetScale = startScale + _scaleByAmount.Value;
            }

            return targetScale;
        }

        protected override void StopAllTweens()
        {
            if (_toScale == null || _toScale.Value == null)
            {
                return;
            }

            var manager = TweenManager.S;
            manager.KillAllOn(_toScale.Value);
        }

        protected override void ValidateTweener()
        {
            if (_tweenerSO != null && _tweenerSO is not ITransformTweenAdapter)
            {
                Debug.LogWarning($"The tweener assigned to {name} is not a transform tween adapter. " +
                    $"Reverting to default one.");
                GoWithDefaultTweener();
            }

            base.ValidateTweener();
            _tweener = _tweenerSO as ITransformTweenAdapter;
        }

        private ITransformTweenAdapter _tweener;

        public override Color GetButtonColor()
        {
            return CommandColors.Animation;
        }

        public override string GetSummary()
        {
            bool weHaveTarget = _toScale != null && _toScale.Value != null;
            if (!weHaveTarget)
            {
                return "Need a target.";
            }

            string targetStr;
            if (_toScale.RepresentingVar)
            {
                targetStr = $"{_toScale.VarRef.Key}";
            }
            else
            {
                targetStr = $"{_toScale.Value.name}";
            }

            string toFromStr = _startMode.ToString();

            if (_startMode == StartFromMode.FromValue)
            {
                string scaleFromStr;
                if (_scaleFromValue.RepresentingVar)
                {
                    scaleFromStr = $"{_scaleFromValue.VarRef.Key}";
                }
                else
                {
                    scaleFromStr = $"{_scaleFromValue.Value}";
                }
                toFromStr += $"({scaleFromStr}) to";
            }

            string targetScaleStr;
            if (_relativity == TweenRelativity.Absolute)
            {
                if (_absoluteScale.RepresentingVar)
                {
                    targetScaleStr = $"{_absoluteScale.VarRef.Key}";
                }
                else
                {
                    targetScaleStr = $"{_absoluteScale.Value}";
                }
            }
            else
            {
                if (_scaleByAmount.RepresentingVar)
                {
                    targetScaleStr = $"{_scaleByAmount.VarRef.Key}";
                }
                else
                {
                    targetScaleStr = $"{_scaleByAmount.Value}";
                }
                targetScaleStr += " relative to start";
            }

            string durationStr;
            if (_duration.RepresentingVar)
            {
                durationStr = $"{_duration.VarRef.Key}";
            }
            else
            {
                durationStr = $"{_duration.Value}";
            }

            string result = $"{targetStr} {toFromStr} {targetScaleStr} over {durationStr} seconds.";
            return result;
        }
    }
}