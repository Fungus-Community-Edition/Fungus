using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Amanita.EditorUtils;
using StylePos = UnityEngine.UIElements.Position;

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

        /// <summary>
        /// Helper class to maintain list of blocks that are currently executing when the game is running in editor
        /// </summary>
        public class ExecutingBlocks
        {
            public List<Block> areExecuting = new List<Block>(),
                                 wereExecuting = new List<Block>(),
                                 workspace = new List<Block>();

            public bool IsChangeDetected { get; set; }

            protected float lastFade;

            public virtual void ProcessAllBlocks(IReadOnlyCollection<Block> blocks)
            {
                IsChangeDetected = false;
                workspace.Clear();
                //cache these once as they can end up being called thousands of times per frame otherwise
                var curRealTime = Time.realtimeSinceStartup;
                var fadeTimer = curRealTime + AmanitaConstants.ExecutingIconFadeTime;
                foreach (var blockEl in blocks)
                {
                    var bIsExec = blockEl.IsExecuting();
                    if (bIsExec)
                    {
                        blockEl.ExecutingIconTimer = fadeTimer;
                        blockEl.ActiveCommand.ExecutingIconTimer = fadeTimer;
                        workspace.Add(blockEl);
                    }
                }

                if (areExecuting.Count != workspace.Count || !WorkspaceMatchesExeucting())
                {
                    wereExecuting.Clear();
                    wereExecuting.AddRange(areExecuting);
                    areExecuting.Clear();
                    areExecuting.AddRange(workspace);
                    IsChangeDetected = true;
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
                IsChangeDetected = true;
                lastFade = 0;
            }
        }

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
                if (FlowchartCtx == null)
                {
                    return Rect.zero;
                }

                return FlowchartCtx.Interaction.SelectionBox;
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


        protected bool wasControl;
        protected ExecutingBlocks executingBlocks = new ExecutingBlocks();

        protected GUIStyle toolbarSearchTextFieldStyle;
        protected GUIStyle ToolbarSearchTextFieldStyle
        {
            get
            {
                if (toolbarSearchTextFieldStyle == null)
                {
                    toolbarSearchTextFieldStyle = GUI.skin?.FindStyle("ToolbarSearchTextField")
                        ?? EditorStyles.toolbarTextField;
                }

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
                {
                    toolbarSearchCancelButtonStyle = GUI.skin?.FindStyle("ToolbarSeachCancelButton")
                        ?? EditorStyles.toolbarButton;
                }

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
            _gridLineColor.a = EditorGUIUtility.isProSkin ? 0.5f : 0.25f;

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
                searchStyle.position = StylePos.Absolute;
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

                PrepVarsComponent();
                void PrepVarsComponent()
                {
                    var varsComponent = new FcWindowVariablesComponent();
                    string pathToUxml = "UIToolkitTemplates/VariableDisplayEditor";
                    var uxml = Resources.Load<VisualTreeAsset>(pathToUxml);
                    varsComponent.VariableDisplayEditorUxml = uxml;
                    _components.Add(varsComponent);
                }

                foreach (var comp in _components)
                    comp.Initialize(this);
            }

            EditorSelectionTracker.SelectedFlowchartChanged -= HandleActiveFlowchartChanged;
            EditorSelectionTracker.SelectedFlowchartChanged += HandleActiveFlowchartChanged;

            ToggleSubs(true);
        }

        protected GridRenderer _gridRenderer;
        protected ConnectionRenderer _connectionRenderer;
        
        public virtual BlockClipboard Clipboard { get; set; }
        public virtual bool HasClipboard => Clipboard != null && Clipboard.HasEntries;
        public DrawGridContext DrawGridCtx { get; set; } = new DrawGridContext();
        public DrawBlockContext DrawBlockCtx { get; set; } = new DrawBlockContext();
        protected Texture2D addTexture;
        protected GUIContent addButtonContent;
        protected Texture2D connectionPointTexture;
        
        protected IList<BlockClipboardEntry> copyList = new List<BlockClipboardEntry>();
        protected BlockRenderer _blockRenderer;

        public static Flowchart GetFlowchart()
        {
            return EditorSelectionTracker.ResolveActiveFlowchart();
        }

        protected SearchPanel searchPanel;

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                EditorApplication.update += OnEditorUpdate;
                Undo.undoRedoPerformed += Undo_ForceRepaint;
                EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
                ListenForUiToolkitEvents();
                FlowchartWindowSignals.EmptySpaceLeftClicked += OnEmptySpaceClicked;
            }
            else
            {
                EditorApplication.update -= OnEditorUpdate;
                Undo.undoRedoPerformed -= Undo_ForceRepaint;
                EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
                UnregisterUiToolkitCallbacks();
                FlowchartWindowSignals.EmptySpaceLeftClicked -= OnEmptySpaceClicked;
            }
        }

        private void HandleActiveFlowchartChanged(Flowchart previous, Flowchart current)
        {
            Flowchart = current;
        }

        protected virtual void OnEmptySpaceClicked(PointerEventInfo info)
        {
            UpdateBlockCollection();
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
            EditorSelectionTracker.SelectedFlowchartChanged -= HandleActiveFlowchartChanged;

            Clipboard?.Dispose();
            ToggleSubs(false);
            CleanUpSearchPanel();

            for (int i = 0; i < _components.Count; i++)
            {
                var componentEl = _components[i];
                componentEl.Dispose();
            }
            _components.Clear();
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

        protected void EditorApplication_playModeStateChanged(PlayModeStateChange state)
        {
            // Force null so it can refresh context on the other side of the context
            Flowchart = null;
            _prevFlowchart = null;
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
            if (AmanitaManager.S == null)
            {
                return;
            }

            if (Flowchart == null)
            {
                Flowchart = GetFlowchart();
            }

            foreach (var comp in _components)
            {
                comp.OnEditorUpdate();
            }

            if (Application.isPlaying)
            {
                executingBlocks.ProcessAllBlocks(Blocks);
                if (executingBlocks.IsChangeDetected || executingBlocks.IsAnimFadeoutNeed())
                    Repaint();
            }
        }

        public virtual void UpdateBlockCollection()
        {
            if (Flowchart == null)
            {
                Flowchart = GetFlowchart();
            }

            Flowchart current = Flowchart;
            if (current == null)
            {
                filteredBlocks.Clear();
            }

            FlowchartCtx.Flowchart = current;
            filterStale = true;
            UpdateFilteredBlocks();
        }

        public IReadOnlyCollection<Block> Blocks
        {
            get => Flowchart != null ?
                Flowchart.Blocks :
                Array.Empty<Block>();
        }
        protected IList<Block> filteredBlocks = new List<Block>();
        protected bool filterStale = true;
        protected string cachedSearchString = string.Empty;

        protected void UpdateFilteredBlocks()
        {
            // Recompute the filtered list and block.FilterState in one call.
            // Only do this if the filter is stale or the search string has changed.
            // Otherwise, we end up doing this every frame while typing in the search box,
            // panning the screen, etc. Looots of garbage.
            bool outOfDateSearchString = !string.Equals(SearchString, cachedSearchString, StringComparison.Ordinal);
            if (filterStale || outOfDateSearchString)
            {
                cachedSearchString = SearchString;
                filteredBlocks = FilterUtils.FilterBlocks(Blocks, cachedSearchString);
                filterStale = false;
            }

            // Keep popup-selection index in range
            int max = Mathf.Max(filteredBlocks.Count - 1, 0);
            blockPopupSelection = Mathf.Clamp(blockPopupSelection, 0, max);
        }

        protected int blockPopupSelection = -1;

        public Flowchart Flowchart
        {
            get { return _flowchart; }
            set
            {
                if (!ReferenceEquals(value, _flowchart))
                {
                    _prevFlowchart = _flowchart;
                    _flowchart = value;
                    OnFlowchartChanged(_flowchart);
                }
            }
        }

        protected Flowchart _flowchart;
        protected virtual void OnFlowchartChanged(Flowchart newFlowchart)
        {
            if (_prevFlowchart != null)
            {
                _prevFlowchart.SelectedBlock = null;
            }

            executingBlocks.ClearAll();

            UpdateBlockCollection();

            if (Flowchart != null)
            {
                Flowchart.SelectedBlock = null;
                Flowchart.ReverseUpdateSelectedCache(); // becomes reverse restore selected cache
            }

            Repaint();

            FlowchartWindowSignals.ChangedFlowchart(_prevFlowchart, Flowchart);
            // ^Why here instead of the setter? So we can make sure we're the first responder
            // to the flowchart-change
        }

        protected Flowchart _prevFlowchart;

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

        public FlowchartContext FlowchartCtx { get; set; } = new FlowchartContext();

        protected NodeStyleProvider _nodeStyleProvider = new NodeStyleProvider();
        public virtual void OnGUI()
        {
            UpdateContexts();

            if (Flowchart == null)
            {
                Flowchart = GetFlowchart();
                if (Flowchart == null)
                {
                    DrawNoFlowchartMessage();
                    return;
                }
            }

            DrawToolbarAndSearch(Event.current);
            UpdateFilteredBlocks();

            foreach (var comp in _components)
                comp.OnGUI(DrawBlockCtx, FlowchartCtx);

            DrawSelectionBox();
            
            // Draw toolbar, search popup, and variables window
            //  need try catch here as we are now invalidating the drawer if the target flowchart
            //      has changed which makes unity GUILayouts upset and this function appears to 
            //      actually get called partially outside our control
            try
            {
                DrawOverlay(Event.current);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                GUIUtility.ExitGUI();
            }

            // Handle events for custom GUI
            base.HandleEvents(Event.current);

            if (forceRepaintCount > 0)
            {
                // Redraw on next frame to get crisp refresh rate
                Repaint();
            }
        }

        private void UpdateContexts()
        {
            FlowchartCtx.FcHost = this;
            FlowchartCtx.Flowchart = Flowchart;
            FlowchartCtx.Position = position;

            DrawGridCtx.GridLineSpacingSize = 120;
            DrawGridCtx.GridLineColor = GridLineColor;

            DrawBlockCtx.FlowchartCtx = FlowchartCtx;
            DrawBlockCtx.DefaultBlockHeight = 40;
            DrawBlockCtx.BlockMinWidth = 60;
            DrawBlockCtx.BlockMaxWidth = 240;
            _nodeStyleProvider.ProvideStylesTo(DrawBlockCtx);
            DrawBlockCtx.ViewRect = CalcFlowchartWindowViewRect();
        }

        void DrawNoFlowchartMessage()
        {
            GUILayout.Label("No Flowchart in the scene is selected");
        }

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
                    // This lets you change the selected block through the arrow keys,
                    // deselect everything through the Escape key, and... still trying to
                    // figure out how the Return key factors into all of this
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

        void DrawSelectionBox()
        {
            // After your _inputProcessor.Process(...) and your DrawFlowchartView(...)…
            bool thereIsBoxToDraw = SelectionBox.size != Vector2.zero;
            if (thereIsBoxToDraw && this.IsBeingRepainted)
            {
                GUI.Box(SelectionBox, "", GUI.skin.FindStyle("SelectionRect"));
            }
        }

        public virtual VisualElement RootVisualElement { get { return rootVisualElement; } }
        public virtual Rect Position { get { return position; } }
        public Color GridLineColor
        {
            get { return _gridLineColor; }
            set { _gridLineColor = value; }
        }

        protected Color _gridLineColor = Color.black;

        protected virtual void DrawOverlay(Event guiEvent)
        {
            if (Flowchart == null)
            {
                return;
            }
            DrawMainToolbarGroup();
            void DrawMainToolbarGroup()
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    GUILayout.Space(2);

                    GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.Width(8)); // Separator

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

                        if (!string.IsNullOrEmpty(Flowchart.Description))
                        {
                            GUILayout.Label(Flowchart.Description, EditorStyles.helpBox);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
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

        public virtual Vector2 GetBlockCenter(IReadOnlyCollection<Block> blocks)
        {
            if (blocks.Count == 0)
            {
                return Vector2.zero;
            }

            var firstBlock = blocks.First();
            Vector2 min = firstBlock._NodeRect.min;
            Vector2 max = firstBlock._NodeRect.max;

            foreach (var blockEl in blocks)
            {
                min.x = Mathf.Min(min.x, blockEl._NodeRect.min.x);
                min.y = Mathf.Min(min.y, blockEl._NodeRect.min.y);
                max.x = Mathf.Max(max.x, blockEl._NodeRect.max.x);
                max.y = Mathf.Max(max.y, blockEl._NodeRect.max.y);
            }

            return (min + max) * 0.5f;
        }

        public virtual void CenterFlowchart()
        {
            UpdateBlockCollection();

            if (Blocks.Count > 0)
            {
                var center = -GetBlockCenter(Blocks);
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
            //SetBlockForInspector(Flowchart, block);
        }

        public virtual void DeselectAll()
        {
            Undo.RecordObject(Flowchart, "Deselect");
            Flowchart.ClearSelectedCommands();
            EndControlSelection();
            Flowchart.ClearSelectedBlocks();

            if (Selection.activeGameObject != Flowchart.gameObject)
            {
                Selection.activeGameObject = Flowchart.gameObject;
            }
        }

        /// <summary>
        /// Only for when you want to create a single Block at a time instead of multiple at once.
        /// </summary>
        /// <returns></returns>
        public Block CreateBlock(Flowchart flowchart, Vector2 position)
        {
            Block newBlock = flowchart.CreateBlock(position);
            UpdateBlockCollection();
            Undo.RegisterCreatedObjectUndo(newBlock, "New Block");

            flowchart.AddToSelection(newBlock);
            return newBlock;
        }

        public Block CreateBlockSuppressSelect(Flowchart flowchart, Vector2 position)
        {
            Block newBlock = flowchart.CreateBlock(position);
            UpdateBlockCollection();
            Undo.RegisterCreatedObjectUndo(newBlock, "New Block");

            return newBlock;
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
