namespace Amanita.SaveSys
{
    public static class FileUtils 
    {
        public static string GetPathToFolder(SaveDirectoryType type, SaveDiskAccessor accessor)
        {
            string result = accessor.GetSaveFolderPath(type);
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



    }
}