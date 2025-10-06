using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Graphics", typeof(Color), "Color",
        "UIToolkitTemplates/VarRows/Graphic/ColorVariableRow")]
    public class ColorVariableRow : RowVisualHandler<Color>
    {
        // Note: when randomly generated, the preview field is white even when the generated color
    }

    [RowVisualHandler("Graphics", typeof(Texture), "Texture",
        "UIToolkitTemplates/VarRows/Graphic/TextureVariableRow")]
    public class TextureVariableRow : RowVisualHandler<Texture>
    {
        
    }

    [RowVisualHandler("Graphics", typeof(Material), "Material",
        "UIToolkitTemplates/VarRows/Graphic/MaterialVariableRow")]
    public class MaterialVariableRow : RowVisualHandler<Material>
    {

    }

    [RowVisualHandler("Graphics", typeof(Sprite), "Sprite",
        "UIToolkitTemplates/VarRows/Graphic/SpriteVariableRow")]
    public class SpriteVariableRow : RowVisualHandler<Sprite>
    {

    }

    [RowVisualHandler("Graphics", typeof(Animator), "Animator",
        "UIToolkitTemplates/VarRows/Graphic/AnimatorVariableRow")]
    public class AnimatorVariableRow : RowVisualHandler<Animator>
    {

    }
}