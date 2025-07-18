using Amanita.Collections;
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
        
        public void CreateBlock(Flowchart fc, Vector2 pos)
        {
            var newBlock = fc.CreateBlock(pos);
            // give it a visible area for hit‐testing
            newBlock._NodeRect = new Rect(pos, defaultNodeSize);
            created.Add(newBlock);
            fc.AddToSelection(newBlock);

        }

        protected readonly static Vector2 defaultNodeSize = new Vector2(20, 20);
        public List<Block> Created { get { return new List<Block>(created); } }
        protected IList<Block> created = new List<Block>();

        public void DeselectAll() => Flowchart.ClearSelectedBlocks();

        public void QueueToDelete(IList<Block> blocks) => queuedForDeletion.AddRange(blocks);
        public IList<Block> QueuedForDeletion { get { return new List<Block>(queuedForDeletion); } }
        protected IList<Block> queuedForDeletion = new List<Block>();

        public void DeleteScheduledBlocks()
        {
            foreach (var block in QueuedForDeletion)
            {
                GameObject.DestroyImmediate(block.gameObject);
            }
            queuedForDeletion.Clear();
        }

        public void UpdateBlockCollection() { /* no-op for tests */ }
        public void Repaint() { /* no-op for tests */ }

        public virtual void Dispose()
        {
            Clipboard = null;
            queuedForDeletion.Clear();
            created.Clear();

            if (Flowchart != null)
            {
                GameObject.DestroyImmediate(Flowchart.gameObject);
            }
        }
    }


}