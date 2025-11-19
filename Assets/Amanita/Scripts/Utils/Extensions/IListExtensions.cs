using System;
using System.Collections.Generic;
using UnityRandom = UnityEngine.Random;

namespace Collections
{
    public static class IListExtensions
    {
        /// <summary>
        /// Returns true if both lists contain the same elements
        /// (but not necessarily in the same order).
        /// </summary>
        public static bool SameContentsAs<T>(this IList<T> thisList, IList<T> otherList)
        {
            if (thisList.Count != otherList.Count) return false;

            HashSet<T> thisSet = new HashSet<T>(thisList);
            HashSet<T> otherSet = new HashSet<T>(otherList);

            return thisSet.SetEquals(otherSet);
        }

        public static bool ContainsReference<T>(this IList<T> list, object item) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], item))
                {
                    return true;
                }
            }

            return false;
        }

        public static int IndexOfReference<T>(this IList<T> list, object item) where T : class
        {
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], item))
                {
                    index = i;
                }
            }

            return index;
        }

        // Adds the item if the list isn't at capacity
        public static void Add<T>(this IList<T> list, T item, int capacity)
        {
            if (list.Count < capacity) list.Add(item);
        }

        public static void AddRange<T>(this IList<T> toAddTo, IList<T> whatToAdd)
        {
            for (int i = 0; i < whatToAdd.Count; i++)
            {
                toAddTo.Add(whatToAdd[i]);
            }
        }

        /// <summary>
        /// AddRange but won't add stuff when toAddTo is at or above the specified capacity.
        /// </summary>
        public static void AddRange<T>(this IList<T> toAddTo, IList<T> whatToAdd, int capacity)
        {
            bool isAtCapacity = false;
            for (int i = 0; i < whatToAdd.Count; i++)
            {
                isAtCapacity = toAddTo.Count >= capacity;
                if (isAtCapacity)
                {
                    return;
                }

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

        public static bool AnyOverlapWith<T>(this IList<T> list, IList<T> otherList)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (otherList.Contains(list[i]))
                    return true;
            }

            return false;
        }
    }
}