using UnityEngine;
using Amanita.EditorUtils;
using System.Collections.Generic;
using UnityEditor;
using UnityObj = UnityEngine.Object;

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
            var selection = ctx.Selection;
            bool correctInput = evt.type == EventType.KeyDown && evt.keyCode == Key;
            if (!correctInput)
                return false;

            if (!_focusChecker.CheckFocus(ctx))
                return false;

            var selected = selection.Blocks;
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
            var selection = ctx.Selection;
            var selected = selection.Blocks;
            if (selected == null || selected.Count == 0)
                return;

            // We'll handle the deletion here instead of passing it to FcWindowEditing since we
            // want to be able to undo the deletion of multiple blocks as a single action.
            // That, and to keep the new flowchart window from needing to involve FcWindowEditing
            // (that class is for the legacy window only).
            Flowchart fChart = selected[0].GetFlowchart();
            if (selected.Count == 1)
            {
                Undo.RecordObject(fChart, "Delete Block");
                

                Block toDelete = selected[0];
                fChart.RemoveBlock(toDelete);

                BlockSignals.PreBlockDelete?.Invoke(toDelete);
                uint id = toDelete.ItemId;

                DestroyThoroughly(toDelete);
                BlockSignals.PostBlockDelete?.Invoke(id);
            }
            else
            {
                Undo.RecordObject(fChart, "Delete Multiple Blocks");

                fChart.RemoveMultiBlocks(selected);

                BlockSignals.PreMultiBlockDelete?.Invoke(selected);

                IList<uint> blockIds = new List<uint>();
                for (int i = 0; i < selected.Count; i++)
                {
                    var block = selected[i];
                    blockIds.Add(block.ItemId);
                }

                for (int i = 0; i < selected.Count; i++)
                {
                    var toDelete = selected[i];
                    DestroyThoroughly(toDelete);
                }

                BlockSignals.PostMultiBlockDelete?.Invoke(blockIds);

            }

            ctx.ForceRepaintCount++;
        }

        private void DestroyThoroughly(Block block)
        {
            DestroyCommandsOf(block);
            UnityObj.DestroyImmediate(block);
        }

        void DestroyCommandsOf(Block block)
        {
            for (int i = 0; i < block.CommandList.Count; i++)
            {
                Command cmd = block.CommandList[i];
                UnityObj.DestroyImmediate(cmd);
            }
        }
    }
}