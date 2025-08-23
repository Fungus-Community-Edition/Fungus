using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Block event signalling system.
    /// You can use this to be notified about various events related to Blocks, such as parts
    /// of its execution process.
    /// </summary>
    public static class BlockSignals
    {
        #region Public members
        public static Action<Block, Event> BlockClicked = delegate { };
        public static Action<Block> BlockCreated = delegate { };

        /// <summary>
        /// For when single Blocks are selected, as opposed to multiple at once.
        /// An example of the latter would be when clicking Duplicate while 2+
        /// Blocks are selected.
        /// </summary>
        public static Action<Block> BlockSelected = delegate { };

        public static Action<Block> BlockRemovedFromSelection = delegate { };

        /// <summary>
        /// For when multiple blocks are selected at once
        /// </summary>
        public static Action<IList<Block>> MultiBlocksSelected = delegate { };

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

        /// <summary>
        /// CommandExecute signal. Sent just before a Command in a Block executes.
        /// </summary>
        public static event CommandExecuteHandler OnCommandExecute = delegate { };
        public delegate void CommandExecuteHandler(Block block, Command command, int commandIndex, int maxCommandIndex);
        public static void DoCommandExecute(Block block, Command command, int commandIndex, int maxCommandIndex)
        {
            OnCommandExecute(block, command, commandIndex, maxCommandIndex);
        }

        #endregion
    }
}
