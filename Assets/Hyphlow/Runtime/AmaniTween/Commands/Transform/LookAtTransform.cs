using UnityEngine;
using System.Collections.Generic;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("BI Tween",
        "Simple/Look At",
        "Rotates a Component's transform to face a target over time.")]
    public class LookAtTransform : BaseSimpleTweenCommand
    {
        [Tooltip("The Component or GameObject with the Transform to rotate.")]
        [ContentTypeConstraint(typeof(Component), typeof(GameObject))]
        [SerializeField] protected AnyVariableData _looker = new AnyVariableData();

        [Tooltip("The Component or GameObject to look at.")]
        [ContentTypeConstraint(typeof(Component), typeof(GameObject))]
        [SerializeField] protected AnyVariableData _lookTarget = new AnyVariableData();

        [Tooltip("Optional Component or GameObject whose up direction should be used. " +
            "If empty, uses world up.")]
        [ContentTypeConstraint(typeof(Component), typeof(GameObject))]
        [SerializeField] protected AnyVariableData _upBase = new AnyVariableData();

        public enum RotationType
        {
            Null, TwoD, ThreeD
        }
        [SerializeField] protected RotationType _rotationType = RotationType.TwoD;

        public enum TwoDFacingAxis
        {
            Null, Right, Up
        }
        [SerializeField] protected TwoDFacingAxis _twoDFacingAxis = TwoDFacingAxis.Right;

        [Header("ThreeD Rotation Settings")]
        [Tooltip("Whether to apply the rotation on the X axis.")]
        [SerializeField] protected BooleanData _onX = new BooleanData(false);

        [Tooltip("Whether to apply the rotation on the Y axis.")]
        [SerializeField] protected BooleanData _onY = new BooleanData(false);

        [Tooltip("Whether to apply the rotation on the Z axis.")]
        [SerializeField] protected BooleanData _onZ = new BooleanData(true);

        public override void OnEnter()
        {
            FetchTransforms();
            base.OnEnter();
        }

        protected override void RegisterAllTargets()
        {
            FetchTransforms();
            _allTargets.Add(_lookerTransform);
            _allTargets.Add(_targetTransform);
            if (_upBaseTransform != null)
            {
                _allTargets.Add(_upBaseTransform);
            }
        }
        private void FetchTransforms()
        {
            _lookerTransform = GetTransformFrom(_looker);
            _targetTransform = GetTransformFrom(_lookTarget);
            _upBaseTransform = GetTransformFrom(_upBase);
        }

        private Transform _lookerTransform;
        private Transform _targetTransform;
        private Transform _upBaseTransform;

        private static Transform GetTransformFrom(AnyVariableData data)
        {
            if (data == null)
            {
                return null;
            }

            if (data.BoxedValue is Component comp && comp != null)
            {
                return comp.transform;
            }

            if (data.BoxedValue is GameObject go && go != null)
            {
                return go.transform;
            }

            return null;
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_looker);
            _variableDataCache.Add(_lookTarget);
            _variableDataCache.Add(_upBase);
            _variableDataCache.Add(_onX);
            _variableDataCache.Add(_onY);
            _variableDataCache.Add(_onZ);
        }

        protected override bool AreTargetsValid()
        {
            bool result = _lookerTransform != null && _targetTransform != null;

            if (!result)
            {
                string errorMessage = $"LookAtTransform on Flowchart {name}, Block {ParentBlock.BlockName} " +
                    $"at index {CommandIndex} is missing required transforms.";
                Debug.LogError(errorMessage);
            }

            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();

            Vector3 toTarget = _targetTransform.position - _lookerTransform.position;
            Vector3 up = _upBaseTransform != null ? 
                _upBaseTransform.up : 
                Vector3.up;

            Quaternion targetRotation = DecideTargetRotation(toTarget, up);

            if (_rotationType == RotationType.ThreeD)
            {
                targetRotation = ApplyAxisFilter(targetRotation);
            }

            _ourTween = _tweener.RotateTo(_lookerTransform, targetRotation, _duration);
            return _ourTween;
        }

        private Quaternion DecideTargetRotation(Vector3 toTarget, Vector3 up)
        {
            if (toTarget.sqrMagnitude <= 0f)
            {
                return _lookerTransform.rotation;
            }

            Quaternion result = default;
            switch (_rotationType)
            {
                case RotationType.Null:
                    result = _lookerTransform.rotation; break;
                case RotationType.TwoD:
                    Vector2 dir = new Vector2(toTarget.x, toTarget.y);
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    if (_twoDFacingAxis == TwoDFacingAxis.Up)
                    {
                        angle -= 90f;
                    }
                    result = Quaternion.Euler(0f, 0f, angle); break;
                case RotationType.ThreeD:
                    result = Quaternion.LookRotation(toTarget.normalized, up); break;
            }

            return result;
        }

        private Quaternion ApplyAxisFilter(Quaternion targetRotation)
        {
            if (!_onX && !_onY && !_onZ)
            {
                return _lookerTransform.rotation;
            }

            Vector3 currentEuler = _lookerTransform.rotation.eulerAngles;
            Vector3 targetEuler = targetRotation.eulerAngles;

            if (!_onX)
            {
                targetEuler.x = currentEuler.x;
            }
            if (!_onY)
            {
                targetEuler.y = currentEuler.y;
            }
            if (!_onZ)
            {
                targetEuler.z = currentEuler.z;
            }

            return Quaternion.Euler(targetEuler);
        }

        private ITransformTweenAdapter _tweener;

        protected override void StopAllTweens()
        {
            if (_lookerTransform == null)
            {
                return;
            }

            var manager = TweenManager.S;
            manager.KillAllOn(_lookerTransform);
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
            FetchTransforms();

            if (_lookerTransform == null || _targetTransform == null)
            {
                return "Need a looker and a target.";
            }

            string lookerStr = _looker.RepresentingVar
                ? $"{_looker.VarRef.Key}"
                : $"{_lookerTransform.name}";

            string targetStr = _lookTarget.RepresentingVar
                ? $"{_lookTarget.VarRef.Key}"
                : $"{_targetTransform.name}";

            string upStr = _upBaseTransform == null
                ? "world up"
                : _upBase.RepresentingVar
                    ? $"{_upBase.VarRef.Key}"
                    : $"{_upBaseTransform.name}";

            string axisStr = BuildAxisSummary();

            string durationStr = _duration.RepresentingVar
                ? $"{_duration.VarRef.Key}"
                : $"{_duration.Value}";

            return $"{lookerStr} look at {targetStr} using {upStr} on {axisStr} over {durationStr} second(s).";
        }

        private string BuildAxisSummary()
        {
            if (!_onX && !_onY && !_onZ)
            {
                return "no axes";
            }

            List<string> axes = new List<string>();
            if (_onX)
            {
                axes.Add("X");
            }
            if (_onY)
            {
                axes.Add("Y");
            }
            if (_onZ)
            {
                axes.Add("Z");
            }

            return string.Join("/", axes);
        }
    }
}