using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [EventHandlerInfo("UI/ButtonInGroupClicked", 
        "Button In Group Clicked",
        "Called when any buttons parented to specified GameObjects are clicked.")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class ButtonInGroupClicked : EventHandler
    {
        [Tooltip("The buttons parented to any of these will be responded to.")]
        [SerializeField] private GameObject[] parents;

        protected override void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // We want a slight delay in case the buttons are programmatically
            // added to the parents at the start of the scene.
            Invoke(nameof(GatherUpButtonsBeforeSubRetoggle), 0.1f);
        }

        void GatherUpButtonsBeforeSubRetoggle()
        {
            for (int i = 0; i < parents.Length; i++)
            {
                Button[] buttons = parents[i].GetComponentsInChildren<Button>();
                for (int j = 0; j < buttons.Length; j++)
                {
                    var currentButton = buttons[j];
                    _buttons.Add(currentButton);
                }
            }

            ToggleSubs(false);
            ToggleSubs(true);
        }

        protected IList<Button> _buttons = new List<Button>();

        protected override void ToggleSubs(bool on)
        {
            if (!Application.isPlaying && ToggleSubsOnlyInRuntime)
            {
                return;
            }

            base.ToggleSubs(on);

            for (int i = 0; i < _buttons.Count; i++)
            {
                var currentButton = _buttons[i];
                var clickEvent = currentButton.onClick;

                if (on)
                {
                    clickEvent.AddListener(OnButtonClicked);
                }
                else
                {
                    clickEvent.RemoveListener(OnButtonClicked);
                }
            }
        }

        protected override bool ToggleSubsOnlyInRuntime => true;

        private void OnButtonClicked()
        {
            ExecuteBlock();
        }

    }
}