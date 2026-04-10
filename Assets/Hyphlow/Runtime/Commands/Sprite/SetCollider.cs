using UnityEngine;
using System.Collections.Generic;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sets all collider (2d or 3d) components on the target objects to be active / inactive.
    /// </summary>
    [CommandInfo("Sprite", 
                 "Set Collider", 
                 "Sets all collider (2d or 3d) components on the target objects to be active / inactive")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetCollider : Command
    {       
        [Tooltip("A list of gameobjects containing collider components to be set active / inactive")]
        [SerializeField] protected List<GameObject> targetObjects = new List<GameObject>();

        [Tooltip("All objects with this tag will have their collider set active / inactive")]
        [SerializeField] protected string targetTag = "";

        [Tooltip("Set to true to enable the collider components")]
        [SerializeField] protected BooleanData activeState;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(activeState);
        }

        protected virtual void SetColliderActive(GameObject go)
        {
            if (go != null)     
            {
                // 3D objects
                var colliders = go.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    var c = colliders[i];
                    c.enabled = activeState.Value;
                }

                // 2D objects
                var collider2Ds = go.GetComponentsInChildren<Collider2D>();
                for (int i = 0; i < collider2Ds.Length; i++)
                {
                    var c = collider2Ds[i];
                    c.enabled = activeState.Value;
                }
            }
        }

        #region Public members

        public override void OnEnter()  
        {
            for (int i = 0; i < targetObjects.Count; i++)
            {
                var go = targetObjects[i];
                SetColliderActive(go);
            }

            var taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);

            if (taggedObjects != null)
            {
                for (int i = 0; i < taggedObjects.Length; i++)
                {
                    var go = taggedObjects[i];
                    SetColliderActive(go);
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            int count = targetObjects.Count;

            if (activeState.Value)
            {
                return "Enable " + count + " collider objects.";
            }
            else
            {
                return "Disable " + count + " collider objects.";
            }
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow; 
        }

        public override bool IsReorderableArray(string propertyName)
        {
            return propertyName == "targetObjects";
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(activeState.VarRef, variable) || base.HasReference(variable);
        }

        #endregion
    }
        
}
