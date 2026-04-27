using System;
using UnityEngine;
using UnityEngine.UIElements;
using EditorObjectField = UnityEditor.UIElements.ObjectField;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public abstract class RowVisualHandler : IRowVisualHandler, IResettable
    {
        public virtual void Init(IVariable toDisplay)
        {
            _isDisposed = false;
            _currentVariable = toDisplay;
            _template = TemplateProvider.GetTemplate(GetType());
        }

        protected bool _isDisposed;
        protected IVariable _currentVariable;
        protected VisualTreeAsset _template;
        protected virtual IRowVisualTemplateProvider TemplateProvider => RowVisualTemplateProviderRegistry.Current;

        public virtual void Refresh()
        {
            ToggleSubs(false);
            EnsureVisualsAreReady();
            ApplyVarFieldsToOurControls();
            RowRoot?.MarkDirtyRepaint();
            KeyField?.MarkDirtyRepaint();
            ScopeField?.MarkDirtyRepaint();
            bool shouldHideScopeField = Variable != null && Variable.Owner is ScriptableObject;
            if (shouldHideScopeField)
            {
                // Variables belonging to ScriptableObjects such as VariableSourceAssets
                // are meant to be global, and thus showing their Scope fields is misleading.
                ScopeField.visible = false;
            }
            ToggleSubs(true);
        }

        #region Subscriptions
        protected virtual void ToggleSubs(bool on)
        {
            ToggleButtonClickSubs(on);
            ToggleValueChangeSubs(on);
        }

        protected virtual void ToggleButtonClickSubs(bool on)
        {
            if (RemoveButton == null)
            {
                return;
            }

            if (on)
            {
                RemoveButton.clicked += OnRemoveButtonClicked;
            }
            else
            {
                RemoveButton.clicked -= OnRemoveButtonClicked;
            }
        }

        protected virtual void OnRemoveButtonClicked()
        {
            RemoveButtonClicked(this);
        }

        public event Action<IRowVisualHandler> RemoveButtonClicked = delegate { };

        protected virtual void ToggleValueChangeSubs(bool on)
        {
            if (VisualSynchronizer == null)
            {
                Debug.LogWarning("RowVisualHandler cannot synchronize because no visual synchronizer is registered.");
                return;
            }

            bool shouldConsiderDisconnect = !on || _currentVariable == null || KeyField == null || ScopeField == null;
            if (shouldConsiderDisconnect)
            {
                if (_syncSession != null)
                {
                    VisualSynchronizer.Disconnect(_syncSession);
                    _syncSession = null;
                }
                return;
            }

            RowSyncContext context = new RowSyncContext(
                _currentVariable,
                KeyField,
                ScopeField,
                ValueField as VisualElement,
                field => KeyFieldChanged(field),
                scope => ScopeFieldChanged(scope),
                ApplyVarValueToValueField);

            _syncSession = VisualSynchronizer.Connect(context);
        }

        private IRowSyncSession _syncSession;
        protected virtual IRowVisualSynchronizer VisualSynchronizer => RowVisualSynchronizerRegistry.Current;

        public event Action<TextField> KeyFieldChanged = delegate { };
        public event Action<VariableScope> ScopeFieldChanged = delegate { };
        public event Action<object> ValueFieldChanged = delegate { };

        protected virtual void TriggerValueFieldChanged(object newValue)
        {
            ValueFieldChanged(newValue);
        }
        #endregion

        protected virtual void EnsureVisualsAreReady()
        {
            if (RowRoot == null && _template != null)
            {
                RegisterVisualElements();
                if (RowRoot != null)
                {
                    RowRoot.name = _currentVariable != null
                        ? $"{_currentVariable.ContentType.Name}_Row"
                        : "EmptyRow";
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

            RowVisualElements builtElements = VisualElementBuilder.Build(_template);
            bool failedToBuild = builtElements == null || builtElements.Root == null;
            if (failedToBuild)
            {
                Debug.LogWarning($"{GetType().Name}: Failed to build visuals from template.");
                return;
            }

            ApplyVisualElements(builtElements);
        }

        protected virtual IRowVisualElementBuilder VisualElementBuilder => RowVisualElementBuilderRegistry.Current;

        protected virtual void ApplyVisualElements(RowVisualElements elements)
        {
            _visualElements = elements;
            RowRoot = elements.Root;
        }

        #region Visual Elements Accessors
        protected RowVisualElements VisualElements => _visualElements;
        private RowVisualElements _visualElements;
        protected TextField KeyField => VisualElements?.KeyField;
        protected IBindable ValueField => VisualElements?.ValueField;
        protected EnumField ScopeField => VisualElements?.ScopeField;
        protected Button RemoveButton => VisualElements?.RemoveButton;
        #endregion

        protected virtual void ApplyVarFieldsToOurControls()
        {
            if (_currentVariable == null)
            {
                Debug.LogWarning($"[RowVisualHandler] BindFields called but _currentVariable is null " +
                    $"for handler = {GetType().FullName}");
                return;
            }

            if (VisualBinder == null)
            {
                Debug.LogWarning("RowVisualHandler cannot bind because no visual binder is registered.");
                return;
            }

            EnsureScopeFieldInitialized();

            RowBindingContext context = new RowBindingContext(_currentVariable, _visualElements,
                ApplyVarValueToValueField);
            VisualBinder.Bind(context);
        }

        protected virtual void EnsureScopeFieldInitialized()
        {
            if (ScopeField == null)
            {
                return;
            }

            Enum currentValue = ScopeField.value;
            if (currentValue == null || currentValue.GetType() != typeof(VariableScope))
            {
                VariableScope initValue = _currentVariable != null ? 
                    _currentVariable.Scope : 
                    VariableScope.Private;
                ScopeField.Init(initValue);
            }
        }

        protected virtual IRowVisualBinder VisualBinder => RowVisualBinderRegistry.Current;

        protected virtual void ApplyVarValueToValueField()
        {
            if (ValueField is EditorObjectField objField)
            {
                objField.SetValueWithoutNotify(_currentVariable.BoxedValue as UnityObj);
                objField.MarkDirtyRepaint();
            }
        }

        public virtual IVariable Variable
        {
            get => _currentVariable;
            set
            {
                if (_currentVariable == value)
                {
                    return;
                }

                _currentVariable = value;
                Refresh();
            }
        }

        public virtual void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            Reset();
            RemoveButtonClicked = delegate { };
            KeyFieldChanged = delegate { };
            ScopeFieldChanged = delegate { };
            ValueFieldChanged = delegate { };
        }

        public virtual void Reset()
        {
            ToggleSubs(false);
            Hide();
            NullOutVars();
        }

        protected virtual void Hide()
        {
            RowRoot?.RemoveFromHierarchy();
        }

        protected virtual void NullOutVars()
        {
            _currentVariable = null;
            RowRoot = null;
            _visualElements = null;
            _syncSession = null;
        }

        public virtual VisualTreeAsset Template => _template;
        public abstract Type VarContentType { get; }
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

    public abstract class RowVisualHandler<TVarContentType> : RowVisualHandler
    {
        public override Type VarContentType => varContentType;
        protected static readonly Type varContentType = typeof(TVarContentType);

        protected RowVisualHandler()
        {
        }

        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();

            unityObjField = ValueField as EditorObjectField;
            if (unityObjField != null)
            {
                unityObjField.objectType = varContentType;
            }
        }

        protected EditorObjectField unityObjField;

        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (unityObjField == null)
            {
                return;
            }

            if (on)
            {
                unityObjField.RegisterValueChangedCallback(OnObjectFieldChanged);
            }
            else
            {
                unityObjField.UnregisterValueChangedCallback(OnObjectFieldChanged);
            }
        }

        protected virtual void OnObjectFieldChanged(ChangeEvent<UnityObj> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
        }

        protected override void NullOutVars()
        {
            base.NullOutVars();
            unityObjField = null;
        }
    }

    [RowVisualHandler("Hidden", typeof(object), "Generic",
        "UIToolkitTemplates/VarRows/_VariableRowTemplate")]
    public class DefaultRowVisualHandler : RowVisualHandler<object>
    {
    }
}