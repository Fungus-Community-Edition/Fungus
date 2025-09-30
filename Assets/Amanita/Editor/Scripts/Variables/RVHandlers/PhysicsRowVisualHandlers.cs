using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Physics", typeof(Collider2D), "ColliderTwoD",
        "UIToolkitTemplates/VarRows/Physics/ColliderTwoDVariableRow")]
    public class ColliderTwoDRowVisualHandler : RowVisualHandler<Collider2D>
    {
    }

    [RowVisualHandler("Physics", typeof(Collider), "ColliderThreeD",
        "UIToolkitTemplates/VarRows/Physics/ColliderThreeDVariableRow")]
    public class ColliderThreeDRowVisualHandler : RowVisualHandler<Collider>
    {
    }

    [RowVisualHandler("Physics", typeof(Rigidbody2D), "RigidbodyTwoD",
        "UIToolkitTemplates/VarRows/Physics/RigidbodyTwoDVariableRow")]
    public class RigidbodyTwoDRowVisualHandler : RowVisualHandler<Rigidbody2D>
    {
    }

    [RowVisualHandler("Physics", typeof(Rigidbody), "RigidbodyThreeD",
        "UIToolkitTemplates/VarRows/Physics/RigidbodyThreeDVariableRow")]
    public class RigidbodyThreeDRowVisualHandler : RowVisualHandler<Rigidbody>
    {
    }
}