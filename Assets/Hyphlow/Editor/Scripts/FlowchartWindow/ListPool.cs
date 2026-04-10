using System;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Lightweight list pool to avoid allocations when diffing block collections.
    /// </summary>
    internal static class ListPool<T>
    {
        private static readonly Stack<List<T>> pool = new();

        public static List<T> Get()
        {
            return pool.Count > 0 ? 
                pool.Pop() : 
                new List<T>();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }

        public struct DisposableList : IDisposable
        {
            private List<T> list;

            public DisposableList(List<T> list)
            {
                this.list = list;
            }

            public static implicit operator List<T>(DisposableList disposable) => disposable.list;

            public void Dispose()
            {
                if (list != null)
                {
                    Release(list);
                    list = null;
                }
            }
        }

        public static DisposableList Get(out List<T> list)
        {
            list = Get();
            return new DisposableList(list);
        }
    }

}