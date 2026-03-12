using System.Collections.Generic;
using System;
using AtMycelia.FSExt;

namespace AtMycelia.SaveSys
{
    [SaveSysDisplayName("Progress Marker Codec (Amanita Default)")]
    [SaveSysAssetName("DefProgMarkerCodec")]
    public class ProgressMarkerSaveCodec : SaveCodec<ProgressMarker, ProgressMarkerSaveData>,
        IMainSaveCodec, IMainSaveDataProducer
    {
        public override bool CanHandle(string typeName)
        {
            return typeName == typeof(ProgressMarkerSaveData).Name || typeName == typeof(ProgressMarker).Name;
        }

        public override ProgressMarkerSaveData Decode(string rawText)
        {
            lock (Serializer)
            {
                ProgressMarkerSaveData result = Serializer.FromJson<ProgressMarkerSaveData>(rawText);
                return result;
            }
        }

        public IList<SaveData> FindAndCreateAll(Action<IList<SaveData>> onComplete = null)
        {
            IList<SaveData> results = new List<SaveData>();
            var markers = SaveSystem.ProgressMarkers;
            for (int i = 0; i < markers.Count; i++)
            {
                var markerEl = markers[i];
                var saveData = EncodeToSave(markerEl);
                results.Add(saveData);
            }
            onComplete?.Invoke(results);
            return results;
        }

        public override ProgressMarkerSaveData EncodeToSave(ProgressMarker toEncode)
        {
            var saveData = new ProgressMarkerSaveData
            {
                Marker = toEncode
            };
            return saveData;
        }

        public void PreInstallInit()
        {
            // No op, at least for now
        }
    }
}