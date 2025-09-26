using System.IO;

namespace Amanita.SaveSys
{
    public static class FileUtils 
    {
        public static string GetPathToFolder(SaveDirectoryType type, SaveDiskAccessor accessor)
        {
            string result = GetPathToFolder(type, accessor.RelativeSavePath);
            return result;
        }

        public static string GetPathToFolder(SaveDirectoryType type, string relative = "")
        {
            relative = RelativePathFormatted(relative);
            string result = SaveSystem.S.SaveDirectoryPaths[type];

            bool thereIsRelativePathToConsider = relative.Length > 1;

            if (thereIsRelativePathToConsider)
            {
                result = Path.Combine(result, relative);
            }

            if (!result.EndsWith("/") && !result.EndsWith("\\"))
            {
                result += "\\";
            }

            return result;
        }

        private static string RelativePathFormatted(string path)
        {
            string result = path;

            if (string.IsNullOrEmpty(path))
            {
                result = string.Empty;
            }
            else
            {
                // Remove leading and trailing slashes
                result = path.Trim('/', '\\');
            }

            return result;
        }

        public static string GetPathToFile(SaveDirectoryType saveDirectoryType,
            int slotNumber,
            SaveDiskAccessor accessor)
        {
            string fileNameWithExtension = GetFileName(slotNumber, accessor);

            string result = GetPathToFile(saveDirectoryType,
                fileNameWithExtension,
                accessor.RelativeSavePath);
            return result;
        }

        public static string GetFileName(int slotNumber,
            SaveDiskAccessor accessor,
            bool includeExtension = true)
        {
            string extension = string.Empty;
            if (includeExtension)
            {
                extension = accessor.FileExtension;
            }

            string fileNumFormatted = slotNumber.ToString(accessor.SaveNumberFormat);
            string result = string.Format(accessor.FileNameFormat,
                accessor.SavePrefix,
                fileNumFormatted,
                extension);

            return result;
        }

        public static string GetPathToFile(SaveDirectoryType saveDirectoryType,
            string fileNameWithExtension,
            string relativePath = "")
        {
            string folderPath = GetPathToFolder(saveDirectoryType, relativePath);
            string result = folderPath + fileNameWithExtension;
            return result;
        }

        public static string GetPathToBackupFile(SaveDirectoryType dirType, int slot, SaveWriter writer)
        {
            string basePath = GetPathToFile(dirType, slot, writer);
            return basePath + writer.BackupFileExtension;
        }

    }
}