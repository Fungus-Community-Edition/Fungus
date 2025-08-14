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
        SerializedObject SerializedVar { get; set; }
        void Refresh();
    }

    public abstract class RowVisualHandler : IRowVisualHandler, IResettable
    {
        public abstract Type VarContentType { get; }

        protected bool _isDisposed;
        protected VisualElement _holder;
        protected IVariable _currentVariable;
        protected IVariable _prevVariable;
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
        protected virtual VisualTreeAsset GetOrResolveTemplate(Type handlerType)
        {
            var key = TemplateKeyFor(handlerType);

            if (LoggedMissingOnce.Contains(handlerType))
            {
                return null;
            }

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
                    string errorMessage = string.Format(missingTemplateFormat, handlerType.Name, attr.PathToTemplate);
                    Debug.LogError(errorMessage);
                    LoggedMissingOnce.Add(handlerType);
                    return null;
                }

                _templateCache[key] = vta;
            }

            return _templateCache[key];
        }

        public static IList<Type> LoggedMissingOnce = new List<Type>();
        protected static string TemplateKeyFor(Type t) => t.AssemblyQualifiedName;
        protected static readonly string missingTemplateFormat =
            "Template for {0} not found at '{1}'.\nPlease update the path in the RowVisualHandlerAttribute of the former.";

        public virtual void Refresh()
        {
            EnsureVisualsAreReady();

            if (_holder != null && Root != null && !_holder.Contains(Root))
                _holder.Add(Root);

            UnbindFields();
            BindFields();
        }

        protected virtual void EnsureVisualsAreReady()
        {
            if (Root == null && _template != null)
            {
                RegisterVisualElements();

                if (_currentVariable != null)
                {
                    // For debug purposes
                    string typeName = _currentVariable.ContentType.Name;
                    Root.name = $"{typeName}_Row";
                }
                else
                {
                    Root.name = "EmptyRow";
                }
            }
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

        protected virtual void UnbindFields()
        {
            Root?.Unbind();
        }

        protected virtual void BindFields()
        {
            if (SerializedVar == null || Root == null)
            {
                return;
            }

            Root.Bind(SerializedVar);
        }

        public virtual SerializedObject SerializedVar
        {
            get { return _serializedVar; }
            set
            {
                if (_serializedVar == value) return;

                UnbindFields();
                _serializedVar = value;
                BindFields();
            }
        }

        protected SerializedObject _serializedVar;

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

    [RowVisualHandler("Primitives", typeof(string), "String",
        "_EditorResources/UIToolkitTemplates/VarRows/StringVariableRow")]
    public class StringRowVisualHandler : RowVisualHandler<object>
    {
    }

    [RowVisualHandler("Hidden", typeof(object), "Generic",
        "_EditorResources/UIToolkitTemplates/VarRows/_VariableRowTemplate")]
    public class DefaultRowVisualHandler : RowVisualHandler<object>
    {
    }
}