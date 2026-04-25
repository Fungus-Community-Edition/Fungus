using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("BI Tween",
        "Simple/Scale Relative",
        "Scales a component's transform by a target amount over time.")]
    public class ScaleRelative : BaseSimpleTweenCommand
    {
        [Tooltip("The Component with the transform to scale.")]
        [SerializeField] protected ComponentData _toScale = new ComponentData();
        [Tooltip("The amount to scale by.")]
        [SerializeField] protected Vector3Data _scaleByAmount = new Vector3Data();
        [Tooltip("The scale the tween will start at. Only applies if ToFrom is set to From.")]
        [SerializeField] protected Vector3Data _scaleFromValue = new Vector3Data();
        [Tooltip("Whether or not the tween starts from the target's current scale or " +
            "another one.")]
        [SerializeField] protected StartFromMode _startMode = StartFromMode.Current;

        protected override void RegisterAllTargets()
        {
            _targTrans = null;
            if (_toScale.Value != null)
            {
                _targTrans = _toScale.Value.transform;
            }
            _allTargets.Add(_targTrans);
        }

        protected Transform _targTrans;

        protected override bool AreTargetsValid()
        {
            bool result = _toScale != null && _toScale.BoxedValue != null;
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();
            Vector3 startScale = DecideStartScale();
            Vector3 endScale = startScale + _scaleByAmount.Value;
            _targTrans.localScale = startScale;

            _ourTween = _tweener.ScaleTo(_targTrans, endScale, _duration);
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

            string targetStr = _toScale.RepresentingVar ? $"{_toScale.VarRef.Key}" : $"{_toScale.Value.name}";
            string toFromStr = _startMode.ToString();

            if (_startMode == StartFromMode.FromValue)
            {
                string scaleFromStr = _scaleFromValue.RepresentingVar
                    ? $"{_scaleFromValue.VarRef.Key}"
                    : $"{_scaleFromValue.Value}";
                toFromStr += $"({scaleFromStr}) to";
            }

            string scaleByStr = _scaleByAmount.RepresentingVar
                ? $"{_scaleByAmount.VarRef.Key}"
                : $"{_scaleByAmount.Value}";

            string durationStr = _duration.RepresentingVar
                ? $"{_duration.VarRef.Key}"
                : $"{_duration.Value}";

            string result = $"{targetStr} {toFromStr} {scaleByStr} relative to start over {durationStr} seconds.";
            return result;
        }
    }
}