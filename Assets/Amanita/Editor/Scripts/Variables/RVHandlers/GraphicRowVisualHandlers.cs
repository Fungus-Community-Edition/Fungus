using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Graphics", typeof(Color), "Color",
        "UIToolkitTemplates/VarRows/ColorVariableRow")]
    public class ColorVariableRow : RowVisualHandler<Color>
    {
        // Note: when randomly generated, the preview field is white even when the generated color
    }

    [RowVisualHandler("Graphics", typeof(Texture), "Texture",
        "UIToolkitTemplates/VarRows/TextureVariableRow")]
    public class TextureVariableRow : RowVisualHandler<Texture>
    {
        
    }

    [RowVisualHandler("Graphics", typeof(Material), "Material",
        "UIToolkitTemplates/VarRows/MaterialVariableRow")]
    public class MaterialVariableRow : RowVisualHandler<Material>
    {

    }

    [RowVisualHandler("Graphics", typeof(Sprite), "Sprite",
        "UIToolkitTemplates/VarRows/SpriteVariableRow")]
    public class SpriteVariableRow : RowVisualHandler<Sprite>
    {

    }

    [RowVisualHandler("Graphics", typeof(Animator), "Animator",
        "UIToolkitTemplates/VarRows/AnimatorVariableRow")]
    public class AnimatorVariableRow : RowVisualHandler<Animator>
    {

    }
}