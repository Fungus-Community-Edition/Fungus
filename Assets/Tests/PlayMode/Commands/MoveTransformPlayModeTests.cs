using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AtMycelia.AmaniTween.VScripting;
using UnityObj = UnityEngine.Object;

namespace AmaniTweenTests.Commands
{
    public class MoveTransformPlayModeTests : CommandTestBase<MoveTransformPlayModeTests.MoveTransformTestWrapper>
    {
        private Transform _target;
        private GameObject _targetGo;

        protected override void ConfigureCommand(MoveTransformTestWrapper cmd)
        {
            _targetGo = new GameObject("MoveTransformTarget");
            _target = _targetGo.transform;

            cmd.SetTarget(_target);
            cmd.SetDuration(_duration);
            cmd.SetRelativity(TweenRelativity.Absolute);
            cmd.SetToFrom(StartFromMode.Current);
        }

        public override void TearDown()
        {
            if (_targetGo != null)
            {
                UnityObj.DestroyImmediate(_targetGo);
            }

            _target = null;
            _targetGo = null;

            base.TearDown();
        }

        protected override void AssertFinalState()
        {
        }

        [UnityTest]
        public IEnumerator MoveTransform_AbsoluteTo_MovesToTarget()
        {
            Vector3 destination = new Vector3(5f, 2f, -1f);

            _target.position = Vector3.zero;
            _command.SetAbsoluteDest(destination);
            _command.SetRelativity(TweenRelativity.Absolute);
            _command.SetToFrom(StartFromMode.Current);
            _command.SetDuration(_duration);

            yield return RunBlockAndWait();

            Assert.That(Vector3.Distance(_target.position, destination), Is.LessThan(_epsilon),
                "Target did not reach absolute destination.");
        }

        [UnityTest]
        public IEnumerator MoveTransform_RelativeTo_MovesByAmount()
        {
            Vector3 startPos = new Vector3(1f, 1f, 1f);
            Vector3 moveBy = new Vector3(2f, 0f, -1f);
            Vector3 expected = startPos + moveBy;

            _target.position = startPos;
            _command.SetMoveByAmount(moveBy);
            _command.SetRelativity(TweenRelativity.Relative);
            _command.SetToFrom(StartFromMode.Current);
            _command.SetDuration(_duration);

            yield return RunBlockAndWait();

            Assert.That(Vector3.Distance(_target.position, expected), Is.LessThan(_epsilon),
                "Target did not reach relative destination.");
        }

        [UnityTest]
        public IEnumerator MoveTransform_From_StartsAtFromPosition_ThenMovesToTarget()
        {
            Vector3 fromPos = new Vector3(10f, 0f, 0f);
            Vector3 destination = new Vector3(0f, 2f, -3f);

            _target.position = new Vector3(3f, 3f, 3f);
            _command.SetMoveFromPosition(fromPos);
            _command.SetAbsoluteDest(destination);
            _command.SetRelativity(TweenRelativity.Absolute);
            _command.SetToFrom(StartFromMode.FromValue);
            _command.SetDuration(_duration);

            _flowchart.ExecuteBlock(_block);

            Assert.That(Vector3.Distance(_target.position, fromPos), Is.LessThan(_epsilon),
                "Target did not snap to the From position before tweening.");

            yield return new WaitForSeconds(_duration + 0.05f);

            Assert.That(Vector3.Distance(_target.position, destination), Is.LessThan(_epsilon),
                "Target did not reach destination after From tween.");
        }

        public class MoveTransformTestWrapper : MoveTransform
        {
            public void SetTarget(Component component)
            {
                _toMove.SetFor(component.GetType());
                _toMove.BoxedValue = component;
            }

            public void SetAbsoluteDest(Vector3 value)
            {
                _absoluteDest.Value = value;
            }

            public void SetMoveByAmount(Vector3 value)
            {
                _moveByAmount.Value = value;
            }

            public void SetMoveFromPosition(Vector3 value)
            {
                _moveFromPosition.Value = value;
            }

            public void SetRelativity(TweenRelativity relativity)
            {
                _relativity = relativity;
            }

            public void SetToFrom(StartFromMode toFrom)
            {
                _startMode = toFrom;
            }

            public void SetDuration(float duration)
            {
                _duration.Value = duration;
            }
        }
    }
}