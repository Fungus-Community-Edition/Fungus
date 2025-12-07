using Amanita.SaveSys.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class SaveSysSettingsUiRegistrarTests
{
    private SaveSysSettingsUiRegistrar _registrar;
    private VisualElement _root;
    private DropdownField _readerDropdown;
    private DropdownField _writerDropdown;
    private ObjectField _storageSettings;
    private Button _refreshButton;

    [SetUp]
    public void SetUp()
    {
        _registrar = new SaveSysSettingsUiRegistrar();

        // Create fake UI tree
        _root = new VisualElement();

        _readerDropdown = new DropdownField
        {
            name = "SaveReaderDropdown",
            choices = new List<string> { "ReaderChoiceA", "ReaderChoiceB" }
        };

        _writerDropdown = new DropdownField
        {
            name = "SaveWriterDropdown",
            choices = new List<string> { "WriterChoiceA", "WriterChoiceB" }
        };

        _storageSettings = new ObjectField { name = "StorageSettings" };
        _refreshButton = new Button { name = "RefreshButton" };

        _root.Add(_readerDropdown);
        _root.Add(_writerDropdown);
        _root.Add(_storageSettings);
        _root.Add(_refreshButton);

        _registrar.Register(_root);
    }

    [Test]
    public void Register_FindsUiElements()
    {
        Assert.AreSame(_readerDropdown, _registrar.ReaderDropdown);
        Assert.AreSame(_writerDropdown, _registrar.WriterDropdown);
        Assert.AreSame(_storageSettings, _registrar.StorageSettings);
        Assert.AreSame(_refreshButton, _registrar.RefreshButton);
    }

    [Test]
    public void RecordCurrentChoices_SavesDropdownValues()
    {
        _readerDropdown.value = "ReaderChoiceA";
        _writerDropdown.value = "WriterChoiceB";

        _registrar.RecordCurrentChoices();

        // Clear values to simulate recreation
        _readerDropdown.value = null;
        _writerDropdown.value = null;

        _registrar.RestoreLastChoices();

        Assert.AreEqual("ReaderChoiceA", _readerDropdown.value);
        Assert.AreEqual("WriterChoiceB", _writerDropdown.value);
    }

    [Test]
    public void RestoreLastChoices_IgnoresInvalidChoices()
    {
        _readerDropdown.value = "ReaderChoiceA";
        _writerDropdown.value = "WriterChoiceB";

        _registrar.RecordCurrentChoices();

        // Remove choices so last ones are invalid
        _readerDropdown.choices.Clear();
        _writerDropdown.choices.Clear();

        _readerDropdown.value = null;
        _writerDropdown.value = null;

        _registrar.RestoreLastChoices();

        Assert.IsNull(_readerDropdown.value, "Invalid reader choice should not be restored.");
        Assert.IsNull(_writerDropdown.value, "Invalid writer choice should not be restored.");
    }
}