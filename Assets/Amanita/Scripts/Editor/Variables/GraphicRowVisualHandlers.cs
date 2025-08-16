using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Primitives", typeof(Color), "Color",
        "_EditorResources/UIToolkitTemplates/VarRows/ColorVariableRow")]
    public sealed class ColorVariableRow : RowVisualHandler
    {
        private ColorField _colorField;
        private VisualElement _preview;
        private bool _hooked;

        public override Type VarContentType
        {
            get
            {
                if (_currentVariable == null)
                {
                    return null;
                }

                return _currentVariable.ContentType;
            }
        }

        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();

            _colorField = Root.Q<ColorField>("ColorField");
            _preview = Root.Q<VisualElement>("preview"); // adjust name as needed

            var swatch = _colorField.Q(className: "unity-color-field__swatch");
            if (swatch != null)
            {
                swatch.style.opacity = 0f; // invisible
                swatch.pickingMode = PickingMode.Position; // still clickable
            }
        }

        public override void Refresh()
        {
            base.Refresh();

            if (SerializedVar == null || _colorField == null)
                return;

            var valueProp = SerializedVar.FindProperty("value");
            if (valueProp == null || valueProp.propertyType != SerializedPropertyType.Color)
                return;

            // Hook events once — works for initial bind + later changes
            if (!_hooked)
            {
                _hooked = true;

                // Binding-driven updates
                Root.TrackPropertyValue(valueProp, prop =>
                {
                    if (prop != null && prop.propertyType == SerializedPropertyType.Color)
                        ApplyPreview(prop.colorValue);
                });

                // User-driven changes
                _colorField.RegisterValueChangedCallback(evt =>
                {
                    ApplyPreview(evt.newValue);
                });
            }

            // Immediate initialization so the preview is correct on first draw
            ApplyPreview(valueProp.colorValue);
        }

        private void ApplyPreview(Color col)
        {
            _colorField.SetValueWithoutNotify(col);

            if (_preview != null)
            {
                _preview.style.backgroundImage = StyleKeyword.None;
                _preview.style.unityBackgroundImageTintColor = Color.clear;
                _preview.style.backgroundColor = col;
                _preview.MarkDirtyRepaint();
            }
        }
    }
}