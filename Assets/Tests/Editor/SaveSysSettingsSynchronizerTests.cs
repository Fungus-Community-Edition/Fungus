using Amanita.SaveSys;
using Amanita.SaveSys.EditorUtils;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObj = UnityEngine.Object;
using System.Collections.Generic;

public class SaveSysSettingsSynchronizerTests
{
    private SaveSystemSettings _settingsAsset;
    private readonly SaveSysDropdownController _dropdownController = new FakeDropdownController();
    private readonly SaveSysSettingsSynchronizer _synchronizer = new SaveSysSettingsSynchronizer();

    private DropdownField _readerDropdown;
    private DropdownField _writerDropdown;
    private ObjectField _storageSettings;

    [SetUp]
    public void SetUp()
    {
        // Create a fresh settings asset in memory
        _settingsAsset = ScriptableObject.CreateInstance<SaveSystemSettings>();

        // Fake dropdowns for testing
        _readerDropdown = new DropdownField { choices = new List<string> { "ReaderChoice" } };
        _writerDropdown = new DropdownField { choices = new List<string> { "WriterChoice" } };
        _storageSettings = new ObjectField();

        // Minimal controller stub (normally Init would query UI)

        // Synchronizer under test
        _synchronizer.Init(_settingsAsset, _dropdownController);

        toDestroyOnTearDown.Add(_settingsAsset);
    }

    private readonly IList<UnityObj> toDestroyOnTearDown = new List<UnityObj>();


    [TearDown]
    public void TearDown()
    {
        _synchronizer.Dispose();

        foreach (var obj in toDestroyOnTearDown)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    [Test]
    public void FillMissingAssetSettings_AssignsReaderAndWriter_WhenDropdownsHaveValues()
    {
        // Arrange
        _readerDropdown.value = "ReaderChoice";
        _writerDropdown.value = "WriterChoice";

        // Act
        _synchronizer.FillMissingAssetSettings(_readerDropdown, _writerDropdown);

        // Assert
        Assert.IsNotNull(_settingsAsset.SaveReader, "SaveReader should be assigned from dropdown.");
        Assert.IsNotNull(_settingsAsset.SaveWriter, "SaveWriter should be assigned from dropdown.");
    }

    [Test]
    public void ApplyAssetToUI_SetsDropdownValues_WhenAssetHasInstances()
    {
        // Arrange: simulate asset already has reader/writer
        _settingsAsset.SaveReader = ScriptableObject.CreateInstance<SaveReader>();
        _settingsAsset.SaveWriter = ScriptableObject.CreateInstance<SaveWriter>();

        toDestroyOnTearDown.Add((UnityObj)_settingsAsset.SaveReader);
        toDestroyOnTearDown.Add((UnityObj)_settingsAsset.SaveWriter);

        string readerDisplay = SaveSysTypeUtils.GetDisplayName(_settingsAsset.SaveReader.GetType());
        string writerDisplay = SaveSysTypeUtils.GetDisplayName(_settingsAsset.SaveWriter.GetType());

        _readerDropdown.choices.Add(readerDisplay);
        _writerDropdown.choices.Add(writerDisplay);

        // Act
        _synchronizer.ApplyAssetToUI(_storageSettings, _readerDropdown, _writerDropdown);

        // Assert
        Assert.AreEqual(readerDisplay, _readerDropdown.value, "Reader dropdown should reflect asset value.");
        Assert.AreEqual(writerDisplay, _writerDropdown.value, "Writer dropdown should reflect asset value.");
    }

    [Test]
    public void MakeChangesStick_MarksAssetDirty()
    {
        // Act
        _synchronizer.MakeChangesStick();

        // Assert
        Assert.IsTrue(EditorUtility.IsDirty(_settingsAsset), "Asset should be marked dirty after changes stick.");
    }

}