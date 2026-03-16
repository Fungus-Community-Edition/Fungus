using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Centralizes editor-side knowledge of which Flowchart/Blocks/Commands are currently selected.
    /// Removes legacy AmanitaState components so selection changes are tracked exclusively here.
    /// </summary>
    [InitializeOnLoad]
    public static class EditorSelectionTracker
    {
        public static Flowchart ActiveFlowchart => activeFlowchart;
        private static Flowchart activeFlowchart;
        public static Flowchart LastActiveFlowchart
        {
            get
            {
                Debug.Log($"Using LastActiveFlowchart getter. activeFlowchart: {(activeFlowchart != null ? activeFlowchart.name : "null")}, " +
                    $"selection cache uid: {_selectionCache.LastSelectedFcUid}");
                if (activeFlowchart != null)
                {
                    return activeFlowchart;
                }

                Flowchart fromSelection = FindFlowchartFromSelection();
                if (fromSelection != null)
                {
                    Debug.Log($"Flowchart found from selection in LastActiveFlowchart getter: {fromSelection.name} (uid: {fromSelection.UniqueId})");
                    return fromSelection;
                }

                Flowchart basedOnCache = FindFlowchartWithCachedId();
                if (basedOnCache != null)
                {
                    return basedOnCache;
                }

                Flowchart inScene = FindFlowchartInScene();
                if (inScene != null)
                {
                    Debug.Log($"Flowchart found from scene in LastActiveFlowchart getter: {inScene.name} (uid: {inScene.UniqueId})");
                }
                return inScene;
            }
        }

        private static bool HasSameUidAsCache(Flowchart fc)
        {
            return fc != null && fc.UniqueId == _selectionCache.LastSelectedFcUid;
        }

        public static IReadOnlyList<Block> CurrentBlocks => blockSelection;
        private static readonly List<Block> blockSelection = new List<Block>();
        public static IReadOnlyList<Command> CurrentCommands => commandSelection;
        private static readonly List<Command> commandSelection = new List<Command>();

        /// <summary>
        /// The "primary" block is the first block in the selection, and is the one that will 
        /// be used for things like inspector display.
        /// </summary>
        public static Block PrimaryBlock { get; private set; }

        /// <summary>
        /// The "primary" command is the first command in the selection, and is the one that will 
        /// be used for things like inspector display.
        /// </summary>
        public static Command PrimaryCommand { get; private set; }

        public static event Action<IReadOnlyList<Block>> BlockSelectionChanged = delegate { };
        public static event Action<Block, Block> PrimaryBlockChanged = delegate { };
        public static event Action<IReadOnlyList<Command>> CommandSelectionChanged = delegate { };
        public static event Action<Command, Command> PrimaryCommandChanged = delegate { };

        static EditorSelectionTracker()
        {
            EnsureSelectionCache(); 
            DestroyLegacyStateInstances();
            AttemptInitialHydration();
            ToggleSubs(true);
        }

        private static void EnsureSelectionCache()
        {
            _selectionCache = SOUtils.EnsureSOExists<EditorSelectionCache>(_selectionCacheSubfolderPath, 
                _selectionCacheAssetName);
        }

        private static EditorSelectionCache _selectionCache;
        private static readonly string _selectionCacheSubfolderPath = "AtMycelia/Amanita/Editor";
        private static readonly string _selectionCacheAssetName = "EditorSelectionCache";

        private static void SelectFlowchartBasedOnCache()
        {
            if (string.IsNullOrEmpty(_selectionCache.LastSelectedFcUid))
            {
                return;
            }
            Flowchart toSelect = FindFlowchartWithCachedId();
            if (toSelect != null)
            {
                Debug.Log($"Selecting flowchart based on cache: {toSelect.name} (uid: {toSelect.UniqueId})");
                SetActiveFlowchart(toSelect);
            }
        }

        private static Flowchart FindFlowchartWithCachedId()
        {
            if (string.IsNullOrEmpty(_selectionCache.LastSelectedFcUid))
            {
                return null;
            }

            Flowchart[] allInScene = UnityObj.FindObjectsByType<Flowchart>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            Flowchart result = allInScene.Where(HasSameUidAsCache).FirstOrDefault();
            return result;
        }

        private static void DestroyLegacyStateInstances()
        {
            AmanitaState[] legacyStates = UnityObj.FindObjectsByType<AmanitaState>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (AmanitaState state in legacyStates)
            {
                if (state == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    UnityObj.Destroy(state);
                }
                else
                {
                    UnityObj.DestroyImmediate(state);
                }
            }
        }

        private static void AttemptInitialHydration()
        {
            Flowchart flowchart = FindFlowchartWithCachedId();
            if (flowchart != null)
            {
                Debug.Log($"Flowchart found from cache during initial hydration: {flowchart.name} (uid: {flowchart.UniqueId})");
            }
            if (flowchart == null)
            {
                flowchart = FindFlowchartFromSelection();
                if (flowchart != null)
                {
                    Debug.Log($"Flowchart found from selection during initial hydration: {flowchart.name} (uid: {flowchart.UniqueId})");
                }
            }
            if (flowchart == null)
            {
                flowchart = FindFlowchartInScene();
                if (flowchart != null)
                {
                    Debug.Log($"Flowchart found from scene during initial hydration: {flowchart.name} (uid: {flowchart.UniqueId})");
                }
            }

            if (flowchart != null)
            {
                SetActiveFlowchart(flowchart);
            }
        }

        private static Flowchart FindFlowchartFromSelection()
        {
            GameObject activeObject = Selection.activeGameObject;
            if (activeObject == null)
            {
                return null;
            }

            activeObject.TryGetComponent(out Flowchart selected);
            if (selected != null)
            {
                Debug.Log($"Flowchart found from selection: {selected.name} (uid: {selected.UniqueId})");
            }
            return selected;
        }

        private static Flowchart FindFlowchartInScene()
        {
            return UnityObj.FindFirstObjectByType<Flowchart>(FindObjectsInactive.Include);
        }

        private static void SetActiveFlowchart(Flowchart flowchart)
        {
            bool alreadySelected = ReferenceEquals(activeFlowchart, flowchart) ||
                (flowchart != null && flowchart.UniqueId == _selectionCache.LastSelectedFcUid);
            if (alreadySelected)
            {
                return;
            }

            Flowchart previous = activeFlowchart;
            activeFlowchart = flowchart;
            if (activeFlowchart != null)
            {
                Debug.Log($"Selected flowchart: {activeFlowchart.name} (uid: {activeFlowchart.UniqueId})");
            }
            UpdateSelectionCache(flowchart);
            SyncSelectionsFromFlowchart(flowchart);
            SelectedFlowchartChanged(previous, flowchart);
        }

        private static void UpdateSelectionCache(Flowchart flowchart)
        {
            _selectionCache.LastSelectedFcUid = flowchart != null ?
                flowchart.UniqueId :
                string.Empty;
        }
        private static void SyncSelectionsFromFlowchart(Flowchart flowchart)
        {
            SyncBlockSelectionFromFlowchart(flowchart);
            SyncCommandSelectionFromFlowchart(flowchart);
        }

        private static void SyncBlockSelectionFromFlowchart(Flowchart flowchart)
        {
            var toReplaceWith = flowchart != null ?
                flowchart.SelectedBlocks :
                null;
            ReplaceBlockSelection(toReplaceWith);
        }

        private static void ReplaceBlockSelection(IEnumerable<Block> toReplaceWith)
        {
            blockSelection.Clear();
            if (toReplaceWith != null)
            {
                foreach (Block block in toReplaceWith)
                {
                    if (block != null)
                    {
                        blockSelection.Add(block);
                    }
                }
            }

            Block previous = PrimaryBlock;
            PrimaryBlock = blockSelection.Count > 0 ?
                blockSelection[0] :
                null;

            BlockSelectionChanged(blockSelection);
            if (!ReferenceEquals(previous, PrimaryBlock))
            {
                PrimaryBlockChanged(previous, PrimaryBlock);
            }
        }

        private static void SyncCommandSelectionFromFlowchart(Flowchart flowchart)
        {
            var toReplaceWith = flowchart != null ?
                flowchart.SelectedCommands :
                null;
            ReplaceCommandSelection(toReplaceWith);
        }

        private static void ReplaceCommandSelection(IEnumerable<Command> toReplaceWith)
        {
            commandSelection.Clear();
            if (toReplaceWith != null)
            {
                foreach (Command cmd in toReplaceWith)
                {
                    if (cmd != null)
                    {
                        commandSelection.Add(cmd);
                    }
                }
            }

            Command previous = PrimaryCommand;
            PrimaryCommand = commandSelection.Count > 0 ?
                commandSelection[0] :
                null;

            CommandSelectionChanged(commandSelection);
            if (!ReferenceEquals(previous, PrimaryCommand))
            {
                PrimaryCommandChanged(previous, PrimaryCommand);
            }
        }

        private static void ToggleSubs(bool on)
        {
            if (on)
            {
                Selection.selectionChanged += OnUnitySelectionChanged;

                BlockSignals.BlockSelected += OnBlockSelected;
                BlockSignals.BlockDeselected += OnBlockRemovedFromSelection;
                BlockSignals.MultiBlocksSelected += OnMultiBlocksSelected;

                FlowchartWindowSignals.EmptySpaceLeftClicked += OnEmptySpaceClicked;

                CommandSignals.CommandSelected += OnCommandSelected;

                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
                EditorApplication.quitting += Cleanup;
            }
            else
            {
                Selection.selectionChanged -= OnUnitySelectionChanged;

                BlockSignals.BlockSelected -= OnBlockSelected;
                BlockSignals.BlockDeselected -= OnBlockRemovedFromSelection;
                BlockSignals.MultiBlocksSelected -= OnMultiBlocksSelected;

                FlowchartWindowSignals.EmptySpaceLeftClicked -= OnEmptySpaceClicked;

                CommandSignals.CommandSelected -= OnCommandSelected;

                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
                EditorApplication.quitting -= Cleanup;
            }
        }

        private static void OnUnitySelectionChanged()
        {
            GameObject activeObject = Selection.activeGameObject;
            if (activeObject == null)
            {
                return;
            }

            if (activeObject.TryGetComponent(out Flowchart selected))
            {
                SetActiveFlowchart(selected);
            }
        }

        private static void OnBlockSelected(Block block)
        {
            Flowchart flowchart = block != null ?
                block.GetFlowchart() :
                null;
            if (flowchart != null)
            {
                SetActiveFlowchart(flowchart);
            }

            Flowchart toSyncFrom = flowchart != null ?
                flowchart :
                activeFlowchart;
            SyncBlockSelectionFromFlowchart(toSyncFrom);
        }

        private static void OnBlockRemovedFromSelection(Block block)
        {
            Flowchart flowchart = block != null ?
                block.GetFlowchart() :
                activeFlowchart;

            SyncBlockSelectionFromFlowchart(flowchart);
        }

        private static void OnMultiBlocksSelected(IList<Block> blocks)
        {
            Flowchart flowchart = null;
            if (blocks != null && blocks.Count > 0)
            {
                Block first = blocks[0];
                if (first != null)
                {
                    flowchart = first.GetFlowchart();
                }
            }

            if (flowchart != null)
            {
                SetActiveFlowchart(flowchart);
            }

            ReplaceBlockSelection(blocks);
        }

        private static void OnEmptySpaceClicked(PointerEventInfo _)
        {
            ClearBlockSelectionInternal();
            ClearCommandSelectionInternal();
        }

        private static void ClearBlockSelectionInternal()
        {
            if (blockSelection.Count == 0 && PrimaryBlock == null)
            {
                return;
            }

            blockSelection.Clear();
            Block previous = PrimaryBlock;
            PrimaryBlock = null;

            BlockSelectionChanged(blockSelection);
            if (previous != null)
            {
                PrimaryBlockChanged(previous, null);
            }
        }

        private static void ClearCommandSelectionInternal()
        {
            if (commandSelection.Count == 0 && PrimaryCommand == null)
            {
                return;
            }

            commandSelection.Clear();
            Command previous = PrimaryCommand;
            PrimaryCommand = null;

            CommandSelectionChanged(commandSelection);
            if (previous != null)
            {
                PrimaryCommandChanged(previous, null);
            }
        }

        /// <summary>
        /// Raised when the user selects a different Flowchart-having GameObject than before.
        /// Params: previous Flowchart, new Flowchart
        /// </summary>
        public static event Action<Flowchart, Flowchart> SelectedFlowchartChanged = delegate { };

        public static Flowchart ResolveActiveFlowchart(bool attemptSceneFallback = true)
        {
            if (activeFlowchart != null)
            {
                return activeFlowchart;
            }

            Flowchart fromSelection = FindFlowchartFromSelection();
            if (fromSelection != null)
            {
                activeFlowchart = fromSelection; 
                // Not going with the method here, for the sake of avoiding more signaling than needed
                return fromSelection;
            }

            Flowchart basedOnCache = FindFlowchartWithCachedId();

            if (basedOnCache != null)
            {
                activeFlowchart = basedOnCache;
                return basedOnCache;
            }

            if (attemptSceneFallback)
            {
                Flowchart fallback = UnityObj.FindFirstObjectByType<Flowchart>(FindObjectsInactive.Include);
                return fallback;
            }

            return null;
        }

        private static void OnCommandSelected(Command command)
        {
            Flowchart flowchart = command != null ? 
                command.GetFlowchart() : 
                null;
            if (flowchart != null)
            {
                SetActiveFlowchart(flowchart);
            }

            SyncCommandSelectionFromFlowchart(flowchart ?? activeFlowchart);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            UpdateSelectionCacheAssetState();
            SelectFlowchartBasedOnCache();
        }

        private static void OnBeforeAssemblyReload()
        {
            Cleanup();
        }

        private static void Cleanup()
        {
            if (isCleaningUp)
            {
                return;
            }

            UpdateSelectionCacheAssetState();
            isCleaningUp = true;

            ToggleSubs(false);
        }

        private static void UpdateSelectionCacheAssetState()
        {
            if (_selectionCache == null)
            {
                return;
            }
            EditorUtility.SetDirty(_selectionCache);
            AssetDatabase.SaveAssetIfDirty(_selectionCache);
        }

        private static bool isCleaningUp;

    }
}