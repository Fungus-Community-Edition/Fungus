using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    public class VariableRow : IDisposable
    {
        public VariableRow() { }

        /// <summary>
        /// Also meant to be used for reuse after disposing. It's fine for the 
        /// IRowVisualHandler passed to be in a disposed state.
        /// </summary>
        public virtual void Init(IVariable toRepresent,
            IRowVisualHandler visHandler)
        {
            _isDisposed = false;
            ToggleSubs(false); // Just in case
            _prevVariable = _currentVariable;
            _currentVariable = toRepresent;

            UpdateSerializedVar();

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
            }
            else
            {
                VisualHandler.RemoveButtonClicked -= OnRemoveButtonClicked;
                VisualHandler.FocusLostOnControl -= OnFocusLostOnControl;
            }
        }

        protected virtual void OnRemoveButtonClicked(IRowVisualHandler handler)
        {
            // Response to the handler's version of the event
            RemoveButtonClicked(this);
        }

        public event Action<VariableRow> RemoveButtonClicked = delegate { };

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
            
            // Clear external subscribers to avoid lingering references if pooled.
            RemoveButtonClicked = delegate { };
        }

        protected virtual void UpdateSerializedVar()
        {
            if ((_prevVariable == _currentVariable) && _serializedVar != null)
                return;

            _serializedVar?.Dispose();

            if (_currentVariable != null)
            {
                // Guard against destroyed UnityEngine.Object
                if (_currentVariable is UnityObject unityObj)
                {
                    if (unityObj == null) // Unity's overloaded null check
                    {
                        _serializedVar = null;
                        return;
                    }

                    _serializedVar = new SerializedObject(unityObj);
                }
                else
                {
                    var holder = ScriptableObject.CreateInstance<MuscariableHolder>();
                    holder.Init(_currentVariable);
                    _serializedVar = new SerializedObject(holder);
                }

                _serializedVar.Update();
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

        protected SerializedObject SerializedObjectFrom(IVariable variable)
        {
            SerializedObject result = null;

            if (variable is UnityObject unityObj) // Should apply even when we have a MuscariableHolder passed in
            {
                if (unityObj != null) // Remember how the == operator is overridden for UnityObjects
                {
                    result = new SerializedObject(unityObj);
                }
                
            }
            else
            {
                MuscariableHolder holder = ScriptableObject.CreateInstance<MuscariableHolder>();
                holder.Init(variable);
                result = new SerializedObject(holder);
            }

            return result;
        }


    }

}