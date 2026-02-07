using System;
using UnityEngine;

namespace Amanita.VScripting
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