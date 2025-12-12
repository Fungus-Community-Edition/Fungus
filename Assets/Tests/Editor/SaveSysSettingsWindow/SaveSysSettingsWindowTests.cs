using Amanita;
using Amanita.SaveSys;
using Amanita.SaveSys.EditorUtils;
using Amanita.VScripting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Type = System.Type;

public class SaveSysSettingsWindowTests
{
    private const string UxmlPath = "Assets/Amanita/Editor/Resources/UIToolkitTemplates/SaveSysSettingsWindow.uxml";
    private const string ResourcesSettingsFolder = "Assets/Resources/SaveSys/Settings";
    private const string ResourcesSettingsSubPath = "SaveSys/Settings";
    private const string SettingsAssetName = "SaveSystemSettings";

    private string GeneratedReaderAssetPath =>
        Path.Combine(ResourcesSettingsFolder, $"Generated{typeof(SaveReader).FullName}.asset").Replace("\\", "/");
    private string GeneratedWriterAssetPath =>
        Path.Combine(ResourcesSettingsFolder, $"Generated{_saveWriterType.FullName}.asset").Replace("\\", "/");

    private SaveSystemSettings _settingsAsset;

    [SetUp]
    public void SetUp()
    {
        // Ensure SaveSystemSettings resource exists (matches the window's behavior).
        _settingsAsset = SOUtils.EnsureSOExists<SaveSystemSettings>(ResourcesSettingsSubPath, SettingsAssetName) as SaveSystemSettings;
        AssetDatabase.Refresh();

        // Ensure type registries are discovered.
        SaveReaderTypeRegistry.DiscoverAndRegister();
        SaveWriterTypeRegistry.DiscoverAndRegister();

        // Remove any stray generated assets for default types from previous runs.
        SafeDeleteAsset(GeneratedReaderAssetPath);
        SafeDeleteAsset(GeneratedWriterAssetPath);
        AssetDatabase.Refresh();
    }

    [TearDown]
    public void TearDown()
    {
        // Close all windows of this type to avoid cross-test interference.
        foreach (var wnd in Resources.FindObjectsOfTypeAll<SaveSysSettingsWindow>())
        {
            wnd.Close();
        }

        // Clean generated assets for default types.
        SafeDeleteAsset(GeneratedReaderAssetPath);
        SafeDeleteAsset(GeneratedWriterAssetPath);
        AssetDatabase.Refresh();
        saveReaderDropdown = null;
        saveWriterDropdown = null;
        storageSettings = null;
        refreshButton = null;
    }

    private static void SafeDeleteAsset(string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }
    }

    [Test]
    public void Open_SetsWindowSize_ToWindowSizeStatic()
    {
        // Ensure no prior instance
        foreach (var wnd in Resources.FindObjectsOfTypeAll<SaveSysSettingsWindow>())
        {
            wnd.Close();
        }
        SaveSysSettingsWindow.Instance = null;

        SaveSysSettingsWindow.Open();

        var wndFound = Resources.FindObjectsOfTypeAll<SaveSysSettingsWindow>().FirstOrDefault();
        Assert.IsNotNull(wndFound, "SaveSysSettingsWindow not found after Open().");

        var expected = new Vector2(600, 700);
        Assert.AreEqual(expected, wndFound.minSize, "minSize should be set to windowSize.");
        Assert.AreEqual(expected, wndFound.maxSize, "maxSize should be set to windowSize.");
    }

    // Helper to create a window instance with UXML assigned before CreateGUI is invoked.
    private SaveSysSettingsWindow CreateWindowWithUxmlAssigned()
    {
        var vTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
        Assert.IsNotNull(vTreeAsset, "Failed to load VisualTreeAsset at: " + UxmlPath);

        // Create window but do not show yet.
        var wnd = ScriptableObject.CreateInstance<SaveSysSettingsWindow>();

        // Assign UXML to the serialized field.
        var vtaField = _windowType.GetField("m_VisualTreeAsset", _reflectionFlags);
        Assert.IsNotNull(vtaField, "Could not reflect m_VisualTreeAsset field.");
        vtaField.SetValue(wnd, vTreeAsset);

        // Assign SysSettings (protected property). Use backing field to avoid calling Refresh prematurely.
        var sysSettingsField = _windowType.GetField("_sysSettings", _reflectionFlags);
        Assert.IsNotNull(sysSettingsField, "Could not reflect _sysSettings field.");
        sysSettingsField.SetValue(wnd, _settingsAsset);

        // Now show the window and explicitly build the UI.
        wnd.Show();
        wnd.CreateGUI();

        return wnd;
    }

    private void RegisterUxmlControls(SaveSysSettingsWindow wnd)
    {
        SaveSysDropdownController dropdownController;
        var dropdownControllerField = _windowType.GetField("_dropdownController", _reflectionFlags);
        Assert.NotNull(dropdownControllerField);
        dropdownController = dropdownControllerField.GetValue(wnd) as SaveSysDropdownController;
        Assert.IsNotNull(dropdownController, "DropdownController should be registered.");

        saveReaderDropdown = dropdownController.ReaderDropdown;
        saveWriterDropdown = dropdownController.WriterDropdown;
        storageSettings = wnd.rootVisualElement.Q<ObjectField>("StorageSettings");
        refreshButton = wnd.rootVisualElement.Q<Button>("RefreshButton");
    }

    private ObjectField storageSettings;
    private Button refreshButton;

    protected static readonly Type _windowType = typeof(SaveSysSettingsWindow);
    protected static readonly BindingFlags _reflectionFlags = BindingFlags.Instance | BindingFlags.NonPublic | 
        BindingFlags.Public | BindingFlags.Static;

    // Shim fields for tests (pre-refactor API compatibility)
    private DropdownField saveReaderDropdown;
    private DropdownField saveWriterDropdown;

    [Test]
    public void CreateGUI_RegistersViews_FromUXML()
    {
        var wnd = CreateWindowWithUxmlAssigned();
        RegisterUxmlControls(wnd);

        Assert.IsNotNull(saveReaderDropdown, "SaveReaderDropdown should be registered.");
        Assert.IsNotNull(saveWriterDropdown, "SaveWriterDropdown should be registered.");
        Assert.IsNotNull(storageSettings, "StorageSettings ObjectField should be registered.");
        Assert.IsNotNull(refreshButton, "RefreshButton should be registered.");

        // Sanity check that elements exist in the visual tree too.
        Assert.IsNotNull(wnd.rootVisualElement.Q<DropdownField>("SaveReaderDropdown"));
        Assert.IsNotNull(wnd.rootVisualElement.Q<DropdownField>("SaveWriterDropdown"));
        Assert.IsNotNull(wnd.rootVisualElement.Q<ObjectField>("StorageSettings"));
        Assert.IsNotNull(wnd.rootVisualElement.Q<Button>("RefreshButton"));
    }

    [Test]
    public void UpdateCache_Enumerates_ValidReaderAndWriterTypes()
    {
        var wnd = CreateWindowWithUxmlAssigned();
        RegisterUxmlControls(wnd);

        var typeCacheField = _windowType.GetField("_typeCache", _reflectionFlags);
        Assert.NotNull(typeCacheField);
        var typeCache = typeCacheField.GetValue(wnd) as SaveSysSettingsTypeCache;
        Assert.IsNotNull(typeCache, "TypeCache should be registered.");
        typeCache.Refresh();


        var validReaderTypes = typeCache.ReaderTypes;
        var validWriterTypes = typeCache.WriterTypes;

        // Must include the default concrete types
        Assert.IsTrue(validReaderTypes.Any(readerType => readerType == _saveReaderType),
            "validReaderTypes should include SaveReader.");
        Assert.IsTrue(validWriterTypes.Any(writerType => writerType == _saveWriterType),
            "validWriterTypes should include SaveWriter.");

        // Ensure all entries are ScriptableObjects and implement expected interfaces.
        Assert.IsTrue(validReaderTypes.All(readerType => _scriptableObjType.IsAssignableFrom(readerType) && 
        _iSaveReaderType.IsAssignableFrom(readerType)),
            "All validReaderTypes must be ScriptableObject and implement ISaveReader.");
        Assert.IsTrue(validWriterTypes.All(readerType => _scriptableObjType.IsAssignableFrom(readerType) && 
        _iSaveWriterType.IsAssignableFrom(readerType)),
            "All validWriterTypes must be ScriptableObject and implement ISaveWriter.");
    }

    protected static readonly Type _scriptableObjType = typeof(ScriptableObject);
    protected static readonly Type _iSaveReaderType = typeof(ISaveReader);
    protected static readonly Type _iSaveWriterType = typeof(ISaveWriter);
    protected static readonly Type _saveReaderType = typeof(SaveReader);
    protected static readonly Type _saveWriterType = typeof(SaveWriter);

    [Test]
    public void ChoosingReader_AssignsInstance_ToSaveSystemSettings()
    {
        var wnd = CreateWindowWithUxmlAssigned();
        RegisterUxmlControls(wnd);

        // Build the expected choice label for SaveReader
        string readerChoiceLabel = GetDisplayName(_saveReaderType);
        Assert.IsTrue(saveReaderDropdown.choices.Contains(readerChoiceLabel), 
            $"Reader dropdown choices should contain '{readerChoiceLabel}'.");

        // Set the label to null so we can make sure assignment is triggered when we assign a legit string value
        saveReaderDropdown.value = string.Empty;

        // Change selection to trigger assignment
        saveReaderDropdown.value = readerChoiceLabel;

        // The window sets _sysSettings.SaveReader in the callback
        Assert.IsNotNull(_settingsAsset.SaveReader, "SaveSystemSettings.SaveReader should be assigned after selection.");
        Assert.AreEqual(_saveReaderType, _settingsAsset.SaveReader.GetType(), 
            "Assigned SaveReader should match selected type.");

        // Prefer the prepackaged default instance if present
        var defaultReader = Resources.LoadAll<ScriptableObject>(AmanitaConstants.PathToSaveSysDefaultsFolder)
            .FirstOrDefault(x => x != null && x.GetType() == _saveReaderType);
        if (defaultReader != null)
        {
            Assert.AreSame(defaultReader, _settingsAsset.SaveReader as ScriptableObject,
                "Assigned SaveReader should be the default asset instance when available.");
        }
    }

    protected static string GetDisplayName(Type type)
    {
        var attr = type.GetCustomAttribute<SaveSysDisplayName>();
        if (attr != null)
        {
            return attr.DisplayName;
        }

        return $"{type.Name} ({type.Namespace})";
    }

    [Test]
    public void ChoosingWriter_AssignsInstance_ToSaveSystemSettings()
    {
        var wnd = CreateWindowWithUxmlAssigned();
        RegisterUxmlControls(wnd);

        // Build the expected choice label for SaveWriter
        string writerChoiceLabel = GetDisplayName(_saveWriterType);
        Assert.IsTrue(saveWriterDropdown.choices.Contains(writerChoiceLabel),
            $"Writer dropdown choices should contain '{writerChoiceLabel}'.");

        // Set the label to null so we can make sure assignment is triggered when we assign a legit string value
        saveWriterDropdown.value = string.Empty;

        // Change selection to trigger assignment
        saveWriterDropdown.value = writerChoiceLabel;

        // The window sets _sysSettings.SaveWriter in the callback
        Assert.IsNotNull(_settingsAsset.SaveWriter,
            "SaveSystemSettings.SaveWriter should be assigned after selection.");
        Assert.AreEqual(_saveWriterType, _settingsAsset.SaveWriter.GetType(),
            "Assigned SaveWriter should match selected type.");

        // Prefer the prepackaged default instance if present
        var defaultWriter = Resources.LoadAll<ScriptableObject>(AmanitaConstants.PathToSaveSysDefaultsFolder)
            .FirstOrDefault(x => x != null && x.GetType() == _saveWriterType);
        if (defaultWriter != null)
        {
            Assert.AreSame(defaultWriter, _settingsAsset.SaveWriter as ScriptableObject,
                "Assigned SaveWriter should be the default asset instance when available.");
        }
    }

    [Test]
    public void Defaults_DoNotGenerate_DuplicateAssets()
    {
        CreateWindowWithUxmlAssigned();

        // Ensure defaults exist
        var defaultReader = Resources.Load<ScriptableObject>(AmanitaConstants.PathToDefaultSaveReader);
        var defaultWriter = Resources.Load<ScriptableObject>(AmanitaConstants.PathToDefaultSaveWriter);
        Assert.IsNotNull(defaultReader, "DefaultSaveReader resource must exist.");
        Assert.IsNotNull(defaultWriter, "DefaultSaveWriter resource must exist.");

        // The window's CreateGUI calls PrepReaderAndWriterInstances internally.
        // That should avoid generating assets for types that already have defaults registered.
        AssetDatabase.Refresh();

        Assert.IsNull(AssetDatabase.LoadAssetAtPath<ScriptableObject>(GeneratedReaderAssetPath),
            $"Should not generate asset for default reader type at: {GeneratedReaderAssetPath}");
        Assert.IsNull(AssetDatabase.LoadAssetAtPath<ScriptableObject>(GeneratedWriterAssetPath),
            $"Should not generate asset for default writer type at: {GeneratedWriterAssetPath}");
    }
}