using UnityEngine;

namespace AtMycelia.Hyphlow.Tweening.VScripting
{
    public abstract class BaseSimpleTweenCommand : Command, ITweenCommand
    {
        [Tooltip("The time in seconds the animation will take to complete")]
        [SerializeField] protected FloatData _duration = new FloatData(1f);

        [Tooltip("Tween adapter that will handle the process")]
        [SerializeField] protected ScriptableObject _tweenerSO = null;

        [Tooltip("Stop any previous tweens on this object before adding this one. " +
            "Warning: expensive.")]
        [SerializeField] protected BooleanData _stopPreviousTweens = new BooleanData(false);

        [Tooltip("Whether or not to wait until the tween has finished before executing the next Command.")]
        [SerializeField] protected BooleanData _waitUntilFinished = new BooleanData(true);

        public FloatData Duration => _duration;
        public ScriptableObject TweenerSO => _tweenerSO;
        public BooleanData StopPreviousTweens => _stopPreviousTweens;
        public BooleanData WaitUntilFinished => _waitUntilFinished;
        public ITweenHandle CurrentTween => _ourTween;

        protected virtual void Awake()
        {
            ValidateTweener();
        }

        protected virtual void ValidateTweener()
        {
            if (_tweenerSO == null)
            {
                GoWithDefaultTweener();
            }
        }

        protected virtual void GoWithDefaultTweener()
        {
            _tweenerSO = DefaultHyphlowAssets.TweenAdapter;
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_duration);
            _variableDataCache.Add(_stopPreviousTweens);
            _variableDataCache.Add(_waitUntilFinished);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!AreTargetsValid())
            {
                string warningMessage = $"{GetType().Name} on {gameObject.name}'s {ParentBlock.BlockName} Block " +
                    $"at index {CommandIndex} has invalid targets.";
                Debug.LogWarning(warningMessage);
                Continue();
                return;
            }

            if (_stopPreviousTweens)
            {
                StopAllTweens();
            }

            _ourTween = PrepAndExecuteTween();

            WaitOrContinueAsAppropriate();
        }

        protected abstract bool AreTargetsValid();
        protected abstract void StopAllTweens();

        protected ITweenHandle _ourTween;

        protected abstract ITweenHandle PrepAndExecuteTween();

        protected virtual void OnTweenComplete()
        {
            if (_waitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual void WaitOrContinueAsAppropriate()
        {
            if (_waitUntilFinished)
            {
                _ourTween?.SetOnComplete(OnTweenComplete);
            }
            else
            {
                Continue();
            }
        }

        protected override void DelayedOnValidate()
        {
            base.DelayedOnValidate();
            ValidateTweener();
        }
    }
}