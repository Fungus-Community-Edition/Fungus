using UnityEngine;
using UnityEngine.UIElements;
using EditorObjectField = UnityEditor.UIElements.ObjectField;

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
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            _colliderTwoDField = Root.Q<EditorObjectField>("UnityObjectField");
            _colliderTwoDField.objectType = typeof(Collider2D);
        }

        protected EditorObjectField _colliderTwoDField;
    }

    [RowVisualHandler("Physics", typeof(Collider), "ColliderThreeD",
        "_EditorResources/UIToolkitTemplates/VarRows/ColliderThreeDVariableRow")]
    public class ColliderThreeDRowVisualHandler : RowVisualHandler<Collider>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            _colliderThreeField = Root.Q<EditorObjectField>("UnityObjectField");
            _colliderThreeField.objectType = typeof(Collider);
        }

        protected EditorObjectField _colliderThreeField;
    }
}