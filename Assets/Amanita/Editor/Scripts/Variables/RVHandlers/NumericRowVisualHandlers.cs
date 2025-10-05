using Amanita.EditorUtils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public abstract class NumericRowVisualHandler<T> : RowVisualHandler<T>
    {
        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            ToggleValueChange(valueField as TextValueField<T>, OnValueFieldChanged, on);
        }

        protected virtual void OnValueFieldChanged(ChangeEvent<T> evt)
        {
            AmanitaEditorSignals.ControlValueChanged(evt);
        }
    }

    [RowVisualHandler("Numeric", typeof(float), "Float",
        "UIToolkitTemplates/VarRows/Numeric/FloatVariableRow")]
    public class FloatRowVisualHandler : NumericRowVisualHandler<float>
    {
        
    }

    [RowVisualHandler("Numeric", typeof(int), "Integer",
        "UIToolkitTemplates/VarRows/Numeric/IntVariableRow")]
    public class IntRowVisualHandler : NumericRowVisualHandler<int>
    {
        
    }

    // Bools work off toggles, not text value fields, so...
    [RowVisualHandler("Numeric", typeof(bool), "Boolean",
        "UIToolkitTemplates/VarRows/Numeric/BoolVariableRow")]
    public class BoolRowVisualHandler : RowVisualHandler<bool>
    {
        
    }

    [RowVisualHandler("Numeric", typeof(Vector2), "VectorTwo",
        "UIToolkitTemplates/VarRows/Numeric/VectorTwoVariableRow")]
    public class VectorTwoRowVisualHandler : RowVisualHandler<Vector2>
    {

    }

    [RowVisualHandler("Numeric", typeof(Vector3), "VectorThree",
        "UIToolkitTemplates/VarRows/Numeric/VectorThreeVariableRow")]
    public class VectorThreeRowVisualHandler : RowVisualHandler<Vector3>
    {

    }

}