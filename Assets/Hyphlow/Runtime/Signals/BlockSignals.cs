using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Block event signalling system.
    /// You can use this to be notified about various events related to Blocks, such as parts
    /// of its execution process.
    /// </summary>
    public static class BlockSignals
    {
        #region Editor-Only Signals
        public static Action<Block, Event> BlockLeftClicked = delegate { };
        public static Action<Block, PointerEventInfo> BlockRightClicked = delegate { };
        public static Action<Block> BlockCreated = delegate { };

        /// <summary>
        /// For when single Blocks are selected, as opposed to multiple at once.
        /// An example of the latter would be when clicking Duplicate while 2+
        /// Blocks are selected.
        /// </summary>
        public static Action<Block> BlockSelected = delegate { };
        public static Action<Block> BlockDeselected = delegate { };
        /// <summary>
        /// For when multiple blocks are selected at once
        /// </summary>
        public static Action<IList<Block>> MultiBlocksSelected = delegate { };

        public static Action<IList<Block>> MultiBlocksDeselected = delegate { };

        public static Action<Block> PreBlockCut = delegate { };
        public static Action<ushort> PostBlockCut = delegate { };

        public static Action<IList<Block>> PreMultiBlockCut = delegate { };
        public static Action<IList<ushort>> PostMultiBlockCut = delegate { };
        /// <summary>
        /// Sent just before a Block is deleted. This should only signal for when the user
        /// is deleting one Block at a time, not when they're deleting multiple at once.
        /// 
        /// </summary>
        public static Action<Block> PreBlockDelete = delegate { };
        /// <summary>
        /// Sent just after a Block is deleted. The ushort argument is the ID of the deleted Block.
        /// </summary>
        public static Action<ushort> PostBlockDelete = delegate { };

        /// <summary>
        /// Sent just before multiple Blocks are deleted at once.
        /// </summary>
        public static Action<IList<Block>> PreMultiBlockDelete = delegate { };
        /// <summary>
        /// Sent just after multiple Blocks are deleted at once. The IList<ushort> argument
        /// contains the IDs of the deleted Blocks.
        /// </summary>
        public static Action<IList<ushort>> PostMultiBlockDelete = delegate { };

        public static Action<IList<Block>> BlocksCopied = delegate { };
        #endregion

        #region Runtime Signals
        /// <summary>
        /// BlockStart signal. Sent when the Block starts execution.
        /// </summary>
        public static event BlockStartHandler OnBlockStart = delegate { };
        public delegate void BlockStartHandler(Block block);
        public static void DoBlockStart(Block block)
        {
            OnBlockStart(block);
        }

        /// <summary>
        /// BlockEnd signal. Sent when the Block ends execution.
        /// </summary>
        public static event BlockEndHandler OnBlockEnd = delegate { };
        public delegate void BlockEndHandler(Block block);
        public static void DoBlockEnd(Block block)
        {
            OnBlockEnd(block);
        }
        #endregion

        /// <summary>
        /// CommandExecute signal. Sent just before a Command in a Block executes.
        /// </summary>
        public static event CommandExecuteHandler OnCommandExecute = delegate { };
        public delegate void CommandExecuteHandler(Block block, Command command, int commandIndex, int maxCommandIndex);
        public static void DoCommandExecute(Block block, Command command, int commandIndex, int maxCommandIndex)
        {
            OnCommandExecute(block, command, commandIndex, maxCommandIndex);
        }
    }

    public interface IPreBlockCutResponder
    {
        void OnPreBlockCut(Block block);
    }

    public interface IPostBlockCutResponder
    {
        void OnPostBlockCut(ushort blockId);
    }

    public interface IPreMultiBlockCutResponder
    {
        void OnPreMultiBlockCut(IList<Block> blocks);
    }

    public interface IPostMultiBlockCutResponder
    {
        void OnPostMultiBlockCut(IList<ushort> blockIds);
    }

    public interface IBlockClickResponder
    {
        void OnBlockClicked(Block block, Event evt);
    }

    public interface IBlockCreatedResponder
    {
        void OnBlockCreated(Block block);
    }

    public interface IBlockSelectionResponder
    {
        void OnBlockSelected(Block block);
    }

    public interface IBlockDeselectionResponder
    {
        void OnBlockDeselected(Block block);
    }

    public interface IMultiBlockSelectionResponder
    {
        void OnMultiBlocksSelected(IList<Block> blocks);
    }

    public interface IMultiBlockDeselectionResponder
    {
        void OnMultiBlocksDeselected(IList<Block> blocks);
    }

    public interface IPreBlockDeletionResponder
    {
        void OnPreBlockDeletion(IList<Block> blocks);
        void OnPreBlockDeletion(Block block);
    }

    public interface IPostBlockDeletionResponder
    {
        void OnPostBlockDeletion(ushort blockId);
    }

    public interface IPostMultiBlockDeletionResponder
    {
        void OnPostMultiBlockDeletion(IList<ushort> blockIds);
    }

    public interface IBlocksCopiedResponder
    {
        void OnBlocksCopied(IList<Block> blocks);
    }

}
