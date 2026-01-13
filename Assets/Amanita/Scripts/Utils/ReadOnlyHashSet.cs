using System.Collections;
using System.Collections.Generic;

namespace Amanita.Collections.Generic
{
    public sealed class ReadOnlyHashSet<T> : IReadOnlyHashSet<T>
    {
        // No need for double bookkeeping, since this is a reference, not a copy
        private readonly HashSet<T> _set;

        public ReadOnlyHashSet(HashSet<T> set)
        {
            _set = set ?? throw new System.ArgumentNullException(nameof(set));
        }

        public int Count => _set.Count;

        public bool Contains(T item) => _set.Contains(item);

        public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();
    }

    public interface IReadOnlyHashSet<T> : IEnumerable<T>
    {
        int Count { get; }
        bool Contains(T item);
    }
}