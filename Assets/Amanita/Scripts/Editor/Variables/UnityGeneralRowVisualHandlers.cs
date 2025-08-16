using UnityEngine;
using UnityEngine.UIElements;
using EditorObjectField = UnityEditor.UIElements.ObjectField;
using UnityObject = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("UnityGeneral", typeof(GameObject), "GameObject",
        "_EditorResources/UIToolkitTemplates/VarRows/GameObjectVariableRow")]
    public class GameObjectRowVisualHandler : RowVisualHandler<GameObject>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            _gameObjectField = Root.Q<EditorObjectField>("GameObjectField");
            _gameObjectField.objectType = typeof(GameObject);
        }

        protected EditorObjectField _gameObjectField;
    }

    [RowVisualHandler("UnityGeneral", typeof(Transform), "Transform",
        "_EditorResources/UIToolkitTemplates/VarRows/TransformVariableRow")]
    public class TransformRowVisualHandler : RowVisualHandler<Transform>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            _transformField = Root.Q<EditorObjectField>("TransformField");
            _transformField.objectType = typeof(Transform);
        }

        protected EditorObjectField _transformField;
    }

    [RowVisualHandler("UnityGeneral", typeof(UnityObject), "UnityObject",
        "_EditorResources/UIToolkitTemplates/VarRows/UnityObjectVariableRow")]
    public class UnityObjectRowVisualHandler : RowVisualHandler<UnityObject>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            _transformField = Root.Q<EditorObjectField>("UnityObjectField");
            _transformField.objectType = typeof(UnityObject);
        }

        protected EditorObjectField _transformField;
    }

}