using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Holds the UI elements to get a VariableRowManager to do its thing with.
    /// </summary>
    public class VRowManagerInitArgs
    {
        public VisualElement HoldsManager;
        public VisualElement Root;
        public Button AddButton;
        public IReorderableVariableSource VariableSource;
        public IVariableListView VariableListView;          // NEW (optional)
    }
}