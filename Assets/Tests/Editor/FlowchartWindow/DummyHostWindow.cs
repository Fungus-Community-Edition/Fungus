using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// A minimal EditorWindow that implements IFlowchartHost by delegating to FakeFlowchartHost
namespace Amanita.EditorUtils
{
    public class DummyHostWindow : EditorWindow, IFlowchartHost
    {
        public FakeFlowchartHost Host { get; private set; }

        public void Init(FakeFlowchartHost host)
        {
            Host = host;
            Host.Init();
        }

        public Flowchart Flowchart => Host.Flowchart;
        public BlockClipboard Clipboard { get => Host.Clipboard; set => Host.Clipboard = value; }
        public bool HasClipboard => Host.HasClipboard;
        public List<Block> Created => Host.Created;
        public void DeselectAll() => Host.DeselectAll();
        public void QueueToDelete(IList<Block> blocks) => Host.QueueToDelete(blocks);
        public IList<Block> QueuedForDeletion => Host.QueuedForDeletion;
        public void DeleteScheduledBlocks() => Host.DeleteScheduledBlocks();
        public void UpdateBlockCollection() => Host.UpdateBlockCollection();
        public new void Repaint() => Host.Repaint();
        public void Dispose() => Host.Dispose();

        public virtual Block CreateBlock(Flowchart fc, Vector2 pos)
        {
            return Host.CreateBlock(fc, pos);
        }
    }
}