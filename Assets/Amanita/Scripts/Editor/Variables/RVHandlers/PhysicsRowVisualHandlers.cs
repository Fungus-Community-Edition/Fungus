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

    [RowVisualHandler("Physics", typeof(Collider2D), "ColliderTwoD",
        "_EditorResources/UIToolkitTemplates/VarRows/ColliderTwoDVariableRow")]
    public class ColliderTwoDRowVisualHandler : RowVisualHandler<Collider2D>
    {
    }

    [RowVisualHandler("Physics", typeof(Collider), "ColliderThreeD",
        "_EditorResources/UIToolkitTemplates/VarRows/ColliderThreeDVariableRow")]
    public class ColliderThreeDRowVisualHandler : RowVisualHandler<Collider>
    {
    }

    [RowVisualHandler("Physics", typeof(Rigidbody2D), "RigidbodyTwoD",
        "_EditorResources/UIToolkitTemplates/VarRows/RigidbodyTwoDVariableRow")]
    public class RigidbodyTwoDRowVisualHandler : RowVisualHandler<Rigidbody2D>
    {
    }

    [RowVisualHandler("Physics", typeof(Rigidbody), "RigidbodyThreeD",
        "_EditorResources/UIToolkitTemplates/VarRows/RigidbodyThreeDVariableRow")]
    public class RigidbodyThreeDRowVisualHandler : RowVisualHandler<Rigidbody>
    {
    }
}