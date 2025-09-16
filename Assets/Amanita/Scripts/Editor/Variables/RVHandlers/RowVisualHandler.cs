using Amanita.EditorUtils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using EditorObjectField = UnityEditor.UIElements.ObjectField;

namespace Amanita.VScripting.EditorUtils
{
    public abstract class RowVisualHandler : IRowVisualHandler, IResettable
    {
        public abstract Type VarContentType { get; }

        protected bool _isDisposed;
        protected IVariable _currentVariable;
        protected IVariable _prevVariable;
        protected VisualElement _valueFieldHolder;
        protected TextField _keyField;
        protected EnumField _scopeField;

        protected VisualTreeAsset _template;
        protected static readonly Dictionary<string, VisualTreeAsset> _templateCache =
            new Dictionary<string, VisualTreeAsset>(StringComparer.Ordinal);

        public virtual void Init(IVariable toDisplay)
        {
            _isDisposed = false;
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

            if (!_templateCache.TryGetValue(key, out var visTreeAsset) || visTreeAsset == null)
            {
                var attr = handlerType.GetCustomAttribute<RowVisualHandlerAttribute>();
                if (attr == null)
                {
                    Debug.LogError($"{handlerType.Name} is missing RowVisualHandlerAttribute.");
                    return null;
                }

                visTreeAsset = Resources.Load<VisualTreeAsset>(attr.PathToTemplate);

                if (visTreeAsset == null)
                {
                    string errorMessage = string.Format(missingTemplateFormat, handlerType.Name, attr.PathToTemplate);
                    Debug.LogError(errorMessage);
                    LoggedMissingOnce.Add(handlerType);
                    return null;
                }

                _templateCache[key] = visTreeAsset;
                Debug.Log($"RowVisualHandler.GetOrResolveTemplate: Loaded template for {handlerType.Name} from '{attr.PathToTemplate}'");
            }
            else
            {
                Debug.Log($"RowVisualHandler.GetOrResolveTemplate: Using cached template for {handlerType.Name}");
            }

            return _templateCache[key];
        }

        public static IList<Type> LoggedMissingOnce = new List<Type>();
        protected static string TemplateKeyFor(Type handlerType) => handlerType.AssemblyQualifiedName;
        protected static readonly string missingTemplateFormat =
            "Template for {0} not found at '{1}'.\nPlease update the path in the RowVisualHandlerAttribute of the former.";

        public virtual void Refresh()
        {
            EnsureVisualsAreReady();

            if (_prevVariable != _currentVariable)
            {
                UnbindFields();
            }

            BindFields();
        }

        protected virtual void EnsureVisualsAreReady()
        {
            if (RowRoot == null && _template != null)
            {
                RegisterVisualElements();

                if (_currentVariable != null)
                {
                    // For debug purposes
                    string typeName = _currentVariable.ContentType.Name;
                    RowRoot.name = $"{typeName}_Row";
                }
                else
                {
                    RowRoot.name = "EmptyRow";
                }
            }
        }

        public virtual VisualElement RowRoot { get; protected set; }

        protected virtual void RegisterVisualElements()
        {
            if (_template == null)
            {
                Debug.LogWarning($"{GetType().Name}: Cannot register visuals; template is null.");
                return;
            }

            RowRoot = _template.CloneTree();
            Debug.Log($"RowVisualHandler.RegisterVisualElements: Cloned template for handler={GetType().Name}, RowRoot != null: {RowRoot != null}");
            _keyField = RowRoot.Q<TextField>("KeyInput");
            _valueFieldHolder = RowRoot.Q<VisualElement>("ValueFieldHolder");
            _scopeField = RowRoot.Q<EnumField>("Scope");
            _removeButton = RowRoot.Q<Button>("RemoveButton");

            toRespondToFocusLoss.Add(_keyField);
            toRespondToFocusLoss.Add(_scopeField);
        }

        protected Button _removeButton;

        // This is to help make sure that changes to variables stick, what with how finicky
        // Unity's serialization can be
        protected IList<VisualElement> toRespondToFocusLoss = new List<VisualElement>();

        protected virtual void DecideBindingPaths()
        {
            ApplyDefaultBindingPaths();

            if (_serializedVar != null && _serializedVar.targetObject is MuscariableHolder)
            {
                // We want to bind to the muscariable it is holding
                ApplyMuscariableBindingPathOverrides();
            }
        }

        protected virtual void ApplyDefaultBindingPaths()
        {
            _keyField.bindingPath = "key";
            _scopeField.bindingPath = "scope";
        }

        protected virtual void ApplyMuscariableBindingPathOverrides()
        {
            _keyField.bindingPath = $"{muscariableMemberName}.{_keyField.bindingPath}";
            _scopeField.bindingPath = $"{muscariableMemberName}.{_scopeField.bindingPath}";
        }

        protected static readonly string muscariableMemberName = "muscariable";

        protected virtual void UnbindFields()
        {
            RowRoot?.Unbind();
            ToggleSubs(false);
        }

        protected virtual void BindFields()
        {
            if (SerializedVar == null || RowRoot == null)
            {
                return;
            }

            DecideBindingPaths();
            
            RowRoot?.Bind(SerializedVar);
            ToggleSubs(false);
            ToggleSubs(true);
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (_removeButton == null || _keyField == null || _scopeField == null)
            {
                return;
            }

            if (on)
            {
                _removeButton.clicked += OnRemoveButtonClicked;
                _keyField.RegisterCallback<FocusOutEvent>(OnAnyControlFocusLost);
                _scopeField.RegisterCallback<FocusOutEvent>(OnAnyControlFocusLost);
            }
            else
            {
                _removeButton.clicked -= OnRemoveButtonClicked;
                _keyField.UnregisterCallback<FocusOutEvent>(OnAnyControlFocusLost);
                _scopeField.UnregisterCallback<FocusOutEvent>(OnAnyControlFocusLost);
            }
        }

        protected virtual void OnRemoveButtonClicked()
        {
            RemoveButtonClicked(this);
        }

        public event Action<IRowVisualHandler> RemoveButtonClicked = delegate { };

        protected virtual void OnAnyControlFocusLost(FocusOutEvent evt)
        {
            FocusLostOnControl(evt);
            AmanitaEditorSignals.VarRowControlLostFocus(evt);
        }

        public event Action<FocusOutEvent> FocusLostOnControl = delegate { };

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
            if (RowRoot == null) return;
            RowRoot.RemoveFromHierarchy();
        }

        public virtual void Reset()
        {
            Hide();
            UnbindFields();

            _prevVariable = null;
            _currentVariable = null;
            _serializedVar?.Dispose();
            _serializedVar = null;
            RowRoot = null;
        }

        public virtual void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Reset();
            toRespondToFocusLoss.Clear();
            _currentVariable = _prevVariable = null;
            RemoveButtonClicked = delegate { };
        }

        public virtual VisualTreeAsset Template => _template;

    }

    public interface IRowVisualHandler : IDisposable
    {
        void Init(IVariable variable);
        IVariable Variable { get; set; }
        VisualElement RowRoot { get; }
        VisualTreeAsset Template { get; }
        Type VarContentType { get; }
        SerializedObject SerializedVar { get; set; }
        void Refresh();
        event Action<IRowVisualHandler> RemoveButtonClicked;
        event Action<FocusOutEvent> FocusLostOnControl;
    }

    public abstract class RowVisualHandler<TVarContentType> : RowVisualHandler
    {
        protected readonly Type _varContentType;

        protected RowVisualHandler()
        {
            _varContentType = typeof(TVarContentType);
        }

        public override Type VarContentType => _varContentType;

        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();

            // For those classes that simply need to hook up a single type to a single ObjectField
            _objField = RowRoot.Q<EditorObjectField>("UnityObjectField");

            if (_objField != null)
            {
                _objField.objectType = typeof(TVarContentType);
            }
        }

        protected EditorObjectField _objField;

    }

    [RowVisualHandler("Hidden", typeof(object), "Generic",
        "_EditorResources/UIToolkitTemplates/VarRows/_VariableRowTemplate")]
    public class DefaultRowVisualHandler : RowVisualHandler<object>
    {
    }
}