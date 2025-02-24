using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CGT.UI
{
    public class SliderStep : MonoBehaviour
    {
        [SerializeField] protected Slider _slider;
        [SerializeField] [Min(0)] protected int _stepCount = 10;

        public virtual int StepCount
        {
            get { return _stepCount; }
            set { _stepCount = value; }
        }

        protected virtual void Awake()
        {
            if (_slider == null)
            {
                _slider = GetComponent<Slider>();
            }

            float diff = _slider.maxValue - _slider.minValue;
            _stepInterval = diff / _stepCount;
        }

        protected float _stepInterval = 1f;

        public virtual float StepInterval
        {
            get { return _stepInterval; }
            protected set { _stepInterval = value; }
        }

        protected virtual void OnEnable()
        {
            _slider.onValueChanged.AddListener(ApplyStep);
        }

        protected virtual void ApplyStep(float newVal)
        {
            float stepToMoveTo = Mathf.Round(newVal / _stepInterval); // Should be between 0 and stepCount
            float steppedValue = stepToMoveTo * _stepInterval;

            // This may execute many times in quick succession due to how many non-stepped
            // value-changes that dragging a slider can cause.
            if (steppedValue != newVal)
            {
                _slider.SetValueWithoutNotify(steppedValue);
                StepApplied(steppedValue);
            }
        }

        public event UnityAction<float> StepApplied = delegate { };

        protected virtual void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(ApplyStep);
        }

    }
}