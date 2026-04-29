using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("BI Tween",
        "Simple/Position Relative",
        "Moves a component's transform to a target position over time (relative to some other position).")]
    public class PositionRelative : BaseSimpleTweenCommand
    {
        [Tooltip("The Component or GameObject with the transform to move.")]
        [ContentTypeConstraint(typeof(Component), typeof(GameObject))]
        [SerializeField] protected AnyVariableData _toMove = new AnyVariableData();
        [Tooltip("The target position to use.")]
        [SerializeField] protected Vector3Data _targetPosition = new Vector3Data();
        [Tooltip("The position the tween will start at. Only applies if ToFrom is set to From.")]
        [SerializeField] protected Vector3Data _positionFromValue = new Vector3Data();
        [Tooltip("Does the tween act from current TO destination or is it reversed and act " +
            "FROM destination to its current")]
        [SerializeField] protected StartFromMode _toFrom = StartFromMode.Current;

        public override void OnEnter()
        {
            _targetTransform = GetTargetTransform();
            base.OnEnter();
        }

        private Transform _targetTransform;

        protected override void RegisterAllTargets()
        {
            _targetTransform = GetTargetTransform();
            _allTargets.Add(_targetTransform);
        }

        private Transform GetTargetTransform()
        {
            Transform result = null;
            if (_toMove == null)
            {
                return result;
            }

            // Need some defenses against fake Unity nulls here, so...
            if (_toMove.BoxedValue is Component comp && comp != null)
            {
                result = comp.transform;
            }
            else if (_toMove.BoxedValue is GameObject go && go != null)
            {
                result = go.transform;
            }

            return result;
        }

        protected override bool AreTargetsValid()
        {
            bool result = _targetTransform != null;
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();
            Vector3 startPos = DecideStartPosition();
            Vector3 endPos = _targetPosition.Value;
            _targetTransform.position = startPos;

            _ourTween = _tweener.MoveTo(_targetTransform, endPos, _duration);
            return _ourTween;
        }

        protected virtual Vector3 DecideStartPosition()
        {
            Vector3 startPos = Vector3.zero;

            if (_toFrom == StartFromMode.FromValue)
            {
                startPos = _positionFromValue.Value;
            }
            else
            {
                startPos = _targetTransform.position;
            }

            return startPos;
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

            string targetStr = _toMove.RepresentingVar ? $"{_toMove.VarRef.Key}" : $"{_targetTransform.name}";
            string toFromStr = _toFrom.ToString();

            if (_toFrom == StartFromMode.FromValue)
            {
                string positionFromStr = _positionFromValue.RepresentingVar
                    ? $"{_positionFromValue.VarRef.Key}"
                    : $"{_positionFromValue.Value}";
                toFromStr += $"({positionFromStr}) to";
            }

            string targetPositionStr = _targetPosition.RepresentingVar
                ? $"{_targetPosition.VarRef.Key}"
                : $"{_targetPosition.Value}";

            string durationStr = _duration.RepresentingVar
                ? $"{_duration.VarRef.Key}"
                : $"{_duration.Value}";

            string result = $"{targetStr} {toFromStr} {targetPositionStr} over {durationStr} seconds.";
            return result;
        }
    }
}