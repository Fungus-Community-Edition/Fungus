using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.EditorUtils
{
    public class FlowchartWindowInputHandler : IInputProcessor
    {
        public static readonly float RightClickTolerance = 5f;
        public static readonly float MinZoomValue = 0.25f;
        public static readonly float MaxZoomValue = 1f;
        public static readonly float GridLineSpacingSize = 120;
        public static readonly float GridObjectSnap = 20;

        public FlowchartWindowInputHandler()
        {
        }

        protected IList<IUGUIEventHandler> subhandlers = new List<IUGUIEventHandler>()
        {
            new SelectionHandler(),
            new BlockDragHandler(),
            new PanZoomHandler(),
        };

        public virtual bool Process(Event eventToProcess, FlowchartContext context)
        {
            foreach (var elem in subhandlers)
                if (elem.Handle(eventToProcess, context))
                    return true;
            return false;

        }

        protected FlowchartContext currentContext;

        public virtual void AddSubhandler(IUGUIEventHandler toAdd)
        {
            subhandlers.Remove(toAdd);
        }

        public virtual void RemoveSubhandler(IUGUIEventHandler toRemove)
        {
            subhandlers.Remove(toRemove);
        }

        public virtual void ClearSubhandlers()
        {
            subhandlers.Clear();
        }
        

        //protected virtual void OnMouseDown(Event mouseEvent)
        //{
        //    var hitBlock = GetBlockAtPoint(mouseEvent.mousePosition);

        //    // Convert Ctrl+Left click to a right click on mac
        //    if (Application.platform == RuntimePlatform.OSXEditor)
        //    {
        //        if (mouseEvent.button == MouseButton.Left &&
        //            mouseEvent.control)
        //        {
        //            mouseEvent.button = MouseButton.Right;
        //        }
        //    }

        //    switch (mouseEvent.button)
        //    {
        //        case MouseButton.Left:
        //            if (!mouseEvent.alt)
        //            {
        //                if (hitBlock != null)
        //                {
        //                    bool doubleClicked = mouseEvent.clickCount == 2;
        //                    if (doubleClicked)
        //                    {
        //                        CenterBlock(hitBlock);

        //                        mouseEvent.Use();
        //                        didDoubleClick = true;
        //                        return;
        //                    }

        //                    startDragPosition = mouseEvent.mousePosition / flowchart.Zoom - flowchart.ScrollPos;
        //                    Undo.RecordObject(flowchart, "Select");

        //                    if (GetAppendModifierDown())
        //                    {
        //                        //ctrl clicking blocks toggles between
        //                        if (mouseDownSelectionState.Contains(hitBlock))
        //                        {
        //                            RemoveMouseDownSelectionState(hitBlock);
        //                        }
        //                        else
        //                        {
        //                            AddMouseDownSelectionState(hitBlock);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (flowchart.SelectedBlocks.Contains(hitBlock))
        //                        {
        //                            SetBlockForInspector(flowchart, hitBlock);
        //                        }
        //                        else
        //                        {
        //                            SelectBlock(hitBlock);
        //                        }

        //                        dragBlock = hitBlock;
        //                        hasDraggedSelected = false;
        //                    }

        //                    mouseEvent.Use();
        //                    GUIUtility.keyboardControl = 0; // Fix for textarea not refeshing (change focus)
        //                }
        //                else if (!(UnityEditor.Tools.current == Tool.View && UnityEditor.Tools.viewTool == ViewTool.Zoom))
        //                {
        //                    startSelectionBoxPosition = mouseEvent.mousePosition;
        //                    SelectionBox = Rect.MinMaxRect(SelectionBox.x, SelectionBox.y, SelectionBox.x, SelectionBox.y);
        //                    mouseEvent.Use();
        //                }
        //            }
        //            break;

        //        case MouseButton.Right:
        //            rightClickDown = mouseEvent.mousePosition;
        //            mouseEvent.Use();
        //            break;
        //    }
        //}

        //protected virtual void OnMouseDrag(Event mouseEvent)
        //{
        //    var draggingWindow = false;
        //    switch (mouseEvent.button)
        //    {
        //        case MouseButton.Left:
        //            // Block dragging
        //            if (dragBlock != null)
        //            {
        //                for (int i = 0; i < flowchart.SelectedBlocks.Count; ++i)
        //                {
        //                    var block = flowchart.SelectedBlocks[i];
        //                    var tempRect = block._NodeRect;
        //                    tempRect.position += mouseEvent.delta / flowchart.Zoom;
        //                    block._NodeRect = tempRect;
        //                }

        //                hasDraggedSelected = true;
        //                mouseEvent.Use();
        //            }
        //            // Pan tool or alt + left click
        //            else if (UnityEditor.Tools.current == Tool.View && UnityEditor.Tools.viewTool == ViewTool.Pan || mouseEvent.alt)
        //            {
        //                draggingWindow = true;
        //            }
        //            else if (UnityEditor.Tools.current == Tool.View && UnityEditor.Tools.viewTool == ViewTool.Zoom)
        //            {
        //                DoZoom(-mouseEvent.delta.y * 0.01f, Vector2.one * 0.5f);
        //                mouseEvent.Use();
        //            }
        //            // Selection box
        //            else if (startSelectionBoxPosition.x >= 0 && startSelectionBoxPosition.y >= 0)
        //            {
        //                if (Mathf.Approximately(mouseEvent.delta.magnitude, 0))
        //                    break;

        //                var topLeft = Vector2.Min(startSelectionBoxPosition, mouseEvent.mousePosition);
        //                var bottomRight = Vector2.Max(startSelectionBoxPosition, mouseEvent.mousePosition);
        //                SelectionBox = Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);

        //                Rect zoomSelectionBox = SelectionBox;
        //                zoomSelectionBox.position -= flowchart.ScrollPos * flowchart.Zoom;
        //                zoomSelectionBox.position /= flowchart.Zoom;
        //                zoomSelectionBox.size /= flowchart.Zoom;


        //                for (int i = 0; i < blocks.Count; ++i)
        //                {
        //                    var block = blocks[i];
        //                    var doesMarqueOverlap = zoomSelectionBox.Overlaps(block._NodeRect);
        //                    if (doesMarqueOverlap)
        //                    {
        //                        flowchart.AddSelectedBlock(block);
        //                    }
        //                    else
        //                    {
        //                        flowchart.DeselectBlockNoCheck(block);
        //                    }
        //                }

        //                mouseEvent.Use();
        //            }
        //            break;

        //        case MouseButton.Right:
        //            if (Vector2.Distance(rightClickDown, mouseEvent.mousePosition) > RightClickTolerance)
        //            {
        //                rightClickDown = -Vector2.one;
        //            }
        //            draggingWindow = true;
        //            break;

        //        case MouseButton.Middle:
        //            draggingWindow = true;
        //            break;
        //    }

        //    if (draggingWindow)
        //    {
        //        flowchart.ScrollPos += mouseEvent.delta / flowchart.Zoom;
        //        mouseEvent.Use();
        //    }
        //}

        //protected virtual void OnRawMouseUp(Event mouseEvent)
        //{
        //    var hitBlock = GetBlockAtPoint(mouseEvent.mousePosition);

        //    // Convert Ctrl+Left click to a right click on mac
        //    if (Application.platform == RuntimePlatform.OSXEditor)
        //    {
        //        if (mouseEvent.button == MouseButton.Left &&
        //            mouseEvent.control)
        //        {
        //            mouseEvent.button = MouseButton.Right;
        //        }
        //    }

        //    switch (mouseEvent.button)
        //    {
        //        case MouseButton.Left:
        //            if (didDoubleClick)
        //            {
        //                didDoubleClick = false;
        //                return;
        //            }


        //            if (dragBlock != null)
        //            {
        //                for (int i = 0; i < flowchart.SelectedBlocks.Count; ++i)
        //                {
        //                    var block = flowchart.SelectedBlocks[i];
        //                    var tempRect = block._NodeRect;
        //                    var distance = mouseEvent.mousePosition / flowchart.Zoom - flowchart.ScrollPos - startDragPosition;
        //                    tempRect.position -= distance;
        //                    block._NodeRect = tempRect;
        //                    Undo.RecordObject(block, "Block Position");
        //                    tempRect.position += distance;
        //                    block._NodeRect = tempRect;
        //                    if (AmanitaEditorPreferences.useGridSnap)
        //                    {
        //                        block._NodeRect = block._NodeRect.SnapPosition(GridObjectSnap);
        //                    }
        //                    currentContext.Window.Repaint();
        //                }

        //                dragBlock = null;
        //            }

        //            // Check to see if selection actually changed?
        //            if (SelectionBox.size.x > 0 && SelectionBox.size.y > 0)
        //            {
        //                Undo.RecordObject(flowchart, "Select");
        //                flowchart.UpdateSelectedCache();

        //                EndControlSelection();
        //                //if ctrl down push them immediately back into mouse down
        //                if (GetAppendModifierDown())
        //                    StartControlSelection();

        //                Window.Repaint();

        //                if (flowchart.SelectedBlock != null)
        //                {
        //                    SetBlockForInspector(flowchart, flowchart.SelectedBlock);
        //                }
        //                Window.Repaint();
        //            }
        //            else
        //            {
        //                if (!GetAppendModifierDown() && !hasDraggedSelected)
        //                {
        //                    DeselectAll();

        //                    if (hitBlock != null)
        //                    {
        //                        SelectBlock(hitBlock);
        //                    }
        //                }
        //            }

        //            hasDraggedSelected = false;
        //            break;

        //        case MouseButton.Right:
        //            if (rightClickDown != -Vector2.one)
        //            {
        //                var menu = new GenericMenu();
        //                var mousePosition = rightClickDown;

        //                // Clicked on a block
        //                if (hitBlock != null)
        //                {
        //                    flowchart.AddSelectedBlock(hitBlock);

        //                    // Use a copy because flowchart.SelectedBlocks gets modified
        //                    var blockList = new List<Block>(flowchart.SelectedBlocks);
        //                    menu.AddItem(new GUIContent("Copy"), false, () => Copy());
        //                    menu.AddItem(new GUIContent("Cut"), false, () => Cut());
        //                    menu.AddItem(new GUIContent("Duplicate"), false, () => Duplicate());
        //                    menu.AddItem(new GUIContent("Delete"), false, () => AddToDeleteList(blockList));
        //                    menu.AddSeparator("");
        //                    if (Application.isPlaying)
        //                    {
        //                        menu.AddItem(new GUIContent("StopAll"), false, () => StopAllBlocks());
        //                        menu.AddItem(new GUIContent("Stop"), false, () => StopThisBlock(hitBlock));
        //                        menu.AddItem(new GUIContent("Execute"), false, () => ExecuteThisBlock(hitBlock, false));
        //                        menu.AddItem(new GUIContent("Execute (Stop All First)"), false, () => ExecuteThisBlock(hitBlock, true));
        //                    }
        //                    else
        //                    {
        //                        menu.AddDisabledItem(new GUIContent("StopAll"));//, false), () => StopAllBlocks());
        //                        menu.AddDisabledItem(new GUIContent("Stop"));//, false);, () => StopThisBlock(hitBlock));
        //                        menu.AddDisabledItem(new GUIContent("Execute"));//, false);, () => ExecuteThisBlock(hitBlock, false));
        //                        menu.AddDisabledItem(new GUIContent("Execute (Stop All First)"));//, false);, () => ExecuteThisBlock(hitBlock, true));
        //                    }
        //                }
        //                else
        //                {
        //                    DeselectAll();

        //                    menu.AddItem(new GUIContent("Add Block"), false, () => CreateBlock(flowchart, mousePosition / flowchart.Zoom - flowchart.ScrollPos));

        //                    if (copyList.Count > 0)
        //                    {
        //                        menu.AddItem(new GUIContent("Paste"), false, () => Paste(mousePosition));
        //                    }
        //                    else
        //                    {
        //                        menu.AddDisabledItem(new GUIContent("Paste"));
        //                    }

        //                    menu.AddSeparator("");
        //                    if (Application.isPlaying)
        //                    {
        //                        menu.AddItem(new GUIContent("StopAll"), false, () => StopAllBlocks());
        //                    }
        //                    else
        //                    {
        //                        menu.AddDisabledItem(new GUIContent("StopAll"));//, false);, () => StopAllBlocks());
        //                    }
        //                }

        //                var menuRect = new Rect();
        //                menuRect.position = new Vector2(mousePosition.x, mousePosition.y - 12f);
        //                menu.DropDown(menuRect);
        //                mouseEvent.Use();
        //            }
        //            break;
        //    }

        //    // Selection box
        //    Rect alteredBox = SelectionBox;

        //    alteredBox.size = Vector2.zero;
        //    alteredBox.position = -Vector2.one;
        //    startSelectionBoxPosition = alteredBox.position;
        //    SelectionBox = alteredBox;
        //}

        //protected void StartControlSelection()
        //{
        //    mouseDownSelectionState.AddRange(flowchart.SelectedBlocks);
        //    flowchart.ClearSelectedBlocks();
        //    for (int i = 0; i < mouseDownSelectionState.Count; i++)
        //    {
        //        if (mouseDownSelectionState[i] != null)
        //        {
        //            mouseDownSelectionState[i].IsControlSelected = true;
        //        }
        //        else
        //        {
        //            Debug.LogWarning("Null block found in mouseDownSelectionState. May be a symptom of an underlying issue");
        //        }
        //    }
        //}


        //protected virtual void OnScrollWheel(Event e)
        //{
        //    if (SelectionBox.size == Vector2.zero)
        //    {
        //        Vector2 zoomCenter;
        //        zoomCenter.x = e.mousePosition.x / flowchart.Zoom / position.width;
        //        zoomCenter.y = e.mousePosition.y / flowchart.Zoom / position.height;
        //        zoomCenter *= flowchart.Zoom;

        //        DoZoom(-e.delta.y * 0.01f, zoomCenter);
        //        e.Use();
        //    }
        //}

        //public Block CreateBlock(Flowchart flowchart, Vector2 position)
        //{
        //    Block newBlock = flowchart.CreateBlock(position);
        //    UpdateBlockCollection();
        //    Undo.RegisterCreatedObjectUndo(newBlock, "New Block");

        //    // Use AddSelected instead of Select for when multiple blocks are duplicated
        //    flowchart.AddSelectedBlock(newBlock);
        //    SetBlockForInspector(flowchart, newBlock);

        //    return newBlock;
        //}

        //protected void UpdateBlockCollection()
        //{
        //    GetFlowchart();
        //    if (FcSelected == null)
        //    {
        //        blocks = new Block[0];
        //        filteredBlocks.Clear();
        //    }
        //    else
        //    {
        //        blocks = FcSelected.GetComponents<Block>();
        //    }
        //    filterStale = true;
        //    UpdateFilteredBlocks();
        //}

        //protected void UpdateFilteredBlocks()
        //{
        //    // Recompute the filtered list and block.FilterState in one call
        //    filteredBlocks = FilterUtils.FilterBlocks(blocks, SearchString);

        //    // Keep popup‐selection index in range
        //    int max = Mathf.Max(filteredBlocks.Count - 1, 0);
        //    blockPopupSelection = Mathf.Clamp(blockPopupSelection, 0, max);
        //}

        //protected int blockPopupSelection = -1;


        //public virtual Flowchart GetFlowchart() => FlowchartWindow.GetFlowchart();
        //public virtual Flowchart FcSelected => GetFlowchart();

        //protected virtual void SelectBlock(Block block)
        //{
        //    // Select the block and also select currently executing command
        //    flowchart.SelectedBlock = block;
        //    SetBlockForInspector(flowchart, block);
        //}

        //protected void RemoveMouseDownSelectionState(Block item)
        //{
        //    mouseDownSelectionState.Remove(item);
        //    item.IsControlSelected = false;
        //}

        //protected void AddMouseDownSelectionState(Block item)
        //{
        //    mouseDownSelectionState.Add(item);
        //    item.IsControlSelected = true;
        //}

        //protected static void SetBlockForInspector(Flowchart flowchart, Block block)
        //{
        //    ShowBlockInspector(flowchart);
        //    flowchart.ClearSelectedCommands();
        //    if (block.ActiveCommand != null)
        //    {
        //        flowchart.AddSelectedCommand(block.ActiveCommand);
        //    }
        //}

        //protected static void ShowBlockInspector(Flowchart flowchart)
        //{
        //    if (blockInspector == null)
        //    {
        //        // Create a Scriptable Object with a custom editor which we can use to inspect the selected block.
        //        // Editors for Scriptable Objects display using the full height of the inspector window.
        //        blockInspector = ScriptableObject.CreateInstance<BlockInspector>() as BlockInspector;
        //        blockInspector.hideFlags = HideFlags.DontSave;
        //    }

        //    Selection.activeObject = blockInspector;

        //    EditorUtility.SetDirty(blockInspector);
        //}

        //protected static BlockInspector blockInspector;

        //protected Block GetBlockAtPoint(Vector2 point)
        //{
        //    for (int i = blocks.Count - 1; i > -1; --i)
        //    {
        //        var block = blocks[i];
        //        var rect = block._NodeRect;
        //        rect.position += flowchart.ScrollPos;

        //        if (rect.Contains(point / flowchart.Zoom))
        //        {
        //            return block;
        //        }
        //    }

        //    return null;
        //}

        //protected virtual void CenterBlock(Block block)
        //{
        //    if (flowchart.Zoom < 1)
        //    {
        //        DoZoom(1 - flowchart.Zoom, Vector2.one * 0.5f);
        //    }

        //    flowchart.ScrollPos = -block._NodeRect.center + position.size * 0.5f / flowchart.Zoom;
        //}

        //protected virtual bool GetAppendModifierDown()
        //{
        //    return (Event.current != null && Event.current.shift) || EditorGUI.actionKey;
        //}

        //protected virtual void DeselectAll()
        //{
        //    Undo.RecordObject(flowchart, "Deselect");
        //    flowchart.ClearSelectedCommands();
        //    EndControlSelection();
        //    flowchart.ClearSelectedBlocks();
        //    Selection.activeGameObject = flowchart.gameObject;
        //}

        //protected virtual void DoZoom(float delta, Vector2 center)
        //{
        //    var prevZoom = flowchart.Zoom;
        //    flowchart.Zoom += delta;
        //    flowchart.Zoom = Mathf.Clamp(flowchart.Zoom, MinZoomValue, MaxZoomValue);
        //    var deltaSize = position.size / prevZoom - position.size / flowchart.Zoom;
        //    var offset = -Vector2.Scale(deltaSize, center);
        //    flowchart.ScrollPos += offset;
        //    ForceRepaintCount = 1;
        //}

        //protected void EndControlSelection()
        //{
        //    //we can be called either by mouse up with control still held or because ctrl was released
        //    if (GetAppendModifierDown())
        //    {
        //        //remove items selected from the mouse down and then move the mouse down to the selection
        //        for (int i = mouseDownSelectionState.Count - 1; i >= 0; i--)
        //        {
        //            var item = mouseDownSelectionState[i];

        //            if (item.IsSelected)
        //            {
        //                flowchart.DeselectBlockNoCheck(item);
        //                RemoveMouseDownSelectionState(item);
        //            }
        //            else
        //            {
        //                flowchart.AddSelectedBlock(item);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //ctrl released moves all back to selection
        //        for (int i = mouseDownSelectionState.Count - 1; i >= 0; i--)
        //        {
        //            var item = mouseDownSelectionState[i];
        //            flowchart.AddSelectedBlock(item);
        //            RemoveMouseDownSelectionState(item);
        //        }
        //    }
        //}

        //protected virtual void Cut()
        //{
        //    Copy();
        //    Undo.RecordObject(flowchart, "Cut");
        //    AddToDeleteList(flowchart.SelectedBlocks);
        //}

        //protected void AddToDeleteList(IList<Block> blocks)
        //{
        //    for (int i = 0; i < blocks.Count; ++i)
        //    {
        //        FlowchartWindow.deleteList.Add(blocks[i]);
        //    }
        //}

        //// Center is position in unscaled window space
        //protected virtual void Paste(Vector2 center, bool relative = false)
        //{
        //    Undo.RecordObject(flowchart, "Deselect");
        //    DeselectAll();

        //    var pasteList = new List<Block>();

        //    foreach (var copy in copyList)
        //    {
        //        pasteList.Add(copy.PasteBlock(this, flowchart));
        //    }

        //    var copiedCenter = GetBlockCenter(pasteList.ToArray()) + flowchart.ScrollPos;
        //    var delta = relative ? center : (center / flowchart.Zoom - copiedCenter);

        //    foreach (var block in pasteList)
        //    {
        //        var tempRect = block._NodeRect;
        //        tempRect.position += delta;
        //        block._NodeRect = tempRect;
        //    }

        //    UpdateBlockCollection();
        //}

        //public virtual Vector2 GetBlockCenter(IList<Block> blocks)
        //{
        //    if (blocks.Count == 0)
        //    {
        //        return Vector2.zero;
        //    }

        //    Vector2 min = blocks[0]._NodeRect.min;
        //    Vector2 max = blocks[0]._NodeRect.max;

        //    for (int i = 0; i < blocks.Count; ++i)
        //    {
        //        var block = blocks[i];
        //        min.x = Mathf.Min(min.x, block._NodeRect.center.x);
        //        min.y = Mathf.Min(min.y, block._NodeRect.center.y);
        //        max.x = Mathf.Max(max.x, block._NodeRect.center.x);
        //        max.y = Mathf.Max(max.y, block._NodeRect.center.y);
        //    }

        //    return (min + max) * 0.5f;
        //}


        //internal void ExecuteThisBlock(Block block, bool stopRunningBlocks)
        //{
        //    if (stopRunningBlocks)
        //        StopAllBlocks();

        //    block.StartExecution();
        //}

        //internal void StopAllBlocks()
        //{
        //    flowchart.StopAllBlocks();
        //}

        //protected virtual void Copy()
        //{
        //    copyList.Clear();

        //    foreach (var block in flowchart.SelectedBlocks
        //        .Union(mouseDownSelectionState))
        //    {
        //        copyList.Add(new BlockCopy(block));
        //    }
        //}

        //protected virtual void Duplicate()
        //{
        //    var tempCopyList = new List<BlockCopy>(copyList);
        //    Copy();
        //    Paste(new Vector2(20, 0), true);
        //    copyList = tempCopyList;
        //}


        //#region Properties From Context
        //protected virtual bool didDoubleClick
        //{
        //    get { return currentContext.DidDoubleClick; }
        //    set { currentContext.DidDoubleClick = value; }
        //}

        //protected virtual Vector2 startDragPosition
        //{
        //    get { return currentContext.StartDragPosition; }
        //    set { currentContext.StartDragPosition = value; }
        //}

        //protected virtual IList<Block> blocks
        //{
        //    get { return currentContext.Blocks; }
        //    set { currentContext.Blocks = value; }
        //}

        //protected virtual Flowchart flowchart
        //{
        //    get { return currentContext.Flowchart; }
        //}

        //protected virtual int ForceRepaintCount
        //{
        //    get { return currentContext.ForceRepaintCount; }
        //    set { currentContext.ForceRepaintCount = value; }
        //}

        //protected virtual IList<Block> mouseDownSelectionState
        //{
        //    get { return currentContext.MouseDownSelectionState; }

        //}

        //protected virtual Rect position
        //{
        //    get { return currentContext.Position; }
        //    set { currentContext.Position = value; }
        //}

        //protected virtual Vector2 startSelectionBoxPosition
        //{
        //    get { return currentContext.StartSelectionBoxPosition; }
        //    set { currentContext.StartSelectionBoxPosition = value; }
        //}

        //protected virtual Rect SelectionBox
        //{
        //    get { return currentContext.SelectionBox; }
        //    set { currentContext.SelectionBox = value; }
        //}

        //protected virtual bool hasDraggedSelected
        //{
        //    get { return currentContext.HasDraggedSelected; }
        //    set { currentContext.HasDraggedSelected = value; }
        //}

        //protected virtual Vector2 rightClickDown
        //{
        //    get { return currentContext.RightClickDown; }
        //    set { currentContext.RightClickDown = value; }
        //}

        //protected virtual IList<BlockCopy> copyList
        //{
        //    get { return currentContext.CopyList; }
        //    set
        //    {
        //        currentContext.CopyList.Clear();
        //        currentContext.CopyList.AddRange(value);
        //    }
        //}

        //protected FlowchartWindow Window { get { return currentContext.Window; } }
        //protected virtual Block dragBlock
        //{
        //    get { return currentContext.DragBlock; }
        //    set { currentContext.DragBlock = value; }
        //}
        //#endregion
    }




}