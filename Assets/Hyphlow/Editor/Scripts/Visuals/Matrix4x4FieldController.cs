using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class MatrixFourByFourFieldController : INotifyValueChanged<Matrix4x4>
    {
        private static readonly int rowCount = 4, colCount = 4;
        private static readonly string floatFieldNameFormat = "R{0}C{1}";


        private Matrix4x4 backingVal;
        public event Action<Matrix4x4, Matrix4x4> ValueChanged = delegate { };

        public void Init(VisualElement root)
        {
            fieldRoot = root.Q<VisualElement>("ValueField");
            if (fieldRoot == null)
            {
                Debug.LogError($"MatrixFourByFourFieldController could not find a ValueField VisualElement in" +
                    $"the provided root. Best check your UXML.");
                return;
            }
            RegisterFloatFields();

            if (!this.IsValid)
            {
                return;
            }
            SetFloatFieldValChangeCallbacks();
        }

        private VisualElement fieldRoot;

        public virtual bool IsValid => fieldRoot != null && _fields != null;

        private void RegisterFloatFields()
        {
            _fields = new FloatField[rowCount, colCount];
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colCount; colIndex++)
                {
                    string fieldName = string.Format(floatFieldNameFormat, rowIndex, colIndex);
                    var fieldFound = fieldRoot.Q<FloatField>(fieldName);
                    bool success = fieldFound != null;
                    if (!success)
                    {
                        Debug.LogError($"MatrixFourByFourFieldController could not find FloatField named '{fieldName}'" +
                            $" in the provided root. Best check your UXML.");
                        _fields = null; // <- So IsValid returns false
                        return;
                    }

                    _fields[rowIndex, colIndex] = fieldFound;
                    fieldFound.isDelayed = true; // So change events only fire on enter or focus loss
                }
            }
        }

        private FloatField[,] _fields;

        private void SetFloatFieldValChangeCallbacks()
        {
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colCount; colIndex++)
                {
                    var currentField = _fields[rowIndex, colIndex];
                    currentField.RegisterValueChangedCallback(OnFloatFieldValueChanged);
                }
            }
        }

        private void OnFloatFieldValueChanged(ChangeEvent<float> evt)
        {
            Matrix4x4 prev = backingVal;
            backingVal = ReadMatrixFromFields();
            NotifyValueChanged(prev, backingVal);
        }

        private Matrix4x4 ReadMatrixFromFields()
        {
            Matrix4x4 matrix = new Matrix4x4();
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colCount; colIndex++)
                {
                    FloatField fieldToReadFrom = _fields[rowIndex, colIndex];
                    matrix[rowIndex, colIndex] = fieldToReadFrom.value;
                }
            }
            return matrix;
        }

        private void WriteMatrixToFields(Matrix4x4 matrix)
        {
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colCount; colIndex++)
                {
                    _fields[rowIndex, colIndex].SetValueWithoutNotify(matrix[rowIndex, colIndex]);
                }
            }
        }

        private void NotifyValueChanged(Matrix4x4 prev, Matrix4x4 next)
        {
            ValueChanged.Invoke(prev, next);
            using var evt = ChangeEvent<Matrix4x4>.GetPooled(prev, next);
        }

        public Matrix4x4 value
        {
            get => backingVal;
            set
            {
                if (backingVal == value)
                    return;

                Matrix4x4 prev = backingVal;
                backingVal = value;
                WriteMatrixToFields(value);
                NotifyValueChanged(prev, backingVal);
            }
        }

        public void SetValueWithoutNotify(Matrix4x4 newValue)
        {
            backingVal = newValue;
            WriteMatrixToFields(newValue);
        }

        public virtual void MarkDirtyRepaint()
        {
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colCount; colIndex++)
                {
                    _fields[rowIndex, colIndex].MarkDirtyRepaint();
                }
            }
        }


    }
}