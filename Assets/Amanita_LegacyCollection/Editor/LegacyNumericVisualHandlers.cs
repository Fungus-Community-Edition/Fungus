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
            Vector4Field = ValueField as Vector4Field;
            if (Vector4Field == null)
            {
                Debug.LogError($"VectorThreeRowVisualHandler could not find a Vector4Field named in the UXML template. Check your UXML.");
                return;
            }
        }

        protected Vector4Field Vector4Field;

        protected override void ApplyVarValueToValueField()
        {
            Vector4Field.SetValueWithoutNotify((Vector4)_currentVariable.BoxedValue);
            Vector4Field.MarkDirtyRepaint();
        }

        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (Vector4Field == null)
            {
                return;
            }
            if (on)
            {
                Vector4Field.RegisterValueChangedCallback(OnVector4FieldChanged);
            }
            else
            {
                Vector4Field.UnregisterValueChangedCallback(OnVector4FieldChanged);
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
            Matrix4x4Field = ValueField as Matrix4x4Field;
            if (Matrix4x4Field == null)
            {
                Debug.LogError($"MatrixFourByFourRowVisualHandler could not find a Matrix4x4Field named in the UXML template. Check your UXML.");
                return;
            }
        }
        protected Matrix4x4Field Matrix4x4Field;
        protected override void ApplyVarValueToValueField()
        {
            Matrix4x4Field.SetValueWithoutNotify((Matrix4x4)_currentVariable.BoxedValue);
            Matrix4x4Field.MarkDirtyRepaint();
        }
        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (Matrix4x4Field == null)
            {
                return;
            }
            if (on)
            {
                Matrix4x4Field.RegisterValueChangedCallback(OnMatrix4x4FieldChanged);
            }
            else
            {
                Matrix4x4Field.UnregisterValueChangedCallback(OnMatrix4x4FieldChanged);
            }
        }
        private void OnMatrix4x4FieldChanged(ChangeEvent<Matrix4x4> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
        }
    }

}