using System.IO;

namespace Amanita.SaveSys
{
    public static class FileUtils 
    {
        public static string GetPathToFolder(SaveDirectoryType type, string relative = "")
        {
            relative = RelativePathFormatted(relative);
            string result = SaveSystem.SaveDirectoryPaths[type];

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
                bool endsWithADash = path.EndsWith("/") || path.EndsWith("\\");
                if (!endsWithADash)
                {
                    result += "/";
                }
            }

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
    }
}