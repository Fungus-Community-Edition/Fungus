using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Amanita.UI.Legacy;

namespace AtMycelia.Amaniphlow
{
    /// <summary>
    /// Updates Amanita's Narrative Log in response to certain Hyphlow events.
    /// </summary>
    public class MR_NarrativeLogUpdater : MonoBehaviour
    {
        [SerializeField] protected NarrativeLogMenu _narrLog;

        protected virtual void Awake()
        {
            if (_narrLog == null)
            {
                _narrLog = GetComponent<NarrativeLogMenu>();
            }

            if (_narrLog == null)
            {
                string errorMessage = $"Narrative Log Menu not found by Narrative Log Updater!";
                Debug.LogError(errorMessage);
            }
        }

        protected virtual void OnEnable()
        {
            ToggleSubs(true);
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                BlockSignals.BlockExecEnded += OnBlockEnd;
            }
            else
            {
                BlockSignals.BlockExecEnded -= OnBlockEnd;
            }
            
        }

        protected virtual void OnBlockEnd(IBlock block)
        {
            // At block end update to get the last line of the block
            bool defaultPreviousLines = _narrLog.PreviousLines;
            _narrLog.PreviousLines = false;
            _narrLog.UpdateVisuals();
            _narrLog.PreviousLines = defaultPreviousLines;
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }
    }
}