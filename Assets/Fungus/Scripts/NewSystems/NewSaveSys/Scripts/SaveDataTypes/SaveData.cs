
using System;
using System.Globalization;
using UnityEngine;

namespace Amanita.SaveSys
{
    [Serializable]
    public abstract class SaveData
    {
        [SerializeField] protected string saveID;
        [SerializeField] protected float saveVersion = 1;
        [SerializeField] protected string utcTimeStamp = string.Empty;

        public string SaveID => saveID;
        public float SaveVersion => saveVersion;
        public string SavedAtUtc => utcTimeStamp;

        protected SaveData()
        {
            saveID = Guid.NewGuid().ToString();
            UpdateTimeStamp();
        }

        protected virtual void UpdateTimeStamp()
        {
            utcTimeStamp = DateTime.UtcNow.ToString(iso8601Format);
            UpdateTimeStampDateTime();
        }
        protected static string iso8601Format = "o";

        protected virtual void UpdateTimeStampDateTime()
        {
            IFormatProvider provider = CultureInfo.InvariantCulture;
            DateTimeStyles style = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

            bool successfulParse = DateTime.TryParseExact(
                utcTimeStamp, iso8601Format,
                provider, style,
                out var result);

            TimeStamp = result;
            if (!successfulParse)
            {
                TimeStamp = DateTime.UnixEpoch;
            }
        }

        protected SaveData(string saveID)
        {
            this.saveID = saveID;
            UpdateTimeStamp();
        }

        // Optionally, allow setting timestamp manually
        protected SaveData(string saveID, string savedAtUtc)
        {
            this.saveID = saveID;
            this.utcTimeStamp = savedAtUtc;
            UpdateTimeStampDateTime();
        }

        public abstract SaveDataItem ToSaveDataItem();

        public virtual void OnDeserialize()
        {
            UpdateTimeStampDateTime();
        }

        public DateTime TimeStamp { get; protected set; }
    }


}