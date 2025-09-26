using System.Collections.Generic;

namespace Amanita.VScripting.RuntimeTesting
{
    // Runtime (non-Editor) test support commands.
    // IMPORTANT: This file must NOT be inside an Editor folder so that these
    // MonoBehaviours can be attached via AddComponent in EditMode tests.

    // Command WITH CommandInfo and a reorderable array.
    [CommandInfo("Test", "Dummy Array Cmd", "Dummy command with reorderable list")]
    public class DummyArrayCommand : Command
    {
        public List<string> names = new List<string> { "A", "B" };
        public int someValue = 10;

        public override bool IsReorderableArray(string propertyName) =>
            propertyName == nameof(names);
    }

    // Command WITHOUT CommandInfo (tests early-return path in CommandEditor).
    public class NoInfoCommand : Command
    {
        public int value = 1;
    }
}