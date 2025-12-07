using Amanita.SaveSys;
using Amanita.SaveSys.EditorUtils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityObj = UnityEngine.Object;
using System.Linq;

public class SaveSysListControllerTests
{
    private SaveSystemSettings _settingsAsset;
    private SaveSysSettingsTypeCache _typeCache;
    private ListView _applierListView, _codecListView;
    private SaveSysListController<ISaveDataApplier> _appliersController;
    private SaveSysListController<IMainSaveCodec> _codecsController;

    [SetUp]
    public void SetUp()
    {
        _settingsAsset = ScriptableObject.CreateInstance<SaveSystemSettings>();
        DummyApplier dummyApplier = ScriptableObject.CreateInstance<DummyApplier>();
        _settingsAsset.AddMainApplier(dummyApplier);
        DummyCodec dummyCodec = ScriptableObject.CreateInstance<DummyCodec>();
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
        var root = new VisualElement();
        root.Add(_applierListView);

        _appliersController = new SaveSysListController<ISaveDataApplier>(
            "MainAppliers",
            cache => cache.MainApplierChoices,
            settings => (System.Collections.IList)settings.MainAppliers,
            (settings, inst, idx) => settings.SetMainApplierAtIndex(inst, idx),
            (settings, inst) => settings.AddMainApplier(inst)
        );

        _codecsController = new SaveSysListController<IMainSaveCodec>(
            "MainCodecs",
            cache => cache.MainCodecChoices,
            settings => (System.Collections.IList)settings.MainCodecs,
            (settings, inst, idx) => settings.SetMainCodecAtIndex(inst, idx),
            (settings, inst) => settings.AddMainCodec(inst)
        );

        _appliersController.Init(root, _typeCache);
        _appliersController.BindToSettings(_settingsAsset);
        _appliersController.ToggleSubs(true);

        _codecListView = new ListView { name = "MainCodecs", fixedItemHeight = 20 };
        root.Add(_codecListView);
        _codecsController.Init(root, _typeCache);
        _codecsController.BindToSettings(_settingsAsset);
        _codecsController.ToggleSubs(true);

    }

    private readonly IList<UnityObj> destroyOnTearDown = new List<UnityObj>();

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in destroyOnTearDown)
        {
            if (obj != null)
            {
                UnityObj.DestroyImmediate(obj);
            }
        }
    }

    [Test]
    public void BindToSettings_SetsItemsSource_ForAppliers()
    {
        CollectionAssert.AreEqual(_settingsAsset.MainAppliers, _applierListView.itemsSource,
            "ListView itemsSource should be bound to settings.MainAppliers.");
    }

    [Test]
    public void BindToSettings_SetsItemsSource_ForCodecs()
    {
        CollectionAssert.AreEqual(_settingsAsset.MainCodecs, _codecListView.itemsSource,
            "ListView itemsSource should be bound to settings.MainCodecs.");
    }

    [Test]
    public void OnChoiceChanged_AddsNewApplier_WhenIndexBeyondCollection()
    {
        var dropdown = (DropdownField)_controllerTestHelpers.MakeAndBindItem(_appliersController, _applierListView, 0);

        // Simulate user choice change
        dropdown.SetValueWithoutNotify("DummyApplier");
        var evt = ChangeEvent<string>.GetPooled(null, "DummyApplier");
        evt.target = dropdown;
        dropdown.SendEvent(evt);

        Assert.AreEqual(1, _settingsAsset.MainAppliers.Count,
            "MainAppliers should contain one item after choice change.");
        Assert.IsInstanceOf<DummyApplier>(_settingsAsset.MainAppliers[0],
            "Item should be of type DummyApplier.");
    }

    [Test]
    public void OnChoiceChanged_AddsNewCodec_WhenIndexBeyondCollection()
    {
        var dropdown = (DropdownField)_controllerTestHelpers.MakeAndBindItem(_codecsController, _codecListView, 0);

        dropdown.SetValueWithoutNotify("DummyCodec");
        var evt = ChangeEvent<string>.GetPooled(null, "DummyCodec");
        evt.target = dropdown;
        dropdown.SendEvent(evt);

        Assert.AreEqual(1, _settingsAsset.MainCodecs.Count,
            "MainCodecs should contain one item after choice change.");
        Assert.IsInstanceOf<DummyCodec>(_settingsAsset.MainCodecs[0],
            "Item should be of type DummyCodec.");
    }
}

// Helper class for test setup
internal static class _controllerTestHelpers
{
    public static VisualElement MakeAndBindItem<T>(SaveSysListController<T> controller, ListView listView, int index)
    {
        var item = listView.makeItem.Invoke();
        listView.bindItem.Invoke(item, index);
        return item;
    }
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