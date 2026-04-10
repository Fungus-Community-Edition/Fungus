using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Color),
        typeDisplayName: "Color",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/ColorVariableRow")]
    public class ColorRowVisualHandler : RowVisualHandler<Color>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            colorValueField = ValueField as ColorField;

            if (colorValueField == null)
            {
                Debug.LogError($"ColorVariableRow could not find a ColorField named in the UXML template " +
                    $"for {GetType().Name}. Check your UXML.");
                return;
            }
        }

        protected ColorField colorValueField;

        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (colorValueField == null)
            {
                return;
            }

            if (on)
            {
                colorValueField.RegisterValueChangedCallback(OnColorFieldChanged);
            }
            else
            {
                colorValueField.UnregisterValueChangedCallback(OnColorFieldChanged);
            }
        }

        protected virtual void OnColorFieldChanged(ChangeEvent<Color> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
        }

        protected override void ApplyVarValueToValueField()
        {
            if (colorValueField == null || _currentVariable == null)
            {
                return;
            }

            Color currentCol = (Color)_currentVariable.BoxedValue;
            colorValueField.SetValueWithoutNotify(currentCol);
            colorValueField.MarkDirtyRepaint();
        }
    }

    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Texture),
        typeDisplayName: "Texture",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/TextureVariableRow")]
    public class TextureRowVisualHandler : RowVisualHandler<Texture>
    {

    }

    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Material),
        typeDisplayName: "Material",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/MaterialVariableRow")]
    public class MaterialRowVisualHandler : RowVisualHandler<Material>
    {

    }

    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Sprite),
        typeDisplayName: "Sprite",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/SpriteVariableRow")]
    public class SpriteRowVisualHandler : RowVisualHandler<Sprite>
    {

    }

    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(Animator),
        typeDisplayName: "Animator",
        pathToTemplate: "UIToolkitTemplates/VarRows/Graphic/AnimatorVariableRow")]
    public class AnimatorRowVisualHandler : RowVisualHandler<Animator>
    {

    }
}