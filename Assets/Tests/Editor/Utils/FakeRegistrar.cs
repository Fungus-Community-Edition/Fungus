using Amanita.SaveSys.EditorUtils;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public sealed class FakeRegistrar : SaveSysSettingsUiRegistrar
{
    public DropdownField ReaderDropdownStub { get; } = new DropdownField();
    public DropdownField WriterDropdownStub { get; } = new DropdownField();
    public ObjectField StorageSettingsStub { get; } = new ObjectField();
    public Button RefreshButtonStub { get; } = new Button();

    public FakeRegistrar()
    {
        // Pre-populate with dummy choices so tests can assign values
        ReaderDropdownStub.choices.Add("ReaderChoice");
        WriterDropdownStub.choices.Add("WriterChoice");
    }

    // Override properties to return stubs
    public override DropdownField ReaderDropdown => ReaderDropdownStub;
    public override DropdownField WriterDropdown => WriterDropdownStub;
    public override ObjectField StorageSettings => StorageSettingsStub;
    public override Button RefreshButton => RefreshButtonStub;

    // No-op implementations for persistence
    public override void Register(VisualElement root) { /* not needed in tests */ }
    public override void RestoreLastChoices() { /* not needed in tests */ }
    public override void RecordCurrentChoices() { /* not needed in tests */ }
}