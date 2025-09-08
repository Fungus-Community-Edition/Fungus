using Amanita;
using Amanita.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityObj = UnityEngine.Object;

public class DefaultAdapterTests : MonoBehaviour
{
    protected GameObject _testGo;
    protected DefaultTweenAdapter _adapter;

    protected const float Duration = 1f;
    protected const float Epsilon = 1e-3f;

    [SetUp]
    public virtual void SetUp()
    {
        _testGo = new GameObject("TweenTestGO");
        AmanitaManager prefab = Resources.Load<AmanitaManager>(pathToManager);
        if (prefab == null)
        {
            throw new System.MissingFieldException("Wrong path to the Amanita Manager");
        }
        manager = UnityObj.Instantiate(prefab);
        _adapter = AmanitaManager.DefaultTweener; //ScriptableObject.CreateInstance<DefaultTweenAdapter>();
    }

    protected AmanitaManager manager;
    protected readonly string pathToManager = "Prefabs/AmanitaManager";

    [TearDown]
    public virtual void TearDown()
    {
        if (_testGo) UnityObj.DestroyImmediate(_testGo);
        if (manager) UnityObj.DestroyImmediate(manager.gameObject);
    }
}
