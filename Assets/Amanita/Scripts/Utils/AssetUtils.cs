using UnityEditor;
using System.IO;

namespace Amanita
{
    public static class AssetUtils
    {
        /// <summary>
        /// Ensures that the given folder path exists in the AssetDatabase.
        /// Creates any missing subfolders step by step.
        /// Example: EnsureFolderExists("Assets/Resources/SaveSys/SaveAppliers");
        /// </summary>
        public static void EnsureFolderExists(string fullPath)
        {
            // Normalize separators
            fullPath = fullPath.Replace("\\", "/");

            string[] parts = fullPath.Split('/');
            if (parts.Length == 0)
            {
                return;
            }

            // Walk down the hierarchy
            string current = parts[0]; // usually "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = Path.Combine(current, parts[i]).Replace("\\", "/");
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}