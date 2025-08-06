using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    // 
    /// </summary>
    public class VariableRowPool
    {
        public VariableRow GetOrCreate()
        {
            VariableRow result;

            if (_rows.Count > 0)
            {
                result = _rows.Pop();
            }
            else
            {
                result = new VariableRow();
            }

            return result;
        }

        protected readonly Stack<VariableRow> _rows = new Stack<VariableRow>();

        /// <summary>
        /// Disposes and returns the row to this pool
        /// </summary>
        public void Release(VariableRow row)
        {
            if (!_rows.Contains(row))
            {
                row.Dispose();
                _rows.Push(row);
            }
        }

        public int Count => _rows.Count;

        public virtual void ReleaseRange(IEnumerable<VariableRow> rows)
        {
            foreach (var row in rows)
            {
                Release(row);
            }
        }

        public virtual void ReleaseRange(IList<VariableRow> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                VariableRow row = rows[i];
                Release(row);
            }
        }

        public virtual void Clear()
        {
            foreach (var row in _rows)
            {
                row.Clear();
                row.Dispose();
            }

            _rows.Clear();
        }
    }
}