using UnityEngine;

namespace AtMycelia.Amanita.VScripting.EditorUtils.FcWindow
{
    public sealed class FcwPlayModeFocusService
    {
        private string _lastPlayModeFcUid;

        public bool HasCachedFocus => !string.IsNullOrEmpty(_lastPlayModeFcUid);
        public string LastFocusedUid => _lastPlayModeFcUid;

        public bool TryCacheFromSelection(Flowchart resolved, out string cachedUid)
        {
            cachedUid = null;
            if (!Application.isPlaying || resolved == null)
            {
                return false;
            }

            _lastPlayModeFcUid = resolved.UniqueId;
            cachedUid = _lastPlayModeFcUid;
            return true;
        }

        public bool TryCacheFromActiveFlowchart(Flowchart activeFlowchart, out string cachedUid)
        {
            cachedUid = null;
            if (activeFlowchart == null)
            {
                return false;
            }

            _lastPlayModeFcUid = activeFlowchart.UniqueId;
            cachedUid = _lastPlayModeFcUid;
            return true;
        }

        public bool TryResolveLastFocused(FcwFlowchartStateService stateService, out Flowchart flowchart)
        {
            flowchart = null;
            if (stateService == null || string.IsNullOrEmpty(_lastPlayModeFcUid))
            {
                return false;
            }

            return stateService.TryGetFlowchartByUid(_lastPlayModeFcUid, out flowchart);
        }
    }
}