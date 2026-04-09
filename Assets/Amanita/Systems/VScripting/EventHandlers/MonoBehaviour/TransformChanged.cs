using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// The block will execute when the desired OnTransform related
    /// message for the Monobehaviour is received.
    /// </summary>
    [EventHandlerInfo("MonoBehaviour",
                      "Transform",
                      "The block will execute when the desired OnTransform " +
                      "related message for the Nonobehaviour is received.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class TransformChanged : EventHandler
    {

        [System.Flags]
        public enum TransformMessageFlags
        {
            OnTransformChildrenChanged = 1 << 0,
            OnTransformParentChanged = 1 << 1,
        }

        [Tooltip("Which of the OnTransformChanged messages to trigger on.")]
        [SerializeField]
        [EnumFlag]
        protected TransformMessageFlags FireOn = TransformMessageFlags.OnTransformChildrenChanged | TransformMessageFlags.OnTransformParentChanged;

        private void OnTransformChildrenChanged()
        {
            if ((FireOn & TransformMessageFlags.OnTransformChildrenChanged) != 0)
            {
                ExecuteBlock();
            }
        }

        private void OnTransformParentChanged()
        {
            if ((FireOn & TransformMessageFlags.OnTransformParentChanged) != 0)
            {
                ExecuteBlock();
            }
        }
    }
}
