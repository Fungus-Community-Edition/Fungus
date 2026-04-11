using System;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Encapsulates Flowchart selection operations so callers don't have
    /// to reach into Flowchart directly for every query or mutation.
    /// </summary>
    public class SelectionState : IDisposable
    {
        private static readonly IList<Block> EmptyBlocks = Array.Empty<Block>();

        public Flowchart Flowchart { get; set; }

        public IList<Block> Blocks => Flowchart != null ? Flowchart.SelectedBlocks : EmptyBlocks;

        public int BlockCount => Flowchart != null ? 
            Flowchart.SelectedBlockCount :
            0;

        public int CommandCount => Flowchart != null ? 
            Flowchart.SelectedCommandCount : 
            0;

        public bool HasSelection => BlockCount > 0 || CommandCount > 0;

        public bool HasMultipleBlocks => BlockCount > 1;

        public bool Contains(Block block)
        {
            return block != null &&
                   Flowchart != null &&
                   Flowchart.SelectedBlocks.Contains(block);
        }

        public void ClearBlocks()
        {
            if (Flowchart == null)
            {
                return;
            }

            Flowchart.ClearSelectedBlocks();
        }

        public void ClearCommands()
        {
            if (Flowchart == null)
            {
                return;
            }

            Flowchart.ClearSelectedCommands();
        }

        public void ReplaceWith(Block block)
        {
            if (Flowchart == null)
            {
                return;
            }

            Flowchart.ClearSelectedBlocks();

            if (block != null)
            {
                Flowchart.AddToSelection(block);
            }
        }

        public void Add(Block block)
        {
            if (Flowchart == null || block == null)
            {
                return;
            }

            if (!Flowchart.SelectedBlocks.Contains(block))
            {
                Flowchart.AddToSelection(block);
            }
        }

        public void Remove(Block block)
        {
            if (Flowchart == null || block == null)
            {
                return;
            }

            if (Flowchart.SelectedBlocks.Contains(block))
            {
                Flowchart.DeselectBlockNoCheck(block);
            }
        }

        public void Toggle(Block block)
        {
            if (Flowchart == null || block == null)
            {
                return;
            }

            if (Flowchart.SelectedBlocks.Contains(block))
            {
                Flowchart.DeselectBlockNoCheck(block);
            }
            else
            {
                Flowchart.AddToSelection(block);
            }
        }

        public void Dispose()
        {
            ClearBlocks();
            ClearCommands();
            Flowchart = null;
        }
    }
}