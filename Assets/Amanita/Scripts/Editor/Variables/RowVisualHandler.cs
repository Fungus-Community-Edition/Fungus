using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using UnityObject = UnityEngine.Object;
using System.Linq;
using System.CodeDom;

namespace Amanita.VScripting.EditorUtils
{
    // For now, we assume that all classes of this interface
    // will only work with up to 1 template throughout its entire lifetime
    public interface IRowVisualHandler : IDisposable
    {
        /// <summary>
        /// Used not just for initialization, but reuse after disposing
        /// </summary>
        void Init(VisualElement holder, IVariable variable);
        IVariable Variable { get; set; }
        VisualElement Root { get; }
        VisualTreeAsset Template { get; }
        /// <summary>
        /// The type of the value that the variable holds. float for FloatVariable, 
        /// string for StringVariables, etc.
        /// </summary>
        Type VarContentType { get; }
        void Refresh();
    }

    /// <summary>
    /// Handles the shared behaviour for showing the visuals in a VariableRow.
    /// Naturally, all based on the IVariable they're assigned.
    /// </summary>
    public abstract class RowVisualHandler : IRowVisualHandler, IResettable
    {
        public abstract Type VarContentType { get; }

        public virtual void Init(VisualElement rowHolder, IVariable toDisplay)
        {
            ReadyTheTemplate();
            if (!TemplateReadied)
            {
                return;
            }
            _holder = rowHolder;
            _prevVariable = _currentVariable;
            _currentVariable = toDisplay;
        }

        protected virtual void ReadyTheTemplate()
        {
            if (!TemplateReadied)
            {
                Type ourType = GetType();
                _handlerAttr = ourType.GetCustomAttribute<RowVisualHandlerAttribute>();
                if (_handlerAttr == null)
                {
                    string errorMessage = $"{ourType.Name} does not have a RowVisualHandlerAttribute set. Please set one.";
                    Debug.LogError(errorMessage);
                    return;
                }

                string path = _handlerAttr.PathToTemplate;
                _template = Resources.Load<VisualTreeAsset>(path);

                if (_template == null)
                {
                    string errorMessage = $"Template for {ourType.Name} class could not be found at {path}. Please update the path arg.";
                    Debug.LogError(errorMessage);
                }
            }
        }

        protected static RowVisualHandlerAttribute _handlerAttr;
        
        protected static VisualTreeAsset _template;
        // ^Note that in terms of VisualTreeAssets, templates are basically prefabs but
        // UI-Toolkit-centric

        // Only concerns itself with the variable and the visual elements. Not the flowchart.
        // We assume that this always gets called before any of the other methods in this
        // class, hence certain if-checks being omitted.

        protected static bool TemplateReadied => _template != null;

        protected VisualElement _holder;
        protected IVariable _currentVariable;

        public virtual void Refresh()
        {
            EnsureVisualsAreReady();
            // Rather than recreating the root each time we have to be shown, we'll
            // just create it once and use it until we're disposed. If asked 
            // to be shown after said disposal, then we recreate the root.
            if (!_holder.Contains(Root))
            {
                _holder.Add(Root);
            }
            UpdateSerializedObject();
            UnbindFields(); // In case we had a previous var to work with
            BindFields();
        }

        protected virtual void EnsureVisualsAreReady()
        {
            if (Root == null) // Implying this is our first time being shown or we were disposed earlier
            {
                RegisterVisualElements();
            }
        }

        public virtual VisualElement Root { get; protected set; }

        protected virtual void RegisterVisualElements()
        {
            // We assume that all IVariables we work with inherit from UnityObject, 
            // and thus that we can easily bind them to the fields
            Root = _template.CloneTree();
            _keyField = Root.Q<TextField>("KeyInput");
            _valueFieldHolder = Root.Q<VisualElement>("ValueFieldHolder");
            _scopeField = Root.Q<EnumField>("Scope");
        }

        protected TextField _keyField;
        protected VisualElement _valueFieldHolder;
        protected EnumField _scopeField;

        protected virtual void UpdateSerializedObject()
        {
            // We only need to update this when we're assigned a different variable to
            // work with. Why? The field-binding implies automatically updating the
            // IVariable and the display, saving us headache we'd otherwise have with
            // binding SerializedProperties manually
            if (_prevVariable != _currentVariable)
            {
                if (_currentVariable != null)
                {
                    _serializedObject?.Dispose();
                    _serializedObject = new SerializedObject((UnityObject)_currentVariable);
                    _serializedObject.Update();
                }
                else
                {
                    _serializedObject = null;
                }
            }
        }

        protected IVariable _prevVariable;
        protected SerializedObject _serializedObject;

        protected virtual void UnbindFields()
        {
            _keyField?.Unbind();
            _valueFieldHolder?.Unbind();
            _scopeField?.Unbind();
        }

        protected virtual void BindFields()
        {
            if (_serializedObject == null)
            {
                return;
            }

            _keyField.Bind(_serializedObject);
            _valueFieldHolder.Bind(_serializedObject);
            _scopeField.Bind(_serializedObject);
        }

        public virtual IVariable Variable
        {
            get { return _currentVariable; }
            set
            {
                if (_currentVariable == value)
                {
                    return;
                }

                _prevVariable = _currentVariable;
                _currentVariable = value;
            }
        }

        protected virtual void Hide()
        {
            if (Root == null)
            {
                Debug.LogWarning($"Cannot hide a VariableRow that doesn't have its visuals readied.");
                return;
            }

            if (_holder != null && _holder.Children().Contains(Root))
            {
                _holder.Remove(Root);
                // ^We should be able to add the root back in later, assuming we're not disposed before then
            }
        }

        public virtual void Reset()
        {
            ClearVisuals();
            void ClearVisuals()
            {
                Hide();
                UnbindFields();
            }

            ResetInstanceFields();
            void ResetInstanceFields()
            {
                // We want to leave the static fields alone since reloading them every time
                // just adds to overhead
                _prevVariable = null;
                _currentVariable = null;
                _holder = null;
                Root = null;
            }
        }


        public virtual void Dispose()
        {
            Reset(); // So pooling can call either
        }

        public virtual VisualTreeAsset Template
        {
            get { return _template; }
        }

        
    }

    public abstract class RowVisualHandler<TVarContentType> : RowVisualHandler
    {
        public RowVisualHandler()
        {
            // Caching it to reduce GC cruft
            _varContentType = typeof(TVarContentType);
        }

        protected Type _varContentType;
        public override Type VarContentType
        {
            get { return _varContentType; }
        }
    }
    [RowVisualHandler("Primitives", typeof(float), "Float", "_EditorResources/UIToolkitTemplates/VarRows/FloatVariableRow")]
    public class FloatRowVisualHandler : RowVisualHandler<float>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            _floatField = Root.Q<FloatField>("FloatField");
        }

        protected FloatField _floatField;

        protected override void UnbindFields()
        {
            base.UnbindFields();
            _floatField.Unbind();
        }

        protected override void BindFields()
        {
            base.BindFields();
            _floatField.Bind(_serializedObject);
        }
    }

    [RowVisualHandler("Misc", typeof(System.Object), "Generic", "_EditorResources/UIToolkitTemplates/VarRows/VariableRowTemplate")]
    public class DefaultRowVisualHandler : RowVisualHandler<System.Object>
    {
    }
}