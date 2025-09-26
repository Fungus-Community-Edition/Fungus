using Amanita;
using Amanita.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityObj = UnityEngine.Object;

public class DefaultAdapterTests : MonoBehaviour
{
    protected DefaultTweenAdapter _adapter;

    protected const float Duration = 1f;
    protected const float Epsilon = 1e-3f;

    [SetUp]
    public virtual void SetUp()
    {
        _testGo = new GameObject("TweenTestGO");
        manager = AmanitaManager.EnsureExists();
        _adapter = AmanitaManager.DefaultTweener; 
    }

    protected GameObject _testGo;
    protected AmanitaManager manager;

    [TearDown]
    public virtual void TearDown()
    {
        if (_testGo) UnityObj.DestroyImmediate(_testGo);
        if (manager) UnityObj.DestroyImmediate(manager.gameObject);
    }
}
