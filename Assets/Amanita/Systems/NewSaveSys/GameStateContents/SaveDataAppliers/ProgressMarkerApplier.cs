namespace Amanita.SaveSys
{
    [SaveSysDisplayName("Progress Marker Applier (Amanita Default)")]
    public class ProgressMarkerApplier : SaveDataApplier<ProgressMarkerSaveData>
    {
        public override void Apply(SaveData saveData, System.Action onComplete)
        {
            Apply(saveData as ProgressMarkerSaveData);
            onComplete?.Invoke();
        }

        public override void Apply(ProgressMarkerSaveData saveData)
        {
            SaveSystem saveSys = SaveSystem.S;
            var marker = saveData.Marker;
            saveSys.ClearProgressMarkers();
            saveSys.EnsureMarkerRegistered(marker.Id, marker.Order);
        }

    }
}