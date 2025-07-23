using System.Collections.Generic;
using UnityEngine;

namespace Amanita.EditorUtils
{
    public interface IFlowchartHost
    {
        Flowchart Flowchart { get; }
        BlockClipboard Clipboard { get; }
        bool HasClipboard { get; }
        Block CreateBlock(Flowchart fc, Vector2 pos);
        void DeselectAll();
        void UpdateBlockCollection();
        void Repaint();
        T GetComponent<T>() where T : IFcWindowComponent;
    }
}