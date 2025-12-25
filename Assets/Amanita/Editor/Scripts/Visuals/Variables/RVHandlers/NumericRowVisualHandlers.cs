using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public abstract class NumericRowVisualHandler<T> : RowVisualHandler<T>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            numericField = ValueField as TextValueField<T>;
            
            if (numericField == null)
            {
                Debug.LogError($"NumericRowVisualHandler could not find a TextValueField<{typeof(T).Name}> " +
                    $"named in the UXML template. Check your UXML.");
                return;
            }

            numericField.isDelayed = true; // So changes only fire on enter or focus lost
        }

        protected TextValueField<T> numericField;

        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (on)
            {
                numericField.RegisterValueChangedCallback(OnValueFieldChanged);
            }
            else
            {
                numericField.UnregisterValueChangedCallback(OnValueFieldChanged);
            }
        }

        protected virtual void OnValueFieldChanged(ChangeEvent<T> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
        }

        protected override void ApplyVarValueToValueField()
        {
            numericField?.SetValueWithoutNotify((T)_currentVariable.BoxedValue);
            numericField.MarkDirtyRepaint();
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
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            vector2Field = ValueField as Vector2Field;
            if (vector2Field == null)
            {
                Debug.LogError($"VectorTwoRowVisualHandler could not find a Vector2Field named in the UXML template. Check your UXML.");
                return;
            }
        }

        protected Vector2Field vector2Field;

        protected override void ApplyVarValueToValueField()
        {
            vector2Field.SetValueWithoutNotify((Vector2)_currentVariable.BoxedValue);
            vector2Field.MarkDirtyRepaint();
        }

        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (vector2Field == null)
            {
                return;
            }

            if (on)
            {
                vector2Field.RegisterValueChangedCallback(OnVector2FieldChanged);
            }
            else
            {
                vector2Field.UnregisterValueChangedCallback(OnVector2FieldChanged);
            }
        }

        private void OnVector2FieldChanged(ChangeEvent<Vector2> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
        }
    }

    [RowVisualHandler(menuName: "Numeric",
        contentType: typeof(Vector3), 
        typeDisplayName: "VectorThree",
        pathToTemplate: "UIToolkitTemplates/VarRows/Numeric/VectorThreeVariableRow")]
    public class VectorThreeRowVisualHandler : RowVisualHandler<Vector3>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            vector3Field = ValueField as Vector3Field;
            if (vector3Field == null)
            {
                Debug.LogError($"VectorThreeRowVisualHandler could not find a Vector3Field named in the UXML template. Check your UXML.");
                return;
            }
        }

        protected Vector3Field vector3Field;

        protected override void ApplyVarValueToValueField()
        {
            vector3Field.SetValueWithoutNotify((Vector3)_currentVariable.BoxedValue);
            vector3Field.MarkDirtyRepaint();
        }

        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (vector3Field == null)
            {
                return;
            }
            if (on)
            {
                vector3Field.RegisterValueChangedCallback(OnVector3FieldChanged);
            }
            else
            {
                vector3Field.UnregisterValueChangedCallback(OnVector3FieldChanged);
            }
        }

        private void OnVector3FieldChanged(ChangeEvent<Vector3> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
        }
    }

}