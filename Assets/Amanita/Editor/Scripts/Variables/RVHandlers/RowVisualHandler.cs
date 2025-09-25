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
        public virtual void Init(IVariable toDisplay)
        {
            _isDisposed = false;
            _prevVariable = _currentVariable;
            _currentVariable = toDisplay;
            _template = GetOrResolveTemplate(GetType());
        }

        protected bool _isDisposed;
        protected IVariable _prevVariable;
        protected IVariable _currentVariable;
        protected VisualTreeAsset _template;

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
            }

            return _templateCache[key];
        }
        public static IList<Type> LoggedMissingOnce = new List<Type>();

        protected static readonly Dictionary<string, VisualTreeAsset> _templateCache =
            new Dictionary<string, VisualTreeAsset>(StringComparer.Ordinal);
        
        protected static string TemplateKeyFor(Type handlerType) => handlerType.AssemblyQualifiedName;
        protected static readonly string missingTemplateFormat =
            "Template for {0} not found at '{1}'.\nPlease update the path in the RowVisualHandlerAttribute of the former.";

        public virtual void Refresh()
        {
            EnsureVisualsAreReady();
            UnbindFields();
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
            _keyField = RowRoot.Q<TextField>("KeyInput");
            _valueFieldHolder = RowRoot.Q<VisualElement>("ValueFieldHolder");
            _scopeField = RowRoot.Q<EnumField>("Scope");
            _removeButton = RowRoot.Q<Button>("RemoveButton");
            valueField = (IBindable)RowRoot.Q("ValueField");

            RegisterElementsForFocusLoss();
        }

        protected TextField _keyField;
        protected VisualElement _valueFieldHolder;
        protected EnumField _scopeField;
        protected Button _removeButton;
        protected IBindable valueField;

        protected virtual void RegisterElementsForFocusLoss()
        {
            toRespondToFocusLoss.Add(_keyField);
            toRespondToFocusLoss.Add(_scopeField);
            if (valueField != null)
            {
                toRespondToFocusLoss.Add((BindableElement)valueField);
            }
        }

        // This is to help make sure that changes to variables stick, what with how finicky
        // Unity's serialization can be
        protected IList<VisualElement> toRespondToFocusLoss = new List<VisualElement>();

        protected virtual void UnbindFields()
        {
            Debug.Log($"[RowVisualHandler] UnbindFields called for varKey='{_currentVariable?.Key}' handler={GetType().FullName}");
            RowRoot?.Unbind();
            ToggleSubs(false);
        }

        protected virtual void BindFields()
        {
            Debug.Log($"[RowVisualHandler] BindFields called for varKey='{_currentVariable?.Key}' handler={GetType().FullName} serializedTarget='{_serializedVar?.targetObject?.name ?? "null"}' targetType='{_serializedVar?.targetObject?.GetType().FullName ?? "null"}'");
            if (SerializedVar == null || RowRoot == null)
            {
                Debug.Log($"[RowVisualHandler] BindFields aborted: SerializedVar or RowRoot null. SerializedVar target='{_serializedVar?.targetObject?.name ?? "null"}' RowRoot is null={RowRoot==null}");
                return;
            }

            DecideBindingPaths();

            // Diagnostics: report binding paths
            try
            {
                string k = _keyField?.bindingPath ?? "(null)";
                string s = _scopeField?.bindingPath ?? "(null)";
                string v = valueField != null ? (valueField as VisualElement)?.name + "->" + (valueField as IBindable)?.bindingPath : "(null)";
                Debug.Log($"[RowVisualHandler] Binding paths: Key='{k}' Scope='{s}' Value='{(valueField != null ? (valueField as IBindable)?.bindingPath ?? "(unknown)" : "(null)")}'");
            }
            catch { }

            // Ensure the SerializedObject is up-to-date before binding.
            try
            {
                _serializedVar.Update();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RowVisualHandler] SerializedObject.Update() failed: {ex.Message}");
            }

            // Sanity-check that the properties we plan to bind exist on the SerializedObject.
            try
            {
                string keyPath = _keyField?.bindingPath;
                string scopePath = _scopeField?.bindingPath;
                string valuePath = valueField != null ? (valueField as IBindable)?.bindingPath : null;

                SerializedProperty pKey = null, pScope = null, pValue = null;
                if (!string.IsNullOrEmpty(keyPath)) pKey = _serializedVar.FindProperty(keyPath);
                if (!string.IsNullOrEmpty(scopePath)) pScope = _serializedVar.FindProperty(scopePath);
                if (!string.IsNullOrEmpty(valuePath)) pValue = _serializedVar.FindProperty(valuePath);

                Debug.Log($"[RowVisualHandler] Property lookup: keyProp={(pKey != null ? "FOUND" : "MISSING")} scopeProp={(pScope != null ? "FOUND" : "MISSING")} valueProp={(pValue != null ? "FOUND" : "MISSING")}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RowVisualHandler] Property lookup failed: {ex.Message}");
            }

            // Perform binding
            RowRoot?.Bind(SerializedVar);

            // Force a repaint/update so bound controls reflect the just-updated SerializedObject.
            try
            {
                RowRoot?.MarkDirtyRepaint();
            }
            catch { /* MarkDirtyRepaint is a best-effort call */ }

            ToggleSubs(false);
            ToggleSubs(true);
        }

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
            if (valueField != null)
            {
                valueField.bindingPath = "value";
            }
        }

        protected virtual void ApplyMuscariableBindingPathOverrides()
        {
            _keyField.bindingPath = $"{muscariableMemberName}.{_keyField.bindingPath}";
            _scopeField.bindingPath = $"{muscariableMemberName}.{_scopeField.bindingPath}";
            if (valueField != null)
            {
                valueField.bindingPath = $"{muscariableMemberName}.{valueField.bindingPath}";
            }
        }
        protected static readonly string muscariableMemberName = "muscariable";


        protected virtual void ToggleSubs(bool on)
        {
            ToggleButtonClickSubs(on);
            ToggleFocusLossSubs(on);
            ToggleValueChangeSubs(on);
        }

        protected virtual void ToggleButtonClickSubs(bool on)
        {
            if (_removeButton == null)
            {
                return;
            }

            if (on)
            {
                _removeButton.clicked += OnRemoveButtonClicked;
            }
            else
            {
                _removeButton.clicked -= OnRemoveButtonClicked;
            }
        }

        protected virtual void OnRemoveButtonClicked()
        {
            RemoveButtonClicked(this);
        }
        public event Action<IRowVisualHandler> RemoveButtonClicked = delegate { };

        protected virtual void ToggleFocusLossSubs(bool on)
        {
            foreach (var elem in toRespondToFocusLoss)
            {
                if (elem == null)
                {
                    continue;
                }

                if (on)
                {
                    elem.RegisterCallback<FocusOutEvent>(OnAnyControlFocusLost);
                }
                else
                {
                    elem.UnregisterCallback<FocusOutEvent>(OnAnyControlFocusLost);
                }
            }
        }

        protected virtual void OnAnyControlFocusLost(FocusOutEvent evt)
        {
            FocusLostOnControl(evt);
            AmanitaEditorSignals.VarRowControlLostFocus(evt);
        }

        public event Action<FocusOutEvent> FocusLostOnControl = delegate { };

        protected virtual void ToggleValueChangeSubs(bool on)
        {
            ToggleValueChange(_keyField, OnKeyFieldChanged, on);
            ToggleValueChange(_scopeField, OnEnumFieldChanged, on);
        }

        protected virtual void ToggleValueChange<T>(INotifyValueChanged<T> field,
            EventCallback<ChangeEvent<T>> callback,
            bool on)
        {
            if (field == null)
            {
                return;
            }

            if (on)
            {
                field.RegisterValueChangedCallback(callback);
            }
            else
            {
                field.UnregisterValueChangedCallback(callback);
            }
        }

        protected virtual void OnKeyFieldChanged(ChangeEvent<string> evt)
        {
            AnyControlValueChanged();
            AmanitaEditorSignals.ControlValueChanged(evt);
        }

        protected virtual void OnEnumFieldChanged(ChangeEvent<Enum> evt)
        {
            AnyControlValueChanged();
            AmanitaEditorSignals.ControlValueChanged(evt);
        }

        public event Action AnyControlValueChanged = delegate { };

        public virtual SerializedObject SerializedVar
        {
            get { return _serializedVar; }
            set
            {
                if (_serializedVar == value) return;

                // Diagnostics: log changes to SerializedVar target
                try
                {
                    var oldTarget = _serializedVar?.targetObject;
                    var newTarget = value?.targetObject;
                    Debug.Log($"[RowVisualHandler.SerializedVar] Changing for varKey='{_currentVariable?.Key}' oldTarget='{oldTarget?.name ?? "null"}' oldType='{oldTarget?.GetType().FullName ?? "null"}' newTarget='{newTarget?.name ?? "null"}' newType='{newTarget?.GetType().FullName ?? "null"}'");
                }
                catch { }//

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

        public virtual void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Reset();
            RemoveButtonClicked = delegate { };
        }

        public virtual void Reset()
        {
            Hide();
            UnbindFields();
            _serializedVar?.Dispose();
            toRespondToFocusLoss.Clear();
            NullOutVars();
        }

        protected virtual void Hide()
        {
            if (RowRoot == null) return;
            RowRoot.RemoveFromHierarchy();
        }

        protected virtual void NullOutVars()
        {
            _prevVariable = _currentVariable = null;
            _serializedVar = null;
            RowRoot = null;
        }

        public virtual VisualTreeAsset Template => _template;
        public abstract Type VarContentType { get; }

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
            if (valueField != null && valueField is EditorObjectField objField)
            {
                objField.objectType = typeof(TVarContentType);
            }
        }

    }

    [RowVisualHandler("Hidden", typeof(object), "Generic",
        "UIToolkitTemplates/VarRows/_VariableRowTemplate")]
    public class DefaultRowVisualHandler : RowVisualHandler<object>
    {
    }
}