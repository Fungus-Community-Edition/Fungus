using System;

namespace AtMycelia.Hyphlow
{
    public static class CommandSignals
    {
        public static Action<Command> CommandSelected = delegate { };
    }

    public interface ICommandSelectionResponder
    {
        void OnCommandSelected(Command command);
    }
}