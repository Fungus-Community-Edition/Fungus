using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lorekeeper.EditorCode
{
    public class LorekeeperSettingsFactory
    {
        public LorekeeperSettings GetSettings()
        {
            LorekeeperSettings result = ReadSettingsFromDisk();
            if (result == null)
            {
                Debug.Log($"Creating a new LorekeeperSettings file at {pathToSettings}");
                result = new LorekeeperSettings();
                WriteSettingsToDisk(result);
                AssetDatabase.Refresh();
            }
            return result;
        }

        protected static LorekeeperSettings ReadSettingsFromDisk()
        {
            if (!File.Exists(pathToSettings))
            {
                string logMessage = $"No Lorekeeper settings file found at {pathToSettings}.";
                Debug.Log(logMessage);
                return null;
            }
            else
            {
                string jsonFromFile = File.ReadAllText(pathToSettings);
                LorekeeperSettings settings = JsonUtility.FromJson<LorekeeperSettings>(jsonFromFile);
                settings.OnDeserialize();
                return settings;
            }
        }

        /// <summary>
        /// Absolute path to the settings file on disk.
        /// </summary>
        protected static readonly string pathToSettings = Path.Join(Application.dataPath, "/Resources/LorekeeperSettings.json");

        protected static void WriteSettingsToDisk(LorekeeperSettings settings)
        {
            string jsonToWrite = JsonUtility.ToJson(settings, true);
            File.WriteAllText(pathToSettings, jsonToWrite);
        }
    }
}