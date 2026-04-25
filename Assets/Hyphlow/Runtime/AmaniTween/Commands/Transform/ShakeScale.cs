using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("BI Tween",
        "Simple/Shake Scale",
        "Randomly shakes a component's scale by a diminishing amount over time.")]
    public class ShakeScale : BaseSimpleTweenCommand
    {
        [Tooltip("The Component with the transform to shake.")]
        [SerializeField] protected ComponentData _toShake = new ComponentData();
        [Tooltip("The maximum scale offset for the shake.")]
        [SerializeField] protected Vector3Data _amount = new Vector3Data();

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_toShake);
            _variableDataCache.Add(_amount);
        }

        protected override void RegisterAllTargets()
        {
            _targTrans = null;
            if (_toShake.Value != null)
            {
                _targTrans = _toShake.Value.transform;
            }
            _allTargets.Add(_targTrans);
        }

        protected Transform _targTrans;
        protected override bool AreTargetsValid()
        {
            bool result = _targTrans != null;
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();

            _startScale = _targTrans.localScale;
            _shakeProgress = 0f;

            _ourTween = _tweener.TweenGeneral(GetShakeProgress, UpdateShakeProgress, _progressDest, _duration);
            return _ourTween;
        }

        private Vector3 _startScale;
        private IGeneralTweenAdapter<float> _tweener;
        private static readonly int _progressDest = 1; // = 100%

        private float _shakeProgress;

        private float GetShakeProgress()
        {
            return _shakeProgress;
        }

        private void UpdateShakeProgress(float progress)
        {
            _shakeProgress = progress;
            ApplyShake(progress);
        }

        private void ApplyShake(float progress)
        {
            if (_toShake == null || _toShake.Value == null)
            {
                string errorMessage = $"Cannot apply scale shake because target is null. Command: {name} " +
                    $"in Block: {ParentBlock.BlockName} on GameObject: {gameObject.name} " +
                    $"at index {CommandIndex}";
                Debug.LogError(errorMessage);
                return;
            }

            Vector3 randomOffset = Random.insideUnitSphere;
            randomOffset = Vector3.Scale(randomOffset, _amount.Value);

            float damper = 1f - Mathf.Clamp01(progress);
            Vector3 finalOffset = randomOffset * damper;

            Transform tForm = _toShake.Value.transform;
            tForm.localScale = _startScale + finalOffset;
        }

        protected override void StopAllTweens()
        {
            if (_toShake == null || _toShake.Value == null)
            {
                return;
            }

            var manager = TweenManager.S;
            manager.KillAllOn(_toShake.Value);
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
            if (_toShake == null || _toShake.Value == null)
            {
                return;
            }

            Transform tForm = _toShake.Value.transform;
            tForm.localScale = _startScale;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Animation;
        }

        public override string GetSummary()
        {
            bool weHaveTarget = _toShake != null && _toShake.Value != null;
            if (!weHaveTarget)
            {
                return "Need a target.";
            }

            string targetStr = _toShake.RepresentingVar ?
                $"{_toShake.VarRef.Key}" :
                $"{_toShake.Value.name}";
            string amountStr = _amount.RepresentingVar ?
                $"{_amount.VarRef.Key}" :
                $"{_amount.Value}";
            string durationStr = _duration.RepresentingVar ?
                $"{_duration.VarRef.Key}" :
                $"{_duration.Value}";

            string result = $"{targetStr} shake scale {amountStr} over {durationStr} seconds.";
            return result;
        }
    }
}