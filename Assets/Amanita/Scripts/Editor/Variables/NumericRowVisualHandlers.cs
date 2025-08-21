namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Primitives", typeof(float), "Float",
        "_EditorResources/UIToolkitTemplates/VarRows/FloatVariableRow")]
    public class FloatRowVisualHandler : RowVisualHandler<float>
    {
        
    }

    [RowVisualHandler("Primitives", typeof(int), "Integer",
        "_EditorResources/UIToolkitTemplates/VarRows/IntVariableRow")]
    public class IntRowVisualHandler : RowVisualHandler<int>
    {
        
    }

    [RowVisualHandler("Primitives", typeof(bool), "Boolean",
        "_EditorResources/UIToolkitTemplates/VarRows/BoolVariableRow")]
    public class BoolRowVisualHandler : RowVisualHandler<bool>
    {
        
    }

}