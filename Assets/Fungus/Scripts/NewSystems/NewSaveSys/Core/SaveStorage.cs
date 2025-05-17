using UnityEngine;
using System.IO;

namespace Amanita.SaveSys
{
    public class SaveStorage
    {
        private string SavePath(string saveName) => $"{Application.persistentDataPath}/{saveName}.json";

        public void WriteSaveFile(string saveName, string json)
        {
            File.WriteAllText(SavePath(saveName), json);
        }

        public string ReadSaveFile(string saveName)
        {
            var path = SavePath(saveName);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public void DeleteSaveFile(string saveName)
        {
            var path = SavePath(saveName);
            if (File.Exists(path)) File.Delete(path);
        }
    }

}