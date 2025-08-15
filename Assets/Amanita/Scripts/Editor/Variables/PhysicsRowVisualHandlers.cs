using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Physics", typeof(Vector2), "VectorTwo",
        "_EditorResources/UIToolkitTemplates/VarRows/VectorTwoVariableRow")]
    public class VectorTwoRowVisualHandler : RowVisualHandler<Vector2>
    {

    }

    [RowVisualHandler("Physics", typeof(Vector3), "VectorThree",
        "_EditorResources/UIToolkitTemplates/VarRows/VectorThreeVariableRow")]
    public class VectorThreeRowVisualHandler : RowVisualHandler<Vector3>
    {

    }
}