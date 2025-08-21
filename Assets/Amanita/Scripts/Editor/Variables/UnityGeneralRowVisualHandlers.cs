using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("UnityGeneral", typeof(GameObject), "GameObject",
        "_EditorResources/UIToolkitTemplates/VarRows/GameObjectVariableRow")]
    public class GameObjectRowVisualHandler : RowVisualHandler<GameObject>
    {
    }

    [RowVisualHandler("UnityGeneral", typeof(Transform), "Transform",
        "_EditorResources/UIToolkitTemplates/VarRows/TransformVariableRow")]
    public class TransformRowVisualHandler : RowVisualHandler<Transform>
    {
    }

    [RowVisualHandler("UnityGeneral", typeof(UnityObject), "UnityObject",
        "_EditorResources/UIToolkitTemplates/VarRows/UnityObjectVariableRow")]
    public class UnityObjectRowVisualHandler : RowVisualHandler<UnityObject>
    {
    }

}