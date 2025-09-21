using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Physics", typeof(Vector2), "VectorTwo",
        "UIToolkitTemplates/VarRows/VectorTwoVariableRow")]
    public class VectorTwoRowVisualHandler : RowVisualHandler<Vector2>
    {

    }

    [RowVisualHandler("Physics", typeof(Vector3), "VectorThree",
        "UIToolkitTemplates/VarRows/VectorThreeVariableRow")]
    public class VectorThreeRowVisualHandler : RowVisualHandler<Vector3>
    {

    }

    [RowVisualHandler("Physics", typeof(Collider2D), "ColliderTwoD",
        "UIToolkitTemplates/VarRows/ColliderTwoDVariableRow")]
    public class ColliderTwoDRowVisualHandler : RowVisualHandler<Collider2D>
    {
    }

    [RowVisualHandler("Physics", typeof(Collider), "ColliderThreeD",
        "UIToolkitTemplates/VarRows/ColliderThreeDVariableRow")]
    public class ColliderThreeDRowVisualHandler : RowVisualHandler<Collider>
    {
    }

    [RowVisualHandler("Physics", typeof(Rigidbody2D), "RigidbodyTwoD",
        "UIToolkitTemplates/VarRows/RigidbodyTwoDVariableRow")]
    public class RigidbodyTwoDRowVisualHandler : RowVisualHandler<Rigidbody2D>
    {
    }

    [RowVisualHandler("Physics", typeof(Rigidbody), "RigidbodyThreeD",
        "UIToolkitTemplates/VarRows/RigidbodyThreeDVariableRow")]
    public class RigidbodyThreeDRowVisualHandler : RowVisualHandler<Rigidbody>
    {
    }
}