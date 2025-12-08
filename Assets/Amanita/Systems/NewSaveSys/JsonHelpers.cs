using FullSerializer;
using Amanita.FSExt;

namespace Amanita.IO
{
    public static class JsonHelpers 
    {
        public static bool TryFromJsonOverwrite<T>(string jsonString, ref T toOverwrite)
        {
            try
            {
                // FullSerializer is not thread-safe; serialize access to the shared serializer.
                lock (AmanitaManager.DefaultSerializer)
                {
                    bool result = Serializer.TryFromJsonOverwrite(jsonString, toOverwrite);
                    return result;
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static fsSerializer Serializer => AmanitaManager.DefaultSerializer;
    }
}