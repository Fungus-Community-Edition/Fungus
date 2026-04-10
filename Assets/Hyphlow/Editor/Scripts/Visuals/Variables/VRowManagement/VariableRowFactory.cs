using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class VariableRowFactory : IVariableRowFactory<VariableRowFactoryInitArgs>
    {
        public virtual void Init(object input)
        {
            _isDisposed = false;
            if (input is VariableRowFactoryInitArgs)
            {
                Init(input as VariableRowFactoryInitArgs);
            }
        }

        protected bool _isDisposed;

        public virtual void Init(VariableRowFactoryInitArgs initArgs)
        {
            _isDisposed = false;
            _rowPool = initArgs.RowPool;
            _handlerPool = initArgs.HandlerPool;
        }

        protected VariableRowPool _rowPool;
        protected RowVisualHandlerPool _handlerPool;

        public VariableRow Create(IVariable toRepresent)
        {
            VariableRow row = _rowPool.GetOrCreate();
            IRowVisualHandler handler = _handlerPool.GetHandlerFor(toRepresent.ContentType, toRepresent);
            row.Init(toRepresent, handler);
            return row;
        }

        public void Release(VariableRow row)
        {
            if (row == null) return;

            if (row.VisualHandler != null)
            {
                _handlerPool.Release(row.VisualHandler);
            }

            _rowPool.Release(row);
        }

        public virtual void ReleaseMulti(IEnumerable<VariableRow> toRelease)
        {
            foreach (var elem in toRelease)
            {
                Release(elem);
            }
        }

        // --- Exposed for manager + tests (DIP-friendly pass-throughs) ---
        public int PooledRowCount => _rowPool.Count;
        public int PooledHandlerCount => _handlerPool.PooledHandlerCount;
        public RowVisualHandlerPool HandlerPool => _handlerPool;
        public VariableRowPool RowPool => _rowPool;

        public virtual void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
        }
    }

    public interface IVariableRowFactory : IDisposable
    {
        void Init(object input);
        VariableRow Create(IVariable variable);
        void Release(VariableRow row);
    }

    public interface IVariableRowFactory<T> : IVariableRowFactory
    {
        void Init(T input);
    }

    public class VariableRowFactoryInitArgs
    {
        public VariableRowPool RowPool { get; set; }
        public RowVisualHandlerPool HandlerPool { get; set; }
        public VisualElement Holder { get; set; }
    }
}