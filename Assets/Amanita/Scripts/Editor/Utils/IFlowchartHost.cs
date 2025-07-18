using Amanita;
using Amanita.EditorUtils;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.EditorUtils
{
    public interface IFlowchartHost
    {
        Flowchart Flowchart { get; }
        BlockClipboard Clipboard { get; }
        bool HasClipboard { get; }
        void CreateBlock(Flowchart fc, Vector2 pos);
        void DeselectAll();
        void QueueToDelete(IList<Block> blocks);
        void DeleteScheduledBlocks();
        void UpdateBlockCollection();
        void Repaint();
    }
}