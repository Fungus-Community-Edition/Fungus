using Amanita.EditorUtils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using EditorObjectField = UnityEditor.UIElements.ObjectField;
using UnityObj = UnityEngine.Object;

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
            ToggleSubs(false);
            EnsureVisualsAreReady();
            ApplyVarValuesToOurControls();
            ToggleSubs(true);
        }

        protected virtual void EnsureVisualsAreReady()
        {
            if (RowRoot == null && _template != null)
            {
                RegisterVisualElements();
                SetRootName();
                void SetRootName()
                {
                    // For easier identification in the debug window
                    if (_currentVariable != null)
                    {
                        string typeName = _currentVariable.ContentType.Name;
                        RowRoot.name = $"{typeName}_Row";
                    }
                    else
                    {
                        RowRoot.name = "EmptyRow";
                    }
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
            _keyField.isDelayed = true; // So that changes only register on enter or focus loss
            _keyField.multiline = false;

            _valueFieldHolder = RowRoot.Q<VisualElement>("ValueFieldHolder");
            valueField = (IBindable)RowRoot.Q("ValueField");

            _scopeField = RowRoot.Q<EnumField>("Scope");
            _removeButton = RowRoot.Q<Button>("RemoveButton");
        }

        protected TextField _keyField;
        protected VisualElement _valueFieldHolder;
        protected IBindable valueField;
        protected EnumField _scopeField;
        protected Button _removeButton;
        
        protected virtual void ApplyVarValuesToOurControls()
        {
            if (_currentVariable == null)
            {
                Debug.LogWarning($"[RowVisualHandler] BindFields called but _currentVariable is null " +
                    $"for handler={GetType().FullName}");
                return;
            }

            // Rather than rely on UITK's auto-binding, we are going to manually
            // set field values for Muscaris in Flowcharts, since this way, not
            // only will we no longer need MuscariableHolders, but we can also avoid
            // some of the serialization pitfalls UITK binding has.
            _keyField.SetValueWithoutNotify(_currentVariable.Key);
            _scopeField.SetValueWithoutNotify(_currentVariable.Scope);
            ApplyVarValueToValueField();

            MarkForRepainting();
            void MarkForRepainting()
            {
                _keyField.MarkDirtyRepaint();
                _scopeField.MarkDirtyRepaint();
                RowRoot?.MarkDirtyRepaint();
            }

        }

        /// <summary>
        /// This base implementation only works with ObjectFields. If the value is not a
        /// UnityObject, override this method in your derived class.
        /// </summary>
        protected virtual void ApplyVarValueToValueField()
        {
            if (valueField is EditorObjectField objField)
            {
                objField.SetValueWithoutNotify(_currentVariable.BoxedValue as UnityObj);
                objField.MarkDirtyRepaint();
            }
        }

        protected virtual void ToggleSubs(bool on)
        {
            ToggleButtonClickSubs(on);
            ToggleValueChangeSubs(on);
        }

        protected virtual void ToggleValueChangeSubs(bool on)
        {
            if (on)
            {
                _keyField.RegisterValueChangedCallback(OnKeyFieldChanged);
                _scopeField.RegisterValueChangedCallback(OnScopeValueChanged);
            }
            else
            {
                _keyField.UnregisterValueChangedCallback(OnKeyFieldChanged);
                _scopeField.UnregisterValueChangedCallback(OnScopeValueChanged);
            }
        }

        protected virtual void OnKeyFieldChanged(ChangeEvent<string> evt)
        {
            KeyFieldChanged(_keyField);
        }
        public event Action<TextField> KeyFieldChanged = delegate { };

        protected virtual void TriggerValueFieldChanged(object newValue)
        {
            ValueFieldChanged(newValue);
        }
        public event Action<object> ValueFieldChanged = delegate { };

        protected virtual void OnScopeValueChanged(ChangeEvent<Enum> evt)
        {
            VariableScope newVal = (VariableScope)evt.newValue;
            ScopeFieldChanged(newVal);
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

        public virtual IVariable Variable
        {
            get => _currentVariable;
            set
            {
                if (_currentVariable == value) return;
                _prevVariable = _currentVariable;
                _currentVariable = value;
                Refresh();
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
            ToggleSubs(false);
            Hide();
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
            RowRoot = null;
        }

        public virtual VisualTreeAsset Template => _template;
        public abstract Type VarContentType { get; }

        public event Action<VariableScope> ScopeFieldChanged = delegate { };

    }

    public interface IRowVisualHandler : IDisposable, IVarRowEventSignaler
    {
        void Init(IVariable variable);
        IVariable Variable { get; set; }
        VisualElement RowRoot { get; }
        VisualTreeAsset Template { get; }
        Type VarContentType { get; }
        void Refresh();
    }

    public interface IVarRowEventSignaler
    {
        event Action<IRowVisualHandler> RemoveButtonClicked;
        event Action<TextField> KeyFieldChanged;
        event Action<VariableScope> ScopeFieldChanged;
        event Action<object> ValueFieldChanged;
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

            // For those classes that simply need to hook up a UnityObj type to a single ObjectField
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