using Amanita.SaveSys;
using Amanita.SaveSys.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObj = UnityEngine.Object;

public class SaveSysSettingsSynchronizerTests
{
    private SaveSystemSettings _settingsAsset;
    private FakeDropdownController _dropdownController;
    private SaveSysSettingsUiRegistrar _registrar;
    private SaveSysSettingsSynchronizer _synchronizer;

    private DropdownField _readerDropdown;
    private DropdownField _writerDropdown;
    private ObjectField _storageSettings;

    private readonly IList<UnityObj> toDestroyOnTearDown = new List<UnityObj>();

    [SetUp]
    public void SetUp()
    {
        _settingsAsset = ScriptableObject.CreateInstance<SaveSystemSettings>();
        toDestroyOnTearDown.Add(_settingsAsset);

        CreateUiControls();
        void CreateUiControls()
        {
            _readerDropdown = new DropdownField { choices = new List<string> { "ReaderChoice" } };
            _readerDropdown.name = "SaveReaderDropdown";
            _writerDropdown = new DropdownField { choices = new List<string> { "WriterChoice" } };
            _writerDropdown.name = "SaveWriterDropdown";
            _storageSettings = new ObjectField();
            _storageSettings.name = "StorageSettings";
            _storageSettings.dataSourceType = typeof(SaveStorageSettings);
        }

        PrepRoot();
        void PrepRoot()
        {
            root = new VisualElement();
            root.Add(_readerDropdown);
            root.Add(_writerDropdown);
            root.Add(_storageSettings);
        }

        _registrar = new SaveSysSettingsUiRegistrar();
        _registrar.Register(root);

        _dropdownController = new FakeDropdownController();
        var typeCache = new SaveSysSettingsTypeCache();
        typeCache.Refresh();
        _dropdownController.Init(root, typeCache);

        _synchronizer = new SaveSysSettingsSynchronizer();
        _synchronizer.Init(_settingsAsset, _dropdownController, _registrar);
    }

    private VisualElement root;

    [TearDown]
    public void TearDown()
    {
        _synchronizer.Dispose();
        foreach (var obj in toDestroyOnTearDown)
        {
            if (obj != null) UnityObj.DestroyImmediate(obj);
        }

        root = null;
        _readerDropdown = null;
        _writerDropdown = null;
        _storageSettings = null;
        _registrar = null;
        _dropdownController = null;
        _settingsAsset = null;
        _synchronizer = null;
        _registrar = null;
    }

    [Test]
    public void FillMissingAssetSettings_AssignsReaderAndWriter_WhenDropdownsHaveValues()
    {
        _readerDropdown.value = "ReaderChoice";
        _writerDropdown.value = "WriterChoice";

        _synchronizer.FillMissingAssetSettings();

        Assert.IsNotNull(_settingsAsset.SaveReader, "SaveReader should be assigned from dropdown.");
        Assert.IsNotNull(_settingsAsset.SaveWriter, "SaveWriter should be assigned from dropdown.");
    }

    [Test]
    public void FillMissingAssetSettings_RecordsChoices()
    {
        _readerDropdown.value = "ReaderChoice";
        _writerDropdown.value = "WriterChoice";

        _synchronizer.FillMissingAssetSettings();

        // Registrar should persist choices
        Assert.AreEqual("ReaderChoice", _registrar.ReaderDropdown.value);
        Assert.AreEqual("WriterChoice", _registrar.WriterDropdown.value);
    }

    [Test]
    public void ApplyAssetToUI_SetsDropdownValues_WhenAssetHasInstances()
    {
        _settingsAsset.SaveReader = ScriptableObject.CreateInstance<SaveReader>();
        _settingsAsset.SaveWriter = ScriptableObject.CreateInstance<SaveWriter>();
        toDestroyOnTearDown.Add((UnityObj)_settingsAsset.SaveReader);
        toDestroyOnTearDown.Add((UnityObj)_settingsAsset.SaveWriter);

        string readerDisplay = SaveSysTypeUtils.GetDisplayName(_settingsAsset.SaveReader.GetType());
        string writerDisplay = SaveSysTypeUtils.GetDisplayName(_settingsAsset.SaveWriter.GetType());

        _readerDropdown.choices.Add(readerDisplay);
        _writerDropdown.choices.Add(writerDisplay);

        _synchronizer.ApplyAssetToUI();

        Assert.AreEqual(readerDisplay, _readerDropdown.value, "Reader dropdown should reflect asset value.");
        Assert.AreEqual(writerDisplay, _writerDropdown.value, "Writer dropdown should reflect asset value.");
    }

    [Test]
    public void ApplyAssetToUI_SetsStorageSettings()
    {
        var newStorage = ScriptableObject.CreateInstance<SaveStorageSettings>();
        toDestroyOnTearDown.Add(newStorage);

        _settingsAsset.StorageSettings = newStorage;

        _synchronizer.ApplyAssetToUI();

        Assert.AreSame(newStorage, _registrar.StorageSettings.value,
            "StorageSettings field should reflect asset value.");
    }

    [Test]
    public void MakeChangesStick_MarksAssetDirty()
    {
        _synchronizer.MakeChangesStick();
        Assert.IsTrue(EditorUtility.IsDirty(_settingsAsset),
            "Asset should be marked dirty after changes stick.");
    }

    [Test]
    public void FillMissingAssetSettings_DoesNothing_WhenSysSettingsIsNull()
    {
        _synchronizer.Dispose(); // clears sysSettings
        Assert.DoesNotThrow(() => _synchronizer.FillMissingAssetSettings(),
            "Should safely handle null sysSettings.");
    }
}