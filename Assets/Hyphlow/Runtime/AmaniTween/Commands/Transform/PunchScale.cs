using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("BI Tween",
        "Simple/Punch Scale",
        "Applies a jolt of force to a component's scale and wobbles it back to its initial scale.")]
    public class PunchScale : BaseSimpleTweenCommand
    {
        [Tooltip("The Component with the transform to punch.")]
        [SerializeField] protected ComponentData _toPunch = new ComponentData();
        [Tooltip("The maximum scale offset for the punch.")]
        [SerializeField] protected Vector3Data _amount = new Vector3Data();

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_toPunch);
            _variableDataCache.Add(_amount);
        }

        protected override void RegisterAllTargets()
        {
            _targTrans = null;
            if (_toPunch.Value != null)
            {
                _targTrans = _toPunch.Value.transform;
            }
            _allTargets.Add(_targTrans);
        }

        protected Transform _targTrans;

        protected override bool AreTargetsValid()
        {
            bool result = _toPunch != null && _toPunch.BoxedValue != null;
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();

            _startScale = _targTrans.localScale;
            _punchProgress = 0f;

            _ourTween = _tweener.TweenGeneral(GetPunchProgress, UpdatePunchProgress,
                _progressDest, _duration);
            return _ourTween;
        }

        private Vector3 _startScale;
        private float _punchProgress;
        private IGeneralTweenAdapter<float> _tweener;
        private static readonly int _progressDest = 1;

        private float GetPunchProgress()
        {
            return _punchProgress;
        }

        private void UpdatePunchProgress(float progress)
        {
            _punchProgress = progress;
            ApplyPunch(progress);
        }

        private void ApplyPunch(float progress)
        {
            if (_toPunch == null || _toPunch.Value == null)
            {
                string errorMessage = $"Cannot apply punch because target is null. Command: {name} " +
                    $"in Block: {ParentBlock.BlockName} on GameObject: {gameObject.name} " +
                    $"at index {CommandIndex}";
                Debug.LogError(errorMessage);
                return;
            }

            float damper = 1f - Mathf.Clamp01(progress);
            float oscillation = Mathf.Sin((progress + _phaseOffset) * _oscillationCycles * Mathf.PI * 2f);
            float punchScale = damper * oscillation;
            Vector3 offset = _amount.Value * punchScale;

            Transform tForm = _toPunch.Value.transform;
            tForm.localScale = _startScale + offset;
        }

        private const float _phaseOffset = 0.25f;
        private const float _oscillationCycles = 2f;

        protected override void StopAllTweens()
        {
            if (_toPunch == null || _toPunch.Value == null)
            {
                return;
            }

            var manager = TweenManager.S;
            manager.KillAllOn(_toPunch.Value);
        }

        protected override void ValidateTweener()
        {
            if (_tweenerSO != null && _tweenerSO is not IGeneralTweenAdapter<float>)
            {
                Debug.LogWarning($"The tweener assigned to {name} is not a float tween adapter. " +
                    $"Reverting to default one.");
                GoWithDefaultTweener();
            }

            base.ValidateTweener();
            _tweener = _tweenerSO as IGeneralTweenAdapter<float>;
        }

        protected override void WaitOrContinueAsAppropriate()
        {
            _ourTween?.SetOnComplete(HandleTweenComplete);

            if (!_waitUntilFinished)
            {
                Continue();
            }
        }

        private void HandleTweenComplete()
        {
            ReturnToStartScale();

            if (_waitUntilFinished)
            {
                Continue();
            }
        }

        private void ReturnToStartScale()
        {
            if (_toPunch == null || _toPunch.Value == null)
            {
                return;
            }

            Transform tForm = _toPunch.Value.transform;
            tForm.localScale = _startScale;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Animation;
        }

        public override string GetSummary()
        {
            bool weHaveTarget = _toPunch != null && _toPunch.Value != null;
            if (!weHaveTarget)
            {
                return "Need a target.";
            }

            string targetStr = _toPunch.RepresentingVar ? $"{_toPunch.VarRef.Key}" : $"{_toPunch.Value.name}";
            string amountStr = _amount.RepresentingVar ? $"{_amount.VarRef.Key}" : $"{_amount.Value}";
            string durationStr = _duration.RepresentingVar ? $"{_duration.VarRef.Key}" : $"{_duration.Value}";

            string result = $"{targetStr} punch scale {amountStr} over {durationStr} seconds.";
            return result;
        }
    }
}