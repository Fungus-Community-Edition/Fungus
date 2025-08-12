using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    public interface IRowVisualHandler : IDisposable
    {
        void Init(VisualElement holder, IVariable variable);
        IVariable Variable { get; set; }
        VisualElement Root { get; }
        VisualTreeAsset Template { get; }
        Type VarContentType { get; }
        void Refresh();
    }

    public abstract class RowVisualHandler : IRowVisualHandler, IResettable
    {
        public abstract Type VarContentType { get; }

        protected bool _isDisposed;
        protected VisualElement _holder;
        protected IVariable _currentVariable;
        protected IVariable _prevVariable;
        protected SerializedObject _serializedObject;
        protected VisualElement _valueFieldHolder;
        protected TextField _keyField;
        protected EnumField _scopeField;

        protected VisualTreeAsset _template;
        protected static readonly Dictionary<string, VisualTreeAsset> _templateCache =
            new Dictionary<string, VisualTreeAsset>(StringComparer.Ordinal);

        public virtual void Init(VisualElement rowHolder, IVariable toDisplay)
        {
            _isDisposed = false;
            _holder = rowHolder;
            _prevVariable = _currentVariable;
            _currentVariable = toDisplay;

            _template = GetOrResolveTemplate(GetType());
        }

        /// <summary>
        /// Centralized entry for resolving a handler’s template from cache or resources.
        /// </summary>
        protected VisualTreeAsset GetOrResolveTemplate(Type handlerType)
        {
            var key = TemplateKeyFor(handlerType);

            if (!_templateCache.TryGetValue(key, out var vta) || vta == null)
            {
                var attr = handlerType.GetCustomAttribute<RowVisualHandlerAttribute>();
                if (attr == null)
                {
                    Debug.LogError($"{handlerType.Name} is missing RowVisualHandlerAttribute.");
                    return null;
                }

                vta = Resources.Load<VisualTreeAsset>(attr.PathToTemplate);
                if (vta == null)
                {
                    Debug.LogError(string.Format(missingTemplateFormat, handlerType.Name, attr.PathToTemplate));
                    return null;
                }

                _templateCache[key] = vta;
            }

            return _templateCache[key];
        }

        protected static string TemplateKeyFor(Type t) => t.AssemblyQualifiedName;
        protected static readonly string missingTemplateFormat =
            "Template for {0} not found at '{1}'.\nPlease update the path in the RowVisualHandlerAttribute of the former.";

        public virtual void Refresh()
        {
            EnsureVisualsAreReady();

            if (_holder != null && !_holder.Contains(Root))
                _holder.Add(Root);

            UpdateSerializedObject();
            UnbindFields();
            BindFields();
        }

        protected virtual void EnsureVisualsAreReady()
        {
            if (Root == null && _template != null)
                RegisterVisualElements();
        }

        public virtual VisualElement Root { get; protected set; }

        protected virtual void RegisterVisualElements()
        {
            if (_template == null)
            {
                Debug.LogWarning($"{GetType().Name}: Cannot register visuals; template is null.");
                return;
            }

            Root = _template.CloneTree();
            _keyField = Root.Q<TextField>("KeyInput");
            _valueFieldHolder = Root.Q<VisualElement>("ValueFieldHolder");
            _scopeField = Root.Q<EnumField>("Scope");
        }

        protected virtual void UpdateSerializedObject()
        {
            if ((_prevVariable == _currentVariable) && _serializedObject != null) return;

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

        protected virtual void UnbindFields()
        {
            _keyField?.Unbind();
            _valueFieldHolder?.Unbind();
            _scopeField?.Unbind();
        }

        protected virtual void BindFields()
        {
            if (_serializedObject == null) return;

            _keyField?.Bind(_serializedObject);
            _valueFieldHolder?.Bind(_serializedObject);
            _scopeField?.Bind(_serializedObject);
        }

        public virtual IVariable Variable
        {
            get => _currentVariable;
            set
            {
                if (_currentVariable == value) return;
                _prevVariable = _currentVariable;
                _currentVariable = value;
            }
        }

        protected virtual void Hide()
        {
            if (Root == null) return;
            if (_holder != null && _holder.Contains(Root))
                _holder.Remove(Root);
        }

        public virtual void Reset()
        {
            Hide();
            UnbindFields();

            _prevVariable = null;
            _currentVariable = null;
            _holder = null;
            Root = null;
        }

        public virtual void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Reset();
        }

        public virtual VisualTreeAsset Template => _template;
    }

    public abstract class RowVisualHandler<TVarContentType> : RowVisualHandler
    {
        protected readonly Type _varContentType;

        protected RowVisualHandler()
        {
            _varContentType = typeof(TVarContentType);
        }

        public override Type VarContentType => _varContentType;

    }

    [RowVisualHandler("Primitives", typeof(float), "Float",
        "_EditorResources/UIToolkitTemplates/VarRows/FloatVariableRow")]
    public class FloatRowVisualHandler : RowVisualHandler<float>
    {
        private FloatField _floatField;

        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            _floatField = Root.Q<FloatField>("FloatField");
        }

        protected override void UnbindFields()
        {
            base.UnbindFields();
            _floatField?.Unbind();
        }

        protected override void BindFields()
        {
            base.BindFields();
            _floatField?.Bind(_serializedObject);
        }
    }

    [RowVisualHandler("Misc", typeof(object), "Generic",
        "_EditorResources/UIToolkitTemplates/VarRows/_VariableRowTemplate")]
    public class DefaultRowVisualHandler : RowVisualHandler<object>
    {
    }
}