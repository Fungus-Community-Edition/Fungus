using System.Collections;
using System.IO;
using Amanita;
using Amanita.VScripting;
using Lorekeeper;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEditor;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
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

    [UnityTest]
    public IEnumerator EnsureExists_AddsEventSystem_WithInputModule()
    {
        yield return CreateManagerAsync();

        var eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        Assert.IsNotNull(eventSystem, "AmanitaManager should guarantee an EventSystem exists in the scene.");

#if ENABLE_INPUT_SYSTEM
        var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        Assert.IsNotNull(inputModule, "EventSystem should include an InputSystemUIInputModule.");
        Assert.AreSame(eventSystem.gameObject, inputModule.gameObject, "InputSystemUIInputModule must reside on the EventSystem GameObject.");
#endif
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

    [UnityTest]
    public IEnumerator GuidRegistries_Exist_For_Flowchart_And_VariableSourceAsset()
    {
        yield return CreateManagerAsync();

        var flowchartRegistry = AmanitaManager.GetOrAddGuidRegistryFor<Flowchart>();
        var vsaRegistry = AmanitaManager.GetOrAddGuidRegistryFor<VariableSourceAsset>();

        Assert.IsNotNull(flowchartRegistry, "Flowchart GuidRegistry should have been created.");
        Assert.IsNotNull(vsaRegistry, "VariableSourceAsset GuidRegistry should have been created.");

        bool fcRegistryCreated = AssetDatabase.GetAssetPath(flowchartRegistry) != string.Empty;
        bool vsaRegistryCreated = AssetDatabase.GetAssetPath(vsaRegistry) != string.Empty;
        Assert.IsTrue(fcRegistryCreated, "Flowchart GuidRegistry asset should exist on disk.");
        Assert.IsTrue(vsaRegistryCreated, "VariableSourceAsset GuidRegistry asset should exist on disk.");
    }

    [UnityTest]
    public IEnumerator Init_SetsUp_VariableRegistry()
    {
        yield return CreateManagerAsync();

        Assert.IsNotNull(activeManager.VariableRegistry, "VariableRegistry should be initialized during AmanitaManager.Init.");
        Assert.IsNotNull(activeManager.VariableRegistry.Variables, "VariableRegistry should expose a variables dictionary.");
    }

    private static string GetGuidRegistryPath(string typeName)
    {
        string relativePath = Path.Combine("Resources", "GuidRegistries", $"{typeName}GuidRegistry.asset");
        return Path.Combine(Application.dataPath, relativePath);
    }
}
