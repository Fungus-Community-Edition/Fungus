using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    public class DeleteShortcutHandler : IUGUIEventHandler
    {
        readonly FcWindowBlockDeletion _deletion;
        readonly IFocusChecker _focusChecker;

        public DeleteShortcutHandler(FcWindowBlockDeletion deletion, KeyCode key,
            IFocusChecker focusChecker)
        {
            _deletion = deletion;
            Key = key;
            _focusChecker = focusChecker;
        }

        public KeyCode Key { get; }

        public bool Handle(Event evt, FlowchartContext ctx)
        {
            bool correctInput = evt.type == EventType.KeyDown && evt.keyCode == Key;
            if (!correctInput)
                return false;

            if (!_focusChecker.CheckFocus(ctx))
                return false;

            var selected = ctx.SelectedBlocks;
            if (selected == null || selected.Count == 0)
                return false;

            _deletion.Execute(ctx);
            evt.Use();
            return true;
        }
    }

    public class FcWindowBlockDeletion
    {
        public void Execute(FlowchartContext ctx)
        {
            var selected = ctx.SelectedBlocks;
            if (selected == null || selected.Count == 0)
                return;

            FcWindowEditing windowEditing = ctx.FcHost.GetComponent<FcWindowEditing>();
            windowEditing.QueueToDelete(selected);
            ctx.ForceRepaintCount++;
        }

    }
}