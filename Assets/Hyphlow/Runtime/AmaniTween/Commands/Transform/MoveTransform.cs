using UnityEngine;
using UnityEngine.Serialization;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("BI Tween", 
        "Move", 
        "Moves a component's transform to a target position over time.")]
    public class MoveTransform : BaseTweenCommand
    {
        [Tooltip("The Component or GameObject with the Transform to move.")]
        [ContentTypeConstraint(typeof(Component), typeof(GameObject))]
        [SerializeField] protected AnyVariableData _toMove = new AnyVariableData();
        [Tooltip("The target position to move to. Only applies if Tween Relativity is set to Absolute.")]
        [SerializeField] protected Vector3Data _absoluteDest = new Vector3Data();
        [Tooltip("The amount to move by. Only applies if Tween Relativity is set to Relative.")]
        [SerializeField] protected Vector3Data _moveByAmount = new Vector3Data();
        [Tooltip("The position the tween will start at. Only applies if ToFrom is set to From.")]
        [SerializeField] protected Vector3Data _moveFromPosition = new Vector3Data();

        public override void OnEnter()
        {
            _targetTransform = GetTargetTransform();
            base.OnEnter();
        }

        private Transform _targetTransform;

        private Transform GetTargetTransform()
        {
            Transform result = null;
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
            
            if (!result)
            {
                string errorMessage = $"MoveTransform on Flowchart {name}, Block {ParentBlock.BlockName} " +
                    $"at index {CommandIndex} is missing a target transform.";
                Debug.LogError(errorMessage);
            }
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();
            Vector3 startPos = DecideStartPosition();
            Vector3 endPos = DecideTargetPosition(startPos);
            _targetTransform.position = startPos;

            _ourTween = _tweener.MoveTo(_targetTransform, endPos, _duration);
            return _ourTween;
        }

        protected virtual Vector3 DecideStartPosition()
        {
            Vector3 startPos = Vector3.zero;

            if (_startMode == StartFromMode.FromValue)
            {
                startPos = _moveFromPosition.Value;
            }
            else
            {
                startPos = _targetTransform.position;
            }

            return startPos;
        }

        

        /// <summary>
        /// Determines the target position for the tween based on the Tween Relativity 
        /// setting and the ToFrom setting. The startPos arg is there for when
        /// we need to decide the result relative to it. Of course, it gets ignored
        /// when Tween Relativity is Absolute.
        /// </summary>
        protected virtual Vector3 DecideTargetPosition(Vector3 startPos)
        {
            Vector3 targetPos = Vector3.zero;

            if (_relativity == TweenRelativity.Absolute)
            {
                targetPos = _absoluteDest.Value;
            }
            else
            {
                targetPos = startPos + _moveByAmount.Value;
            }

            return targetPos;
        }

        protected override void StopAllTweens()
        {
            if (_toMove == null || _targetTransform == null)
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

            string targetStr;
            if (_toMove.RepresentingVar)
            {
                targetStr = $"{_toMove.VarRef.Key}";
            }
            else
            {
                targetStr = $"{_targetTransform.name}";
            }

            string toFromStr = _startMode.ToString();

            if (_startMode == StartFromMode.FromValue)
            {
                string moveFromStr;
                if (_moveFromPosition.RepresentingVar)
                {
                    moveFromStr = $"{_moveFromPosition.VarRef.Key}";
                }
                else
                {
                    moveFromStr = $"{_moveFromPosition.Value}";
                }
                toFromStr += $"({moveFromStr}) to";
            }

            string targDestStr;
            if (_relativity == TweenRelativity.Absolute)
            {
                if (_absoluteDest.RepresentingVar)
                {
                    targDestStr = $"{_absoluteDest.VarRef.Key}";
                }
                else
                {
                    targDestStr = $"{_absoluteDest.Value}";
                }
            }
            else
            {
                if (_moveByAmount.RepresentingVar)
                {
                    targDestStr = $"{_moveByAmount.VarRef.Key}";
                }
                else
                {
                    targDestStr = $"{_moveByAmount.Value}";
                }
                targDestStr += " relative to start";

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

            string result = $"{targetStr} {toFromStr} {targDestStr} over {durationStr} seconds.";
            return result;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_toMoveOld != null && _toMoveOld.Value != null)
            {
                if (_toMoveOld.RepresentingVar)
                {
                    _toMove.VarRef = _toMoveOld.VarRef;
                }
                else
                {
                    _toMove.BoxedValue = _toMoveOld.Value;
                }

                _toMoveOld = null;
            }
        }

        [FormerlySerializedAs("_toMove")]
        [SerializeField][HideInInspector] protected ComponentData _toMoveOld = new ComponentData();
    }

}