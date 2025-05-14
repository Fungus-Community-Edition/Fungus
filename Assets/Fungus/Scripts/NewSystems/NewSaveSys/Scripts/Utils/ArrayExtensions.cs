using System;

namespace CGTUnity.Fungus.SaveSystem
{
    public static class ArrayExtensions
    {
        public static bool Contains<T>(this T[] arr, T element) where T: IEquatable<T>
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Equals(element))
                    return true;
            }

            return false;
        }

    }
}