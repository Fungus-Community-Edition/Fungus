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
        public virtual void Init(VisualElement holder, IVariable toRepresent,
            IRowVisualHandler visHandler)
        {
            _isDisposed = false;
            _holder = holder;
            _prevVariable = _currentVariable;
            _currentVariable = toRepresent;

            UpdateSerializedVar();

            VisualHandler = visHandler;
            VisualHandler.SerializedVar = _serializedVar;
            VisualHandler.Init(holder, toRepresent);
            VisualHandler.Refresh();
        }

        protected bool _isDisposed;
        protected VisualElement _holder;
        protected IVariable _prevVariable;
        protected IVariable _currentVariable;

        protected virtual void UpdateSerializedVar()
        {
            if ((_prevVariable == _currentVariable) && _serializedVar != null) return;

            if (_currentVariable != null)
            {
                _serializedVar?.Dispose();
                _serializedVar = SerializedObjectFrom(_currentVariable);
                _serializedVar.Update();
                var prop = _serializedVar.FindProperty("value");
                //Debug.Log($"{prop.propertyType} at path 'value'");
            }
            else
            {
                _serializedVar = null;
            }
        }

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

                return VisualHandler.Root;
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

        public virtual void Refresh()
        {
            UpdateSerializedVar();
            VisualHandler?.Refresh();
        }

        protected SerializedObject SerializedObjectFrom(IVariable variable)
        {
            SerializedObject result;

            if (variable is UnityObject unityObj) // Should apply even when we have a MuscariableHolder passed in
            {
                result = new SerializedObject(unityObj);
            }
            else
            {
                MuscariableHolder holder = ScriptableObject.CreateInstance<MuscariableHolder>();
                holder.Init(variable);
                result = new SerializedObject(holder);
            }

            return result;
        }

        /// <summary>
        /// Resets the state of this row, including how it's meant to start out 
        /// non-parented.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            Clear();
            var rootParent = RootElement?.parent;
            rootParent?.Remove(RootElement);

            VisualHandler?.Dispose();
            VisualHandler = null;
            _currentVariable = null;
            _holder = null;
            _isDisposed = true;
        }

    }

}