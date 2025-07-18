using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Amanita.EditorUtils
{
    public class FlowchartWindow : EventWindow, IFlowchartHost
    {
        BlockClipboard IFlowchartHost.Clipboard => this.BlockClipboard;
        bool IFlowchartHost.HasClipboard => this.HasClipboard;
        Flowchart IFlowchartHost.Flowchart => this.currentFlowchart;
        void IFlowchartHost.CreateBlock(Flowchart fc, Vector2 p) => CreateBlock(fc, p);
        void IFlowchartHost.DeselectAll() => DeselectAll();
        void IFlowchartHost.QueueToDelete(IList<Block> bs) => QueueToDelete(bs);
        void IFlowchartHost.DeleteScheduledBlocks() => DeleteScheduledBlocks();
        void IFlowchartHost.UpdateBlockCollection() => UpdateBlockCollection();
        void IFlowchartHost.Repaint() => Repaint();


        public class ClipboardObject
        {
            internal SerializedObject serializedObject;
            internal Type type;

            internal ClipboardObject(Object obj)
            {
                serializedObject = new SerializedObject(obj);
                type = obj.GetType();
            }
        }

        public struct BlockGraphics
        {
            internal Color tint;
            internal Texture2D onTexture;
            internal Texture2D offTexture;
        }

        /// <summary>
        /// Helper class to maintain list of blocks that are currently executing when the game is running in editor
        /// </summary>
        public class ExecutingBlocks
        {
            internal List<Block> areExecuting = new List<Block>(),
                                 wereExecuting = new List<Block>(),
                                 workspace = new List<Block>();

            internal bool isChangeDetected { get; set; }

            protected float lastFade;

            internal void ProcessAllBlocks(IList<Block> blocks)
            {
                isChangeDetected = false;
                workspace.Clear();
                //cache these once as they can end up being called thousands of times per frame otherwise
                var curRealTime = Time.realtimeSinceStartup;
                var fadeTimer = curRealTime + AmanitaConstants.ExecutingIconFadeTime;
                for (int i = 0; i < blocks.Count; ++i)
                {
                    var b = blocks[i];
                    var bIsExec = b.IsExecuting();
                    if (bIsExec)
                    {
                        b.ExecutingIconTimer = fadeTimer;
                        b.ActiveCommand.ExecutingIconTimer = fadeTimer;
                        workspace.Add(b);
                    }
                }

                if (areExecuting.Count != workspace.Count || !WorkspaceMatchesExeucting())
                {
                    wereExecuting.Clear();
                    wereExecuting.AddRange(areExecuting);
                    areExecuting.Clear();
                    areExecuting.AddRange(workspace);
                    isChangeDetected = true;
                    lastFade = fadeTimer;
                }
            }

            internal bool WorkspaceMatchesExeucting()
            {
                for (int i = 0; i < areExecuting.Count; i++)
                {
                    if (areExecuting[i] != workspace[i])
                        return false;
                }
                return true;
            }

            internal bool IsAnimFadeoutNeed()
            {
                return (lastFade - Time.realtimeSinceStartup) >= 0;
            }

            internal void ClearAll()
            {
                areExecuting.Clear();
                wereExecuting.Clear();
                workspace.Clear();
                isChangeDetected = true;
                lastFade = 0;
            }
        }

        private FlowchartWindowInputHandler _inputPipeline = new FlowchartWindowInputHandler();
        public const float GridLineSpacingSize = 120;
        public const float GridObjectSnap = 20;
        public const float DefaultBlockHeight = 40;
        public const float BlockMinWidth = 60;
        public const float BlockMaxWidth = 240;
        public const float MinZoomValue = 0.25f;
        public const float MaxZoomValue = 1f;
        public const int HorizontalPad = 20;
        public const int VerticalPad = 5;
        //defines the distance between a down and up for a right click to be a click rather than a drag
        public const float RightClickTolerance = 5f;
        public const string SearchFieldName = "search";


        protected readonly Color connectionColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);

        public static IList<Block> deleteList = new List<Block>();
        protected Vector2 startDragPosition;
        protected GUIStyle nodeStyle, descriptionStyle, handlerStyle, blockSearchPopupNormalStyle, blockSearchPopupSelectedStyle;
        protected static BlockInspector blockInspector;
        protected int forceRepaintCount;

        protected Rect SelectionBox
        {
            get
            {
                if (flowchartCtx == null)
                {
                    return Rect.zero;
                }

                return flowchartCtx.SelectionBox;
            }
        }
        protected Vector2 startSelectionBoxPosition = -Vector2.one;
        protected List<Block> mouseDownSelectionState = new List<Block>();

        // Context Click occurs on MouseDown which interferes with panning
        // Track right click positions manually to show menus on MouseUp
        protected Vector2 rightClickDown = -Vector2.one;
        protected string SearchString
        {
            get
            {
                if (searchPanel != null)
                {
                    return searchPanel.Query;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        protected Rect searchRect;
        protected Rect popupRect;


        protected Vector2 popupScroll;

        protected int prevVarCount;

        protected Block dragBlock;
        protected bool hasDraggedSelected = false;

        static protected VariableListAdaptor variableListAdaptor;

        protected bool wasControl;
        protected ExecutingBlocks executingBlocks = new ExecutingBlocks();

        protected GUIStyle toolbarSearchTextFieldStyle;
        protected GUIStyle ToolbarSearchTextFieldStyle
        {
            get
            {
                if (toolbarSearchTextFieldStyle == null)
                    toolbarSearchTextFieldStyle = GUI.skin.FindStyle("ToolbarSearchTextField");

                return toolbarSearchTextFieldStyle;
            }
        }
        protected GUIStyle toolbarSearchCancelButtonStyle;
        protected bool didDoubleClick;

        protected GUIStyle ToolbarSearchCancelButtonStyle
        {
            get
            {
                if (toolbarSearchCancelButtonStyle == null)
                    toolbarSearchCancelButtonStyle = GUI.skin.FindStyle("ToolbarSeachCancelButton");

                return toolbarSearchCancelButtonStyle;
            }
        }

        [MenuItem("Tools/Fungus/Flowchart Window")]
        static void Init()
        {
            GetWindow(typeof(FlowchartWindow), false, "Flowchart");
        }

        protected virtual void OnEnable()
        {
            BlockClipboard = new BlockClipboard(this);

            PrepInputProcessors();
            void PrepInputProcessors()
            {
                _inputPipeline?.Dispose(); // Since we might have IDisposable subhandlers
                _inputPipeline = new FlowchartWindowInputHandler
                    (
                        new HitDetectionHandler(),
                        new SingleSelectionHandler(),
                        new BoxSelectionHandler(),
                        new BlockDragHandler(),
                        new PanZoomHandler(),
                        new BlockContextMenuHandler(this, new GenericMenuFactory())
                    );

            }

            addTexture = AmanitaEditorResources.AddSmall;
            addButtonContent = new GUIContent(addTexture, "Add a new block");
            connectionPointTexture = AmanitaEditorResources.ConnectionPoint;
            gridLineColor.a = EditorGUIUtility.isProSkin ? 0.5f : 0.25f;

            wantsMouseMove = true; // For hover selection in block search popup  

            currentFlowchart = GetFlowchart();

            WireUpUIToolkitControls();
            void WireUpUIToolkitControls()
            {
                searchPanel = new SearchPanel(currentFlowchart);
                searchPanel.BlockChosen += CenterBlock;
                rootVisualElement.Add(searchPanel.Root);

                // Optional: tweak its layout right here
                IStyle searchStyle = searchPanel.Root.style;
                searchStyle.position = Position.Absolute;
                searchStyle.top = 20;   // just below your toolbar
                searchStyle.right = 10;
                searchStyle.width = 200;
                searchStyle.height = 180;
            }

            UpdateBlockCollection();
            ListenForEvents();

        }

        public virtual BlockClipboard BlockClipboard { get; set; }
        public virtual bool HasClipboard => BlockClipboard != null && BlockClipboard.HasEntries;
        protected DrawGridContext drawGridCtx = new DrawGridContext();
        protected Texture2D addTexture;
        protected GUIContent addButtonContent;
        protected Texture2D connectionPointTexture;
        protected Color gridLineColor = Color.black;
        protected IList<BlockClipboardEntry> copyList = new List<BlockClipboardEntry>();

        public static Flowchart GetFlowchart()
        {
            // Using a temp hidden object to track the active Flowchart across 
            // serialization / deserialization when playing the game in the editor.

            EnsureThereIsFungusState();
            void EnsureThereIsFungusState()
            {
                if (fungusState == null)
                {
#if UNITY_6000
                    fungusState = GameObject.FindFirstObjectByType<FungusState>();
#else
                    fungusState = GameObject.FindObjectOfType<FungusState>();
#endif
                    if (fungusState == null)
                    {
                        GameObject stateHolder = new GameObject("_FungusState");
                        stateHolder.hideFlags = HideFlags.HideInHierarchy;
                        fungusState = stateHolder.AddComponent<FungusState>();
                    }
                }
            }

            FindSelectedFlowchart();
            void FindSelectedFlowchart()
            {
                GameObject selectedGo = Selection.activeGameObject;
                if (selectedGo != null)
                {
                    selectedGo.TryGetComponent(out Flowchart flowchartSelected);
                    if (flowchartSelected != null)
                    {
                        fungusState.SelectedFlowchart = flowchartSelected;
                    }
                }
            }

            DecideWhatToDoWithVarListAdaptor();
            void DecideWhatToDoWithVarListAdaptor()
            {
                if (FcSelected == null)
                {
                    variableListAdaptor = null;
                }
                else if (variableListAdaptor == null || variableListAdaptor.TargetFlowchart != FcSelected)
                {
                    var fsSO = new SerializedObject(FcSelected);
                    var varProp = fsSO.FindProperty("variables");
                    variableListAdaptor = new VariableListAdaptor(varProp, FcSelected);
                }
            }

            return fungusState.SelectedFlowchart;
        }

        protected static FungusState fungusState;

        protected static Flowchart FcSelected
        {
            get
            {
                Flowchart result = null;
                if (fungusState != null)
                {
                    result = fungusState.SelectedFlowchart;
                }

                return result;
            }
        }

        protected SearchPanel searchPanel;

        protected virtual void ListenForEvents()
        {
            EditorApplication.update += OnEditorUpdate;
            Undo.undoRedoPerformed += Undo_ForceRepaint;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            ListenForUiToolkitEvents();
        }

        protected virtual void ListenForUiToolkitEvents()
        {
            searchPanel.QueryChanged += OnSearchPanelQueryChanged;
        }

        protected virtual void OnSearchPanelQueryChanged(string newQuery)
        {
            UpdateFilteredBlocks();
            Repaint();
        }

        //cache styles here, rather than duping them for every block we may ever draw,
        // does mean any modifications made to the style when drawing must be undone as you go
        protected void InitStyles()
        {
            if (nodeStyle == null)
            {
                nodeStyle = new GUIStyle();
            }

            // All block nodes use the same GUIStyle, but with a different background
            nodeStyle.border = new RectOffset(HorizontalPad, HorizontalPad, VerticalPad, VerticalPad);
            nodeStyle.padding = nodeStyle.border;
            nodeStyle.contentOffset = Vector2.zero;
            nodeStyle.alignment = TextAnchor.MiddleCenter;
            nodeStyle.wordWrap = true;

            if (EditorStyles.helpBox != null && descriptionStyle == null)
            {
                descriptionStyle = new GUIStyle(EditorStyles.helpBox);
            }
            descriptionStyle.wordWrap = true;

            if (EditorStyles.whiteLabel != null && handlerStyle == null)
            {
                handlerStyle = new GUIStyle(EditorStyles.label);
            }
            handlerStyle.wordWrap = true;
            handlerStyle.margin.top = 0;
            handlerStyle.margin.bottom = 0;
            handlerStyle.alignment = TextAnchor.MiddleCenter;

            if (blockSearchPopupNormalStyle == null || blockSearchPopupSelectedStyle == null)
            {
                blockSearchPopupNormalStyle = new GUIStyle(GUI.skin.FindStyle("MenuItem"));
            }
            blockSearchPopupNormalStyle.padding = new RectOffset(8, 0, 0, 0);
            blockSearchPopupNormalStyle.imagePosition = ImagePosition.ImageLeft;
            blockSearchPopupSelectedStyle = new GUIStyle(blockSearchPopupNormalStyle);
            blockSearchPopupSelectedStyle.normal = blockSearchPopupSelectedStyle.hover;
            blockSearchPopupNormalStyle.hover = blockSearchPopupNormalStyle.normal;
        }

        protected virtual void OnDisable()
        {
            BlockClipboard?.Dispose();
            UnregisterCallbacks();
            CleanUpSearchPanel();
        }

        protected virtual void UnregisterCallbacks()
        {
            EditorApplication.update -= OnEditorUpdate;
            Undo.undoRedoPerformed -= Undo_ForceRepaint;
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
            UnregisterUiToolkitCallbacks();
        }

        protected virtual void UnregisterUiToolkitCallbacks()
        {
            if (searchPanel != null)
            {
                searchPanel.BlockChosen -= CenterBlock;
                searchPanel.QueryChanged -= OnSearchPanelQueryChanged;
            }
        }

        protected virtual void CleanUpSearchPanel()
        {
            if (searchPanel != null)
            {
                rootVisualElement.Remove(searchPanel.Root);
                searchPanel.Dispose();
                searchPanel = null;
            }
        }

        protected void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            // Force null so it can refresh context on the other side of the context
            currentFlowchart = null;
            prevFlowchart = null;
            blockInspector = null;
        }

        protected void Undo_ForceRepaint()
        {
            // An undo redo may have added or removed blocks, so...
            UpdateBlockCollection();
            currentFlowchart.UpdateSelectedCache();
            Repaint();
        }

        protected void OnEditorUpdate()
        {
            HandleFlowchartSelectionChange();

            if (currentFlowchart != null)
            {
                var varCount = currentFlowchart.VariableCount;
                if (varCount != prevVarCount)
                {
                    prevVarCount = varCount;
                    Repaint();
                }

                if (currentFlowchart.SelectedCommandsStale)
                {
                    currentFlowchart.SelectedCommandsStale = false;
                    Repaint();
                }

                if (CommandEditor.SelectedCommandDataStale)
                {
                    CommandEditor.SelectedCommandDataStale = false;
                    Repaint();
                }

                if (BlockEditor.SelectedBlockDataStale)
                {
                    BlockEditor.SelectedBlockDataStale = false;
                    Repaint();
                }

                if (FlowchartEditor.FlowchartDataStale)
                {
                    FlowchartEditor.FlowchartDataStale = false;
                    Repaint();
                }
            }
            else
            {
                prevVarCount = 0;
            }

            if (Application.isPlaying)
            {
                executingBlocks.ProcessAllBlocks(blocks);
                if (executingBlocks.isChangeDetected || executingBlocks.IsAnimFadeoutNeed())
                    Repaint();
            }
        }

        public virtual int BlocksQueuedToCopy => copyList.Count;

        public virtual void UpdateBlockCollection()
        {
            GetFlowchart();
            if (FcSelected == null)
            {
                blocks = new Block[0];
                filteredBlocks.Clear();
            }
            else
            {
                blocks = FcSelected.GetComponents<Block>();
            }
            filterStale = true;
            UpdateFilteredBlocks();
        }

        protected IList<Block> blocks = new Block[0];
        protected IList<Block> filteredBlocks = new List<Block>();
        protected bool filterStale = true;

        protected void UpdateFilteredBlocks()
        {
            // Recompute the filtered list and block.FilterState in one call
            filteredBlocks = FilterUtils.FilterBlocks(blocks, SearchString);

            // Keep popup‐selection index in range
            int max = Mathf.Max(filteredBlocks.Count - 1, 0);
            blockPopupSelection = Mathf.Clamp(blockPopupSelection, 0, max);
        }

        protected int blockPopupSelection = -1;

        public Flowchart currentFlowchart;
        protected Flowchart prevFlowchart;

        protected virtual void OnInspectorUpdate()
        {
            if (HandleFlowchartSelectionChange()) return;

            // Ensure the Block Inspector is always showing the currently selected block
            var flowchart = GetFlowchart();
            if (flowchart == null || AnyNullBLocks())
            {
                UpdateBlockCollection();
                Repaint();
                return;
            }

            if (Selection.activeGameObject == null &&
                flowchart.SelectedBlock != null)
            {
                if (blockInspector == null)
                {
                    ShowBlockInspector(flowchart);
                }
                blockInspector.block = (Block)flowchart.SelectedBlock;
            }

            if (forceRepaintCount != 0)
            {
                forceRepaintCount--;
                forceRepaintCount = Math.Max(0, forceRepaintCount);

                Repaint();
            }
        }

        protected bool AnyNullBLocks()
        {
            bool result = false;
            if (blocks != null)
            {
                result = (from elem in blocks
                          where elem == null
                          select elem).Any();
            }
            return result;
        }

        protected virtual void OnBecameVisible()
        {
            // Ensure that toolbar looks correct in both docked and undocked windows
            // The docked value doesn't always report correctly without the delayCall
            EditorApplication.delayCall += OnEditorAppDelayCall;
        }

        protected virtual void OnEditorAppDelayCall()
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var isDockedMethod = typeof(EditorWindow).GetProperty("docked", flags).GetGetMethod(true);
            if ((bool)isDockedMethod.Invoke(this, null))
            {
                EditorZoomArea.Offset = new Vector2(2.0f, 19.0f);
            }
            else
            {
                EditorZoomArea.Offset = new Vector2(0.0f, 22.0f);
            }
        }

        protected virtual void OnBecameInvisible()
        {
            EditorApplication.delayCall -= OnEditorAppDelayCall;
        }

        protected void StartControlSelection()
        {
            mouseDownSelectionState.AddRange(currentFlowchart.SelectedBlocks);
            currentFlowchart.ClearSelectedBlocks();
            for (int i = 0; i < mouseDownSelectionState.Count; i++)
            {
                if (mouseDownSelectionState[i] != null)
                {
                    mouseDownSelectionState[i].IsControlSelected = true;
                }
                else
                {
                    Debug.LogWarning("Null block found in mouseDownSelectionState. May be a symptom of an underlying issue");
                }
            }
        }

        protected void AddMouseDownSelectionState(Block item)
        {
            mouseDownSelectionState.Add(item);
            item.IsControlSelected = true;
        }

        protected void RemoveMouseDownSelectionState(Block item)
        {
            mouseDownSelectionState.Remove(item);
            item.IsControlSelected = false;
        }

        protected void EndControlSelection()
        {
            //we can be called either by mouse up with control still held or because ctrl was released
            if (GetAppendModifierDown())
            {
                //remove items selected from the mouse down and then move the mouse down to the selection
                for (int i = mouseDownSelectionState.Count - 1; i >= 0; i--)
                {
                    var item = mouseDownSelectionState[i];

                    if (item.IsSelected)
                    {
                        currentFlowchart.DeselectBlockNoCheck(item);
                        RemoveMouseDownSelectionState(item);
                    }
                    else
                    {
                        currentFlowchart.AddSelectedBlock(item);
                    }
                }
            }
            else
            {
                //ctrl released moves all back to selection
                for (int i = mouseDownSelectionState.Count - 1; i >= 0; i--)
                {
                    var item = mouseDownSelectionState[i];
                    currentFlowchart.AddSelectedBlock(item);
                    RemoveMouseDownSelectionState(item);
                }
            }
        }

        internal bool HandleFlowchartSelectionChange()
        {
            currentFlowchart = GetFlowchart();
            //target has changed, so clear the blockinspector
            if (currentFlowchart != prevFlowchart)
            {
                blockInspector = null;
                prevFlowchart = currentFlowchart;
                executingBlocks.ClearAll();

                UpdateBlockCollection();

                if (currentFlowchart != null)
                    currentFlowchart.ReverseUpdateSelectedCache(); //becomes reverse restore selected cache

                Repaint();
                return true;
            }
            return false;
        }

        protected FlowchartContext flowchartCtx = new FlowchartContext();

        protected virtual void OnGUI()
        {
            UpdateContexts();
            void UpdateContexts()
            {
                flowchartCtx.Window = this;
                flowchartCtx.Flowchart = currentFlowchart;
                flowchartCtx.Position = position;

                drawGridCtx.GridLineSpacingSize = GridLineSpacingSize;
                drawGridCtx.GridLineColor = gridLineColor;
            }

            ProcessInputPipeline();
            void ProcessInputPipeline()
            {
                if (_inputPipeline.Process(Event.current, flowchartCtx))
                    Event.current.Use();
            }

            if (currentFlowchart == null)
            {
                DrawNoFlowchartMessage();
                return;
            }
            void DrawNoFlowchartMessage()
            {
                GUILayout.Label("No Flowchart scene object selected");
            }

            if (HandleFlowchartSelectionChange()) return;

            InitStyles();

            DrawBackgroundAndGrid(Event.current);
            void DrawBackgroundAndGrid(Event guiEvent)
            {
                if (this.IsBeingRepainted)
                {
                    UnityEditor.Graphs.Styles.graphBackground.Draw
                    (
                      new Rect(0, 17, position.width, position.height - 17),
                      false, false, false, false
                    );
                    DrawGrid();
                }

            }

            DrawToolbarAndSearch(Event.current);
            void DrawToolbarAndSearch(Event guiEvent)
            {
                switch (guiEvent.type)
                {
                    case EventType.MouseDown:
                        // Clear search filter focus
                        if (!searchRect.Contains(guiEvent.mousePosition) && !popupRect.Contains(guiEvent.mousePosition))
                        {
                            CloseBlockPopup();
                        }

                        if (guiEvent.button == 0 && searchRect.Contains(guiEvent.mousePosition))
                        {
                            blockPopupSelection = 0;
                            popupScroll = Vector2.zero;
                        }

                        rightClickDown = -Vector2.one;
                        break;

                    case EventType.KeyDown:
                        if (GUI.GetNameOfFocusedControl() == SearchFieldName)
                        {
                            var centerBlock = false;
                            var selectBlock = false;
                            var closePopup = false;
                            var useEvent = false;

                            switch (guiEvent.keyCode)
                            {
                                case KeyCode.DownArrow:
                                    ++blockPopupSelection;
                                    centerBlock = true;
                                    useEvent = true;
                                    break;

                                case KeyCode.UpArrow:
                                    --blockPopupSelection;
                                    centerBlock = true;
                                    useEvent = true;
                                    break;

                                case KeyCode.Return:
                                    centerBlock = true;
                                    selectBlock = true;
                                    closePopup = true;
                                    useEvent = true;
                                    break;

                                case KeyCode.Escape:
                                    closePopup = true;
                                    useEvent = true;
                                    break;
                            }

                            blockPopupSelection = Mathf.Clamp(blockPopupSelection, 0, filteredBlocks.Count - 1);

                            if (centerBlock && filteredBlocks.Count > 0)
                            {
                                var block = filteredBlocks[blockPopupSelection];
                                CenterBlock(block);

                                if (selectBlock)
                                {
                                    SelectBlock(block);
                                }
                            }

                            if (closePopup)
                            {
                                CloseBlockPopup();
                            }

                            if (useEvent)
                            {
                                guiEvent.Use();
                            }
                        }
                        else if (guiEvent.keyCode == KeyCode.Escape)
                        {
                            DeselectAll();
                            guiEvent.Use();
                        }
                        else if (guiEvent.control && !wasControl)
                        {
                            StartControlSelection();
                            Repaint();
                            wasControl = true;
                        }
                        break;
                    case EventType.KeyUp:
                        if (!guiEvent.control && wasControl)
                        {
                            wasControl = false;
                            EndControlSelection();
                            Repaint();
                        }
                        break;
                }
            }

            UpdateFilteredBlocks();

            //HandleEarlyEvents(Event.current);

            // Draw blocks and connections
            DrawFlowchartView(Event.current);

            DrawSelectionBox();
            void DrawSelectionBox()
            {
                // After your _inputProcessor.Process(...) and your DrawFlowchartView(...)…
                bool thereIsBoxToDraw = SelectionBox.size != Vector2.zero;
                if (thereIsBoxToDraw && this.IsBeingRepainted)
                {
                    GUI.Box(SelectionBox, "", GUI.skin.FindStyle("SelectionRect"));
                }
            }

            // Draw toolbar, search popup, and variables window
            //  need try catch here as we are now invalidating the drawer if the target flowchart
            //      has changed which makes unity GUILayouts upset and this function appears to 
            //      actually get called partially outside our control
            try
            {
                DrawOverlay(Event.current);
            }
            catch (Exception)
            {
                //Debug.Log("Failed to draw overlay in some way");
            }

            // Handle events for custom GUI
            base.HandleEvents(Event.current);

            if (forceRepaintCount > 0)
            {
                // Redraw on next frame to get crisp refresh rate
                Repaint();
            }

            GUIUtility.ExitGUI();
        }

        public virtual void DeleteScheduledBlocks()
        {
            for (int i = 0; i < deleteList.Count; ++i)
            {
                var deleteBlock = deleteList[i];

                var commandList = deleteBlock.CommandList;
                for (int j = 0; j < commandList.Count; ++j)
                {
                    Undo.DestroyObjectImmediate(commandList[j]);
                }

                if (deleteBlock._EventHandler != null)
                {
                    Undo.DestroyObjectImmediate(deleteBlock._EventHandler);
                }

                if (deleteBlock.IsSelected)
                {
                    // Deselect
                    currentFlowchart.DeselectBlockNoCheck(deleteBlock);
                }

                Undo.DestroyObjectImmediate(deleteBlock);
            }

            if (deleteList.Count > 0)
            {
                UpdateBlockCollection();
                // Revert to showing properties for the Flowchart
                Selection.activeGameObject = currentFlowchart.gameObject;
                currentFlowchart.ClearSelectedCommands();
                Repaint();
            }

            deleteList.Clear();
        }

        protected virtual void DrawOverlay(Event guiEvent)
        {
            DrawMainToolbarGroup();
            void DrawMainToolbarGroup()
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    GUILayout.Space(2);

                    DrawAddBlockButton();
                    void DrawAddBlockButton()
                    {
                        if (GUILayout.Button(addButtonContent, EditorStyles.toolbarButton))
                        {
                            DeselectAll();
                            Vector2 newNodePosition = new Vector2(
                                50 / currentFlowchart.Zoom - currentFlowchart.ScrollPos.x, 50 / currentFlowchart.Zoom - currentFlowchart.ScrollPos.y
                            );
                            CreateBlock(currentFlowchart, newNodePosition);
                            UpdateBlockCollection();
                        }
                    }

                    GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.Width(8)); // Separator

                    DrawScalePanel();
                    void DrawScalePanel()
                    {
                        // Draw scale bar and labels
                        GUILayout.Label("Scale", EditorStyles.miniLabel);
                        var newZoom = GUILayout.HorizontalSlider(
                            currentFlowchart.Zoom, MinZoomValue, MaxZoomValue, GUILayout.MinWidth(40), GUILayout.MaxWidth(100)
                        );
                        GUILayout.Label(currentFlowchart.Zoom.ToString("0.0#x"), EditorStyles.miniLabel, GUILayout.Width(30));
                    }

                    DrawCenterButton();
                    void DrawCenterButton()
                    {
                        if (GUILayout.Button("Center", EditorStyles.toolbarButton))
                        {
                            CenterFlowchart();
                        }
                    }

                    GUILayout.FlexibleSpace();

                    GUI.SetNextControlName(string.Empty);

                    if (GUILayout.Button("", ToolbarSearchCancelButtonStyle))
                    {
                        CloseBlockPopup();
                    }

                    EatClickEventsOnToolbar();
                    void EatClickEventsOnToolbar()
                    {
                        if (guiEvent.type == EventType.MouseDown)
                        {
                            if (guiEvent.mousePosition.y < searchRect.height)
                            {
                                guiEvent.Use();
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            // Name and description group
            DrawNameAndDescGroup();
            void DrawNameAndDescGroup()
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label(currentFlowchart.name, EditorStyles.boldLabel);

                        GUILayout.Space(2);

                        if (currentFlowchart.Description.Length > 0)
                        {
                            GUILayout.Label(currentFlowchart.Description, EditorStyles.helpBox);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }

            DrawVariablesBlock(guiEvent);
        }

        public virtual void QueueToDelete(IList<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; ++i)
            {
                var target = blocks[i];
                QueueToDelete(target);
            }
        }

        public virtual void QueueToDelete(Block block)
        {
            if (block != null && !deleteList.Contains(block))
            {
                deleteList.Add(block);
            }
            else
            {
                Debug.LogWarning("Tried queueing a null Block for deletion");
            }
        }

        protected virtual void DrawVariablesBlock(Event guiEvent)
        {
            // Variables group
            const int groupWidth = 440, scrollbarWidth = 40;
            const int varListWidth = groupWidth - scrollbarWidth;
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUILayout.Width(groupWidth));
                {
                    GUILayout.FlexibleSpace();

                    currentFlowchart.VariablesScrollPos = GUILayout.BeginScrollView(currentFlowchart.VariablesScrollPos);
                    {
                        GUILayout.Space(8);
                        EditorGUI.BeginChangeCheck();

                        if (variableListAdaptor != null)
                        {
                            if (variableListAdaptor.TargetFlowchart != null)
                            {
                                variableListAdaptor.DrawVarList(varListWidth);
                            }
                            else
                            {
                                variableListAdaptor = null;
                            }
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorUtility.SetDirty(currentFlowchart);
                        }
                    }

                    GUILayout.EndScrollView();

                    EatMouseEvents();
                    void EatMouseEvents()
                    {
                        if (guiEvent.type == EventType.MouseDown)
                        {
                            Rect variableWindowRect = GUILayoutUtility.GetLastRect();
                            if (currentFlowchart.VariablesExpanded && currentFlowchart.Variables.Count > 0)
                            {
                                variableWindowRect.y -= 20;
                                variableWindowRect.height += 20;
                            }

                            if (variableWindowRect.Contains(guiEvent.mousePosition))
                            {
                                guiEvent.Use();
                            }
                        }
                    }
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        protected virtual bool IsBeingRepainted => Event.current.type == EventType.Repaint;

        protected virtual void DrawFlowchartView(Event e)
        {
            // Calc rect for script view
            Rect scriptViewRect = CalcFlowchartWindowViewRect();

            EditorZoomArea.Begin(currentFlowchart.Zoom, scriptViewRect);

            var prevCol = GUI.color;

            if (this.IsBeingRepainted)
            {
                DrawAllBlocks();
                void DrawAllBlocks()
                {
                    for (int i = 0; i < blocks.Count; ++i)
                    {
                        var block = blocks[i];
                        DrawBlock(block, scriptViewRect);
                    }
                }
            }

            DrawPlayIconsBesideExecutingBlocks();
            void DrawPlayIconsBesideExecutingBlocks()
            {
                if (Application.isPlaying)
                {
                    var emptyStyle = new GUIStyle();
                    var curRealTime = Time.realtimeSinceStartup;

                    for (int i = 0; i < blocks.Count; ++i)
                    {
                        var currentBlock = blocks[i];
                        float alpha = (currentBlock.ExecutingIconTimer - curRealTime) / AmanitaConstants.ExecutingIconFadeTime;
                        DrawExecutingBlockIcon(currentBlock, scriptViewRect, alpha, emptyStyle);
                    }
                }
            }

            GUI.color = prevCol;
            EditorZoomArea.End();
        }

        protected void DrawExecutingBlockIcon(Block executingBlock, Rect scriptViewRect, float alpha, GUIStyle style)
        {
            if (alpha <= 0)
                return;

            Rect rect = new Rect(executingBlock._NodeRect);

            rect.x += currentFlowchart.ScrollPos.x - 37;
            rect.y += currentFlowchart.ScrollPos.y + 3;
            rect.width = 34;
            rect.height = 34;

            if (scriptViewRect.Overlaps(rect))
            {
                GUI.color = new Color(1f, 1f, 1f, alpha);

                if (GUI.Button(rect, AmanitaEditorResources.PlayBig, style))
                {
                    SelectBlock(executingBlock);
                }

                GUI.color = Color.white;
            }
        }

        protected Rect CalcFlowchartWindowViewRect()
        {
            return new Rect(0, 0, this.position.width / currentFlowchart.Zoom, this.position.height / currentFlowchart.Zoom);
        }

        public virtual Vector2 GetBlockCenter(IList<Block> blocks)
        {
            if (blocks.Count == 0)
            {
                return Vector2.zero;
            }

            Vector2 min = blocks[0]._NodeRect.min;
            Vector2 max = blocks[0]._NodeRect.max;

            for (int i = 0; i < blocks.Count; ++i)
            {
                var block = blocks[i];
                min.x = Mathf.Min(min.x, block._NodeRect.center.x);
                min.y = Mathf.Min(min.y, block._NodeRect.center.y);
                max.x = Mathf.Max(max.x, block._NodeRect.center.x);
                max.y = Mathf.Max(max.y, block._NodeRect.center.y);
            }

            return (min + max) * 0.5f;
        }

        protected virtual void CenterFlowchart()
        {
            UpdateBlockCollection();

            if (blocks.Count > 0)
            {
                var center = -GetBlockCenter(blocks);
                center.x += position.width * 0.5f / currentFlowchart.Zoom;
                center.y += position.height * 0.5f / currentFlowchart.Zoom;

                currentFlowchart.CenterPosition = center;
                currentFlowchart.ScrollPos = currentFlowchart.CenterPosition;
            }
        }

        protected virtual void DoZoom(float delta, Vector2 center)
        {
            var prevZoom = currentFlowchart.Zoom;
            currentFlowchart.Zoom += delta;
            currentFlowchart.Zoom = Mathf.Clamp(currentFlowchart.Zoom, MinZoomValue, MaxZoomValue);
            var deltaSize = position.size / prevZoom - position.size / currentFlowchart.Zoom;
            var offset = -Vector2.Scale(deltaSize, center);
            currentFlowchart.ScrollPos += offset;
            forceRepaintCount = 1;
        }

        protected virtual void DrawGrid()
        {
            gridDrawer.Draw(flowchartCtx, drawGridCtx);
        }

        protected FlowchartWindowDrawGrid gridDrawer = new FlowchartWindowDrawGrid();

        public virtual void SelectBlock(Block block)
        {
            // Select the block and also select currently executing command
            currentFlowchart.SelectedBlock = block;
            SetBlockForInspector(currentFlowchart, block);
        }

        public virtual void DeselectAll()
        {
            Undo.RecordObject(currentFlowchart, "Deselect");
            currentFlowchart.ClearSelectedCommands();
            EndControlSelection();
            currentFlowchart.ClearSelectedBlocks();
            Selection.activeGameObject = currentFlowchart.gameObject;
        }

        public Block CreateBlock(Flowchart flowchart, Vector2 position)
        {
            Block newBlock = flowchart.CreateBlock(position);
            UpdateBlockCollection();
            Undo.RegisterCreatedObjectUndo(newBlock, "New Block");

            // Use AddSelected instead of Select for when multiple blocks are duplicated
            flowchart.AddSelectedBlock(newBlock);
            SetBlockForInspector(flowchart, newBlock);

            return newBlock;
        }

        public Block CreateBlockSuppressSelect(Flowchart flowchart, Vector2 position)
        {
            Block newBlock = flowchart.CreateBlock(position);
            UpdateBlockCollection();
            Undo.RegisterCreatedObjectUndo(newBlock, "New Block");

            return newBlock;
        }

        //prevent every DrawConnections from allocating a new list for all of its connections
        protected List<Block> connectedBlocksWorkSpace = new List<Block>();

        protected virtual void DrawConnections(Block block)
        {
            if (block == null)
            {
                return;
            }


            bool blockIsSelected = currentFlowchart.SelectedBlock == block;


            Rect scriptViewRect = CalcFlowchartWindowViewRect();

            var commandList = block.CommandList;
            foreach (var command in commandList)
            {
                if (command == null)
                {
                    continue;
                }

                bool commandIsSelected = false;
                var selectedCommands = currentFlowchart.SelectedCommands;
                foreach (var selectedCommand in selectedCommands)
                {
                    if (selectedCommand == command)
                    {
                        commandIsSelected = true;
                        break;
                    }
                }

                bool highlight = command.IsExecuting || (blockIsSelected && commandIsSelected);

                connectedBlocksWorkSpace.Clear();
                command.GetConnectedBlocks(ref connectedBlocksWorkSpace);

                foreach (var blockB in connectedBlocksWorkSpace)
                {
                    if (blockB == null ||
                        block == blockB ||
                        !blockB.GetFlowchart().Equals(currentFlowchart))
                    {
                        continue;
                    }

                    Rect startRect = new Rect(block._NodeRect);
                    startRect.x += currentFlowchart.ScrollPos.x;
                    startRect.y += currentFlowchart.ScrollPos.y;

                    Rect endRect = new Rect(blockB._NodeRect);
                    endRect.x += currentFlowchart.ScrollPos.x;
                    endRect.y += currentFlowchart.ScrollPos.y;

                    Rect boundRect = new Rect();
                    boundRect.xMin = Mathf.Min(startRect.xMin, endRect.xMin);
                    boundRect.xMax = Mathf.Max(startRect.xMax, endRect.xMax);
                    boundRect.yMin = Mathf.Min(startRect.yMin, endRect.yMin);
                    boundRect.yMax = Mathf.Max(startRect.yMax, endRect.yMax);

                    if (boundRect.Overlaps(scriptViewRect))
                        DrawRectConnection(startRect, endRect, highlight);
                }
            }
        }

        static readonly Vector2[] pointsA = new Vector2[4];
        static readonly Vector2[] pointsB = new Vector2[4];

        //we only connect mids on sides to matching opposing middle side on other block
        protected struct IndexPair { public int a, b; public IndexPair(int a, int b) { this.a = a; this.b = b; } }
        static readonly IndexPair[] closestCornerIndexPairs = new IndexPair[]
        {
            new IndexPair(){a=0,b=3 },
            new IndexPair(){a=3,b=0 },
            new IndexPair(){a=1,b=2 },
            new IndexPair(){a=2,b=1 },
        };

        //prevent alloc in DrawAAConvexPolygon
        static readonly Vector3[] beizerWorkSpace = new Vector3[3];

        protected virtual void DrawRectConnection(Rect rectA, Rect rectB, bool highlight)
        {
            //previous method made a lot of garbage so now we reuse the same array
            pointsA[0] = new Vector2(rectA.xMin, rectA.center.y);
            pointsA[1] = new Vector2(rectA.xMin + rectA.width / 2, rectA.yMin);
            pointsA[2] = new Vector2(rectA.xMin + rectA.width / 2, rectA.yMax);
            pointsA[3] = new Vector2(rectA.xMax, rectA.center.y);

            pointsB[0] = new Vector2(rectB.xMin, rectB.center.y);
            pointsB[1] = new Vector2(rectB.xMin + rectB.width / 2, rectB.yMin);
            pointsB[2] = new Vector2(rectB.xMin + rectB.width / 2, rectB.yMax);
            pointsB[3] = new Vector2(rectB.xMax, rectB.center.y);

            Vector2 pointA = Vector2.zero;
            Vector2 pointB = Vector2.zero;
            float minDist = float.MaxValue;

            //previous method compared every point to every point
            //  we only check mathcing opposing mids
            for (int i = 0; i < closestCornerIndexPairs.Length; i++)
            {
                var a = pointsA[closestCornerIndexPairs[i].a];
                var b = pointsB[closestCornerIndexPairs[i].b];
                float d = Vector2.Distance(a, b);
                if (d < minDist)
                {
                    pointA = a;
                    pointB = b;
                    minDist = d;
                }
            }

            Color color = connectionColor;
            if (highlight)
            {
                color = Color.green;
            }

            Handles.color = color;

            // Place control based on distance between points
            // Weight the min component more so things don't get overly curvy
            var diff = pointA - pointB;
            diff.x = Mathf.Abs(diff.x);
            diff.y = Mathf.Abs(diff.y);
            var min = Mathf.Min(diff.x, diff.y);
            var max = Mathf.Max(diff.x, diff.y);
            var mod = min * 0.75f + max * 0.25f;

            // Draw bezier curve connecting blocks
            var directionA = (rectA.center - pointA).normalized;
            var directionB = (rectB.center - pointB).normalized;
            var controlA = pointA - directionA * mod * 0.67f;
            var controlB = pointB - directionB * mod * 0.67f;
            Handles.DrawBezier(pointA, pointB, controlA, controlB, color, null, 3f);

            // Draw arrow on curve
            var point = GetPointOnCurve(pointA, controlA, pointB, controlB, 0.7f);
            var direction = (GetPointOnCurve(pointA, controlA, pointB, controlB, 0.6f) - point).normalized;
            var perp = new Vector2(direction.y, -direction.x);
            //reuse same array to avoid the auto alloced one in DrawAAConvexPolygon
            beizerWorkSpace[0] = point;
            beizerWorkSpace[1] = point + direction * 10 + perp * 5;
            beizerWorkSpace[2] = point + direction * 10 - perp * 5;
            Handles.DrawAAConvexPolygon(beizerWorkSpace);

            var connectionPointA = pointA + directionA * 4f;
            var connectionRectA = new Rect(connectionPointA.x - 4f, connectionPointA.y - 4f, 8f, 8f);
            var connectionPointB = pointB + directionB * 4f;
            var connectionRectB = new Rect(connectionPointB.x - 4f, connectionPointB.y - 4f, 8f, 8f);

            GUI.DrawTexture(connectionRectA, connectionPointTexture, ScaleMode.ScaleToFit);
            GUI.DrawTexture(connectionRectB, connectionPointTexture, ScaleMode.ScaleToFit);

            Handles.color = Color.white;
        }

        protected static Vector2 GetPointOnCurve(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
        {
            float rt = 1 - t;
            float rtt = rt * t;
            return rt * rt * rt * s + 3 * rt * rtt * st + 3 * rtt * t * et + t * t * t * e;
        }

        protected static void ShowBlockInspector(Flowchart flowchart)
        {
            if (blockInspector == null)
            {
                // Create a Scriptable Object with a custom editor which we can use to inspect the selected block.
                // Editors for Scriptable Objects display using the full height of the inspector window.
                blockInspector = ScriptableObject.CreateInstance<BlockInspector>() as BlockInspector;
                blockInspector.hideFlags = HideFlags.DontSave;
            }

            Selection.activeObject = blockInspector;

            EditorUtility.SetDirty(blockInspector);
        }

        public static void SetBlockForInspector(Flowchart flowchart, Block block)
        {
            ShowBlockInspector(flowchart);
            flowchart.ClearSelectedCommands();
            if (block.ActiveCommand != null)
            {
                flowchart.AddSelectedCommand(block.ActiveCommand);
            }
        }

        /// <summary>
        /// Displays a temporary text alert in the center of the Flowchart window.
        /// </summary>
        public static void ShowNotification(string notificationText)
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(FlowchartWindow), false, "Flowchart");
            if (window != null)
            {
                window.ShowNotification(new GUIContent(notificationText));
            }
        }

        protected virtual bool GetAppendModifierDown()
        {
            return (Event.current != null && Event.current.shift) || EditorGUI.actionKey;
        }

        protected override void OnExecuteCommand(Event guiEvent)
        {
            switch (guiEvent.commandName)
            {
                case "Find":
                    blockPopupSelection = 0;
                    popupScroll = Vector2.zero;
                    EditorGUI.FocusTextInControl(SearchFieldName);
                    guiEvent.Use();
                    break;
            }
        }

        public virtual void CenterBlock(Block block)
        {
            if (currentFlowchart.Zoom < 1)
            {
                DoZoom(1 - currentFlowchart.Zoom, Vector2.one * 0.5f);
            }

            currentFlowchart.ScrollPos = -block._NodeRect.center + position.size * 0.5f / currentFlowchart.Zoom;
        }

        protected virtual void CloseBlockPopup()
        {
            GUIUtility.keyboardControl = 0;
            if (searchPanel != null)
            {
                searchPanel.Query = string.Empty;
            }
            filterStale = true;
        }

        static protected IList<Block> blockGraphicsUniqueListWorkSpace = new List<Block>();
        static protected List<Block> blockGraphicsConnectedWorkSpace = new List<Block>();
        protected virtual BlockGraphics GetBlockGraphics(Block block)
        {
            var graphics = new BlockGraphics();

            blockGraphicsUniqueListWorkSpace.Clear();
            blockGraphicsConnectedWorkSpace.Clear();
            Color defaultTint;
            if (block._EventHandler != null)
            {
                graphics.offTexture = AmanitaEditorResources.EventNodeOff;
                graphics.onTexture = AmanitaEditorResources.EventNodeOn;
                defaultTint = AmanitaConstants.DefaultEventBlockTint;
            }
            else
            {
                // Count the number of unique connections (excluding self references)
                block.GetConnectedBlocks(ref blockGraphicsConnectedWorkSpace);
                foreach (var connectedBlock in blockGraphicsConnectedWorkSpace)
                {
                    if (connectedBlock == block ||
                        blockGraphicsUniqueListWorkSpace.Contains(connectedBlock))
                    {
                        continue;
                    }
                    blockGraphicsUniqueListWorkSpace.Add(connectedBlock);
                }

                if (blockGraphicsUniqueListWorkSpace.Count > 1)
                {
                    graphics.offTexture = AmanitaEditorResources.ChoiceNodeOff;
                    graphics.onTexture = AmanitaEditorResources.ChoiceNodeOn;
                    defaultTint = AmanitaConstants.DefaultChoiceBlockTint;
                }
                else
                {
                    graphics.offTexture = AmanitaEditorResources.ProcessNodeOff;
                    graphics.onTexture = AmanitaEditorResources.ProcessNodeOn;
                    defaultTint = AmanitaConstants.DefaultProcessBlockTint;
                }
            }

            graphics.tint = (block.UseCustomTint ? block.Tint : defaultTint) * AmanitaEditorPreferences.flowchatBlockTint;

            return graphics;
        }

        protected void DrawBlock(Block block, Rect scriptViewRect)
        {
            float nodeWidthA = nodeStyle.CalcSize(new GUIContent(block.BlockName)).x + 10;

            Rect tempRect = block._NodeRect;
            tempRect.width = Mathf.Clamp(nodeWidthA, BlockMinWidth, BlockMaxWidth);
            tempRect.height = DefaultBlockHeight;
            if (AmanitaEditorPreferences.useGridSnap)
            {
                tempRect = tempRect.SnapWidth(GridObjectSnap);
            }
            block._NodeRect = tempRect;

            // Draw blocks
            var graphics = GetBlockGraphics(block);

            Rect windowRelativeRect = new Rect(block._NodeRect);
            if (AmanitaEditorPreferences.useGridSnap)
            {
                windowRelativeRect = windowRelativeRect.SnapPosition(GridObjectSnap);
            }
            windowRelativeRect.position += currentFlowchart.ScrollPos;

            //skip if outside of view
            if (scriptViewRect.Overlaps(windowRelativeRect))
            {

                var tmpNormBg = nodeStyle.normal.background;

                // Draw untinted highlight
                if (block.IsSelected && !block.IsControlSelected)
                {
                    GUI.backgroundColor = Color.white;
                    nodeStyle.normal.background = graphics.onTexture;
                    GUI.Box(windowRelativeRect, "", nodeStyle);
                    nodeStyle.normal.background = tmpNormBg;
                }

                if (block.IsControlSelected && !block.IsSelected)
                {
                    GUI.backgroundColor = Color.white;
                    nodeStyle.normal.background = graphics.onTexture;
                    var c = GUI.backgroundColor;
                    c.a = 0.5f;
                    GUI.backgroundColor = c;
                    GUI.Box(windowRelativeRect, "", nodeStyle);
                    nodeStyle.normal.background = tmpNormBg;
                }

                // Draw tinted block; ensure text is readable
                var brightness = graphics.tint.r * 0.3 + graphics.tint.g * 0.59 + graphics.tint.b * 0.11;
                var tmpNormTxtCol = nodeStyle.normal.textColor;
                nodeStyle.normal.textColor = brightness >= 0.5 ? Color.black : Color.white;

                switch (block.FilterState)
                {
                    case Block.FilteredState.Full:
                        break;
                    case Block.FilteredState.Partial:
                        graphics.tint.a *= 0.65f;
                        break;
                    case Block.FilteredState.None:
                        graphics.tint.a *= 0.2f;
                        break;
                    default:
                        break;
                }

                nodeStyle.normal.background = graphics.offTexture;
                GUI.backgroundColor = graphics.tint;
                GUI.Box(windowRelativeRect, block.BlockName, nodeStyle);

                GUI.backgroundColor = Color.white;

                if (block.Description.Length > 0)
                {
                    var content = new GUIContent(block.Description);
                    windowRelativeRect.y += windowRelativeRect.height;
                    windowRelativeRect.height = descriptionStyle.CalcHeight(content, windowRelativeRect.width);
                    GUI.Label(windowRelativeRect, content, descriptionStyle);
                }

                GUI.backgroundColor = Color.white;

                nodeStyle.normal.textColor = tmpNormTxtCol;
                nodeStyle.normal.background = tmpNormBg;

                // Draw Event Handler labels
                if (block._EventHandler != null)
                {
                    string handlerLabel = "";
                    var eventType = block._EventHandler.GetType();
                    EventHandlerInfoAttribute info = EventHandlerEditor.GetEventHandlerInfo(eventType);
                    if (info != null)
                    {
                        var obsAttr = eventType.GetCustomAttribute<System.ObsoleteAttribute>();
                        if (obsAttr != null)
                        {
                            handlerLabel = "<" + AmanitaConstants.UIPrefixForDeprecated_RichText + info.EventHandlerName + "> ";
                        }
                        else
                        {
                            handlerLabel = "<" + info.EventHandlerName + "> ";
                        }
                    }

                    Rect rect = new Rect(block._NodeRect);
                    rect.height = handlerStyle.CalcHeight(new GUIContent(handlerLabel), block._NodeRect.width);
                    rect.x += currentFlowchart.ScrollPos.x;
                    rect.y += currentFlowchart.ScrollPos.y - rect.height;

                    GUI.Label(rect, handlerLabel, handlerStyle);
                }
            }

            DrawConnections(block);
        }
    }

    
}