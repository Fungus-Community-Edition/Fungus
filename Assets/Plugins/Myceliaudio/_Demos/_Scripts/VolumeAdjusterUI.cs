using UnityEngine;
using UnityEngine.UI;

namespace AtMycelia.Myceliaudio
{
    public class VolumeAdjusterUI : MonoBehaviour
    {
        [SerializeField] protected TrackGroup _trackType;
        [SerializeField] protected int _track = 0;
        [SerializeField] [Min(1)] protected int adjustmentInterval = 10;
        [SerializeField] protected Button lowerVolButton, raiseVolButton;

        protected virtual void Awake()
        {
            UpdateCurrentVol();
        }

        protected virtual void UpdateCurrentVol()
        {
            _currentVol = AudioSys.GetTrackGroupVol(_trackType);
        }

        protected float _currentVol;

        protected virtual AudioSystem AudioSys { get { return AudioSystem.S; } }

        protected virtual void OnEnable()
        {
            lowerVolButton.onClick.AddListener(OnLowerVolButtonClicked);
            raiseVolButton.onClick.AddListener(OnRaiseVolButtonClicked);
        }

        protected virtual void OnLowerVolButtonClicked()
        {
            AlterVol(lowerIt);
        }

        protected static int lowerIt = -1;

        protected virtual void AlterVol(int sign)
        {
            UpdateCurrentVol();
            float volAdjustment = adjustmentInterval * sign;
            float targetVol = _currentVol + volAdjustment;
            targetVol = Mathf.Clamp(targetVol, AudioMath.MinVol, AudioMath.MaxVol);
            AudioSys.SetTrackGroupVol(_trackType, targetVol);
            UpdateCurrentVol();
        }

        protected virtual void OnRaiseVolButtonClicked()
        {
            AlterVol(raiseIt);
        }

        protected static int raiseIt = 1;

        protected virtual void OnDisable()
        {
            lowerVolButton.onClick.RemoveListener(OnLowerVolButtonClicked);
            raiseVolButton.onClick.RemoveListener(OnRaiseVolButtonClicked);
        }

    }
}