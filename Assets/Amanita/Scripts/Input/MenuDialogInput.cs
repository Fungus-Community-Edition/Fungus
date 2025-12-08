using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Amanita.DialogueSys.UI
{
    /// <summary>
    /// Handles how MenuDialogs respond to input.
    /// </summary>
    public class MenuDialogInput : MonoBehaviour
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        [SerializeField] protected bool useAxes = true;
        [Tooltip("In response to any of these axes, this will make sure there's one button selected in the menu dialog.")]
        [SerializeField]
        protected string[] inputAxes = new string[]
        {
            "Horizontal",
            "Vertical"
        };
#endif
#if ENABLE_INPUT_SYSTEM
        [SerializeField] protected bool useActions = true;
        [Tooltip("In response to any of these actions, this will make sure there's one button selected in the menu dialog.")]
        [SerializeField] protected InputActionReference[] inputActions = new InputActionReference[0];
#endif

        protected virtual void Awake()
        {
            menuDialog = GetComponent<MenuDialog>();
        }

        protected MenuDialog menuDialog;

        protected virtual void OnEnable()
        {
            ToggleSubs(true);
        }

        protected virtual void ToggleSubs(bool on)
        {
            ToggleForNewInputSys(on);
        }

        protected virtual void ToggleForNewInputSys(bool on)
        {
#if ENABLE_INPUT_SYSTEM
            if (on)
            {
                EnableAndListenForInputActions();

                void EnableAndListenForInputActions()
                {
                    foreach (InputActionReference actionRef in inputActions)
                    {
                        if (actionRef != null && actionRef.action != null)
                        {
                            actionRef.action.Enable();
                            actionRef.action.performed += OnActionPerformed;
                        }
                    }
                }
            }
            else
            {
                UNlistenForInput();
                void UNlistenForInput()
                {
                    foreach (InputActionReference actionRef in inputActions)
                    {
                        if (actionRef != null && actionRef.action != null)
                        {
                            actionRef.action.performed -= OnActionPerformed;
                        }
                    }
                }
            }
#endif
        }

#if ENABLE_INPUT_SYSTEM
        protected virtual void OnActionPerformed(InputAction.CallbackContext context)
        {
            EnsureOneOptionIsSelected();
        }
#endif

        protected virtual void EnsureOneOptionIsSelected()
        {
            bool anyOptionsSelected = CachedButtons.Any
                (
                option => option.gameObject.activeInHierarchy
                && option.interactable &&
                EventSystem.current.currentSelectedGameObject == option.gameObject
                );

            if (!anyOptionsSelected)
            {
                Button toSelect = CachedButtons.FirstOrDefault(option => option.gameObject.activeInHierarchy && 
                option.interactable);

                if (toSelect != null)
                {
                    EventSystem.current.SetSelectedGameObject(toSelect.gameObject);
                }
            }
        }

        protected virtual IList<Button> CachedButtons
        {
            get
            {
                return menuDialog.CachedButtons;
            }
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        protected virtual void Update()
        {
            HandleResponseToInputAxes();
        }

        protected virtual void HandleResponseToInputAxes()
        {
            foreach (string inputAxisEl in inputAxes)
            {
                bool inputDetected = Input.GetAxis(inputAxisEl) != 0;
                if (inputDetected)
                {
                    EnsureOneOptionIsSelected();
                    return; // So we don't iterate over more axes per frame than necessary
                }
            }
        }
#endif
    }
}