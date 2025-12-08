using Amanita;
using Amanita.DOTweenIntegration;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityObj = UnityEngine.Object;

public class DoTweenAdapterTests
{
    protected GameObject _testGo;
    protected AmaniDoTweenAdapter _adapter;

    protected const float Duration = 1f;
    protected const float Epsilon = 1e-3f;

    [SetUp]
    public virtual void SetUp()
    {
        DOTween.KillAll(false);
        _testGo = new GameObject("TweenTestGO");
        manager = AmanitaManager.EnsureExists();
        _adapter = ScriptableObject.CreateInstance<AmaniDoTweenAdapter>();
    }

    protected AmanitaManager manager;

    [TearDown]
    public virtual void TearDown()
    {
        DOTween.KillAll(false);
        if (_testGo) UnityObj.DestroyImmediate(_testGo);
        if (_adapter) UnityObj.DestroyImmediate(_adapter);
        if (manager) UnityObj.DestroyImmediate(manager.gameObject);
    }
}
