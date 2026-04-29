using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Set the interactable state of selectable objects.
    /// </summary>
    [CommandInfo("UI", 
                 "Set Interactable", 
                 "Set the interactable state of selectable objects.")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetInteractable : Command 
    {
        [Tooltip("List of objects to be affected by the command")]
        [FormerlySerializedAs("targetObjects")]
        [SerializeField] protected List<GameObject> _targetObjects = new List<GameObject>();

        [Tooltip("Controls if the selectable UI object be interactable or not")]
        [FormerlySerializedAs("interactableState")]
        [SerializeField] protected BooleanData _interactableState = new BooleanData(true);

        [SerializeField] protected BooleanData _affectChildren = new BooleanData(false);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_interactableState);
        }

        #region Public members

        public override void OnEnter()
        {
            if (_targetObjects.Count == 0)
            {
                Continue();
                return;
            }

            for (int i = 0; i < _targetObjects.Count; i++)
            {
                var targetObject = _targetObjects[i];
                var selectables = targetObject.GetComponents<Selectable>();
                for (int j = 0; j < selectables.Length; j++)
                {
                    var selectable = selectables[j];
                    selectable.interactable = _interactableState.Value;

                    if (_affectChildren.Value)
                    {
                        var childSelectables = selectable.GetComponentsInChildren<Selectable>(true);
                        for (int k = 0; k < childSelectables.Length; k++)
                        {
                            var childSelectable = childSelectables[k];
                            childSelectable.interactable = _interactableState.Value;
                        }
                    }
                }
            }
                
            Continue();
        }

        public override string GetSummary()
        {
            if (_targetObjects.Count == 0)
            {
                return "Error: No targetObjects selected";
            }
            else if (_targetObjects.Count == 1)
            {
                if (_targetObjects[0] == null)
                {
                    return "Error: No targetObjects selected";
                }
                return _targetObjects[0].name + " = " + _interactableState.Value;
            }
            
            string objectList = "";
            for (int i = 0; i < _targetObjects.Count; i++)
            {
                var go = _targetObjects[i];
                if (go == null)
                {
                    continue;
                }
                if (objectList == "")
                {
                    objectList += go.name;
                }
                else
                {
                    objectList += ", " + go.name;
                }
            }
            
            return objectList + " = " + _interactableState.Value;
        }
        
        public override Color GetButtonColor()
        {
            return new Color32(180, 250, 250, 255);
        }

        public override void OnCommandAdded(Block parentBlock)
        {
            _targetObjects.Add(null);
        }

        public override bool IsReorderableArray(string propertyName)
        {
            if (propertyName == "targetObjects")
            {
                return true;
            }

            return false;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_interactableState.VarRef, variable) || base.HasReference(variable);
        }

        #endregion
    }
}