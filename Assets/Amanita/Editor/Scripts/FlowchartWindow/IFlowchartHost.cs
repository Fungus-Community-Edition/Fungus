using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public interface IFlowchartHost
    {
        Flowchart Flowchart { get; }
        BlockClipboard Clipboard { get; set;  }
        bool HasClipboard { get; }
        Block CreateBlock(Flowchart fc, Vector2 pos);
        void DeselectAll();
        void UpdateBlockCollection();
        void Repaint();
        T GetComponent<T>() where T : IFcWindowComponent;
        Vector2 GetBlockCenter(IList<Block> blocks);

        void OnGUI();
        Rect CalcFlowchartWindowViewRect();
        Color GridLineColor { get; }

        DrawGridContext DrawGridCtx { get; }
        DrawBlockContext DrawBlockCtx { get; }
        FlowchartContext FlowchartCtx { get; }
        IList<Block> Blocks { get; }
        Rect Position { get; }
        VisualElement RootVisualElement { get; }
        void DoZoom(float delta, Vector2 center);
        void CenterFlowchart();
        void SelectBlock(Block block);
    }
}