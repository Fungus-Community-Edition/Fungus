using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AtMycelia.Collections;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Clipboard for copying and pasting Flowchart blocks. Stores a list of 
    /// BlockClipboardEntry objects, which are snapshots of the copied blocks.
    /// </summary>
    public class BlockClipboard : IDisposable
    {
        readonly List<BlockClipboardEntry> _entries = new List<BlockClipboardEntry>();

        public BlockClipboard()
            : this(null)
        {
        }

        public BlockClipboard(IFlowchartHostCore window)
        {
            Window = window;
        }

        public void Copy(IEnumerable<Block> blocks)
        {
            Copy(blocks, false);
        }

        public void Copy(IEnumerable<Block> blocks, bool isCut)
        {
            origBlocks.Clear();
            _entries.Clear();
            IEnumerable<BlockClipboardEntry> newEntries = blocks.Select(toCopy => new BlockClipboardEntry(toCopy, isCut));
            _entries.AddRange(newEntries);
            origBlocks.AddRange(blocks.ToList());
        }

        public int EntryCount => _entries.Count;
        public bool HasEntries => _entries.Count > 0;
        protected IList<Block> origBlocks = new List<Block>();

        public virtual bool HasEntriesFor(IList<Block> blocks)
        {
            bool result = true;

            foreach (var elem in blocks)
            {
                if (!HasEntryFor(elem))
                {
                    result = false; break;
                }
            }

            return result;
        }

        public virtual bool HasEntryFor(Block block)
        {
            bool result = (from elem in origBlocks
                           where elem.Equals(block)
                           select elem).Any();
            return result;
        }

        public virtual bool HasMultiEntriesWithIDs(IList<ushort> ids)
        {
            bool result = true;

            for (int i = 0; i < ids.Count; i++)
            {
                int currentId = ids[i];
                if (!HasEntryWithID(currentId))
                {
                    result = false; break;
                }
            }

            return result;
        }

        public virtual bool HasEntryWithID(int id)
        {
            bool result = (from elem in origBlocks
                           where elem.ItemId == id
                           select elem).Any();
            return result;
        }
        public virtual IFlowchartHostCore Window { get; protected set; }
        protected virtual Flowchart Flowchart
        {
            get
            {
                if (Window == null)
                {
                    return null;
                }

                return Window.Flowchart;
            }
        }

        public void Paste(Vector2 screenMousePos, bool relative = false)
        {
            if (Window == null || Flowchart == null)
            {
                return;
            }

            // 1) Undo + clear out old selection
            Window.DeselectAll();
            Undo.RecordObject(Flowchart, "Paste Block(s)");
            
            // 2) Actually instantiate each snapshot
            var pasted = _entries
                .Select(entry => entry.PasteBlock(Window, Flowchart))
                .ToList();

            var pastedById = new Dictionary<ushort, Block>();
            for (int i = 0; i < _entries.Count && i < pasted.Count; i++)
            {
                pastedById[(ushort)_entries[i].BlockID] = pasted[i];
            }

            for (int i = 0; i < _entries.Count && i < pasted.Count; i++)
            {
                _entries[i].RestoreObjectReferences(pasted[i], Flowchart, pastedById);
                _entries[i].RefreshPastedObjects(pasted[i]);
            }

            // 3) Compute offset so center of pasted blocks is at mouse
            Vector2 copiedCenter = Window.GetBlockCenter(pasted) + Flowchart.ScrollPos;
            Vector2 worldMouse = screenMousePos / Flowchart.Zoom;
            Vector2 delta = relative
                ? screenMousePos
                : (worldMouse - copiedCenter);

            // 4) Move each block and re‐select it
            foreach (var elem in pasted)
            {
                var elemRect = elem._NodeRect;
                elemRect.position += delta;
                elem._NodeRect = elemRect;
                Flowchart.AddToSelection(elem);
            }

            // 5) Refresh the window’s block cache
            Window.UpdateBlockCollection();
        }
    
        public virtual void Dispose()
        {
            origBlocks.Clear();
            _entries.Clear();
            Window = null;
        }
    }

}