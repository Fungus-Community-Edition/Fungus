using FullSerializer;
using AtMycelia.FSExt;

namespace AtMycelia.IO
{
    public static class JsonHelpers 
    {
        public static bool TryFromJsonOverwrite<T>(string jsonString, ref T toOverwrite, fsSerializer serializerToUse)
        {
            try
            {
                // FullSerializer is not thread-safe; serialize access to the shared serializer.
                lock (serializerToUse)
                {
                    bool result = serializerToUse.TryFromJsonOverwrite(jsonString, toOverwrite);
                    return result;
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }

    }
}