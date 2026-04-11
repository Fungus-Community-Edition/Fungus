using System;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class VariableRow : IDisposable
    {
        public VariableRow() { }

        /// <summary>
        /// Also meant to be used for reuse after disposing. It's fine for the 
        /// IRowVisualHandler passed to be in a disposed state.
        /// </summary>
        public virtual void Init(IVariable toRepresent, IRowVisualHandler visHandler)
        {
            _isDisposed = false;
            ToggleSubs(false); 
            // ^In case we're reusing. Best unsub from the prev vis handler we were working with, so...

            _currentVariable = toRepresent;

            PrepVisualHandler();
            void PrepVisualHandler()
            {
                VisualHandler = visHandler;
                VisualHandler.Init(toRepresent);
                VisualHandler.Variable = _currentVariable;
                VisualHandler.Refresh();
            }

            ToggleSubs(true);
        }

        protected bool _isDisposed;
        protected IVariable _currentVariable;

        protected virtual void ToggleSubs(bool on)
        { 
            if (VisualHandler == null)
            {
                return;
            }

            if (on)
            {
                VisualHandler.RemoveButtonClicked += OnRemoveButtonClicked;

                VisualHandler.KeyFieldChanged += OnKeyFieldChanged;
                VisualHandler.ScopeFieldChanged += OnScopeFieldChanged;
                VisualHandler.ValueFieldChanged += OnValueFieldChanged;
            }
            else
            {
                VisualHandler.RemoveButtonClicked -= OnRemoveButtonClicked;
                
                VisualHandler.KeyFieldChanged -= OnKeyFieldChanged;
                VisualHandler.ScopeFieldChanged -= OnScopeFieldChanged;
                VisualHandler.ValueFieldChanged -= OnValueFieldChanged;
            }
        }

        protected virtual void OnRemoveButtonClicked(IRowVisualHandler handler)
        {
            HyphlowEditorSignals.VarRowRemoveButtonClicked(this);
        }

        protected virtual void OnKeyFieldChanged(TextField field)
        {
            HyphlowEditorSignals.KeyFieldChanged(this, field.value);
        }

        protected virtual void OnScopeFieldChanged(VariableScope scope)
        {
            HyphlowEditorSignals.ScopeFieldChanged(this, scope);
        }

        protected virtual void OnValueFieldChanged(object obj)
        {
            HyphlowEditorSignals.ValueFieldChanged(this, obj);
        }

        /// <summary>
        /// Test helper to bypass UI event timing and apply a value change directly.
        /// </summary>
        public void ApplyValueForTests(object value)
        {
            OnValueFieldChanged(value);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            ToggleSubs(false);
            Clear();
            RootElement?.RemoveFromHierarchy();
            _currentVariable = null;
            _isDisposed = true;

            VisualHandler?.Dispose();
            VisualHandler = null;
        }

        public IRowVisualHandler VisualHandler { get; protected set; }

        public virtual IVariable VarToRepresent
        {
            get { return _currentVariable; }
            set
            {
                if (_currentVariable == value)
                {
                    return;
                }

                _currentVariable = value;

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

                return VisualHandler.RowRoot;
            }
        }
        
        /// <summary>
        /// Makes this row stop representing (and by extension, displaying) any IVariables.
        /// This does not necessarily imply that this row should be returned to the pool.
        /// </summary>
        public virtual void Clear()
        {
            _currentVariable = VisualHandler.Variable = null;
        }

    }

}