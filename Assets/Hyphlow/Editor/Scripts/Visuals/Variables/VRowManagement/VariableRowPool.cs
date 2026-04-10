using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class VariableRowPool
    {
        public VariableRow GetOrCreate()
        {
            VariableRow result;

            if (_rowsPooled.Count > 0)
            {
                result = _rowsPooled.Pop();
                //Debug.Log($"Reusing a row.");//
            }
            else
            {
                result = new VariableRow();
            }

            return result;
        }

        protected readonly Stack<VariableRow> _rowsPooled = new Stack<VariableRow>();

        public virtual void ReleaseRange(IEnumerable<VariableRow> rows)
        {
            foreach (var row in rows)
            {
                Release(row);
            }
        }

        /// <summary>
        /// Disposes and returns the row to this pool
        /// </summary>
        public void Release(VariableRow row)
        {
            if (!_rowsPooled.Contains(row))
            {
                row.Dispose();
                _rowsPooled.Push(row);
            }
        }

        public int Count => _rowsPooled.Count;

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
            foreach (var rowToClear in _rowsPooled)
            {
                rowToClear.Dispose(); // Disposal implies clearing
            }

            _rowsPooled.Clear();
        }

    }
}