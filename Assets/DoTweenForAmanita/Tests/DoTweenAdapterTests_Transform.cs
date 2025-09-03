using Amanita.ThirdPartyInt.DGDOTween;
using Amanita.Tweening;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class DoTweenAdapterTests_Transform
{
    [SetUp]
    public virtual void SetUp()
    {
        // Ensure DOTween state is clean before each test
        DOTween.KillAll(false);
        testGO = new GameObject("DoTweenTestGO");
        adapter = ScriptableObject.CreateInstance<AmaniDoTweenAdapter>();
        _nullHandle = new DOTweenHandle(null);
        _moveToHandle = adapter.MoveTo(testGO.transform, new Vector3(1f, 2f, 3f), 1f);
        _rotateToHandle = adapter.RotateTo(testGO.transform, Quaternion.Euler(0f, 90f, 0f), 1f);
        _scaleToHandle = adapter.ScaleTo(testGO.transform, new Vector3(2f, 2f, 2f), 1f);
    }

    protected GameObject testGO;
    protected AmaniDoTweenAdapter adapter;
    protected ITweenHandle _nullHandle, _moveToHandle, _rotateToHandle, _scaleToHandle;
    protected const float Epsilon = 1e-3f;

    [TearDown]
    public virtual void TearDown()
    {
        // Kill any remaining tweens and destroy created objects
        DOTween.KillAll(false);
        if (testGO != null) Object.DestroyImmediate(testGO);
        if (adapter != null) Object.DestroyImmediate(adapter);

        _nullHandle = _moveToHandle = _rotateToHandle = _scaleToHandle = null;
    }

    [Test]
    public virtual void CreateTween_MoveTo_ValidHandle()
    {
        var dtHandle = _moveToHandle as DOTweenHandle;
        Assert.IsNotNull(dtHandle, "Returned handle is not a DOTweenHandle.");
        Assert.IsNotNull(dtHandle.Tween, "DOTweenHandle.Tween should not be null after MoveTo.");
    }

    [Test]
    public virtual void MoveTo_KillStopsItProperly()
    {
        Assert.DoesNotThrow(() => _moveToHandle.Kill(), "Killing the tween should not throw.");
        Assert.IsFalse(_moveToHandle.IsPlaying, "Handle should not be playing after Kill.");
    }

    [Test]
    public virtual void CreateTween_RotateTo_ValidHandle()
    {
        var dtHandle = _rotateToHandle as DOTweenHandle;
        Assert.IsNotNull(dtHandle, "Returned handle is not a DOTweenHandle.");
        Assert.IsNotNull(dtHandle.Tween, "DOTweenHandle.Tween should not be null after RotateTo.");
    }

    [Test]
    public virtual void RotateTo_KillStopsItProperly()
    {
        Assert.DoesNotThrow(() => _rotateToHandle.Kill(), "Killing the tween should not throw.");
        Assert.IsFalse(_rotateToHandle.IsPlaying, "Handle should not be playing after Kill.");
    }

    [Test]
    public virtual void CreateTween_ScaleTo_ValidHandle()
    {
        var dtHandle = _scaleToHandle as DOTweenHandle;
        Assert.IsNotNull(dtHandle, "Returned handle is not a DOTweenHandle.");
        Assert.IsNotNull(dtHandle.Tween, "DOTweenHandle.Tween should not be null after ScaleTo.");
    }

    [Test]
    public virtual void ScaleTo_KillStopsItProperly()
    {
        Assert.DoesNotThrow(() => _scaleToHandle.Kill(), "Killing the tween should not throw.");
        Assert.IsFalse(_scaleToHandle.IsPlaying, "Handle should not be playing after Kill.");
    }

    [Test]
    public virtual void HandleWithNullTween_IsPlayingFalseOnInit()
    {
        Assert.IsFalse(_nullHandle.IsPlaying, "New DOTweenHandle(null) should report IsPlaying = false.");
    }

    [Test]
    public virtual void HandleWithNullTween_KillDoesNotThrow()
    {
        Assert.DoesNotThrow(() => _nullHandle.Kill(), "Calling Kill on a null-backed handle should not throw.");
    }

    [Test]
    public virtual void HandleWithNullTween_IsPlayingFalseAfterKill()
    {
        _nullHandle.Kill();
        Assert.IsFalse(_nullHandle.IsPlaying, "Handle should still report IsPlaying = false after Kill.");
    }

    // PlayMode tests that assert the tweens apply to the expected Transform
    [UnityTest]
    public virtual IEnumerator MoveTo_AppliesToTargetTransform()
    {
        var expected = new Vector3(1f, 2f, 3f);
        // Wait slightly longer than the tween duration to allow DOTween to complete
        yield return new WaitForSeconds(1.05f);

        var actual = testGO.transform.position;
        Assert.AreEqual(expected.x, actual.x, Epsilon, "X position did not reach expected value.");
        Assert.AreEqual(expected.y, actual.y, Epsilon, "Y position did not reach expected value.");
        Assert.AreEqual(expected.z, actual.z, Epsilon, "Z position did not reach expected value.");
    }

    [UnityTest]
    public virtual IEnumerator RotateTo_AppliesToTargetTransform()
    {
        var expectedEuler = new Vector3(0f, 90f, 0f);
        yield return new WaitForSeconds(1.05f);

        var actualEuler = testGO.transform.rotation.eulerAngles;
        // Compare each euler component; handle wrap-around for angles near 360
        float actualY = Mathf.Repeat(actualEuler.y + 360f, 360f);
        float expectedY = Mathf.Repeat(expectedEuler.y + 360f, 360f);
        Assert.AreEqual(expectedY, actualY, 1f, "Y rotation did not reach expected value (tolerance 1 deg).");
    }

    [UnityTest]
    public virtual IEnumerator ScaleTo_AppliesToTargetTransform()
    {
        var expected = new Vector3(2f, 2f, 2f);
        yield return new WaitForSeconds(1.05f);

        var actual = testGO.transform.localScale;
        Assert.AreEqual(expected.x, actual.x, Epsilon, "X scale did not reach expected value.");
        Assert.AreEqual(expected.y, actual.y, Epsilon, "Y scale did not reach expected value.");
        Assert.AreEqual(expected.z, actual.z, Epsilon, "Z scale did not reach expected value.");
    }
}
