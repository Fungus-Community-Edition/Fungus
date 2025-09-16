using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public abstract class NumericRowVisualHandler<T> : RowVisualHandler<T>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            valueField = RowRoot.Q<TextValueField<T>>("ValueField");
        }

        protected TextValueField<T> valueField;

        protected override void ApplyDefaultBindingPaths()
        {
            base.ApplyDefaultBindingPaths();
            valueField.bindingPath = "value";
        }

        protected override void ApplyMuscariableBindingPathOverrides()
        {
            base.ApplyMuscariableBindingPathOverrides();
            valueField.bindingPath = $"{muscariableMemberName}.{valueField.bindingPath}";
        }
    }

    [RowVisualHandler("Primitives", typeof(float), "Float",
        "_EditorResources/UIToolkitTemplates/VarRows/FloatVariableRow")]
    public class FloatRowVisualHandler : NumericRowVisualHandler<float>
    {
        
    }

    [RowVisualHandler("Primitives", typeof(int), "Integer",
        "_EditorResources/UIToolkitTemplates/VarRows/IntVariableRow")]
    public class IntRowVisualHandler : NumericRowVisualHandler<int>
    {
        
    }

    // Bools work off toggles, not text value fields, so...
    [RowVisualHandler("Primitives", typeof(bool), "Boolean",
        "_EditorResources/UIToolkitTemplates/VarRows/BoolVariableRow")]
    public class BoolRowVisualHandler : RowVisualHandler<bool>
    {
        
    }

}