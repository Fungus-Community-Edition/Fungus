using UnityEngine;
using UnityEngine.UI;
using Amanita.UI;

namespace Amanita.Myceliaudio
{
    public abstract class AudioSliderComponent : MonoBehaviour
    {
        [SerializeField] protected Slider _slider;
        [SerializeField] protected TrackGroup _trackGroup;

        [Tooltip("If this is assigned, this only does its thing when steps are done")]
        [SerializeField] protected SliderStep _sliderStep;
        [Tooltip("Whether or not this should do its thing based on the SliderStep component")]
        [SerializeField] protected bool _useSliderStep;

        [Tooltip("Whether this should do its thing right away")]
        [SerializeField] protected bool _applyOnStart = false;

        protected virtual void Awake()
        {
            _prevStepVal = _slider.value;
            // ^To prevent things from activating twice in quick succession in
            // response to a single step
        }

        protected float _prevStepVal;

        protected virtual void Start()
        {
            if (_applyOnStart)
            {
                InitApply();
            }
        }

        protected virtual void InitApply()
        {

        }

        protected virtual bool ShouldUseSliderStep { get { return _useSliderStep && _sliderStep != null; } }

        protected virtual bool IsDifferentStepValue(float newValue)
        {
            return newValue != _prevStepVal;
        }

    }
}