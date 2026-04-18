using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
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
        contentType: typeof(double),
        typeDisplayName: "Double",
        pathToTemplate: "UIToolkitTemplates/VarRows/Numeric/DoubleVariableRow")]
    public class DoubleRowVisualHandler : NumericRowVisualHandler<double>
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
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            toggleField = ValueField as Toggle;
            if (toggleField == null)
            {
                Debug.LogError($"BoolRowVisualHandler could not find a Toggle named in the UXML template. Check your UXML.");
                return;
            }
        }

        protected Toggle toggleField;

        protected override void ApplyVarValueToValueField()
        {
            toggleField.SetValueWithoutNotify((bool)_currentVariable.BoxedValue);
            toggleField.MarkDirtyRepaint();
        }

        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (toggleField == null)
            {
                return;
            }

            if (on)
            {
                toggleField.RegisterValueChangedCallback(OnToggleFieldChanged);
            }
            else
            {
                toggleField.UnregisterValueChangedCallback(OnToggleFieldChanged);
            }
        }

        private void OnToggleFieldChanged(ChangeEvent<bool> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
        }
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


    [RowVisualHandler(menuName: "Numeric",
        contentType: typeof(Vector4),
        typeDisplayName: "VectorFour",
        pathToTemplate: "UIToolkitTemplates/VarRows/Numeric/VectorFourVariableRow")]
    public class VectorFourVisualHandler : RowVisualHandler<Vector4>//
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            vector4Field = ValueField as Vector4Field;
            if (vector4Field == null)
            {
                Debug.LogError($"VectorThreeRowVisualHandler could not find a Vector4Field named in the UXML template. Check your UXML.");
                return;
            }
        }

        protected Vector4Field vector4Field;

        protected override void ApplyVarValueToValueField()
        {
            vector4Field.SetValueWithoutNotify((Vector4)_currentVariable.BoxedValue);
            vector4Field.MarkDirtyRepaint();
        }

        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (vector4Field == null)
            {
                return;
            }
            if (on)
            {
                vector4Field.RegisterValueChangedCallback(OnVector4FieldChanged);
            }
            else
            {
                vector4Field.UnregisterValueChangedCallback(OnVector4FieldChanged);
            }
        }

        private void OnVector4FieldChanged(ChangeEvent<Vector4> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
        }
    }

    [RowVisualHandler(menuName: "Numeric",
        contentType: typeof(Matrix4x4),
        typeDisplayName: "MatrixFourByFour",
        pathToTemplate: "UIToolkitTemplates/VarRows/Numeric/MatrixFourByFourVariableRow")]
    public class MatrixFourByFourVisualHandler : RowVisualHandler<Matrix4x4>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            fieldController.Init(this.RowRoot);
            if (!fieldController.IsValid)
            {
                Debug.LogError($"MatrixFourByFourRowVisualHandler could not find a Matrix4x4Field " +
                    $"named in the UXML template. Check your UXML.");
                return;
            }
        }

        private MatrixFourByFourFieldController fieldController = new MatrixFourByFourFieldController();

        protected override void ApplyVarValueToValueField()
        {
            fieldController.SetValueWithoutNotify((Matrix4x4)_currentVariable.BoxedValue);
            fieldController.MarkDirtyRepaint();
        }

        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (fieldController == null)
            {
                return;
            }
            if (on)
            {
                fieldController.ValueChanged += OnFieldChanged;
            }
            else
            {
                fieldController.ValueChanged -= OnFieldChanged;
            }
        }

        private void OnFieldChanged(Matrix4x4 prev, Matrix4x4 current)
        {
            TriggerValueFieldChanged(current);
        }
    }
}