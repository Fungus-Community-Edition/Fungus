using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Primitives", typeof(Color), "Color",
        "_EditorResources/UIToolkitTemplates/VarRows/ColorVariableRow")]
    public class ColorVariableRow : RowVisualHandler<Color>
    {
        // Note: when randomly generated, the preview field is white even when the generated color
    }
}