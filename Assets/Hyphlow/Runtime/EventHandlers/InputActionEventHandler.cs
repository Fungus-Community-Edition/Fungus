#if ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Execute a block when a targeted InputAction is performed. Optionally reads the value from the action.
    /// </summary>
    [EventHandlerInfo("Input",
                      "Input Action",
                      "Execute a block when a targeted InputAction is performed. " +
                      "Optionally reads the value from the action.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class InputActionEventHandler : EventHandler
    {
        [SerializeField] protected InputActionReference inputAction;

        [VariableProperty()]
        [SerializeField] protected Variable variable;

        protected override void ToggleSubs(bool on)
        {
            base.ToggleSubs(on);
            if (inputAction == null)
            {
                Debug.LogError($"[InputActionEventHandler]: Missing InputAction in Flowchart {_fChart.name}, " +
                    $"Block {ParentBlock.BlockName}");
                return;
            }

            if (on)
            {
                inputAction.action.performed += OnInputActionPerformed;
            }
            else
            {
                inputAction.action.performed -= OnInputActionPerformed;
            }
        }

        protected virtual void OnInputActionPerformed(InputAction.CallbackContext obj)
        {
            if (variable != null)
            {
                variable.SetValue(inputAction.action.ReadValueAsObject());
            }

            ExecuteBlock();
        }

        public override string GetSummary()
        {
            if (inputAction == null)
            {
                return "Error: no InputAction set";
            }

            return inputAction.action.name;
        }
    }
}

#endif