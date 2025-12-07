using Amanita.SaveSys;
using Amanita.SaveSys.EditorUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObj = UnityEngine.Object;
using Type = System.Type;

[TestFixture]
public class SaveSysListControllerTests
{
    private SaveSystemSettings _settingsAsset;
    private SaveSysSettingsTypeCache _typeCache;
    private ListView _applierListView, _codecListView;
    private SaveSysListController<ISaveDataApplier> _appliersController;
    private SaveSysListController<IMainSaveCodec> _codecsController;

    private readonly IList<UnityObj> destroyOnTearDown = new List<UnityObj>();

    [SetUp]
    public void SetUp()
    {
        _settingsAsset = ScriptableObject.CreateInstance<SaveSystemSettings>();

        var dummyApplier = ScriptableObject.CreateInstance<DummyApplier>();
        var dummyCodec = ScriptableObject.CreateInstance<DummyCodec>();

        _settingsAsset.AddMainApplier(dummyApplier);
        _settingsAsset.AddMainCodec(dummyCodec);

        destroyOnTearDown.Add(_settingsAsset);
        destroyOnTearDown.Add(dummyApplier);
        destroyOnTearDown.Add(dummyCodec);

        _typeCache = new SaveSysSettingsTypeCache();
        _typeCache.Refresh();

        // Inject dummy choices via reflection
        TypeCacheTestHelpers.SetApplierChoices(_typeCache,
            new Dictionary<string, ISaveDataApplier> { { "DummyApplier", dummyApplier } });
        TypeCacheTestHelpers.SetCodecChoices(_typeCache,
            new Dictionary<string, IMainSaveCodec> { { "DummyCodec", dummyCodec } });

        _applierListView = new ListView { name = "MainAppliers", fixedItemHeight = 20 };
        _codecListView = new ListView { name = "MainCodecs", fixedItemHeight = 20 };

        var root = new VisualElement();
        root.Add(_applierListView);
        root.Add(_codecListView);

        _appliersController = new SaveSysListController<ISaveDataApplier>(
            "MainAppliers",
            cache => cache.MainApplierChoices,
            settings => (System.Collections.IList)settings.MainAppliers,
            (settings, inst, idx) => settings.SetMainApplierAtIndex(inst, idx),
            (settings, inst) => settings.AddMainApplier(inst),
            (settings, inst) => settings.RemoveMainApplier(inst)

        );

        _codecsController = new SaveSysListController<IMainSaveCodec>(
            "MainCodecs",
            cache => cache.MainCodecChoices,
            settings => (System.Collections.IList)settings.MainCodecs,
            (settings, inst, idx) => settings.SetMainCodecAtIndex(inst, idx),
            (settings, inst) => settings.AddMainCodec(inst),
            (settings, inst) => settings.RemoveMainCodec(inst)
        );

        _appliersController.Init(root, _typeCache);
        _appliersController.BindToSettings(_settingsAsset);
        _appliersController.ToggleSubs(true);

        _codecsController.Init(root, _typeCache);
        _codecsController.BindToSettings(_settingsAsset);
        _codecsController.ToggleSubs(true);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in destroyOnTearDown)
        {
            if (obj != null) UnityObj.DestroyImmediate(obj);
        }
    }

    // Parameter source
    private static IEnumerable<TestCaseData> ControllerCases()
    {
        yield return new TestCaseData(
            "Appliers",
            (Func<SaveSystemSettings, System.Collections.IList>)(s => (System.Collections.IList)s.MainAppliers),
            "DummyApplier",
            typeof(DummyApplier)
        ).SetName("BindAndAdd_Applier");

        yield return new TestCaseData(
            "Codecs",
            (Func<SaveSystemSettings, System.Collections.IList>)(s => (System.Collections.IList)s.MainCodecs),
            "DummyCodec",
            typeof(DummyCodec)
        ).SetName("BindAndAdd_Codec");
    }

    [TestCaseSource(nameof(ControllerCases))]
    public void BindToSettings_SetsItemsSource(string label,
        Func<SaveSystemSettings, System.Collections.IList> collectionSelector,
        string dummyChoice,
        Type expectedType)
    {
        var collection = collectionSelector(_settingsAsset);
        var listView = label == "Appliers" ? _applierListView : _codecListView;

        CollectionAssert.AreEqual(collection, listView.itemsSource,
            $"ListView itemsSource should be bound to settings.{label}.");
    }

    [TestCaseSource(nameof(ControllerCases))]
    public void OnChoiceChanged_AddsNewItem(string label,
        Func<SaveSystemSettings, System.Collections.IList> collectionSelector,
        string dummyChoice,
        Type expectedType)
    {
        var listView = label == "Appliers" ? _applierListView : _codecListView;
        var controller = label == "Appliers" ? (object)_appliersController : _codecsController;

        var dropdown = (DropdownField)_controllerTestHelpers.MakeAndBindItem((dynamic)controller, listView, 0);

        dropdown.SetValueWithoutNotify(dummyChoice);
        var evt = ChangeEvent<string>.GetPooled(null, dummyChoice);
        evt.target = dropdown;
        dropdown.SendEvent(evt);

        var collection = collectionSelector(_settingsAsset);
        Assert.AreEqual(1, collection.Count, $"{label} should contain one item after choice change.");
        Assert.IsInstanceOf(expectedType, collection[0], $"Item should be of type {expectedType.Name}.");
    }
}

// Helper class for test setup
internal static class _controllerTestHelpers
{
    public static VisualElement MakeAndBindItem<T>(SaveSysListController<T> controller, ListView listView, int index)
    {
        // Make sure handlers are actually attached
        if (listView.makeItem == null)
        {
            Assert.Fail($"ListView.makeItem is null. Ensure controller.Init(root, cache) and ToggleSubs(true) were called, and that a ListView named '{GetExpectedName(controller)}' exists in root.");
        }
        if (listView.bindItem == null)
        {
            Assert.Fail($"ListView.bindItem is null. Ensure controller.ToggleSubs(true) was called.");
        }

        var item = listView.makeItem.Invoke();
        listView.bindItem.Invoke(item, index);

        // Sanity: verify we actually got a DropdownField
        var dropdown = item as DropdownField;
        Assert.IsNotNull(dropdown, "Controller's makeItem should create a DropdownField.");

        return item;
    }

    // If you can expose the controller’s listViewName, use that instead of hardcoding.
    private static string GetExpectedName<T>(SaveSysListController<T> controller) => "MainAppliers"; // or "MainCodecs" based on the test
}

// Dummy types for testing
public class DummyApplier : ScriptableObject, ISaveDataApplier
{
    public int Order => 0;

    public void Apply() { }

    public Task Apply(SaveData saveData)
    {
        return Task.CompletedTask;
    }

    public Task ApplyRange(IList<SaveData> datas)
    {
        return Task.CompletedTask;
    }

    public bool CanApply(SaveData saveData)
    {
        return false;
    }

    public void PreInstallInit()
    {

    }
}

public class DummyCodec : ScriptableObject, IMainSaveCodec
{
    public int Order => 0;

    public object ToMakeFrom
    {
        get => null;
        set { }
    }

    public bool NeedsInput => false;

    public void Encode() { }
    public void Decode() { }

    public void PreInstallInit()
    {

    }

    public IList<SaveData> FindAndCreateAll(System.Action<IList<SaveData>> onComplete = null)
    {
        return new List<SaveData>();
    }

    public bool CanHandle(object toMakeFrom)
    {
        return false;
    }

    public bool CanHandle(string typeName)
    {
        return false;
    }
}