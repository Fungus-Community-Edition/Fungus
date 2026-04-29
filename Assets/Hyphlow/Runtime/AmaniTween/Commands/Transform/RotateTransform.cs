using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("BI Tween",
        "Rotate",
        "Rotates a component's transform to a target rotation over time.")]
    public class RotateTransform : BaseTweenCommand
    {
        [Tooltip("The Component or GameObject with the Transform to rotate.")]
        [ContentTypeConstraint(typeof(Component), typeof(GameObject))]
        [SerializeField] protected AnyVariableData _toRotate = new AnyVariableData();
        [Tooltip("The target rotation (Euler angles). Only applies if Tween Relativity is set to Absolute.")]
        [SerializeField] protected Vector3Data _absoluteRotation = new Vector3Data();
        [Tooltip("The amount to rotate by (Euler angles). Only applies if Tween Relativity is set to Relative.")]
        [SerializeField] protected Vector3Data _rotateByAmount = new Vector3Data();
        [Tooltip("The rotation (Euler angles) the tween will start at. Only applies if ToFrom is set to From.")]
        [SerializeField] protected Vector3Data _rotateFromRotation = new Vector3Data();

        private Transform _targetTransform;
        private ITransformTweenAdapter _tweener;

        public override void OnEnter()
        {
            _targetTransform = GetTargetTransform();
            base.OnEnter();
        }

        private Transform GetTargetTransform()
        {
            Transform result = null;
            if (_toRotate.BoxedValue is Component comp && comp != null)
            {
                result = comp.transform;
            }
            else if (_toRotate.BoxedValue is GameObject go && go != null)
            {
                result = go.transform;
            }
            return result;
        }

        protected override bool AreTargetsValid()
        {
            bool result = _targetTransform != null;

            if (!result)
            {
                string errorMessage = $"RotateTransform on Flowchart {name}, Block {ParentBlock.BlockName} " +
                    $"at index {CommandIndex} is missing a target transform.";
                Debug.LogError(errorMessage);
            }
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();
            Quaternion startRot = DecideStartRotation();
            Quaternion endRot = DecideTargetRotation(startRot);
            _targetTransform.rotation = startRot;

            _ourTween = _tweener.RotateTo(_targetTransform, endRot, _duration);
            return _ourTween;
        }

        protected virtual Quaternion DecideStartRotation()
        {
            Quaternion startRot;

            if (_startMode == StartFromMode.FromValue)
            {
                startRot = Quaternion.Euler(_rotateFromRotation.Value);
            }
            else
            {
                startRot = _targetTransform.rotation;
            }

            return startRot;
        }

        /// <summary>
        /// Determines the target rotation for the tween based on the Tween Relativity
        /// setting and the ToFrom setting. The startRot arg is there for when
        /// we need to decide the result relative to it. Of course, it gets ignored
        /// when Tween Relativity is Absolute.
        /// </summary>
        protected virtual Quaternion DecideTargetRotation(Quaternion startRot)
        {
            Quaternion targetRot;

            if (_relativity == TweenRelativity.Absolute)
            {
                targetRot = Quaternion.Euler(_absoluteRotation.Value);
            }
            else
            {
                targetRot = startRot * Quaternion.Euler(_rotateByAmount.Value);
            }

            return targetRot;
        }

        protected override void StopAllTweens()
        {
            if (_toRotate == null || _targetTransform == null)
            {
                return;
            }

            var manager = TweenManager.S;
            manager.KillAllOn(_targetTransform);
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

        public override Color GetButtonColor()
        {
            return CommandColors.Animation;
        }

        public override string GetSummary()
        {
            _targetTransform = GetTargetTransform();
            bool weHaveTarget = _targetTransform != null;
            if (!weHaveTarget)
            {
                return "Need a target.";
            }

            string targetStr;
            if (_toRotate.RepresentingVar)
            {
                targetStr = $"{_toRotate.VarRef.Key}";
            }
            else
            {
                targetStr = $"{_targetTransform.name}";
            }

            string toFromStr = _startMode.ToString();

            if (_startMode == StartFromMode.FromValue)
            {
                string rotateFromStr;
                if (_rotateFromRotation.RepresentingVar)
                {
                    rotateFromStr = $"{_rotateFromRotation.VarRef.Key}";
                }
                else
                {
                    rotateFromStr = $"{_rotateFromRotation.Value}";
                }
                toFromStr += $"({rotateFromStr}) to";
            }

            string targetRotStr;
            if (_relativity == TweenRelativity.Absolute)
            {
                if (_absoluteRotation.RepresentingVar)
                {
                    targetRotStr = $"{_absoluteRotation.VarRef.Key}";
                }
                else
                {
                    targetRotStr = $"{_absoluteRotation.Value}";
                }
            }
            else
            {
                if (_rotateByAmount.RepresentingVar)
                {
                    targetRotStr = $"{_rotateByAmount.VarRef.Key}";
                }
                else
                {
                    targetRotStr = $"{_rotateByAmount.Value}";
                }
                targetRotStr += " relative to start";
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

            string result = $"{targetStr} {toFromStr} {targetRotStr} over {durationStr} seconds.";
            return result;
        }
    }
}