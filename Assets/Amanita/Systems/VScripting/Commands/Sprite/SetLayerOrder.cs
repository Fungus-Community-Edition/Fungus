using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Sets the Renderer sorting layer of every child of a game object. Applies to all Renderers (including mesh, skinned mesh, and sprite).
    /// </summary>
    [CommandInfo("Sprite", 
                 "Set Sorting Layer", 
                 "Sets the Renderer sorting layer of every child of a game object. Applies to all Renderers (including mesh, skinned mesh, and sprite).")]
    [AddComponentMenu("")]
    public class SetSortingLayer : Command 
    {
        [Tooltip("Root Object that will have the Sorting Layer set. Any children will also be affected")]
        [SerializeField] protected GameObject targetObject;
        
        [Tooltip("The New Layer Name to apply")]
        [SerializeField] protected string sortingLayer;

        protected void ApplySortingLayer(Transform target, string layerName) 
        {
            var renderer = target.gameObject.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.sortingLayerName = layerName;
                Debug.Log(target.name);
            }

            var targetTransform = target.transform;
            foreach (Transform child in targetTransform)
            {
                ApplySortingLayer(child, layerName);
            }
        }       

        #region Public members

        public override void OnEnter()
        {
            if (targetObject != null)
            {
                ApplySortingLayer(targetObject.transform, sortingLayer);
            }
            
            Continue();
        }
        
        public override string GetSummary()
        {
            if (targetObject == null)
            {
                return "Error: No game object selected";
            }
            
            return targetObject.name;
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        #endregion
    }
}
