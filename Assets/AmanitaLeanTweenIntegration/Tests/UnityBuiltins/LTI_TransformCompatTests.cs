using Amanita.Tweening;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils; // for Vector3EqualityComparer

namespace BuiltinCompat
{
    public class LTI_TransformCompatTests : LeanTweenAdapterTests
    {
        private static readonly LeanTweenCase<Transform, Vector3> MoveCase = new LeanTweenCase<Transform, Vector3>
        {
            Name = "MoveTo_Transform",
            CreateTween = (adapter, t) => adapter.MoveTo(t, new Vector3(1f, 2f, 3f), Duration),
            GetValue = t => t.position,
            SetValue = (t, v) => t.position = v,
            CreateComponent = go => go.transform,
            TargetValue = new Vector3(1f, 2f, 3f)
        };

        private static readonly LeanTweenCase<Transform, Quaternion> RotateCase = new LeanTweenCase<Transform, Quaternion>
        {
            Name = "RotateTo_Transform",
            CreateTween = (adapter, t) => adapter.RotateTo(t, Quaternion.Euler(0f, 90f, 0f), Duration),
            GetValue = t => t.rotation,
            SetValue = (t, v) => t.rotation = v,
            CreateComponent = go => go.transform,
            TargetValue = Quaternion.Euler(0f, 90f, 0f)
        };

        private static readonly LeanTweenCase<Transform, Vector3> ScaleCase = new LeanTweenCase<Transform, Vector3>
        {
            Name = "ScaleTo_Transform",
            CreateTween = (adapter, t) => adapter.ScaleTo(t, new Vector3(2f, 2f, 2f), Duration),
            GetValue = t => t.localScale,
            SetValue = (t, v) => t.localScale = v,
            CreateComponent = go => go.transform,
            TargetValue = new Vector3(2f, 2f, 2f)
        };

        [Test]
        public void Handle_IsValid_MoveTo_ForCoverage()
        {
            var comp = MoveCase.CreateComponent(_testGo);
            var handle = MoveCase.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        [Test]
        public void Handle_IsValid_RotateTo_ForCoverage()
        {
            var comp = RotateCase.CreateComponent(_testGo);
            var handle = RotateCase.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        [Test]
        public void Handle_IsValid_ScaleTo_ForCoverage()
        {
            var comp = ScaleCase.CreateComponent(_testGo);
            var handle = ScaleCase.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        private ITweenHandle _nullHandle, _moveToHandle, _rotateToHandle, _scaleToHandle;

        public override void SetUp()
        {
            base.SetUp();
            _nullHandle = new Amanita.LeanTweenIntegration.LeanTweenHandle(null);
            _moveToHandle = _adapter.MoveTo(_testGo.transform, new Vector3(1f, 2f, 3f), Duration);
            _rotateToHandle = _adapter.RotateTo(_testGo.transform, Quaternion.Euler(0f, 90f, 0f), Duration);
            _scaleToHandle = _adapter.ScaleTo(_testGo.transform, new Vector3(2f, 2f, 2f), Duration);
        }

        public override void TearDown()
        {
            base.TearDown();
            _nullHandle = _moveToHandle = _rotateToHandle = _scaleToHandle = null;
        }

        [Test]
        public void CreateTween_MoveTo_ValidHandle()
        {
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(_moveToHandle, "Returned handle is not a LeanTweenHandle.");
        }

        [Test]
        public void MoveTo_KillStopsItProperly()
        {
            Assert.DoesNotThrow(() => _moveToHandle.Kill(), "Killing the tween should not throw.");
            Assert.IsFalse(_moveToHandle.IsPlaying, "Handle should not be playing after Kill.");
        }

        [Test]
        public void CreateTween_RotateTo_ValidHandle()
        {
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(_rotateToHandle, "Returned handle is not a LeanTweenHandle.");
        }

        [Test]
        public void RotateTo_KillStopsItProperly()
        {
            Assert.DoesNotThrow(() => _rotateToHandle.Kill(), "Killing the tween should not throw.");
            Assert.IsFalse(_rotateToHandle.IsPlaying, "Handle should not be playing after Kill.");
        }

        [Test]
        public void CreateTween_ScaleTo_ValidHandle()
        {
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(_scaleToHandle, "Returned handle is not a LeanTweenHandle.");
        }

        [Test]
        public void ScaleTo_KillStopsItProperly()
        {
            Assert.DoesNotThrow(() => _scaleToHandle.Kill(), "Killing the tween should not throw.");
            Assert.IsFalse(_scaleToHandle.IsPlaying, "Handle should not be playing after Kill.");
        }

        [Test]
        public void HandleWithNullTween_IsPlayingFalseOnInit()
        {
            Assert.IsFalse(_nullHandle.IsPlaying, "New LeanTweenHandle(null) should report IsPlaying = false.");
        }

        [Test]
        public void HandleWithNullTween_KillDoesNotThrow()
        {
            Assert.DoesNotThrow(() => _nullHandle.Kill(), "Calling Kill on a null-backed handle should not throw.");
        }

        [Test]
        public void HandleWithNullTween_IsPlayingFalseAfterKill()
        {
            _nullHandle.Kill();
            Assert.IsFalse(_nullHandle.IsPlaying, "Handle should still report IsPlaying = false after Kill.");
        }

        [UnityTest]
        public IEnumerator MoveTo_AppliesToTargetTransform()
        {
            var expected = new Vector3(1f, 2f, 3f);
            _testGo.transform.position = Vector3.zero;
            yield return new UnityEngine.WaitForSeconds(Duration + 0.05f);
            var actual = _testGo.transform.position;
            var vec3 = new Vector3EqualityComparer(Epsilon);
            Assert.That(actual, Is.EqualTo(expected).Using(vec3));
        }

        [UnityTest]
        public IEnumerator RotateTo_AppliesToTargetTransform()
        {
            var expectedEuler = new Vector3(0f, 90f, 0f);
            _testGo.transform.rotation = Quaternion.identity;
            yield return new UnityEngine.WaitForSeconds(Duration + 0.05f);
            var actualEuler = _testGo.transform.rotation.eulerAngles;
            float norm(float a) => Mathf.Repeat(a, 360f);
            Assert.AreEqual(norm(expectedEuler.x), norm(actualEuler.x), 1f, "X rotation mismatch (±1°).");
            Assert.AreEqual(norm(expectedEuler.y), norm(actualEuler.y), 1f, "Y rotation mismatch (±1°).");
            Assert.AreEqual(norm(expectedEuler.z), norm(actualEuler.z), 1f, "Z rotation mismatch (±1°).");
        }

        [UnityTest]
        public IEnumerator ScaleTo_AppliesToTargetTransform()
        {
            var expected = new Vector3(2f, 2f, 2f);
            _testGo.transform.localScale = Vector3.one;
            yield return new UnityEngine.WaitForSeconds(Duration + 0.05f);
            var actual = _testGo.transform.localScale;
            Assert.AreEqual(expected.x, actual.x, Epsilon, "X scale did not reach expected value.");
            Assert.AreEqual(expected.y, actual.y, Epsilon, "Y scale did not reach expected value.");
            Assert.AreEqual(expected.z, actual.z, Epsilon, "Z scale did not reach expected value.");
        }
    }
}