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
            VisualHandler = nonInitializedHandler;
            VisualHandler.Init(holder, toRepresent);
            VisualHandler.Refresh();
        }

        protected VisualElement _holder;
        protected IVariable _varToRepresent;
        public IRowVisualHandler VisualHandler { get; protected set; }

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

                if (VisualHandler != null)
                {
                    VisualHandler.Variable = value;
                }
            }
        }

        public VisualElement RootElement
        {
            get
            {
                if (VisualHandler == null)
                {
                    return null;
                }

                return VisualHandler.Root;
            }
        }
        
        /// <summary>
        /// Makes this row stop representing (and by extension, displaying) any IVariables.
        /// </summary>
        public virtual void Clear()
        {
            _varToRepresent = VisualHandler.Variable = null;
        }

        public virtual void Refresh()
        {
            VisualHandler?.Refresh();
        }

        public void Dispose()
        {
            this.VarToRepresent = null;
            _flowchart = null;
            _varToRepresent = null;
            _holder = null;
            VisualHandler.Dispose();
        }

    }

}