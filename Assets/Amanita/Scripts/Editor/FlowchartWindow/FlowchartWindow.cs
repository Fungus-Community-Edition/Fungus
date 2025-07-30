using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    public class FlowchartWindow : EventWindow, IFlowchartHost
    {
        // Settings for the submodules to consider
        public const float MinZoomValue = 0.25f;
        public const float MaxZoomValue = 1f;
        //defines the distance between a down and up for a right click to be a click rather than a drag
        public const string SearchFieldName = "search";

        protected readonly Color connectionColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);
        // /Settings

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
            public List<Block> areExecuting = new List<Block>(),
                                 wereExecuting = new List<Block>(),
                                 workspace = new List<Block>();

            public bool isChangeDetected { get; set; }

            protected float lastFade;

            public virtual void ProcessAllBlocks(IList<Block> blocks)
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

            public bool WorkspaceMatchesExeucting()
            {
                for (int i = 0; i < areExecuting.Count; i++)
                {
                    if (areExecuting[i] != workspace[i])
                        return false;
                }
                return true;
            }

            public bool IsAnimFadeoutNeed()
            {
                return (lastFade - Time.realtimeSinceStartup) >= 0;
            }

            public void ClearAll()
            {
                areExecuting.Clear();
                wereExecuting.Clear();
                workspace.Clear();
                isChangeDetected = true;
                lastFade = 0;
            }
        }

        public static BlockInspector blockInspector;
        protected int forceRepaintCount;
        private readonly List<IFcWindowComponent> _components = new();

        public virtual T GetComponent<T>() where T: IFcWindowComponent
        {
            return _components.OfType<T>().FirstOrDefault();
        }

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

            PrepComponents();
            void PrepComponents()
            {
                _components.Clear();
                _components.Add(new FcWindowCanvas());
                _components.Add(new FcWindowEditing());
                _components.Add(new FcWindowExecutionVisualizer());
                _components.Add(new FcWindowSelectionSync());
                _components.Add(new FcWindowVariablesComponent());
                // ^Commented this out due to the compiler errors

                foreach (var comp in _components)
                    comp.Initialize(this);
            }

            ListenForEvents();

        }

        protected GridRenderer _gridRenderer;
        protected ConnectionRenderer _connectionRenderer;
        
        public virtual BlockClipboard Clipboard { get; set; }
        public virtual bool HasClipboard => Clipboard != null && Clipboard.HasEntries;
        public DrawGridContext drawGridCtx = new DrawGridContext();
        public DrawBlockContext _drawBlockContext = new DrawBlockContext();
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
            if (Flowchart != null)
            {
                UpdateBlockCollection();
                Flowchart.UpdateSelectedCache();
            }
            Repaint();
        }

        protected void OnEditorUpdate()
        {
            foreach (var comp in _components)
                comp.OnEditorUpdate();

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

        public IList<Block> blocks = new Block[0];
        protected IList<Block> filteredBlocks = new List<Block>();
        protected bool filterStale = true;

        protected void UpdateFilteredBlocks()
        {
            // Recompute the filtered list and block.FilterState in one call
            filteredBlocks = FilterUtils.FilterBlocks(blocks, SearchString);

            // Keep popup-selection index in range
            int max = Mathf.Max(filteredBlocks.Count - 1, 0);
            blockPopupSelection = Mathf.Clamp(blockPopupSelection, 0, max);
        }

        protected int blockPopupSelection = -1;

        public Flowchart Flowchart { get; set; }
        protected Flowchart prevFlowchart;

        protected virtual void OnInspectorUpdate()
        {
            foreach (var comp in _components)
                comp.OnInspectorUpdate();

            if (forceRepaintCount != 0)
            {
                forceRepaintCount--;
                forceRepaintCount = Math.Max(0, forceRepaintCount);

                Repaint();
            }
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
                if (prevFlowchart != null)
                {
                    prevFlowchart.SelectedBlock = null;
                }
                prevFlowchart = Flowchart;
                executingBlocks.ClearAll();

                UpdateBlockCollection();

                if (Flowchart != null)
                    Flowchart.ReverseUpdateSelectedCache(); //becomes reverse restore selected cache
                //Flowchart.SelectedBlock = null;
                Repaint();
                return true;
            }
            return false;
        }

        public FlowchartContext flowchartCtx = new FlowchartContext();

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

            if (HandleFlowchartSelectionChange())
            {
                return;
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

            foreach (var comp in _components)
                comp.OnGUI(_drawBlockContext, flowchartCtx);

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

        public Color gridLineColor = Color.black;

        protected virtual void DrawOverlay(Event guiEvent)
        {
            DrawMainToolbarGroup();
            void DrawMainToolbarGroup()
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    GUILayout.Space(2);

                    GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.Width(8)); // Separator

                    //DrawCenterButton();
                    //void DrawCenterButton()
                    //{
                    //    if (GUILayout.Button("Center", EditorStyles.toolbarButton))
                    //    {
                    //        CenterFlowchart();
                    //    }
                    //}

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

            //DrawVariablesBlock(guiEvent);
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

        public Rect CalcFlowchartWindowViewRect()
        {
            if (Flowchart == null)
            {
                return Rect.zero;
            }

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

        public virtual void CenterFlowchart()
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

        public virtual void DoZoom(float delta, Vector2 center)
        {
            var prevZoom = Flowchart.Zoom;
            Flowchart.Zoom += delta;
            Flowchart.Zoom = Mathf.Clamp(Flowchart.Zoom, MinZoomValue, MaxZoomValue);
            var deltaSize = position.size / prevZoom - position.size / Flowchart.Zoom;
            var offset = -Vector2.Scale(deltaSize, center);
            Flowchart.ScrollPos += offset;
            forceRepaintCount = 1;
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
            bool wasAlreadyShowingThisBlock = blockInspector != null && blockInspector.block == block;
            ShowBlockInspector(flowchart);

            if (!wasAlreadyShowingThisBlock) 
            {
                // ^We need this check to make sure that when a Command is selected in the 
                // Inspector, it's not immediately unselected
                flowchart.ClearSelectedCommands();
            }
            
            if (block != null && block.ActiveCommand != null)
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
