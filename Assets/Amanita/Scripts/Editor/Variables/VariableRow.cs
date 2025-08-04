using System;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public class VariableRow : IDisposable
    {
        public VariableRow() { }

        /// <summary>
        /// Also meant to be used for reuse after disposing. It's fine for the 
        /// IRowVisualHandler passed to be in a disposed state.
        /// </summary>
        public virtual void Init(VisualElement holder, IVariable toRepresent,
            IRowVisualHandler nonInitializedHandler)
        {
            _holder = holder;
            _varToRepresent = toRepresent;
            _visualHandler = nonInitializedHandler;
            _visualHandler.Init(holder, toRepresent);
        }

        protected VisualElement _holder;
        protected IVariable _varToRepresent;
        protected IRowVisualHandler _visualHandler;

        protected Flowchart _flowchart;

        public virtual IVariable VarToRepresent
        {
            get { return _varToRepresent; }
            set
            {
                if (_varToRepresent == value)
                {
                    return;
                }

                _varToRepresent = value;

                if (_visualHandler != null)
                {
                    _visualHandler.Variable = value;
                }
            }
        }

        public VisualElement RootElement
        {
            get
            {
                if (_visualHandler == null)
                {
                    return null;
                }

                return _visualHandler.Root;
            }
        }
        
        /// <summary>
        /// Makes this row stop representing (and by extension, displaying) any IVariables.
        /// </summary>
        public virtual void Clear()
        {
            _varToRepresent = _visualHandler.Variable = null;
        }

        public virtual void Refresh()
        {
            _visualHandler?.Refresh();
        }

        public void Dispose()
        {
            this.VarToRepresent = null;
            _flowchart = null;
            _varToRepresent = null;
            _holder = null;
            _visualHandler.Dispose();
        }

    }

    


}