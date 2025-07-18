using Amanita;
using Amanita.EditorUtils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.EditorUtils
{
    class FakeFlowchartHost : IFlowchartHost, IDisposable
    {
        public virtual void Init()
        {
            Flowchart = new GameObject("fc").AddComponent<Flowchart>();
        }

        public Flowchart Flowchart { get; protected set; }
        public BlockClipboard Clipboard { get; set; } = new BlockClipboard(null);
        public bool HasClipboard => Clipboard.HasEntries;
        public List<Block> Created = new List<Block>();
        public List<Block> Queued = new List<Block>();

        public void CreateBlock(Flowchart fc, Vector2 pos)
        {
            var newBlock = fc.CreateBlock(pos);
            // give it a visible area for hit‐testing
            newBlock._NodeRect = new Rect(pos, defaultNodeSize);
            Created.Add(newBlock);
            fc.AddSelectedBlock(newBlock);

        }

        protected readonly static Vector2 defaultNodeSize = new Vector2(20, 20);

        public void DeselectAll() => Flowchart.ClearSelectedBlocks();
        public void QueueToDelete(IList<Block> blocks) => Queued.AddRange(blocks);
        public void DeleteScheduledBlocks()
        {
            foreach (var block in Queued)
            {
                GameObject.DestroyImmediate(block.gameObject);
            }
            Queued.Clear();
        }

        public void UpdateBlockCollection() { /* no-op for tests */ }
        public void Repaint() { /* no-op for tests */ }

        public virtual void Dispose()
        {
            Clipboard = null;
            Queued.Clear();
            Created.Clear();
            GameObject.DestroyImmediate(Flowchart.gameObject);
        }
    }


}