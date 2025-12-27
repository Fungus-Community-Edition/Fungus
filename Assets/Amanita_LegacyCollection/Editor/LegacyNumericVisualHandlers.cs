using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
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