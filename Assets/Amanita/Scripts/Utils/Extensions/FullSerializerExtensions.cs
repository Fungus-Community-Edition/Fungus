using FullSerializer;
using UnityEngine;

namespace Amanita
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

    }
}