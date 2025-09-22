using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("UnityGeneral", typeof(GameObject), "GameObject",
        "UIToolkitTemplates/VarRows/UnityGeneral/GameObjectVariableRow")]
    public class GameObjectRowVisualHandler : RowVisualHandler<GameObject>
    {
    }

    [RowVisualHandler("UnityGeneral", typeof(Transform), "Transform",
        "UIToolkitTemplates/VarRows/UnityGeneral/TransformVariableRow")]
    public class TransformRowVisualHandler : RowVisualHandler<Transform>
    {
    }

    [RowVisualHandler("UnityGeneral", typeof(UnityObject), "UnityObject",
        "UIToolkitTemplates/VarRows/UnityGeneral/UnityObjectVariableRow")]
    public class UnityObjectRowVisualHandler : RowVisualHandler<UnityObject>
    {
    }

}