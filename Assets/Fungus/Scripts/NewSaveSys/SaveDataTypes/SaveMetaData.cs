

using System.Globalization;
using System;
using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// For things that you'd want to show in the Save Slot UI or things that you'd otherwise
    /// not really consider part of the save's main state.
    /// </summary>
    public class SaveMetaData : SaveData, IEquatable<SaveMetaData>
    {
        [SerializeField] protected string saveID = string.Empty;
        [SerializeField] protected float saveVersion = 1;
        [SerializeField] protected string utcTimeStamp = string.Empty;

        public string SaveID => saveID;
        public float SaveVersion
        {
            get { return saveVersion; }
            set { saveVersion = value; }
        }
        public string UTCTimeStamp
        {
            get { return utcTimeStamp; }
            set
            {
                utcTimeStamp = value;
                UpdateTimeStampStructure();
            }
        }

        protected virtual void UpdateTimeStampStructure()
        {
            IFormatProvider provider = CultureInfo.InvariantCulture;
            DateTimeStyles style = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

            bool successfulParse = DateTime.TryParseExact(
                utcTimeStamp, iso8601Format,
                provider, style,
                out var result);

            timeStamp = result;
            if (!successfulParse)
            {
                timeStamp = DateTime.UnixEpoch;
            }
        }

        protected static string iso8601Format = "o";

        public DateTime TimeStamp
        {
            get { return timeStamp; }
            set
            {
                timeStamp = value;
                UpdateTimeStampString();
            }
        }

        protected DateTime timeStamp;

        protected virtual void UpdateTimeStampString()
        {
            utcTimeStamp = timeStamp.ToString(iso8601Format);
        }
        protected virtual void UpdateTimeStamp()
        {
            utcTimeStamp = DateTime.UtcNow.ToString(iso8601Format);
            UpdateTimeStampStructure();
        }

        public override SaveDataUnit Serialized()
        {
            string json = JsonUtility.ToJson(this, true);
            SaveDataUnit result = new(TypeName, json);
            return result;
        }

        public SaveMetaData(string saveID = null, DateTime timeStamp = default)
        {
            this.saveID = saveID;

            if (string.IsNullOrEmpty(this.saveID))
            {
                this.saveID = System.Guid.NewGuid().ToString();
            }

            this.timeStamp = timeStamp;

            if (this.timeStamp == default)
            {
                this.timeStamp = DateTime.UtcNow;
            }

            UpdateTimeStampString();
        }

        public static new SaveMetaData DeserializeFrom(SaveDataUnit item)
        {
            SaveMetaData result = new SaveMetaData();
            JsonUtility.FromJsonOverwrite(item.Content, result);
            result.OnDeserialize();
            return result;
        }

        public override void OnDeserialize()
        {
            base.OnDeserialize();
            UpdateTimeStampStructure();
        }

        public virtual bool Equals(SaveMetaData other)
        {
            if (other == null) return false;
            return saveID == other.saveID &&
                saveVersion == other.saveVersion &&
                utcTimeStamp == other.utcTimeStamp;
        }

    }
}