using Amanita.SaveSys;
using Amanita.SaveSys.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObj = UnityEngine.Object;

public class SaveSysSettingsEventBinderTests
{
    private SaveSystemSettings _settingsAsset;
    private FakeDropdownController _dropdownController;
    private SaveSysSettingsSynchronizer _synchronizer;
    private SaveSysSettingsEventBinder _binder;

    private DropdownField _readerDropdown;
    private DropdownField _writerDropdown;
    private ObjectField _storageSettings;
    private Button _refreshButton;
    private VisualElement _root;

    [SetUp]
    public void SetUp()
    {
        // Fresh asset
        _settingsAsset = ScriptableObject.CreateInstance<SaveSystemSettings>();

        // Fake dropdowns
        _readerDropdown = new DropdownField { name = "SaveReaderDropdown", choices = new List<string> { "ReaderChoice" } };
        _writerDropdown = new DropdownField { name = "SaveWriterDropdown", choices = new List<string> { "WriterChoice" } };
        _storageSettings = new ObjectField { name = "StorageSettings" };
        _refreshButton = new Button { name = "RefreshButton" };

        // Root visual element to simulate UXML tree
        _root = new VisualElement();
        _root.Add(_readerDropdown);
        _root.Add(_writerDropdown);
        _root.Add(_storageSettings);
        _root.Add(_refreshButton);

        // Stub controller that always returns dummy instances
        _dropdownController = new FakeDropdownController();
        _dropdownController.Init(_root, new SaveSysSettingsTypeCache());

        // Synchronizer
        _synchronizer = new SaveSysSettingsSynchronizer();
        _fakeRegistrar = new FakeRegistrar();
        _synchronizer.Init(_settingsAsset, _dropdownController, _fakeRegistrar);

        // Binder under test
        _binder = new SaveSysSettingsEventBinder();
        _binder.Init(_settingsAsset, _dropdownController, _synchronizer, _storageSettings, _refreshButton);

        destroyOnTearDown.Add(_settingsAsset);
    }

    private IList<UnityObj> destroyOnTearDown = new List<UnityObj>();
    private FakeRegistrar _fakeRegistrar;

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in destroyOnTearDown)
        {
            UnityObj.DestroyImmediate(obj);
        }

        _synchronizer.Dispose();
        _dropdownController.Dispose();
    }

    [Test]
    public void Toggle_On_BindsCallbacks()
    {
        _binder.Toggle(true);

        // Simulate reader dropdown change by invoking the handler directly
        var readerEvt = ChangeEvent<string>.GetPooled(null, "ReaderChoice");
        readerEvt.target = _readerDropdown;
        _binder.OnReaderDropdownChoiceChanged(readerEvt);

        Assert.IsNotNull(_settingsAsset.SaveReader, "SaveReader should be assigned after event fires.");

        // Simulate writer dropdown change
        var writerEvt = ChangeEvent<string>.GetPooled(null, "WriterChoice");
        writerEvt.target = _writerDropdown;
        _binder.OnWriterDropdownChoiceChanged(writerEvt);

        Assert.IsNotNull(_settingsAsset.SaveWriter, "SaveWriter should be assigned after event fires.");
    }

    [Test]
    public void Toggle_Off_UnbindsCallbacks()
    {
        // First bind, then unbind
        _binder.Toggle(true);
        _binder.Toggle(false);

        // Simulate dropdown changes by setting values
        _readerDropdown.value = "ReaderChoice";
        _writerDropdown.value = "WriterChoice";

        // Because we toggled off, the binder should have unsubscribed
        // so the asset should remain unchanged
        Assert.IsNull(_settingsAsset.SaveReader, "SaveReader should remain null after unbinding.");
        Assert.IsNull(_settingsAsset.SaveWriter, "SaveWriter should remain null after unbinding.");
    }

    [Test]
    public void StorageSettingsChange_UpdatesAsset()
    {
        _binder.Toggle(true);

        var newStorage = ScriptableObject.CreateInstance<SaveStorageSettings>();
        var evt = ChangeEvent<Object>.GetPooled(null, newStorage);
        evt.target = _storageSettings;

        // Directly invoke the binder’s handler
        _binder.OnStorageSettingsChanged(evt);

        Assert.AreSame(newStorage, _settingsAsset.StorageSettings,
            "StorageSettings should update on change event.");
    }

}