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

    [RowVisualHandler(menuName: "Numeric",
        contentType: typeof(float), 
        typeDisplayName: "Float",
        pathToTemplate: "UIToolkitTemplates/VarRows/Numeric/FloatVariableRow")]
    public class FloatRowVisualHandler : NumericRowVisualHandler<float>
    {
        
    }

    [RowVisualHandler(menuName: "Numeric",
        contentType: typeof(int), 
        typeDisplayName: "Integer",
        pathToTemplate: "UIToolkitTemplates/VarRows/Numeric/IntVariableRow")]
    public class IntRowVisualHandler : NumericRowVisualHandler<int>
    {
        
    }

    // Bools work off toggles, not text value fields, so...
    [RowVisualHandler(menuName: "Numeric",
        contentType: typeof(bool),
        typeDisplayName: "Boolean",
        pathToTemplate: "UIToolkitTemplates/VarRows/Numeric/BoolVariableRow")]
    public class BoolRowVisualHandler : RowVisualHandler<bool>
    {
        
    }

    [RowVisualHandler(menuName: "Numeric",
        contentType: typeof(Vector2), 
        typeDisplayName: "VectorTwo",
        pathToTemplate: "UIToolkitTemplates/VarRows/Numeric/VectorTwoVariableRow")]
    public class VectorTwoRowVisualHandler : RowVisualHandler<Vector2>
    {

    }

    [RowVisualHandler(menuName: "Numeric",
        contentType: typeof(Vector3), 
        typeDisplayName: "VectorThree",
        pathToTemplate: "UIToolkitTemplates/VarRows/Numeric/VectorThreeVariableRow")]
    public class VectorThreeRowVisualHandler : RowVisualHandler<Vector3>
    {

    }

}