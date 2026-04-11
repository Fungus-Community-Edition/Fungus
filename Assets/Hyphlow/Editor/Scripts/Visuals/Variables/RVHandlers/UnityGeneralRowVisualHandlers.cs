using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [RowVisualHandler(menuName: "UnityGeneral",
        contentType: typeof(GameObject),
        typeDisplayName: "GameObject",
        pathToTemplate: "UIToolkitTemplates/VarRows/UnityGeneral/GameObjectVariableRow")]
    public class GameObjectRowVisualHandler : RowVisualHandler<GameObject>
    {
    }

    [RowVisualHandler(menuName: "UnityGeneral",
        contentType: typeof(Transform),
        typeDisplayName: "Transform",
        pathToTemplate: "UIToolkitTemplates/VarRows/UnityGeneral/TransformVariableRow")]
    public class TransformRowVisualHandler : RowVisualHandler<Transform>
    {
    }

    [RowVisualHandler(menuName: "UnityGeneral",
        contentType: typeof(UnityObject),
        typeDisplayName: "UnityObject",
        pathToTemplate: "UIToolkitTemplates/VarRows/UnityGeneral/UnityObjectVariableRow")]
    public class UnityObjectRowVisualHandler : RowVisualHandler<UnityObject>
    {
    }

}