using Amanita.ThirdPartyInt.DGDOTween;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityObj = UnityEngine.Object;

public class DoTweenAdapterTests : MonoBehaviour
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
        _adapter = ScriptableObject.CreateInstance<AmaniDoTweenAdapter>();
    }

    [TearDown]
    public virtual void TearDown()
    {
        DOTween.KillAll(false);
        if (_testGo) UnityObj.DestroyImmediate(_testGo);
        if (_adapter) UnityObj.DestroyImmediate(_adapter);
    }
}
