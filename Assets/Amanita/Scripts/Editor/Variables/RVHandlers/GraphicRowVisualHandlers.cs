using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Graphics", typeof(Color), "Color",
        "_EditorResources/UIToolkitTemplates/VarRows/ColorVariableRow")]
    public class ColorVariableRow : RowVisualHandler<Color>
    {
        // Note: when randomly generated, the preview field is white even when the generated color
    }

    [RowVisualHandler("Graphics", typeof(Texture), "Texture",
        "_EditorResources/UIToolkitTemplates/VarRows/TextureVariableRow")]
    public class TextureVariableRow : RowVisualHandler<Texture>
    {
        
    }

    [RowVisualHandler("Graphics", typeof(Material), "Material",
        "_EditorResources/UIToolkitTemplates/VarRows/MaterialVariableRow")]
    public class MaterialVariableRow : RowVisualHandler<Material>
    {

    }

    [RowVisualHandler("Graphics", typeof(Sprite), "Sprite",
        "_EditorResources/UIToolkitTemplates/VarRows/SpriteVariableRow")]
    public class SpriteVariableRow : RowVisualHandler<Sprite>
    {

    }
}