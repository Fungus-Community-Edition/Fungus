using System.Collections;
using AtMycelia.Amanita;
using Lorekeeper;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

#if ENABLE_INPUT_SYSTEM
#endif

[TestFixture]
public class AmanitaManagerTests
{
    private AmanitaManager activeManager;

    [SetUp]
    public void SetUp()
    {
        CleanupScene();
        AmanitaManager.ResetStaticsForTest();
    }

    private static void CleanupScene()
    {
        DestroyManagers();
        DestroyEventSystems();
    }

    private static void DestroyManagers()
    {
        var managers = Object.FindObjectsByType<AmanitaManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < managers.Length; i++)
        {
            var manager = managers[i];
            if (manager == null)
            {
                continue;
            }

            var go = manager.gameObject;
            if (go == null)
            {
                continue;
            }

#if UNITY_EDITOR
            Object.DestroyImmediate(go);
#else
            Object.Destroy(go);
#endif
        }
    }

    private static void DestroyEventSystems()
    {
        var systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < systems.Length; i++)
        {
            var system = systems[i];
            if (system == null)
            {
                continue;
            }

            var go = system.gameObject;
            if (go == null)
            {
                continue;
            }

#if UNITY_EDITOR
            Object.DestroyImmediate(go);
#else
            Object.Destroy(go);
#endif
        }
    }

    [TearDown]
    public void TearDown()
    {
        CleanupScene();
        AmanitaManager.ResetStaticsForTest();
        activeManager = null;
    }

    private IEnumerator CreateManagerAsync()
    {
        activeManager = AmanitaManager.EnsureExists();
        Assert.IsNotNull(activeManager, "EnsureExists should return a valid AmanitaManager instance.");

        yield return null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Duplicate_Managers_Destroy_Themselves()
    {
        yield return CreateManagerAsync();
        Assert.IsNotNull(AmanitaManager.S, "Primary AmanitaManager instance was not initialized.");

        var prefab = Resources.Load<AmanitaManager>(AmanitaConstants.PathToAmanitaManagerPrefab);
        Assert.IsNotNull(prefab, "Unable to load AmanitaManager prefab from Resources.");

        var duplicate = Object.Instantiate(prefab);
        duplicate.gameObject.name = "Duplicate_AmanitaManager";
        yield return null;

        Assert.IsTrue(duplicate == null, "Duplicate AmanitaManager should self-destruct when another instance already exists.");
    }

    [UnityTest]
    public IEnumerator ShadowDatabaseAsset_IsAvailable()
    {
        yield return CreateManagerAsync();

        var loadedDb = AmanitaManager.ShadowDB;
        Assert.IsNotNull(loadedDb, "ShadowDatabase asset should be available via AmanitaManager.ShadowDB.");

        var directResource = Resources.Load<ShadowDatabase>("ShadowDatabase");
        Assert.IsNotNull(directResource, "ShadowDatabase asset is missing from Resources/ShadowDatabase.");
    }

}
