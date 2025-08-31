using System.Collections.Generic;

namespace Amanita
{
    public static class IDictionaryExtensions
    {
        public static bool ContainsKeyEqualTo<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey toCheckFor)
        {
            bool result = false;

            foreach (var keyEl in dict.Keys)
            {
                if (keyEl.Equals(toCheckFor))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public static bool TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            TKey key, out TValue output, bool checkByEquals)
        {
            bool result = false;
            output = default;

            if (!checkByEquals)
            {
                result = dict.TryGetValue(key, out output);
            }
            else
            {
                bool canGetValue = dict.TryGetKeyEqualTo(key, out var keyNeeded);
                if (canGetValue)
                {
                    output = dict[keyNeeded];
                    result = true;
                }
            }

            return result;
        }

        public static bool TryGetKeyEqualTo<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            TKey keyToCheckAgainst, out TKey output)
        {
            output = default;
            bool result = false;
            foreach (var keyEl in dict.Keys)
            {
                if (keyEl.Equals(keyToCheckAgainst))
                {
                    result = true;
                    output = keyEl;
                    break;
                }
            }

            return result;
        }
    }
}