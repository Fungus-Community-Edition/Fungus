using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [RowVisualHandler(menuName: "Physics",
        contentType: typeof(Collider2D),
        typeDisplayName: "ColliderTwoD",
        pathToTemplate: "UIToolkitTemplates/VarRows/Physics/ColliderTwoDVariableRow")]
    public class ColliderTwoDRowVisualHandler : RowVisualHandler<Collider2D>
    {
    }

    [RowVisualHandler(menuName: "Physics",
        contentType: typeof(Collider), 
        typeDisplayName: "ColliderThreeD",
        pathToTemplate: "UIToolkitTemplates/VarRows/Physics/ColliderThreeDVariableRow")]
    public class ColliderThreeDRowVisualHandler : RowVisualHandler<Collider>
    {
    }

    [RowVisualHandler(menuName: "Physics",
        contentType: typeof(Rigidbody2D),
        typeDisplayName: "RigidbodyTwoD",
        pathToTemplate: "UIToolkitTemplates/VarRows/Physics/RigidbodyTwoDVariableRow")]
    public class RigidbodyTwoDRowVisualHandler : RowVisualHandler<Rigidbody2D>
    {
    }

    [RowVisualHandler(menuName: "Physics",
        contentType: typeof(Rigidbody),
        typeDisplayName: "RigidbodyThreeD",
        pathToTemplate: "UIToolkitTemplates/VarRows/Physics/RigidbodyThreeDVariableRow")]
    public class RigidbodyThreeDRowVisualHandler : RowVisualHandler<Rigidbody>
    {
    }
}