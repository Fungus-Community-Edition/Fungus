using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Centralizes editor-side knowledge of which Flowchart/Blocks/Commands are currently selected.
    /// Removes legacy AmanitaState components so selection changes are tracked exclusively here.
    /// </summary>
    [InitializeOnLoad]
    public static class EditorSelectionTracker
    {
        private const string LastSelectedFlowchartUidKey = "AtMycelia.Amanita.Editor.LastSelectedFlowchartUid";

        public static Flowchart ActiveFlowchart => activeFlowchart;
        private static Flowchart activeFlowchart;
        public static Flowchart LastActiveFlowchart
        {
            get
            {
                if (activeFlowchart != null)
                {
                    return activeFlowchart;
                }

                Flowchart fromSelection = FindFlowchartFromSelection();

                Flowchart basedOnCache = FindFlowchartWithCachedId();
                if (basedOnCache != null)
                {
                    return basedOnCache;
                }

                Flowchart inScene = FindFlowchartInScene();
                return inScene;
            }
        }

        private static bool HasSameUidAsCache(Flowchart fc)
        {
            return fc != null && fc.UniqueId == GetCachedFlowchartUid();
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

        public static event System.Action<IReadOnlyList<Block>> BlockSelectionChanged = delegate { };
        public static event System.Action<Block, Block> PrimaryBlockChanged = delegate { };
        public static event System.Action<IReadOnlyList<Command>> CommandSelectionChanged = delegate { };
        public static event System.Action<Command, Command> PrimaryCommandChanged = delegate { };

        static EditorSelectionTracker()
        {
            DestroyLegacyStateInstances();
            AttemptInitialHydration();
            ToggleSubs(false);
            ToggleSubs(true);
        }

        private static void SelectFlowchartBasedOnCache()
        {
            if (string.IsNullOrEmpty(GetCachedFlowchartUid()))
            {
                return;
            }
            Flowchart toSelect = FindFlowchartWithCachedId();
            if (toSelect != null)
            {
                //Debug.Log($"Selecting flowchart based on cache: {toSelect.name} (uid: {toSelect.UniqueId})");
                SetActiveFlowchart(toSelect);
            }
        }

        private static Flowchart FindFlowchartWithCachedId()
        {
            if (string.IsNullOrEmpty(GetCachedFlowchartUid()))
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
            Flowchart fc = FindFlowchartFromSelection();
            if (fc != null)
            {
                SetActiveFlowchart(fc);
                return;
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
            return selected;
        }

        private static Flowchart FindFlowchartInScene()
        {
            return UnityObj.FindFirstObjectByType<Flowchart>(FindObjectsInactive.Include);
        }

        private static void SetActiveFlowchart(Flowchart flowchart)
        {
            bool alreadySelected = ReferenceEquals(activeFlowchart, flowchart) ||
                (flowchart != null && flowchart.UniqueId == GetCachedFlowchartUid());
            if (alreadySelected)
            {
                return;
            }

            Flowchart previous = activeFlowchart;
            activeFlowchart = flowchart;
            UpdateSelectionCache(flowchart);
            SyncSelectionsFromFlowchart(flowchart);
            SelectedFlowchartChanged(previous, flowchart);
        }

        private static void UpdateSelectionCache(Flowchart flowchart)
        {
            SetCachedFlowchartUid(flowchart != null ? flowchart.UniqueId : string.Empty);
        }

        private static string GetCachedFlowchartUid()
        {
            return EditorPrefs.GetString(LastSelectedFlowchartUidKey, string.Empty);
        }

        private static void SetCachedFlowchartUid(string uid)
        {
            EditorPrefs.SetString(LastSelectedFlowchartUidKey, uid ?? string.Empty);
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

                FlowchartWindowSignals.EmptySpaceLeftClicked += OnEmptySpaceClicked;

                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
                EditorApplication.quitting += Cleanup;
            }
            else
            {
                Selection.selectionChanged -= OnUnitySelectionChanged;

                FlowchartWindowSignals.EmptySpaceLeftClicked -= OnEmptySpaceClicked;

                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
                EditorApplication.quitting -= Cleanup;
            }
        }

        private static void OnUnitySelectionChanged()
        {
            Flowchart fc = FindFlowchartFromSelection();

            if (fc != null)
            {
                SetActiveFlowchart(fc);
            }
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
        public static event System.Action<Flowchart, Flowchart> SelectedFlowchartChanged = delegate { };

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

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SelectFlowchartBasedOnCache();
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            Cleanup();
        }

        private static void Cleanup()
        {
            // Why do this check? Because in some cases (entering play mode, for example), the
            // cleanup method can be called multiple times, and we only want to run this
            // logic once per "cleanup event".
            if (isCleaningUp)
            {
                return;
            }

            isCleaningUp = true;

            ToggleSubs(false);
        }

        private static bool isCleaningUp;

    }
}