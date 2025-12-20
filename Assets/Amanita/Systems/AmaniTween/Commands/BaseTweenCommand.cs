using Amanita.VScripting;
using System;
using UnityEngine;

namespace Amanita.Tweening
{ 
    public abstract class BaseTweenCommand : Command
    {
        [Tooltip("The time in seconds the animation will take to complete")]
        [SerializeField] protected FloatData _duration = new FloatData(1f);

        [Tooltip("Tween adapter that will handle the process")]
        [SerializeField] protected ScriptableObject tweenerSO = null;

        [Tooltip("Does the tween act from current TO destination or is it reversed and act FROM destination to its current")]
        [SerializeField] protected ToFrom _toFrom = ToFrom.To;

        [Tooltip("Does the tween use the value as a target or as a delta to be added to where it already is at the time?")]
        [SerializeField] protected TweenRelativity _relativity = TweenRelativity.Absolute;

        [Tooltip("Number of times to repeat the tween, -1 is infinite.")]
        [SerializeField] protected IntegerData repeats = new IntegerData(0);

        [Tooltip("Stop any previously LeanTweens on this object before adding this one. Warning; expensive.")]
        [SerializeField] protected BooleanData stopPreviousTweens = new BooleanData(false);

        [Tooltip("Wait until the tween has finished before executing the next command")]
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(true);

        protected virtual void Awake()
        {
            ValidateTweener();
        }

        protected abstract void ValidateTweener();

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(_duration);
            variableDataCache.Add(repeats);
            variableDataCache.Add(stopPreviousTweens);
            variableDataCache.Add(waitUntilFinished);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!AreTargetsValid())
            {
                Debug.LogWarning("Tween command targets are not valid, skipping tween.");
                Continue();
                return;
            }

            if (stopPreviousTweens)
            {
                StopAllTweens();
            }

            ourTween = PrepAndExecuteTween();

            WaitOrContinueAsAppropriate();
        }

        protected abstract bool AreTargetsValid();

        private void StopAllTweens()
        {
            throw new NotImplementedException();
        }

        protected ITweenHandle ourTween;

        // TODO: Have this set the repeat and loop type
        protected abstract ITweenHandle PrepAndExecuteTween();
        protected virtual void OnTweenComplete()
        {
            Continue();
        }

        protected virtual void WaitOrContinueAsAppropriate()
        {
            if (waitUntilFinished)
            {
                ourTween?.SetOnComplete(OnTweenComplete);
            }
            else
            {
                Continue();
            }
        }
    }

    public enum ToFrom { Null, To, From }
    public enum TweenRelativity
    {
        Null, Absolute, Relative
    }
}