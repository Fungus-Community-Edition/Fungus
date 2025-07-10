using UnityEngine;

namespace Amanita.IO
{
    public static class JsonHelpers 
    {
        public static bool TryFromJsonOverwrite<T>(string jsonString, ref T toOverwrite)
        {
            try
            {
                JsonUtility.FromJsonOverwrite(jsonString, toOverwrite);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}