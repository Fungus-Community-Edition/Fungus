using System;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Synchronizes the visual elements of a Variable Row with the underlying variable data model.
    /// </summary>
    public interface IRowVisualSynchronizer
    {
        IRowSyncSession Connect(RowSyncContext context);
        void Disconnect(IRowSyncSession session);
    }

    public interface IRowSyncSession
    {
        bool IsConnected { get; }
    }

    public readonly struct RowSyncContext
    {
        public RowSyncContext(
            IVariable variable,
            TextField keyField,
            EnumField scopeField,
            VisualElement valueField,
            Action<TextField> keyFieldChanged,
            Action<VariableScope> scopeFieldChanged,
            Action applyValueFromModel)
        {
            Variable = variable;
            KeyField = keyField;
            ScopeField = scopeField;
            ValueField = valueField;
            KeyFieldChanged = keyFieldChanged;
            ScopeFieldChanged = scopeFieldChanged;
            ApplyValueFromModel = applyValueFromModel;
        }

        public IVariable Variable { get; }
        public TextField KeyField { get; }
        public EnumField ScopeField { get; }
        public VisualElement ValueField { get; }
        public Action<TextField> KeyFieldChanged { get; }
        public Action<VariableScope> ScopeFieldChanged { get; }
        public Action ApplyValueFromModel { get; }
        public bool IsValid => Variable != null && KeyField != null && ScopeField != null;
    }

    public static class RowVisualSynchronizerRegistry
    {
        private static IRowVisualSynchronizer _current = new DefaultRowVisualSynchronizer();

        public static IRowVisualSynchronizer Current
        {
            get => _current;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _current = value;
            }
        }
    }

    public sealed class DefaultRowVisualSynchronizer : IRowVisualSynchronizer
    {
        public IRowSyncSession Connect(RowSyncContext context)
        {
            if (!context.IsValid)
            {
                return null;
            }

            RowSyncSession session = new RowSyncSession(context);
            session.Connect();
            return session;
        }

        public void Disconnect(IRowSyncSession session)
        {
            if (session is RowSyncSession concreteSession)
            {
                concreteSession.Disconnect();
            }
        }

        private sealed class RowSyncSession : IRowSyncSession
        {
            public RowSyncSession(RowSyncContext context)
            {
                _context = context;
                _variable = context.Variable;
            }

            private readonly RowSyncContext _context;
            private readonly IVariable _variable;

            public bool IsConnected { get; private set; }

            public void Connect()
            {
                if (IsConnected)
                {
                    return;
                }

                HandleKeyField();
                void HandleKeyField()
                {
                    if (_context.KeyField != null)
                    {
                        _keyCallback = OnKeyChanged;
                        _context.KeyField.RegisterValueChangedCallback(_keyCallback);
                    }
                }

                HandleScopeField();
                void HandleScopeField()
                {
                    if (_context.ScopeField != null)
                    {
                        _scopeCallback = OnScopeChanged;
                        _context.ScopeField.RegisterValueChangedCallback(_scopeCallback);
                    }
                }

                HandleMuscariableValueChange();
                void HandleMuscariableValueChange()
                {
                    if (_variable != null && _variable is Muscariable muscariable)
                    {
                        muscariable.OnValueChanged += OnMuscariableValueChanged;
                    }
                }

                IsConnected = true;
            }

            private EventCallback<ChangeEvent<string>> _keyCallback;
            
            private void OnKeyChanged(ChangeEvent<string> evt)
            {
                _context.KeyFieldChanged?.Invoke(_context.KeyField);
            }

            private EventCallback<ChangeEvent<Enum>> _scopeCallback;

            private void OnScopeChanged(ChangeEvent<Enum> evt)
            {
                _context.ScopeFieldChanged?.Invoke((VariableScope)evt.newValue);
            }

            private void OnMuscariableValueChanged(Muscariable changedVar)
            {
                _context.ApplyValueFromModel?.Invoke();
            }

            public void Disconnect()
            {
                if (!IsConnected)
                {
                    return;
                }

                HandleKeyField();
                void HandleKeyField()
                {
                    if (_context.KeyField != null && _keyCallback != null)
                    {
                        _context.KeyField.UnregisterValueChangedCallback(_keyCallback);
                    }
                }

                HandleScopeField();
                void HandleScopeField()
                {

                    if (_context.ScopeField != null && _scopeCallback != null)
                    {
                        _context.ScopeField.UnregisterValueChangedCallback(_scopeCallback);
                    }
                }

                HandleMuscariableValueChange();
                void HandleMuscariableValueChange()
                {
                    if (_variable != null && _variable is Muscariable muscariable)
                    {
                        muscariable.OnValueChanged -= OnMuscariableValueChanged;
                    }
                }

                IsConnected = false;
            }

        }
    }
}