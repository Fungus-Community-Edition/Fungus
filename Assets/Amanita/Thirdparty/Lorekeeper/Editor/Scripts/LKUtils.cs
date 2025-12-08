using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lorekeeper.EditorCode
{
    public static class LKUtils
    {
        public static void EnsureWeHaveResourcesFolder()
        {
            if (!Directory.Exists(pathToResources))
            {
                Directory.CreateDirectory(pathToResources);
            }
        }

        public static void WriteSettingsToDisk(LorekeeperSettings settings)
        {
            string jsonToWrite = JsonUtility.ToJson(settings, true);
            File.WriteAllText(pathToSettings, jsonToWrite);
            Debug.Log($"Lorekeeper settings saved to {pathToSettings}");
            AssetDatabase.Refresh();
        }

        public static readonly string pathToResources = Path.Join(Application.dataPath, "Resources");

        /// <summary>
        /// Absolute path to the settings file on disk.
        /// </summary>
        public static readonly string pathToSettings = Path.Join(Application.dataPath, "/Resources/LorekeeperSettings.json");

        public static string EnsureForwardSlashAtStart(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "/";
            }
            if (input[0] != '/')
            {
                input = "/" + input;
            }
            return input;
        }

        public static string LibraryPath
        {
            get
            {
                // Application.dataPath gives "<projectRoot>/Assets"
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                return Path.Combine(projectRoot, "Library");
            }
        }

        public static string PackagesPath
        {   
            get
            {
                // Application.dataPath gives "<projectRoot>/Assets"
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                return Path.Combine(projectRoot, "Packages");
            }
        }

        public static ShadowDatabase GetShadowDatabase()
        {
            EnsureWeHaveResourcesFolder();

            ShadowDatabase database;
            var databaseGuids = AssetDatabase.FindAssets("t:Lorekeeper.ShadowDatabase");
            bool databaseFound = databaseGuids.Length > 0;
            if (databaseFound)
            {
                var path = AssetDatabase.GUIDToAssetPath(databaseGuids[0]);
                database = AssetDatabase.LoadAssetAtPath<ShadowDatabase>(path);
            }
            else
            {
                // Need to create one.
                database = ScriptableObject.CreateInstance<ShadowDatabase>();
                AssetDatabase.CreateAsset(database, "Assets/Resources/ShadowDatabase.asset");
                AssetDatabase.Refresh();
            }
            return database;
        }

    }
}