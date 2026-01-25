using Amanita.Utils;
using System;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Collections;
using System.Linq;

namespace Amanita.SaveSys
{
    /// <summary>
    /// For things that you'd want to show in the Save Slot UI or things that you'd otherwise
    /// not really consider part of the save's main state.
    /// </summary>
    [Serializable]
    public class SaveMetaData : SaveData, ISaveMetaData, IEquatable<SaveMetaData>
    {
        [SerializeField] protected string name = string.Empty;
        // ^To let players personalize their saves and get a better sense
        // of ownership over their progress
        [SerializeField] protected string saveId = string.Empty;
        [SerializeField] protected int slotNumber = 1;
        [SerializeField] protected string saveVersion = "null";
        [SerializeField] protected string utcTimeStamp = string.Empty;
        [SerializeField] protected string sceneName = string.Empty;
        [SerializeField] protected int sceneBuildIndex = -1;
        [SerializeField] protected string playtimeStamp = TimeSpan.Zero.ToString();
        [SerializeField] protected IList<ProgressMarker> progressMarkers = new List<ProgressMarker>();

        public bool IsValid
        {
            get
            {
                bool validID = !string.IsNullOrEmpty(saveId);
                bool validVersion = !string.IsNullOrEmpty(saveVersion);
                bool validTimeStamp = !string.IsNullOrEmpty(utcTimeStamp);
                return validID && validVersion && validTimeStamp;
            }
        }

        public string SaveName
        {
            get { return name; }
            set { name = value; }
        }

        public string SaveId
        {
            get { return saveId; }
            set
            {
                string toApply = value;

                if (toApply.Length > IDAndVersionLengthCap)
                {
                    toApply = toApply[..IDAndVersionLengthCap];
                }
                saveId = toApply;
            }
        }

        public virtual int SlotNumber
        {
            get { return slotNumber; }
            set
            {
                slotNumber = value;

                if (slotNumber <= 0)
                {
                    string errorMessage = $"Cannot assign {value} as a slot number. We need the num to be positive.";
                    throw new ArgumentException(errorMessage);
                }
            }
        }
        public string SaveVersion
        {
            get { return saveVersion; }
            set
            {
                string toApply = value;
                if (toApply != null && toApply.Length > IDAndVersionLengthCap)
                {
                    toApply = toApply[..IDAndVersionLengthCap];
                }
                saveVersion = toApply;
            }
        }

        public string UTCTimeStamp
        {
            get { return utcTimeStamp; }
            protected set
            {
                utcTimeStamp = value;
                UpdateTimeStampStructure();
            }
        }

        public virtual string SceneName
        {
            get { return sceneName; }
            set { sceneName = value; }
        }

        public virtual int SceneBuildIndex
        {
            get { return sceneBuildIndex; }
            set { sceneBuildIndex = value; }
        }

        public virtual TimeSpan Playtime
        {
            get { return playtime; }
            set
            {
                playtime = value;
                playtimeStamp = playtime.ToString();
            }
        }
        protected TimeSpan playtime = TimeSpan.Zero;

        /// <summary>
        /// Markers indicating progress points reached in the game.
        /// Getter returns a copy of the list to prevent external modification.
        /// Setter keeps the same list, only setting the contents to that of the passed one.
        /// </summary>
        public IList<ProgressMarker> ProgressMarkers
        {
            get { return progressMarkers.ToArray(); }
            set
            {
                progressMarkers.Clear();
                progressMarkers.AddRange(value);
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

        public SaveMetaData()
        {
            
        }

        public SaveMetaData(string saveId = "", DateTime timeStamp = default,
            string saveVersion = "", int slotNumber = -1) : this()
        {
            this.SaveId = saveId;
            
            if (timeStamp == default)
            {
                timeStamp = DateTime.UtcNow;
            }
            this.timeStamp = timeStamp;

            this.SaveVersion = saveVersion;
            this.slotNumber = slotNumber;

            UpdateTimeStampString();
        }

        public SaveMetaData(SaveMetaData other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Cannot copy from a null SaveMetaData.");
            }
            this.name = other.name;
            this.saveId = other.saveId;
            this.slotNumber = other.slotNumber;
            this.saveVersion = other.saveVersion;
            this.utcTimeStamp = other.utcTimeStamp;
            this.sceneName = other.sceneName;
            this.sceneBuildIndex = other.sceneBuildIndex;
            this.playtimeStamp = other.playtimeStamp;
            this.playtime = other.playtime;
            this.progressMarkers = new List<ProgressMarker>(other.progressMarkers);
            UpdateTimeStampStructure();
        }

        public static int IDAndVersionLengthCap { get; } = 300;

        public override void OnDeserialize()
        {
            base.OnDeserialize();
            UpdateTimeStampStructure();
            UpdatePlaytimeStructure();
        }

        protected virtual void UpdatePlaytimeStructure()
        {
            if (TimeSpan.TryParse(playtimeStamp, out var parsedTimeSpan))
            {
                playtime = parsedTimeSpan;
            }
            else
            {
                string errorMessage = $"Failed to parse time span string: {playtimeStamp}. " +
                    "Setting playtime to zero.";
                playtime = TimeSpan.Zero;
                throw new FormatException(errorMessage);
            }
        }

        public virtual bool Equals(SaveMetaData other)
        {
            if (other == null)
            {
                return false;
            }

            // Organized like this for ease of debugging.
            bool sameId = saveId == other.saveId;
            bool sameVersion = saveVersion == other.saveVersion;
            bool sameTimeStamp = utcTimeStamp == other.utcTimeStamp;
            bool sameName = SaveName == other.SaveName;
            bool sameSceneName = SceneName == other.SceneName;
            bool sameSceneIndex = sceneBuildIndex == other.sceneBuildIndex;
            bool sameSlotNum = slotNumber == other.slotNumber;
            bool samePlaytime = playtime.Equals(other.playtime);

            bool result = sameId && sameVersion && sameTimeStamp &&
                sameName && sameSceneName && sameSceneIndex &&
                sameSlotNum && samePlaytime;
            return result;
        }

        public static SaveMetaData CreateFrom(ISaveMetaData other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Cannot create SaveMetaData from null ISaveMetaData.");
            }

            SaveMetaData result = new SaveMetaData();
            result.name = other.SaveName;
            result.saveId = other.SaveId;
            result.slotNumber = other.SlotNumber;
            result.saveVersion = other.SaveVersion;
            result.sceneName = other.SceneName;
            result.sceneBuildIndex = other.SceneBuildIndex;
            result.playtime = other.Playtime;
            result.UpdateTimeStampString();
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj is SaveMetaData otherMeta)
            {
                return Equals(otherMeta);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (saveId?.GetHashCode() ?? 0);
                hash = (hash * 23) + (saveVersion?.GetHashCode() ?? 0);
                hash = (hash * 23) + (utcTimeStamp?.GetHashCode() ?? 0);
                hash = (hash * 23) + (name?.GetHashCode() ?? 0);
                hash = (hash * 23) + (sceneName?.GetHashCode() ?? 0);
                hash = (hash * 23) + sceneBuildIndex.GetHashCode();
                hash = (hash * 23) + slotNumber.GetHashCode();
                hash = (hash * 23) + playtime.GetHashCode();
                return hash;
            }
        }
    }

    // For stuff that probably all save meta data should have
    public interface ISaveMetaData : ISaveData
    {
        string SaveId { get; }
        string SaveName { get; set; }
        int SlotNumber { get; }
        string SaveVersion { get; }
        DateTime TimeStamp { get; }
        string SceneName { get; }
        int SceneBuildIndex { get; }
        TimeSpan Playtime { get; }
        bool IsValid { get; }

    }
}