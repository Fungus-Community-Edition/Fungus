using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("BI Tween",
        "Simple/Rotate Relative",
        "Rotates a component's transform to a target rotation over time (relative to some other rotation).")]
    public class RotateRelative : BaseSimpleTweenCommand
    {
        [Tooltip("The Component or GameObject with the transform to rotate.")]
        [ContentTypeConstraint(typeof(Component), typeof(GameObject))]
        [SerializeField] protected AnyVariableData _toRotate = new AnyVariableData();
        [Tooltip("The target rotation (Euler angles) to use.")]
        [SerializeField] protected Vector3Data _targetRotation = new Vector3Data();
        [Tooltip("The rotation (Euler angles) the tween will start at. Only applies if ToFrom is set to From.")]
        [SerializeField] protected Vector3Data _rotationFromValue = new Vector3Data();
        [Tooltip("Does the tween act from current TO destination or is it reversed and act " +
            "FROM destination to its current")]
        [SerializeField] protected StartFromMode _toFrom = StartFromMode.Current;
        [Tooltip("Whether to apply rotation in local space instead of world space.")]
        [SerializeField] protected BooleanData _isLocal = new BooleanData(true);

        protected override void RegisterAllTargets()
        {
            _targetTransform = GetTargetTransform();
            _allTargets.Add(_targetTransform);
        }

        protected virtual Transform GetTargetTransform()
        {
            Transform result = null;
            if (_toRotate == null)
            {
                return result;
            }

            // Need some defenses against fake Unity nulls here, so...
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

        private Transform _targetTransform;

        protected override bool AreTargetsValid()
        {
            bool result = _targetTransform != null;
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();
            Quaternion startRot = DecideStartRotation();
            Vector3 endRotVec = startRot.eulerAngles + _targetRotation.Value;
            Quaternion endRot = Quaternion.Euler(endRotVec);
            ApplyStartRotation(startRot);

            _ourTween = CreateRotationTween(startRot, endRot);
            return _ourTween;
        }

        protected virtual Quaternion DecideStartRotation()
        {
            Quaternion startRot = Quaternion.identity;

            if (_toFrom == StartFromMode.FromValue)
            {
                startRot = Quaternion.Euler(_rotationFromValue.Value);
            }
            else
            {
                startRot = _isLocal.Value ? 
                    _targetTransform.localRotation : 
                    _targetTransform.rotation;
            }

            return startRot;
        }

        private void ApplyStartRotation(Quaternion startRot)
        {
            if (_isLocal.Value)
            {
                _targetTransform.localRotation = startRot;
            }
            else
            {
                _targetTransform.rotation = startRot;
            }
        }

        private ITweenHandle CreateRotationTween(Quaternion startRot, Quaternion endRot)
        {
            if (_isLocal.Value)
            {
                return _tweener.RotateLocalTo(_targetTransform, endRot, _duration);
            }

            return _tweener.RotateTo(_targetTransform, endRot, _duration);
        }

        protected override void StopAllTweens()
        {
            if (_targetTransform == null)
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

        private ITransformTweenAdapter _tweener;

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

            string targetStr = _toRotate.RepresentingVar ? $"{_toRotate.VarRef.Key}" : $"{_targetTransform.name}";
            string toFromStr = _toFrom.ToString();
            string spaceStr = _isLocal.Value ? "local" : "world";

            if (_toFrom == StartFromMode.FromValue)
            {
                string rotationFromStr = _rotationFromValue.RepresentingVar
                    ? $"{_rotationFromValue.VarRef.Key}"
                    : $"{_rotationFromValue.Value}";
                toFromStr += $"({rotationFromStr}) to";
            }

            string targetRotationStr = _targetRotation.RepresentingVar
                ? $"{_targetRotation.VarRef.Key}"
                : $"{_targetRotation.Value}";

            string durationStr = _duration.RepresentingVar
                ? $"{_duration.VarRef.Key}"
                : $"{_duration.Value}";

            string result = $"{targetStr} {toFromStr} {targetRotationStr} ({spaceStr}) over {durationStr} seconds.";
            return result;
        }
    
    }
}