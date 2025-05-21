using System;
using System.Collections.Generic;
using UnityRandom = UnityEngine.Random;

namespace Amanita.Collections
{
    public static class IListExtensions
    {
        public static void AddRange<T>(this IList<T> toAddTo, IList<T> whatToAdd)
        {
            for (int i = 0; i < whatToAdd.Count; i++)
            {
                toAddTo.Add(whatToAdd[i]);
            }
        }

        public static void RemoveAllIn<T>(this IList<T> toRemoveFrom, IList<T> whatToRemove)
        {
            foreach (T item in whatToRemove)
            {
                toRemoveFrom.Remove(item);
            }
        }

        public static IList<T> ReversedCopy<T>(this IList<T> baseList)
        {
            IList<T> result = new List<T>();

            for (int i = baseList.Count - 1; i >= 0; i--)
            {
                result.Add(baseList[i]);
            }

            return result;
        }

        public static T GetRandom<T>(this IList<T> baseList)
        {
            int index = UnityRandom.Range(0, baseList.Count);
            T result = baseList[index];
            return result;
        }

        public static bool Contains<T>(this IList<T> arr, T element) where T : IEquatable<T>
        {
            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i].Equals(element))
                    return true;
            }

            return false;
        }
    }
}