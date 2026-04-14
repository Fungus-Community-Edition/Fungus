using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class DrawBlockContext : IDisposable
    {
        public virtual void Dispose()
        {
            BlockMinWidth = 60;
            BlockMinWidth = 240;
            DefaultBlockHeight = 40;
            NodeStyle = DescriptionStyle = HandlerStyle = BlockSearchPopupNormalStyle =
                BlockSearchPopupSelectedStyle = null;
            Graphics = default;
            ViewRect = CurrentBlockWindowRect = default;
        }

        public virtual FlowchartContext FlowchartCtx { get; set; }
        public virtual float BlockMinWidth { get; set; } = 60;
        public virtual float BlockMaxWidth { get; set; } = 240;
        public virtual float DefaultBlockHeight { get; set; } = 40;
        public virtual bool UseGridSnap { get { return HyphlowEditorPreferences.useGridSnap; } }
        public virtual float GridObjectSnap { get { return FlowchartCtx.GridObjectSnap; } }
        public virtual GUIStyle NodeStyle { get; set; }
        public virtual GUIStyle DescriptionStyle { get; set; }
        public virtual GUIStyle HandlerStyle { get; set; }
        public virtual GUIStyle BlockSearchPopupNormalStyle { get; set; }
        public virtual GUIStyle BlockSearchPopupSelectedStyle { get; set; }
        public virtual BlockGraphics Graphics { get; set; }
        public virtual IReadOnlyCollection<Block> AllBlocks
        {
            get { return FlowchartCtx.Document.AllBlocks; }
        }
        public virtual Rect ViewRect { get; set; }
        public Rect CurrentBlockWindowRect { get; set; }

    }

}