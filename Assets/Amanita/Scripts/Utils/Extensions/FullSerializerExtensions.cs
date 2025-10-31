using FullSerializer;
using UnityEngine;

namespace Amanita.FSExt
{
    public static class FullSerializerExtensions
    {
        public static string ToJson<T>(this fsSerializer serializer, T data, bool prettyPrint = false)
        {
            serializer.TrySerialize(typeof(T), data, out fsData fsData).AssertSuccessWithoutWarnings();
            string result;
            if (prettyPrint)
            {
                result = fsJsonPrinter.PrettyJson(fsData);
            }
            else
            {
                result = fsJsonPrinter.CompressedJson(fsData);
            }
            return result;
        }

        public static T FromJson<T>(this fsSerializer serializer, string json)
        {
            var data = fsJsonParser.Parse(json);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
            return (T)deserialized;
        }

        /// <summary>
        /// Attempts to overwrite the fields of an existing instance with data from JSON.
        /// Mirrors the behavior of JsonUtility.TryFromJsonOverwrite, but using fsSerializer.
        /// </summary>
        public static bool TryFromJsonOverwrite<T>(
            this fsSerializer serializer,
            string json,
            T instance)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("JSON string is null or empty.");
                return false;
            }

            try
            {
                fsData data;
                var parseResult = fsJsonParser.Parse(json, out data);
                if (parseResult.Failed)
                {
                    Debug.LogWarning($"Failed to parse JSON: {parseResult.FormattedMessages}");
                    return false;
                }

                var result = serializer.TryDeserialize<T>(data, ref instance);
                if (result.Failed)
                {
                    Debug.LogWarning($"Deserialization failed: {result.FormattedMessages}");
                    return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Exception during TryFromJsonOverwrite: {ex}");
                return false;
            }
        }


    }
}