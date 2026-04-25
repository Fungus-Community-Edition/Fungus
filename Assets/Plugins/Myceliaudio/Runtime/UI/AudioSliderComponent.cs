using AtMycelia.UI;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;
#endif
using UnityEngine.EventSystems;

namespace AtMycelia.Myceliaudio
{
    public abstract class AudioSliderComponent : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        [SerializeField] protected InputActionReference _movementInput;
#endif
        [SerializeField] protected Slider _slider;
        [SerializeField] protected TrackGroup _trackGroup;
        public virtual TrackGroup TrackGroup { get { return _trackGroup; } }

        [Tooltip("If this is assigned, then when using the mouse to drag, this only does its thing when steps are done ")]
        [SerializeField] protected SliderStep _sliderStep;
        [Tooltip("Whether or not this should do its thing based on the SliderStep component")]
        [SerializeField] protected bool _useSliderStep;
        [Tooltip("Whether or not this should do its thing in response to the specified movement input. Meant mainly for gamepads.")]
        [SerializeField] protected bool _useMovementInput;

        [Tooltip("Whether this should do its thing right away")]
        [SerializeField] protected bool _applyOnStart = false;
        [SerializeField] protected float _delayBeforeApply = 0f;

        protected virtual void OnEnable()
        {
            if (ShouldUseSliderStep)
            {
                _sliderStep.StepApplied += OnSliderValChanged;
            }
            else
            {
                _slider.onValueChanged.AddListener(OnSliderValChanged);
            }
#if ENABLE_INPUT_SYSTEM
            if (_movementInput != null)
            {
                _movementInput.action.Enable();
                _movementInput.action.performed += OnMovementInput;
            }
#endif
        }

        protected virtual void OnSliderValChanged(float newVal)
        {
            
        }

#if ENABLE_INPUT_SYSTEM
        protected virtual void OnMovementInput(CallbackContext ctx)
        {
            if (_useMovementInput)
            {
                // ^Why put the check here instead of OnEnable? For the sake of debugging
                if (EventSystem.current.currentSelectedGameObject == _slider.gameObject)
                {
                    Apply();
                }
            }
        }
#endif
        protected virtual void OnDisable()
        {
            if (ShouldUseSliderStep)
            {
                _sliderStep.StepApplied -= OnSliderValChanged;
            }
            else
            {
                _slider.onValueChanged.RemoveListener(OnSliderValChanged);
            }

#if ENABLE_INPUT_SYSTEM
            if (_movementInput != null)
            {
                _movementInput.action.performed -= OnMovementInput;
            }
#endif
        }

        protected virtual void Start()
        {
            AlignToSystemVol();
            _prevStepVal = _slider.value;
            // ^To prevent things from activating twice in quick succession in
            // response to a single step

            if (_applyOnStart)
            {
                if (_delayBeforeApply <= 0)
                {
                    Apply();
                }
                else
                {
                    Invoke(nameof(Apply), _delayBeforeApply);
                }
            }
        }

        protected virtual void AlignToSystemVol()
        {
            float vol = AudioSystem.S.GetTrackGroupVol(_trackGroup);
            SyncWith(vol);
        }

        protected virtual void SyncWith(float newVol)
        {
            float percentage = newVol / 100f;
            // The formula for lerp is:
            // lerpedValue = startValue + (range * percentage)
            // range = (endValue - startValue)
            float result = Mathf.Lerp(_slider.minValue, _slider.maxValue, percentage);

            _slider.SetValueWithoutNotify(result); // To avoid stack overflows
        }

        protected float _prevStepVal;

        protected virtual void Apply()
        {

        }

        protected virtual bool ShouldUseSliderStep { get { return _useSliderStep && _sliderStep != null; } }

        protected virtual bool IsDifferentStepValue(float newValue)
        {
            return newValue != _prevStepVal;
        }

    }
}