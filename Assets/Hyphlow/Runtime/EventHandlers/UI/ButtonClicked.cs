using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// The block will execute when the user clicks on the target UI button object.
    /// </summary>
    [EventHandlerInfo("UI",
                      "Button Clicked",
                      "The block will execute when the user clicks on the target UI button object.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class ButtonClicked : EventHandler
    {
        [Tooltip("The UI Button that the user can click on")]
        [ContentTypeConstraint(typeof(GameObject), typeof(Component))]
        [SerializeField] protected AnyVariableData _targetButton;

        private void FetchButtonComponent()
        {
            if (_button != null)
            {
                return;
            }
            Component component = _targetButton.GetValue<Component>();
            GameObject gameObject = _targetButton.GetValue<GameObject>();
            if (component != null)
            {
                _button = component.GetComponent<Button>();
            }
            else if (gameObject != null)
            {
                _button = gameObject.GetComponent<Button>();
            }
            else if (Application.isPlaying)
            {
                Debug.LogError("ButtonClicked event handler requires a Button component reference.", this);
                return;
            }
        }

        private Button _button;

        protected override void ToggleSubs(bool on)
        {
            base.ToggleSubs(on);
            
            if (_button == null)
            {
                FetchButtonComponent();
                if (_button == null)
                {
                    return;
                }
            }

            if (on)
            {
                _button.onClick.AddListener(OnButtonClick);
            }
            else
            {
                _button.onClick.RemoveListener(OnButtonClick);
            }
        }

        protected virtual void OnButtonClick()
        {
            ExecuteBlock();
        }

        public override string GetSummary()
        {
            FetchButtonComponent();
            if (_button != null)
            {
                return _button.name;
            }

            return "Error: no targetButton set.";
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_oldTargetButton != null)
            {
                _targetButton.SetFor<GameObjectMuscariable, GameObject>();
                _targetButton.BoxedValue = _oldTargetButton.gameObject;
                _oldTargetButton = null;
            }
        }

        [FormerlySerializedAs("targetButton")]
        [SerializeField] [HideInInspector] protected Button _oldTargetButton;
    }
}
