using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Holds the UI elements to get a VariableRowManager to do its thing with.
    /// </summary>
    public class VRowManagerInitArgs
    {
        public VisualElement HoldsManager { get; set; }
        public VisualElement Root { get; set; }
        public VisualElement ListContainer { get; set; }
        public UITKLabel CountLabel { get; set; }
        public Flowchart Flowchart { get; set; }
        public Button AddButton { get; set; }
    }
}