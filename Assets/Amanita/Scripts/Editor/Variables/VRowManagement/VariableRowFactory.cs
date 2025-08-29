using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public class VariableRowFactory : IVariableRowFactory
    {
        protected readonly VariableRowPool _rowPool;
        protected readonly RowVisualHandlerPool _handlerPool;
        protected readonly VisualElement _holder;

        public VariableRowFactory(VariableRowPool rowPool,
                                  RowVisualHandlerPool handlerPool,
                                  VisualElement holder)
        {
            _rowPool = rowPool;
            _handlerPool = handlerPool;
            _holder = holder;
        }

        public VariableRow Create(IVariable variable, VisualElement parent)
        {
            var row = _rowPool.GetOrCreate();
            var handler = _handlerPool.GetHandlerFor(variable.ContentType, _holder, variable);
            row.Init(_holder, variable, handler);
            parent.Add(row.RootElement);
            return row;
        }

        public void Release(VariableRow row)
        {
            if (row.RootElement?.parent != null)
                row.RootElement.RemoveFromHierarchy();
            _handlerPool.Release(row.VisualHandler);
            _rowPool.Release(row);
        }
    }

    public interface IVariableRowFactory
    {
        VariableRow Create(IVariable variable, VisualElement parent);
        void Release(VariableRow row);
    }
}