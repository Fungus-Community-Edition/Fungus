using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveSerializer
    {
        public string Serialize(SaveData saveData)
        {
            return JsonUtility.ToJson(saveData, true);
        }

        public SaveData Deserialize(string json)
        {
            return JsonUtility.FromJson<SaveData>(json);
        }
    }
}