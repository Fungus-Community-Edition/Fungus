using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Primitives", typeof(string), "String",
        "_EditorResources/UIToolkitTemplates/VarRows/StringVariableRow")]
    public class StringRowVisualHandler : RowVisualHandler<object>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            textField = RowRoot.Q<TextField>("TextField");
        }

        protected TextField textField;

        protected override void ApplyDefaultBindingPaths()
        {
            base.ApplyDefaultBindingPaths();
            textField.bindingPath = "value";
        }

        protected override void ApplyMuscariableBindingPathOverrides()
        {
            base.ApplyMuscariableBindingPathOverrides();
            textField.bindingPath = $"{muscariableMemberName}.{textField.bindingPath}";
        }


    }
}