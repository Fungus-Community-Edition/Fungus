using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Serialization;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AtMycelia.Amanita.DialogueSys.UI
{
    /// <summary>
    /// Handles how MenuDialogs respond to input.
    /// </summary>
    public class MenuDialogInput : MonoBehaviour
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        [SerializeField] protected bool useAxes = true;
        [Tooltip("In response to any of these axes, this will make sure there's " +
            "one button selected in the menu dialog.")]
        [SerializeField]
        [FormerlySerializedAs("inputAxes")]
        protected string[] _inputAxes = new string[]
        {
            "Horizontal",
            "Vertical"
        };
#endif
#if ENABLE_INPUT_SYSTEM
        [SerializeField] protected bool useActions = true;
        [Tooltip("In response to any of these actions, this will make sure there's " +
            "one button selected in the menu dialog.")]
        [FormerlySerializedAs("inputActions")]
        [SerializeField] protected InputActionReference[] _inputActions = new InputActionReference[0];
#endif

        protected virtual void Awake()
        {
            _menuDialog = GetComponent<MenuDialog>();
        }

        protected MenuDialog _menuDialog;

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
            for (int i = 0; i < _inputActions.Length; i++)
            {
                var actionRef = _inputActions[i];
                if (actionRef == null || actionRef.action == null)
                {
                    string logMessage = $"MenuDialogInput on {gameObject.name} has an " +
                        $"element in its input actions array that is null or has a null " +
                        $"action reference. Please fix or remove this element.";
                    Debug.LogWarning(logMessage, this);
                    continue;
                }

                if (on)
                {
                    actionRef.action.Enable();
                    actionRef.action.performed += OnActionPerformed;
                }
                else
                {
                    actionRef.action.performed -= OnActionPerformed;
                }
            }
#endif
        }

#if ENABLE_INPUT_SYSTEM
        protected virtual void OnActionPerformed(InputAction.CallbackContext context)
        {
            if (!ShouldRespondToInput)
            {
                return;
            }
            EnsureOneOptionIsSelected();
        }
#endif

        protected virtual void EnsureOneOptionIsSelected()
        {
            if (CachedButtons == null || CachedButtons.Count == 0)
            {
                return;
            }

            bool anyOptionsSelected = IsAnyOptionSelectedAmong(CachedButtons);

            if (anyOptionsSelected)
            {
                return;
            }

            Button toSelect = FindFirstActiveAndInteractableAmong(CachedButtons);
            if (toSelect == null)
            {
                string logMessage = $"MenuDialogInput on {gameObject.name} was triggered to " +
                    $"ensure an option is selected, but no active and interactable options " +
                    $"were found among the cached buttons.";
                Debug.LogWarning(logMessage, this);
                return;
            }

            toSelect.Select();
        }

        protected virtual IReadOnlyList<Button> CachedButtons
        {
            get
            {
                return _menuDialog.CachedButtons;
            }
        }

        protected virtual bool IsAnyOptionSelectedAmong(IReadOnlyList<Button> buttons)
        {
            bool result = false;
            for (int i = 0; i < buttons.Count; i++)
            {
                Button option = buttons[i];
                if (option == null)
                {
                    continue;
                }

                if (option.gameObject.activeInHierarchy && option.interactable)
                {
                    if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == option.gameObject)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        protected virtual Button FindFirstActiveAndInteractableAmong(IReadOnlyList<Button> buttons)
        {
            Button result = null;
            for (int i = 0; i < buttons.Count; i++)
            {
                Button option = buttons[i];
                if (option == null)
                {
                    continue;
                }

                bool foundIt = option.gameObject.activeInHierarchy && option.interactable;
                if (foundIt)
                {
                    result = option;
                    break;
                }
            }
            return result;
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
            if (!ShouldRespondToInput)
            {
                return;
            }

            foreach (string inputAxisEl in _inputAxes)
            {
                bool inputDetected = Input.GetAxis(inputAxisEl) != 0;
                if (inputDetected)
                {
                    EnsureOneOptionIsSelected();
                    return; // So we don't iterate over more axes per frame than necessary
                }
            }
        }

        protected virtual bool ShouldRespondToInput => _menuDialog.VisibleButtons.Count > 0;
#endif
    }
}