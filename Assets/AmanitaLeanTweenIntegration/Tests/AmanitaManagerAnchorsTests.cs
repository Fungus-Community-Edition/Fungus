using NUnit.Framework;
using UnityEngine;

using System.Linq;
using Amanita;
using Amanita.LeanTweenIntegration;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;
using System.Collections.Generic;
using System.Collections;

[TestFixture]
public class AmanitaManagerAnchorsTests
{
    [SetUp]
    public void SetUp()
    {
        // Ensure a clean static state
        AmanitaManager.ResetStaticsForTest();

        // Destroy any existing AmanitaManager instances to avoid test interference.
        List<AmanitaManager> existing;
#if UNITY_6000_0_OR_NEWER
        existing = UnityObj.FindObjectsByType<AmanitaManager>(FindObjectsSortMode.None).ToList();
#else
        existing = UnityObj.FindObjectsOfType<AmanitaManager>(true).ToList();
#endif
        foreach (var ex in existing)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () => { }; // noop to keep editor happy
#endif
            if (ex != null && ex.gameObject != null)
                UnityObj.DestroyImmediate(ex.gameObject);
        }
        existing.Clear(); // To make sure they get cleaned up before proceeding.

        manager = AmanitaManager.EnsureExists(); 
        // ^Need to make sure it's set up based on the prefab, since that contains
        // dependencies of AmanitaManager
        managerGO = manager.gameObject;

        // Ensure initialization runs (mirrors runtime Awake/Init path).
        manager.Init();

        firstAdaptor = ScriptableObject.CreateInstance<AmaniLeanTweenAdapter>();
        secondAdaptor = ScriptableObject.CreateInstance<AmaniLeanTweenAdapter>();

        _toDestroyInTearDown.Add(managerGO);
        _toDestroyInTearDown.Add(firstAdaptor);
        _toDestroyInTearDown.Add(secondAdaptor);
    }

    private AmanitaManager manager;
    private GameObject managerGO;
    protected readonly IList<UnityObj> _toDestroyInTearDown = new List<UnityObj>();
    protected AmaniLeanTweenAdapter firstAdaptor, secondAdaptor;

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in _toDestroyInTearDown)
        {
            if (obj != null)
                UnityObj.DestroyImmediate(obj);
        }

        managerGO = null;
        manager = null;

        AmanitaManager.ResetStaticsForTest();
        _toDestroyInTearDown.Clear();
    }

    [Test]
    public void GetOrCreateAnchorFor_CreatesAndReusesAnchor_PerAdapter()
    {
        // Create anchors
        var firstAnchor = manager.GetOrCreateAnchorFor(firstAdaptor);
        Assert.IsNotNull(firstAnchor, "Anchor for first adaptor should not be null");

        var secondAnchor = manager.GetOrCreateAnchorFor(secondAdaptor);
        Assert.IsNotNull(secondAnchor, "Anchor for second adaptor should not be null");

        // TweenAnchorHolder should exist under manager
        var anchorHolder = manager.transform.Find("TweenAnchorHolder");
        Assert.IsNotNull(anchorHolder, "TweenAnchorHolder should be created and parented under manager");
        Assert.AreEqual(anchorHolder, firstAnchor.transform.parent,
            "Anchor should be parented under TweenAnchorHolder");

        // Reuse: calling again returns same GameObject
        var shouldBeFirstAnchor = manager.GetOrCreateAnchorFor(firstAdaptor);
        Assert.AreSame(firstAnchor, shouldBeFirstAnchor, 
            "Subsequent GetOrCreateAnchorFor for same adapter should return the same anchor instance");

        // Different adapter -> different anchor
        var shouldBeSecondAnchor = manager.GetOrCreateAnchorFor(secondAdaptor);
        Assert.IsNotNull(shouldBeSecondAnchor, "Anchor for adapter2 should not be null");
        Assert.AreNotSame(firstAnchor, shouldBeSecondAnchor, "Different adapters should get different anchors");

    }

    // Converted to a UnityTest so Destroy() will call OnDestroy on the next frame.
    [UnityTest]
    public IEnumerator RemoveAnchorFor_RemovesAnchorAndOnManagerDestroy_CleansUpAnchors()
    {
        var adapter = firstAdaptor;
        var anchor = manager.GetOrCreateAnchorFor(adapter);
        Assert.IsNotNull(anchor);

        int anchorId = anchor.GetInstanceID();

        // Remove anchor explicitly
        manager.RemoveAnchorFor(adapter);

        // Ensure that specific anchor object has been removed (scene-only query)
        IList<GameObject> allGameObjectsInScene = GetAllGameObjectsInScene();
        bool wasItRemoved = !allGameObjectsInScene.Any(elem => elem.GetInstanceID() == anchorId);
        Assert.IsTrue(wasItRemoved, "Anchor should be removed after RemoveAnchorFor");

        // Recreate anchor to test manager destroy cleanup
        var anchor2 = manager.GetOrCreateAnchorFor(adapter);
        Assert.IsNotNull(anchor2);
        int anchor2Id = anchor2.GetInstanceID();

        // Destroy manager using Destroy so Unity will invoke OnDestroy.
        UnityObj.Destroy(manager.gameObject);

        // Wait a couple frames for Unity to process destruction and call OnDestroy.
        yield return null;
        yield return null;

        // Re-query the scene after destruction; do not reuse old cached arrays.
        allGameObjectsInScene = GetAllGameObjectsInScene();
        var stillExists = allGameObjectsInScene.Any(g => g.GetInstanceID() == anchor2Id);
        Assert.IsFalse(stillExists, "Specific anchor instance should be cleaned up when AmanitaManager is destroyed");

        // Ensure the singleton reference has been cleared.
        Assert.IsNull(AmanitaManager.S, "AmanitaManager.S should be null after manager destruction");

        // Reset static pointer to ensure test isolation
        AmanitaManager.ResetStaticsForTest();
    }

    protected virtual IList<GameObject> GetAllGameObjectsInScene()
    {
#if UNITY_6000_0_OR_NEWER
        return UnityObj.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
        return UnityObj.FindObjectsOfType<GameObject>(true);
#endif
    }
}
