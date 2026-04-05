using UnityEngine;
using AtMycelia.Amanita.VScripting;

namespace AtMycelia.Amanita.Tweening.VScripting
{
    [CommandInfo("Animation",
        "Scale Set",
        "Scales a component's transform to a target scale over time.")]
    public class ScaleSet : BaseSimpleTweenCommand
    {
        [Tooltip("The Component with the transform to scale.")]
        [SerializeField] protected ComponentData _toScale = new ComponentData();
        [Tooltip("The target scale to use.")]
        [SerializeField] protected Vector3Data _targetScale = new Vector3Data();
        [Tooltip("The scale the tween will start at. Only applies if ToFrom is set to From.")]
        [SerializeField] protected Vector3Data _scaleFromValue = new Vector3Data();
        [Tooltip("Does the tween act from current TO destination or is it reversed and act " +
            "FROM destination to its current")]
        [SerializeField] protected StartFromMode _toFrom = StartFromMode.Current;

        protected override bool AreTargetsValid()
        {
            bool result = _toScale != null && _toScale.BoxedValue != null;
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();
            Vector3 startScale = DecideStartScale();
            Vector3 endScale = _targetScale.Value;
            Transform tForm = _toScale.Value.transform;
            tForm.localScale = startScale;

            _ourTween = _tweener.ScaleTo(tForm, endScale, _duration);
            return _ourTween;
        }

        protected virtual Vector3 DecideStartScale()
        {
            Vector3 startScale = Vector3.one;

            if (_toFrom == StartFromMode.FromValue)
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
            string toFromStr = _toFrom.ToString();

            if (_toFrom == StartFromMode.FromValue)
            {
                string scaleFromStr = _scaleFromValue.RepresentingVar
                    ? $"{_scaleFromValue.VarRef.Key}"
                    : $"{_scaleFromValue.Value}";
                toFromStr += $"({scaleFromStr}) to";
            }

            string targetScaleStr = _targetScale.RepresentingVar
                ? $"{_targetScale.VarRef.Key}"
                : $"{_targetScale.Value}";

            string durationStr = _duration.RepresentingVar
                ? $"{_duration.VarRef.Key}"
                : $"{_duration.Value}";

            string result = $"{targetStr} {toFromStr} {targetScaleStr} over {durationStr} seconds.";
            return result;
        }
    }
}