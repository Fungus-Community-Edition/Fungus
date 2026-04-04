using AtMycelia.Amanita.VScripting;
using UnityEngine;

namespace AtMycelia.Amanita.Tweening
{ 
    public abstract class BaseTweenCommand : Command
    {
        [Tooltip("The time in seconds the animation will take to complete")]
        [SerializeField] protected FloatData _duration = new FloatData(1f);

        [Tooltip("Tween adapter that will handle the process")]
        [SerializeField] protected ScriptableObject _tweenerSO = null;

        [Tooltip("Does the tween act from current TO destination or is it reversed and act " +
            "FROM destination to its current")]
        [SerializeField] protected ToFrom _toFrom = ToFrom.To;

        [Tooltip("Does the tween use the value as a target or as a delta to be added to " +
            "where it already is at the time?")]
        [SerializeField] protected TweenRelativity _relativity = TweenRelativity.Absolute;

        [Tooltip("Number of times to repeat the tween. -1 is infinite.")]
        [SerializeField] protected IntegerData _repeats = new IntegerData(0);

        [Tooltip("Stop any previous tweens on this object before adding this one. " +
            "Warning: expensive.")]
        [SerializeField] protected BooleanData _stopPreviousTweens = new BooleanData(false);

        [Tooltip("Whether or not to wait until the tween has finished before executing the next Command.")]
        [SerializeField] protected BooleanData _waitUntilFinished = new BooleanData(true);

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
            _tweenerSO = DefaultAmanitaAssets.TweenAdapter;
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_duration);
            _variableDataCache.Add(_repeats);
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

        // Different tween types may have various types of targets, and thus we want
        // to let subclasses implement their own logic for stopping tweens that are relevant to them.
        protected abstract void StopAllTweens();

        protected ITweenHandle _ourTween;

        // TODO: Have this set the repeat and loop type
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

    /// <summary>
    /// For helping decide where a tween will start.
    /// </summary>
    public enum ToFrom { Null, To, From }

    /// <summary>
    /// For helping decide whether the tween's target value is absolute or relative to the current value.
    /// </summary>
    public enum TweenRelativity
    {
        Null, Absolute, Relative
    }
}