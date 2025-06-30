using UnityEngine;
using System.IO;

namespace Amanita.IO
{
    public static class IOUtils
    {
        public static bool CanWriteToFile(string filePath)
        {
            bool result = false;
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError($"File path is null or empty.");
            }
            else
            {
                try
                {
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    stream.Close(); // If this succeeds, the file's not locked
                    result = true;
                }
                catch (IOException ex)
                {
                    Debug.LogError($"Cannot open file at {filePath}. Exception: {ex.Message}");
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes the file and (if it exists) the corresponding meta file.
        /// </summary>
        public static void UnityFileDelete(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("File path is null or empty.");
                return;
            }
            try
            {
                string metaFilePath = $"{filePath}.meta";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                else
                {
                    Debug.LogWarning($"File at {filePath} does not exist, cannot delete.");
                }

                if (File.Exists(metaFilePath))
                {
                    File.Delete(metaFilePath);
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to delete file at {filePath}. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Moves the file and (if it exists) the corresponding meta file.
        /// </summary>
        public static void UnityFileMove(string sourcePath, string destPath)
        {
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destPath))
            {
                Debug.LogError("Source or destination file path is null or empty.");
                return;
            }
            try
            {
                string sourceMetaPath = $"{sourcePath}.meta";
                string destMetaPath = $"{destPath}.meta";
                if (File.Exists(sourcePath))
                {
                    File.Move(sourcePath, destPath);
                }
                else
                {
                    Debug.LogWarning($"Source file at {sourcePath} does not exist, cannot move.");
                }

                if (File.Exists(sourceMetaPath))
                {
                    File.Move(sourceMetaPath, destMetaPath);
                }
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }
    }
}