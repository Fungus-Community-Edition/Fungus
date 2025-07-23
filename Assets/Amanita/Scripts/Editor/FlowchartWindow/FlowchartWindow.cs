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
        
        public const float MinZoomValue = 0.25f;
        public const float MaxZoomValue = 1f;
        //defines the distance between a down and up for a right click to be a click rather than a drag
        public const string SearchFieldName = "search";

        protected readonly Color connectionColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);

        public static IList<Block> deleteList = new List<Block>();
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

        [MenuItem("Tools/Amanita/Flowchart Window")]
        static void Init()
        {
            GetWindow(typeof(FlowchartWindow), false, "Flowchart");
        }

        protected virtual void OnEnable()
        {
            _gridRenderer = new GridRenderer(new HandlesLineDrawer());
            var connectionDrawer = new ConnectionDrawer(new ConnectionGatherer());
            _connectionRenderer = new ConnectionRenderer(connectionDrawer);
            _blockRenderer = new BlockRenderer(new DefaultBlockDrawer(), new BlockGraphicsGenerator());

            Clipboard = new BlockClipboard(this);

            PrepInputProcessors();
            void PrepInputProcessors()
            {
                _inputPipeline?.Dispose(); // Since we might have IDisposable subhandlers
                _inputPipeline = new FlowchartWindowInputHandler
                    (
                        new DeleteShortcutHandler(new FcWindowBlockDeletion(),
                        KeyCode.Delete,
                        new FcWindowFocusChecker()),
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

            Flowchart = GetFlowchart();

            WireUpUIToolkitControls();
            void WireUpUIToolkitControls()
            {
                searchPanel = new SearchPanel(Flowchart);
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

        protected GridRenderer _gridRenderer;
        protected ConnectionRenderer _connectionRenderer;
        
        public virtual BlockClipboard Clipboard { get; set; }
        public virtual bool HasClipboard => Clipboard != null && Clipboard.HasEntries;
        protected DrawGridContext drawGridCtx = new DrawGridContext();
        protected DrawBlockContext _drawBlockContext = new DrawBlockContext();
        protected Texture2D addTexture;
        protected GUIContent addButtonContent;
        protected Texture2D connectionPointTexture;
        
        protected IList<BlockClipboardEntry> copyList = new List<BlockClipboardEntry>();
        protected BlockRenderer _blockRenderer;

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

        protected virtual void OnDisable()
        {
            Clipboard?.Dispose();
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
            Flowchart = null;
            prevFlowchart = null;
            blockInspector = null;
        }

        protected void Undo_ForceRepaint()
        {
            // An undo redo may have added or removed blocks, so...
            UpdateBlockCollection();
            Flowchart.UpdateSelectedCache();
            Repaint();
        }

        protected void OnEditorUpdate()
        {
            HandleFlowchartSelectionChange();

            if (Flowchart != null)
            {
                var varCount = Flowchart.VariableCount;
                if (varCount != prevVarCount)
                {
                    prevVarCount = varCount;
                    Repaint();
                }

                if (Flowchart.SelectedCommandsStale)
                {
                    Flowchart.SelectedCommandsStale = false;
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
            flowchartCtx.AllBlocks = blocks;
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

        public Flowchart Flowchart { get; set; }
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
            mouseDownSelectionState.AddRange(Flowchart.SelectedBlocks);
            Flowchart.ClearSelectedBlocks();
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
                        Flowchart.DeselectBlockNoCheck(item);
                        RemoveMouseDownSelectionState(item);
                    }
                    else
                    {
                        Flowchart.AddToSelection(item);
                    }
                }
            }
            else
            {
                //ctrl released moves all back to selection
                for (int i = mouseDownSelectionState.Count - 1; i >= 0; i--)
                {
                    var item = mouseDownSelectionState[i];
                    Flowchart.AddToSelection(item);
                    RemoveMouseDownSelectionState(item);
                }
            }
        }

        internal bool HandleFlowchartSelectionChange()
        {
            Flowchart = GetFlowchart();
            //target has changed, so clear the blockinspector
            if (Flowchart != prevFlowchart)
            {
                blockInspector = null;
                prevFlowchart = Flowchart;
                executingBlocks.ClearAll();

                UpdateBlockCollection();

                if (Flowchart != null)
                    Flowchart.ReverseUpdateSelectedCache(); //becomes reverse restore selected cache

                Repaint();
                return true;
            }
            return false;
        }

        protected FlowchartContext flowchartCtx = new FlowchartContext();

        protected NodeStyleProvider _nodeStyleProvider = new NodeStyleProvider();
        protected virtual void OnGUI()
        {
            UpdateContexts();
            void UpdateContexts()
            {
                flowchartCtx.FcHost = this;
                flowchartCtx.Flowchart = Flowchart;
                flowchartCtx.Position = position;

                drawGridCtx.GridLineSpacingSize = 120;
                drawGridCtx.GridLineColor = gridLineColor;

                _drawBlockContext.FlowchartCtx = flowchartCtx;
                _drawBlockContext.DefaultBlockHeight = 40;
                _drawBlockContext.BlockMinWidth = 60;
                _drawBlockContext.BlockMaxWidth = 240;
                _nodeStyleProvider.ProvideStylesTo(_drawBlockContext);
                _drawBlockContext.ViewRect = CalcFlowchartWindowViewRect();
            }

            ProcessInputPipeline();
            void ProcessInputPipeline()
            {
                if (_inputPipeline.Process(Event.current, flowchartCtx))
                    Event.current.Use();
            }

            if (Flowchart == null)
            {
                DrawNoFlowchartMessage();
                return;
            }
            void DrawNoFlowchartMessage()
            {
                GUILayout.Label("No Flowchart scene object selected");
            }

            if (HandleFlowchartSelectionChange()) return;

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

        protected Color gridLineColor = Color.black;

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
                    Flowchart.DeselectBlockNoCheck(deleteBlock);
                }

                Undo.DestroyObjectImmediate(deleteBlock);
            }

            if (deleteList.Count > 0)
            {
                UpdateBlockCollection();
                // Revert to showing properties for the Flowchart
                Selection.activeGameObject = Flowchart.gameObject;
                Flowchart.ClearSelectedCommands();
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
                                50 / Flowchart.Zoom - Flowchart.ScrollPos.x, 50 / Flowchart.Zoom - Flowchart.ScrollPos.y
                            );
                            CreateBlock(Flowchart, newNodePosition);
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
                            Flowchart.Zoom, MinZoomValue, MaxZoomValue, GUILayout.MinWidth(40), GUILayout.MaxWidth(100)
                        );
                        GUILayout.Label(Flowchart.Zoom.ToString("0.0#x"), EditorStyles.miniLabel, GUILayout.Width(30));
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
                        GUILayout.Label(Flowchart.name, EditorStyles.boldLabel);

                        GUILayout.Space(2);

                        if (Flowchart.Description.Length > 0)
                        {
                            GUILayout.Label(Flowchart.Description, EditorStyles.helpBox);
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

                    Debug.Log($"Flowchart variables scroll pos: {Flowchart.VariablesScrollPos}");
                    Flowchart.VariablesScrollPos = GUILayout.BeginScrollView(Flowchart.VariablesScrollPos);
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
                            EditorUtility.SetDirty(Flowchart);
                        }
                    }

                    GUILayout.EndScrollView();

                    EatMouseEvents();
                    void EatMouseEvents()
                    {
                        if (guiEvent.type == EventType.MouseDown)
                        {
                            Rect variableWindowRect = GUILayoutUtility.GetLastRect();
                            if (Flowchart.VariablesExpanded && Flowchart.Variables.Count > 0)
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

            EditorZoomArea.Begin(Flowchart.Zoom, scriptViewRect);
            flowchartCtx.Flowchart = Flowchart;
            _drawBlockContext.ViewRect = scriptViewRect;

            var prevCol = GUI.color;

            if (this.IsBeingRepainted)
            {
                DrawAllBlocksAndConnections();
                void DrawAllBlocksAndConnections()
                {
                    if (_blockRenderer == null || _connectionRenderer == null)
                    {
                        return;
                    }

                    _blockRenderer.Render(_drawBlockContext);
                    _connectionRenderer.Render(_drawBlockContext, flowchartCtx);
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

            rect.x += Flowchart.ScrollPos.x - 37;
            rect.y += Flowchart.ScrollPos.y + 3;
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
            return new Rect(0, 0, this.position.width / Flowchart.Zoom, this.position.height / Flowchart.Zoom);
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
                center.x += position.width * 0.5f / Flowchart.Zoom;
                center.y += position.height * 0.5f / Flowchart.Zoom;

                Flowchart.CenterPosition = center;
                Flowchart.ScrollPos = Flowchart.CenterPosition;
            }
        }

        protected virtual void DoZoom(float delta, Vector2 center)
        {
            var prevZoom = Flowchart.Zoom;
            Flowchart.Zoom += delta;
            Flowchart.Zoom = Mathf.Clamp(Flowchart.Zoom, MinZoomValue, MaxZoomValue);
            var deltaSize = position.size / prevZoom - position.size / Flowchart.Zoom;
            var offset = -Vector2.Scale(deltaSize, center);
            Flowchart.ScrollPos += offset;
            forceRepaintCount = 1;
        }

        protected virtual void DrawGrid()
        {
            if (_gridRenderer == null || flowchartCtx == null || drawGridCtx == null)
            {
                return;
            }

            _gridRenderer.Draw(flowchartCtx, drawGridCtx);
        }

        public virtual void SelectBlock(Block block)
        {
            // Select the block and also select currently executing command
            Flowchart.SelectedBlock = block;
            SetBlockForInspector(Flowchart, block);
        }

        public virtual void DeselectAll()
        {
            Undo.RecordObject(Flowchart, "Deselect");
            Flowchart.ClearSelectedCommands();
            EndControlSelection();
            Flowchart.ClearSelectedBlocks();
            Selection.activeGameObject = Flowchart.gameObject;
        }

        public Block CreateBlock(Flowchart flowchart, Vector2 position)
        {
            Block newBlock = flowchart.CreateBlock(position);
            UpdateBlockCollection();
            Undo.RegisterCreatedObjectUndo(newBlock, "New Block");

            // Use AddSelected instead of Select for when multiple blocks are duplicated
            flowchart.AddToSelection(newBlock);
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
            if (Flowchart.Zoom < 1)
            {
                DoZoom(1 - Flowchart.Zoom, Vector2.one * 0.5f);
            }

            Flowchart.ScrollPos = -block._NodeRect.center + position.size * 0.5f / Flowchart.Zoom;
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

    }

}