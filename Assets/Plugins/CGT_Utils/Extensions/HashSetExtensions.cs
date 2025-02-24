using System.Collections.Generic;

namespace CGT
{
    public static class HashSetExtensions
    {
        public static void AddRange<T>(this HashSet<T> set, IList<T> whatToAdd)
        {
            for (int i = 0; i < whatToAdd.Count; i++)
            {
                T currentItem = whatToAdd[i];
                set.Add(currentItem);
            }
        }
    }
}