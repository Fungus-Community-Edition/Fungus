using System.Collections.Generic;
using System.Linq;

namespace Amanita.SaveSys
{
    public class SaveRegistry
    {
        private readonly HashSet<string> activeSaves = new();

        public void AddSave(string saveName)
        {
            activeSaves.Add(saveName);
        }

        public void RemoveSave(string saveName)
        {
            activeSaves.Remove(saveName);
        }

        public bool SaveExists(string saveName)
        {
            return activeSaves.Contains(saveName);
        }

        public List<string> GetAllSaves()
        {
            return activeSaves.ToList();
        }
    }

}