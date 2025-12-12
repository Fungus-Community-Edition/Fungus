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
        [SerializeField] protected string saveID = string.Empty;
        [SerializeField] protected int slotNumber = 1;
        [SerializeField] protected string saveVersion = "null";
        [SerializeField] protected string utcTimeStamp = string.Empty;
        [SerializeField] protected string sceneName = string.Empty;
        [SerializeField] protected int sceneBuildIndex = -1;
        [SerializeField] protected string timeSpanString = TimeSpan.Zero.ToString();
        [SerializeField] protected IList<ProgressMarker> progressMarkers = new List<ProgressMarker>();

        public bool IsValid
        {
            get
            {
                bool validID = !string.IsNullOrEmpty(saveID);
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

        public string SaveID
        {
            get { return saveID; }
            set
            {
                string toApply = value;

                if (toApply.Length > IDAndVersionLengthCap)
                {
                    toApply = toApply[..IDAndVersionLengthCap];
                }
                saveID = toApply;
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
                if (toApply.Length > IDAndVersionLengthCap)
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
        }

        public virtual int SceneBuildIndex
        {
            get { return sceneBuildIndex; }
        }

        public virtual TimeSpan Playtime
        {
            get { return playtime; }
            set
            {
                playtime = value;
                timeSpanString = playtime.ToString();
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
            this.saveID = string.Empty;
            this.timeStamp = DateTime.UtcNow;
            this.saveVersion = NullSaveVer;
            UpdateTimeStampString();
        }

        protected virtual string NullSaveVer { get { return SaveSysConstants.NullSaveVer; } }

        public SaveMetaData(string saveId = null, DateTime timeStamp = default,
            string saveVersion = "")
        {
            if (string.IsNullOrEmpty(saveId))
            {
                saveId = string.Empty;
            }
            this.SaveID = saveId;

            this.timeStamp = timeStamp;

            if (string.IsNullOrEmpty(saveVersion))
            {
                saveVersion = NullSaveVer;
            }

            this.saveVersion = saveVersion;
            UpdateTimeStampString();
        }

        public SaveMetaData(SaveMetaData other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Cannot copy from a null SaveMetaData.");
            }
            this.name = other.name;
            this.saveID = other.saveID;
            this.slotNumber = other.slotNumber;
            this.saveVersion = other.saveVersion;
            this.utcTimeStamp = other.utcTimeStamp;
            this.sceneName = other.sceneName;
            this.sceneBuildIndex = other.sceneBuildIndex;
            this.timeSpanString = other.timeSpanString;
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
            if (TimeSpan.TryParse(timeSpanString, out var parsedTimeSpan))
            {
                playtime = parsedTimeSpan;
            }
            else
            {
                string errorMessage = $"Failed to parse time span string: {timeSpanString}. " +
                    "Setting playtime to zero.";
                playtime = TimeSpan.Zero;
                throw new FormatException(errorMessage);
            }
        }

        public virtual void RegisterCurrentSceneInfo()
        {
            void GetTheInfo()
            {
                sceneName = SceneManager.GetActiveScene().name;
                sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
            }

            bool onMainThread = UnityThreadUtil.IsMainThread;
            if (onMainThread)
            {
                GetTheInfo();
            }
            else
            {
                using (var countdown = new CountdownEvent(1))
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        GetTheInfo();
                        countdown.Signal();
                    }
                    );

                    countdown.Wait();
                }
            }
            
        }

        public virtual bool Equals(SaveMetaData other)
        {
            if (other == null) return false;

            return saveID == other.saveID &&
                saveVersion == other.saveVersion &&
                utcTimeStamp == other.utcTimeStamp &&
                SaveName == other.SaveName &&
                SceneName == other.SceneName &&
                sceneBuildIndex == other.sceneBuildIndex &&
                slotNumber == other.slotNumber &&
                playtime.Equals(other.playtime);
        }

        public static SaveMetaData CreateFrom(ISaveMetaData other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Cannot create SaveMetaData from null ISaveMetaData.");
            }

            SaveMetaData result = new SaveMetaData();
            result.name = other.SaveName;
            result.saveID = other.SaveID;
            result.slotNumber = other.SlotNumber;
            result.saveVersion = other.SaveVersion;
            result.sceneName = other.SceneName;
            result.sceneBuildIndex = other.SceneBuildIndex;
            result.playtime = other.Playtime;
            result.UpdateTimeStampString();
            return result;
        }

    }

    // For stuff that probably all save meta data should have
    public interface ISaveMetaData : ISaveData
    {
        string SaveID { get; }
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