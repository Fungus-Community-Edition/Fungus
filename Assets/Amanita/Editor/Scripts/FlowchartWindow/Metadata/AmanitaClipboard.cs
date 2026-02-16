using System;
using System.Collections.Generic;
using Amanita.VScripting.EditorUtils;
using Amanita.VScripting;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// Centralized class for handling clipboard operations related to Amanita (such as Flowchart blocks and commands).
    /// </summary>
    public class AmanitaClipboard : IDisposable
    {
        // If we ever find ourselves needing to track more types of copied data
        // (e.g. variables, comments, etc.) we can expand this class to include
        // additional clipboards or a more generic clipboard system.

        public AmanitaClipboard()
            : this(new BlockClipboard(null), new CommandClipboard())
        {
        }

        public AmanitaClipboard(BlockClipboard blockClipboard, CommandClipboard commandClipboard)
        {
            BlockClipboard = blockClipboard;
            CommandClipboard = commandClipboard;
        }

        public AmanitaClipboard(IFlowchartHostCore host)
            : this(new BlockClipboard(host), new CommandClipboard())
        {
        }

        public BlockClipboard BlockClipboard { get; private set; }
        public CommandClipboard CommandClipboard { get; private set; }

        public bool HasBlockEntries => BlockClipboard != null && BlockClipboard.HasEntries;
        public bool HasCommandEntries => CommandClipboard != null && CommandClipboard.HasCommands();

        public virtual void CopyBlocks(FlowchartContext context)
        {
            if (context == null || BlockClipboard == null)
            {
                return;
            }

            IList<Block> selectedBlocks = context.Selection.Blocks;
            if (selectedBlocks == null || selectedBlocks.Count == 0)
            {
                return;
            }

            BlockClipboard.Copy(selectedBlocks);
            BlockSignals.BlocksCopied(selectedBlocks);
        }

        public virtual void CutBlocks(FlowchartContext context)
        {
            if (context == null)
            {
                return;
            }

            CopyBlocks(context);
            DeleteBlocks(context);
        }

        public virtual void DeleteBlocks(FlowchartContext context)
        {
            if (context == null || context.FcHost == null)
            {
                return;
            }

            var selection = context.Selection;
            int blockCount = selection.Blocks?.Count ?? 0;
            if (blockCount == 0)
            {
                return;
            }

            #region Gather up Block IDs for post-deletion signals
            IList<ushort> blockIDs = new List<ushort>();
            for (int i = 0; i < blockCount; i++)
            {
                var currentBlock = selection.Blocks[i];
                blockIDs.Add(currentBlock.ItemId);
            }
            #endregion

            #region Pre-Delete Broadcasts
            if (blockCount == 1)
            {
                BlockSignals.PreBlockDelete(selection.Blocks[0]);
            }
            else
            {
                BlockSignals.PreMultiBlockDelete(selection.Blocks);
            }
            #endregion

            FcWindowBlockDeletion blockDeletion = new FcWindowBlockDeletion();
            blockDeletion.Execute(context);

            #region Post-Delete Broadcasts
            if (blockCount == 1)
            {
                BlockSignals.PostBlockDelete(blockIDs[0]);
            }
            else
            {
                BlockSignals.PostMultiBlockDelete(blockIDs);
            }
            #endregion
        }

        public virtual void CopySelectedCommands(Flowchart flowchart)
        {
            if (CommandClipboard == null)
            {
                return;
            }

            CommandClipboard.CopySelectedCommands(flowchart);
        }

        public virtual void CutSelectedCommands(Flowchart flowchart)
        {
            if (CommandClipboard == null)
            {
                return;
            }

            CommandClipboard.CutSelectedCommands(flowchart);
        }

        public void Dispose()
        {
            BlockClipboard?.Dispose();
            BlockClipboard = null;
            CommandClipboard = null;
        }
    }
}