using System;
using System.Collections.Generic;

namespace AtMycelia.SaveSys
{
    [SaveSysDisplayName("Progress Marker Applier (Amanita Default)")]
    [SaveSysAssetName("DefProgMarkerApplier")]
    public class ProgressMarkerApplier : SaveDataApplier<ProgressMarkerSaveData>
    {
        public override void Apply(SaveData saveData, System.Action onComplete)
        {
            Apply(saveData as ProgressMarkerSaveData);
            onComplete?.Invoke();
        }

        public override void Apply(ProgressMarkerSaveData saveData)
        {
            var marker = saveData.Marker;
            SaveSystem.EnsureMarkerRegistered(marker.Id, marker.Order);
        }

        public override void ApplyRange(IList<SaveData> datas, Action onComplete)
        {
            SaveSystem.ClearProgressMarkers();
            base.ApplyRange(datas, onComplete);
        }

    }
}