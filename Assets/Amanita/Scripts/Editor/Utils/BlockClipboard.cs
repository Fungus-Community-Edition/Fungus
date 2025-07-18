using Amanita.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    public class BlockClipboard
    {
        readonly List<BlockClipboardEntry> _entries = new List<BlockClipboardEntry>();

        public BlockClipboard(FlowchartWindow window)
        {
            this.Window = window;
        }

        public void Copy(IEnumerable<Block> blocks)
        {
            _entries.Clear();
            IEnumerable<BlockClipboardEntry> newEntries = blocks.Select(toCopy => new BlockClipboardEntry(toCopy));
            _entries.AddRange(newEntries);
        }

        public bool HasEntries => _entries.Count > 0;

        public virtual FlowchartWindow Window { get; protected set; }
        protected virtual Flowchart Flowchart
        {
            get
            {
                if (Window == null)
                {
                    return null;
                }

                return Window.currentFlowchart;
            }
        }

        public void Paste(Vector2 screenMousePos, bool relative = false)
        {
            // 1) Undo + clear out old selection
            Undo.RecordObject(Flowchart, "Paste Blocks");
            Window.DeselectAll();

            // 2) Actually instantiate each snapshot
            var pasted = _entries
                .Select(entry => entry.PasteBlock(Window, Flowchart))
                .ToList();

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
                Flowchart.AddSelectedBlock(elem);
            }

            // 5) Refresh the window’s block cache
            Window.UpdateBlockCollection();
        }
    }

}