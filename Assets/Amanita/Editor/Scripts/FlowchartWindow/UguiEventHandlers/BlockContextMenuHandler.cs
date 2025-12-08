using System.Collections.Generic;
using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    public class BlockContextMenuHandler : IUGUIEventHandler
    {
        public BlockContextMenuHandler(IFlowchartHost host, IContextMenuFactory factory)
        {
            _host = host;
            _factory = factory;
        }

        protected readonly IFlowchartHost _host;
        protected readonly IContextMenuFactory _factory;

        public bool Handle(Event guiEvent, FlowchartContext flowchartCtx)
        {
            bool rightMouseDown = guiEvent.type == EventType.MouseDown && guiEvent.button == MouseButton.Right;
            bool weWantToReact = rightMouseDown || guiEvent.type == EventType.ContextClick;
            
            if (weWantToReact)
            {
                ShowMenu(guiEvent, flowchartCtx);
                guiEvent.Use();
            }

            bool consumed = weWantToReact;
            return consumed;
        }
    
        protected virtual void ShowMenu(Event guiEvent, FlowchartContext flowchartCtx)
        {
            IContextMenu menu = _factory.Create();
            Vector2 mousePos = guiEvent.mousePosition;

            Flowchart fc = flowchartCtx.Flowchart;

            // ← Prefer the pre‐populated hit if you set it in a test
            Block hitBlock = flowchartCtx.BlockHitInLastMouseDown;
            if (hitBlock == null)
            {
                hitBlock = flowchartCtx.TopmostBlockOverlapping(mousePos); // Fallback
                
                if (fc.SelectedBlockCount == 0)
                {
                    fc.AddToSelection(hitBlock);
                }
            }

            if (hitBlock != null)
            {
                RegisterBlockTargetingOptions(menu, hitBlock, flowchartCtx);
            }
            else
            {
                RegisterEmptySpaceOptions(guiEvent, menu, flowchartCtx);
            }

            menu.DropDown(new Rect(mousePos, Vector2.zero));
        }

        protected virtual void RegisterBlockTargetingOptions(IContextMenu menu,
            Block hitBlock,
            FlowchartContext flowchartCtx)
        {
            menu.AddItem(CopyLabel, false, () => CopyBlocks(flowchartCtx));

            // We should only allow cutting and deletion when there 
            // are any blocks selected
            if (Flowchart.SelectedBlockCount > 0)
            {
                menu.AddItem(CutLabel, false, () => CutBlocks(flowchartCtx));
                menu.AddItem(DeleteLabel, false, () => blockDeletion.Execute(flowchartCtx));
            }
            else
            {
                menu.AddDisabledItem(CutLabel);
                menu.AddDisabledItem(DeleteLabel);
            }
            
        }

        protected static GUIContent CopyLabel { get { return CtxMenuLabels.CopyLabel; } }
        protected static GUIContent CutLabel { get { return CtxMenuLabels.CutLabel; } }
        protected static GUIContent DeleteLabel { get { return CtxMenuLabels.DeleteLabel; } }

        protected virtual void CopyBlocks(FlowchartContext flowchartCtx)
        {
            IList<Block> selectedBlocks = flowchartCtx.SelectedBlocks;
            _host.Clipboard.Copy(selectedBlocks);
            FlowchartWindowSignals.BlocksCopied(selectedBlocks);
        }

        protected virtual void CutBlocks(FlowchartContext flowchartCtx)
        {
            CopyBlocks(flowchartCtx);
            DeleteBlocks(flowchartCtx);
        }

        protected virtual void DeleteBlocks(FlowchartContext flowchartCtx)
        {
            IList<Block> selectedBlocks = flowchartCtx.SelectedBlocks;
            FcWindowEditing windowEditing = _host.GetComponent<FcWindowEditing>();
            windowEditing?.QueueToDelete(selectedBlocks);
            flowchartCtx.ForceRepaintCount++;
        }

        protected static readonly FcWindowBlockDeletion blockDeletion = new FcWindowBlockDeletion();

        protected virtual void RegisterEmptySpaceOptions(Event guiEvent, IContextMenu menu, FlowchartContext flowchartCtx)
        {
            menu.AddItem(AddLabel, false, () => AddBlock(guiEvent, flowchartCtx));

            if (_host.HasClipboard)
                menu.AddItem(PasteLabel, false, () => PasteBlocks(guiEvent));
            else
                menu.AddDisabledItem(PasteLabel);

            menu.AddSeparator("");

            if (Application.isPlaying)
            {
                menu.AddItem(StopAllLabel, false, StopAllBlocks);
            }
            else
            {
                menu.AddDisabledItem(StopAllLabel);
            }
        }

        protected static GUIContent AddLabel { get { return CtxMenuLabels.addLabel; } }
        protected static GUIContent PasteLabel { get { return CtxMenuLabels.pasteLabel; } }
        protected static GUIContent StopAllLabel { get { return CtxMenuLabels.stopAllLabel; } }

        protected virtual void AddBlock(Event guiEvent, FlowchartContext flowchartCtx)
        {
            Vector2 screenSpaceMousePos = guiEvent.mousePosition;
            Vector2 mousePosInFcSpace = screenSpaceMousePos / Flowchart.Zoom
                                - Flowchart.ScrollPos;
            Flowchart.ClearSelectedBlocks();
            _host.CreateBlock(flowchartCtx.Flowchart, mousePosInFcSpace);
            _host.UpdateBlockCollection();
            _host.Repaint();
        }

        protected virtual Flowchart Flowchart
        {
            get
            {
                if (_host == null)
                {
                    return null;
                }

                return _host.Flowchart;
            }
        }
        protected virtual void PasteBlocks(Event guiEvent)
        {
            Vector2 screenSpaceMousePos = guiEvent.mousePosition;
            _host.Clipboard.Paste(screenSpaceMousePos);
            _host.UpdateBlockCollection();
        }

        protected virtual void StopAllBlocks()
        {
            IList<Block> toStop = _host.Flowchart.GetExecutingBlocks();
            foreach (var elem in toStop)
            {
                elem.Stop();
            }
        }
    }

    // So we don't need to instantiate as many labels, thus cutting down on garbage
    public static class CtxMenuLabels
    {
        public static readonly GUIContent CopyLabel = new GUIContent("Copy"),
            CutLabel = new GUIContent("Cut"),
            DeleteLabel = new GUIContent("Delete"),
            addLabel = new GUIContent("Add"),
            pasteLabel = new GUIContent("Paste"),
            stopAllLabel = new GUIContent("Stop All");
    }
}