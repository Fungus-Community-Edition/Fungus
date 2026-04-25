using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("BI Tween",
        "Simple/Shake Position",
        "Randomly shakes a component's position by a diminishing amount over time.")]
    public class ShakePosition : BaseSimpleTweenCommand
    {
        [Tooltip("The Component with the transform to shake.")]
        [SerializeField] protected ComponentData _toShake = new ComponentData();
        [Tooltip("The maximum positional offset for the shake.")]
        [SerializeField] protected Vector3Data _amount = new Vector3Data();
        [Tooltip("Restricts the shake to these axes. Use 1 for enabled, 0 for disabled.")]
        [SerializeField] protected Vector3Data _axisMask = new Vector3Data(Vector3.one);
        [Tooltip("Whether to shake in local space instead of world space.")]
        [SerializeField] protected BooleanData _isLocalSpace = new BooleanData(false);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_toShake);
            _variableDataCache.Add(_amount);
            _variableDataCache.Add(_axisMask);
            _variableDataCache.Add(_isLocalSpace);
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

            _startPosition = _isLocalSpace ? 
                _targTrans.localPosition : 
                _targTrans.position;
            _shakeProgress = 0f;

            _ourTween = _tweener.TweenGeneral(GetShakeProgress, UpdateShakeProgress, _progressDest, _duration);
            return _ourTween;
        }

        private Vector3 _startPosition;
        private IGeneralTweenAdapter<float> _tweener;
        private static readonly int _progressDest = 1; // = 100%

        private float GetShakeProgress()
        {
            return _shakeProgress;
        }

        private float _shakeProgress;

        private void UpdateShakeProgress(float progress)
        {
            _shakeProgress = progress;
            ApplyShake(progress);
        }

        private void ApplyShake(float progress)
        {
            if (_toShake == null || _toShake.Value == null)
            {
                string errorMessage = $"Cannot apply shake because target is null. Command: {name} " +
                    $"in Block: {ParentBlock.BlockName} on GameObject: {gameObject.name} " + 
                    $"at index {CommandIndex}";
                Debug.LogError(errorMessage);
                return;
            }

            Vector3 axisMask = GetAxisMask();
            Vector3 randomOffset = Random.insideUnitSphere;
            randomOffset = Vector3.Scale(randomOffset, _amount.Value);
            randomOffset = Vector3.Scale(randomOffset, axisMask);

            float damper = 1f - Mathf.Clamp01(progress);
            Vector3 finalOffset = randomOffset * damper;

            Transform tForm = _toShake.Value.transform;
            Vector3 posToApply = _startPosition + finalOffset;
            if (_isLocalSpace)
            {
                tForm.localPosition = posToApply;
            }
            else
            {
                tForm.position = posToApply;
            }
        }

        /// <summary>
        /// We use this so we know which axes to apply the shake on. The tweening process
        /// is the same regardless of axis, so we just apply a mask to the random offset
        /// to get the desired effect.
        /// </summary>
        private Vector3 GetAxisMask()
        {
            Vector3 axisMask = _axisMask.Value;
            Vector3 result = new Vector3(ToAxisMask(axisMask.x), ToAxisMask(axisMask.y), ToAxisMask(axisMask.z));
            return result;
        }

        private static float ToAxisMask(float axisValue)
        {
            return Mathf.Abs(axisValue) > 0f ? 
                1f : 
                0f;
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
            ReturnToStartPos();

            if (_waitUntilFinished)
            {
                Continue();
            }
        }

        private void ReturnToStartPos()
        {
            if (_toShake == null || _toShake.Value == null)
            {
                return;
            }

            Transform tForm = _toShake.Value.transform;
            if (_isLocalSpace)
            {
                tForm.localPosition = _startPosition;
            }
            else
            {
                tForm.position = _startPosition;
            }
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
            string axisStr = _axisMask.RepresentingVar ? 
                $"{_axisMask.VarRef.Key}" : 
                $"{_axisMask.Value}";
            string spaceStr = _isLocalSpace.Value ? 
                "local" : 
                "world";
            string durationStr = _duration.RepresentingVar ? 
                $"{_duration.VarRef.Key}" : 
                $"{_duration.Value}";

            string result = $"{targetStr} shake {amountStr} on {axisStr} axis in {spaceStr} space over " +
                $"{durationStr} seconds.";
            return result;
        }
    }
}