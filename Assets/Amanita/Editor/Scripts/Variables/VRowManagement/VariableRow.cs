using Amanita.EditorUtils;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    public class VariableRow : IDisposable
    {
        public VariableRow() { }

        /// <summary>
        /// Also meant to be used for reuse after disposing. It's fine for the 
        /// IRowVisualHandler passed to be in a disposed state.
        /// </summary>
        public virtual void Init(IVariable toRepresent, IRowVisualHandler visHandler,
            UnityObj targetObject)
        {
            _isDisposed = false;
            ToggleSubs(false);

            _prevVariable = _currentVariable;
            _currentVariable = toRepresent;

            _serializedVar?.Dispose();
            _serializedVar = targetObject != null ? new SerializedObject(targetObject) : null;
            _serializedVar?.Update();

            VisualHandler = visHandler;
            VisualHandler.Init(toRepresent);
            VisualHandler.Variable = _currentVariable;
            VisualHandler.SerializedVar = _serializedVar;
            VisualHandler.Refresh();

            ToggleSubs(true);
        }

        protected bool _isDisposed;
        protected IVariable _prevVariable;
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
                VisualHandler.FocusLostOnControl += OnFocusLostOnControl;
                VisualHandler.KeyFieldFocusLost += OnKeyFieldFocusLost;
                VisualHandler.ValueFieldChanged += OnValueFieldChanged;
            }
            else
            {
                VisualHandler.RemoveButtonClicked -= OnRemoveButtonClicked;
                VisualHandler.FocusLostOnControl -= OnFocusLostOnControl;
                VisualHandler.KeyFieldFocusLost -= OnKeyFieldFocusLost;
                VisualHandler.ValueFieldChanged -= OnValueFieldChanged;
            }
        }

        private void OnValueFieldChanged(object obj)
        {
            AmanitaEditorSignals.ValueFieldChanged(this, obj);
        }

        public virtual string KeyDisplayed
        {
            get
            {
                if (VisualHandler == null)
                {
                    return string.Empty;
                }
                return VisualHandler.KeyDisplayed;
            }
        }

        private void OnKeyFieldFocusLost(TextField field)
        {
            AmanitaEditorSignals.KeyFieldFocusLost(this, field.value);
        }

        protected virtual void OnRemoveButtonClicked(IRowVisualHandler handler)
        {
            AmanitaEditorSignals.VarRowRemoveButtonClicked(this);
        }

        protected virtual void OnFocusLostOnControl(FocusOutEvent evt)
        {
            if (_serializedVar != null)
            {
                _serializedVar.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("Applied changes on focus loss");

            }
            FocusLostOnControl(this);
        }

        public event Action<VariableRow> FocusLostOnControl = delegate { }; 

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            ToggleSubs(false);
            Clear();
            RootElement?.RemoveFromHierarchy();
            _serializedVar?.Dispose();
            _serializedVar = null;
            _currentVariable = null;
            _isDisposed = true;

            VisualHandler?.Dispose();
            VisualHandler = null;
        }

        protected virtual void UpdateSerializedVar()
        {
            if ((_prevVariable == _currentVariable) && _serializedVar != null)
                return;

            _serializedVar?.Dispose();

            if (_currentVariable != null)
            {
                // Guard against destroyed UnityEngine.Object
                if (_currentVariable is UnityObj unityObj)
                {
                    if (unityObj == null) // Unity's overloaded null check
                    {
                        _serializedVar = null;
                        return;
                    }

                    _serializedVar = new SerializedObject(unityObj);
                }

                _serializedVar?.Update();
            }
            else
            {
                _serializedVar = null;
            }
        }

        public SerializedObject SerializedVar => _serializedVar;
        protected SerializedObject _serializedVar;

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

                UpdateSerializedVar();
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