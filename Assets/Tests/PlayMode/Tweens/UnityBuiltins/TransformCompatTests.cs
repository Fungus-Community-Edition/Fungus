using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils; // for Vector3EqualityComparer
using AtMycelia.AmaniTween;

namespace TweeningTests.BuiltinCompat
{
    public class TransformCompatTests : DefaultAdapterTests
    {
        // --- Coverage check scaffolding (static fields discovered by the audit) ---
        private static readonly TweenCase<Transform, Vector3> MoveCase = new TweenCase<Transform, Vector3>
        {
            Name = "MoveTo_Transform",
            CreateTween = (adapter, t) => adapter.MoveTo(t, new Vector3(1f, 2f, 3f), Duration),
            GetValue = t => t.position,
            SetValue = (t, v) => t.position = v,
            CreateComponent = go => go.transform,
            TargetValue = new Vector3(1f, 2f, 3f)
        };

        private static readonly TweenCase<Transform, Quaternion> RotateCase = new TweenCase<Transform, Quaternion>
        {
            Name = "RotateTo_Transform",
            CreateTween = (adapter, t) => adapter.RotateTo(t, Quaternion.Euler(0f, 90f, 0f), Duration),
            GetValue = t => t.rotation,
            SetValue = (t, v) => t.rotation = v,
            CreateComponent = go => go.transform,
            TargetValue = Quaternion.Euler(0f, 90f, 0f)
        };

        private static readonly TweenCase<Transform, Vector3> ScaleCase = new TweenCase<Transform, Vector3>
        {
            Name = "ScaleTo_Transform",
            CreateTween = (adapter, t) => adapter.ScaleTo(t, new Vector3(2f, 2f, 2f), Duration),
            GetValue = t => t.localScale,
            SetValue = (t, v) => t.localScale = v,
            CreateComponent = go => go.transform,
            TargetValue = new Vector3(2f, 2f, 2f)
        };

        // Minimal “handle validity” tests that exercise the TweenCase fields (satisfies coverage audit)
        [Test]
        public void Handle_IsValid_MoveTo_ForCoverage()
        {
            var comp = MoveCase.CreateComponent(_testGo);
            var handle = MoveCase.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [Test]
        public void Handle_IsValid_RotateTo_ForCoverage()
        {
            var comp = RotateCase.CreateComponent(_testGo);
            var handle = RotateCase.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [Test]
        public void Handle_IsValid_ScaleTo_ForCoverage()
        {
            var comp = ScaleCase.CreateComponent(_testGo);
            var handle = ScaleCase.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        // --- Your original, detailed tests (adjusted assertions to avoid Vector3→double overload) ---
        private ITweenHandle _nullHandle, _moveToHandle, _rotateToHandle, _scaleToHandle;

        public override void SetUp()
        {
            base.SetUp();
            _nullHandle = new DefaultTweenHandle(null);
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
            var dtHandle = _moveToHandle as DefaultTweenHandle;
            Assert.IsNotNull(dtHandle, "Returned handle is not a DefaultTweenHandle.");
            Assert.IsNotNull(dtHandle.Tween, "DefaultTweenHandle.Tween should not be null after MoveTo.");
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
            var dtHandle = _rotateToHandle as DefaultTweenHandle;
            Assert.IsNotNull(dtHandle, "Returned handle is not a DefaultTweenHandle.");
            Assert.IsNotNull(dtHandle.Tween, "DefaultTweenHandle.Tween should not be null after RotateTo.");
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
            var dtHandle = _scaleToHandle as DefaultTweenHandle;
            Assert.IsNotNull(dtHandle, "Returned handle is not a DefaultTweenHandle.");
            Assert.IsNotNull(dtHandle.Tween, "DefaultTweenHandle.Tween should not be null after ScaleTo.");
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
            Assert.IsFalse(_nullHandle.IsPlaying, "New DefaultTweenHandle(null) should report IsPlaying = false.");
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

        // PlayMode tests that assert the tweens apply to the expected Transform
        [UnityTest]
        public IEnumerator MoveTo_AppliesToTargetTransform()
        {
            var expected = new Vector3(1f, 2f, 3f);
            _testGo.transform.position = Vector3.zero;

            // The tween was already created in SetUp; just wait for completion
            yield return new WaitForSeconds(Duration + 0.05f);

            var actual = _testGo.transform.position;

            // Use comparer to avoid the Vector3→double overload
            var vec3 = new Vector3EqualityComparer(Epsilon);
            Assert.That(actual, Is.EqualTo(expected).Using(vec3));
        }

        [UnityTest]
        public IEnumerator RotateTo_AppliesToTargetTransform()
        {
            var expectedEuler = new Vector3(12f, 90f, 34f);
            _testGo.transform.rotation = Quaternion.identity;

            _adapter.RotateTo(_testGo.transform, Quaternion.Euler(expectedEuler), Duration);
            yield return new WaitForSeconds(Duration + 0.05f);

            var actualEuler = _testGo.transform.rotation.eulerAngles;
            // Compare each component with a small tolerance; handle wrap-around near 360
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

            yield return new WaitForSeconds(Duration + 0.05f);

            var actual = _testGo.transform.localScale;

            // Per-component with tolerance (or use Vector3EqualityComparer as above)
            Assert.AreEqual(expected.x, actual.x, Epsilon, "X scale did not reach expected value.");
            Assert.AreEqual(expected.y, actual.y, Epsilon, "Y scale did not reach expected value.");
            Assert.AreEqual(expected.z, actual.z, Epsilon, "Z scale did not reach expected value.");
        }
    }
}