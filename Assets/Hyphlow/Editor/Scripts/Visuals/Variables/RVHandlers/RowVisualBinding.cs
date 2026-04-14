using System;

// For binding a RowVisualHandler's visuals to a variable's data.
namespace AtMycelia.Hyphlow.EditorUtils
{
    public sealed class DefaultRowVisualBinder : IRowVisualBinder
    {
        public void Bind(RowBindingContext context)
        {
            if (context.Variable == null || context.Visuals == null)
            {
                return;
            }

            context.Visuals.KeyField?.SetValueWithoutNotify(context.Variable.Key);
            context.Visuals.ScopeField?.SetValueWithoutNotify(context.Variable.Scope);
            context.ApplyValueBinding?.Invoke();
        }
    }

    public interface IRowVisualBinder
    {
        void Bind(RowBindingContext context);
    }

    public readonly struct RowBindingContext
    {
        public RowBindingContext(IVariable variable, RowVisualElements visuals, Action applyValueBinding)
        {
            Variable = variable;
            Visuals = visuals;
            ApplyValueBinding = applyValueBinding;
        }

        public IVariable Variable { get; }
        public RowVisualElements Visuals { get; }
        public Action ApplyValueBinding { get; }
    }

    public static class RowVisualBinderRegistry
    {
        private static IRowVisualBinder _current = new DefaultRowVisualBinder();

        public static IRowVisualBinder Current
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

}