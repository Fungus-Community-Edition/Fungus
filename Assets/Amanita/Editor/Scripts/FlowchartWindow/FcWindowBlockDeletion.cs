using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AtMycelia.Hyphlow.EditorUtils
{
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
            Flowchart fChart = ctx.Flowchart;
            selection.ClearBlocks();
            selection.ClearCommands();
            
            if (selected.Count == 1)
            {
                Block toDelete = selected[0];

                fChart.RemoveBlock(toDelete);

                ushort id = toDelete.ItemId;

                DestroyThoroughly(toDelete);
            }
            else
            {
                fChart.RemoveMultiBlocks(selected);

                IList<ushort> blockIds = selected.Select((elem) => elem.ItemId).ToList();

                for (int i = 0; i < selected.Count; i++)
                {
                    var toDelete = selected[i];
                    DestroyThoroughly(toDelete);
                }

            }

            ctx.ForceRepaintCount++;
        }

        private void DestroyThoroughly(Block block)
        {
            // Destroy each command on the block
            foreach (var cmd in block.CommandList)
                if (cmd != null)
                    Undo.DestroyObjectImmediate(cmd);

            // Destroy any event handler
            if (block._EventHandler != null)
                Undo.DestroyObjectImmediate(block._EventHandler);

            var fc = block.GetFlowchart();

            // Destroy the block itself
            
            Undo.DestroyObjectImmediate(block);

            Selection.activeGameObject = fc.gameObject;

        }

    }
}