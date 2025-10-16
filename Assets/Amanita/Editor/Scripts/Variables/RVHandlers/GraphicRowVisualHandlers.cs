using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Color),
        typeDisplayName: "Color",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/ColorVariableRow")]
    public class ColorVariableRow : RowVisualHandler<Color>
    {
        // Note: when randomly generated, the preview field is white even when the generated color
    }

    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Texture),
        typeDisplayName: "Texture",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/TextureVariableRow")]
    public class TextureVariableRow : RowVisualHandler<Texture>
    {
        
    }

    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Material),
        typeDisplayName: "Material",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/MaterialVariableRow")]
    public class MaterialVariableRow : RowVisualHandler<Material>
    {

    }

    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Sprite),
        typeDisplayName: "Sprite",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/SpriteVariableRow")]
    public class SpriteVariableRow : RowVisualHandler<Sprite>
    {

    }

    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Animator),
        typeDisplayName: "Animator",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/AnimatorVariableRow")]
    public class AnimatorVariableRow : RowVisualHandler<Animator>
    {

    }
}