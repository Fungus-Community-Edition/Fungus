using System;
using System.Collections.Generic;
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
        public static Flowchart ActiveFlowchart => activeFlowchart != null ?
            activeFlowchart :
            ResolveActiveFlowchart();
        private static Flowchart activeFlowchart;
        public static Flowchart LastActiveFlowchart { get; private set; }
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
            DestroyLegacyStateInstances();
            AttemptInitialHydration();
            ToggleSubs(true);
        }

        private static void DestroyLegacyStateInstances()
        {
            AmanitaState[] legacyStates = UnityObj.FindObjectsByType<AmanitaState>(
                FindObjectsInactive.Include,
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
            Flowchart flowchart = FindFlowchartFromSelection();
            if (flowchart == null)
            {
                flowchart = FindFlowchartInScene();
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
            return selected;
        }

        private static Flowchart FindFlowchartInScene()
        {
            return UnityObj.FindFirstObjectByType<Flowchart>(FindObjectsInactive.Include);
        }

        private static void SetActiveFlowchart(Flowchart flowchart)
        {
            if (ReferenceEquals(activeFlowchart, flowchart))
            {
                return;
            }

            Flowchart previous = activeFlowchart;
            activeFlowchart = flowchart;

            if (flowchart != null)
            {
                LastActiveFlowchart = flowchart;
            }

            SyncSelectionsFromFlowchart(flowchart);
            SelectedFlowchartChanged(previous, flowchart);
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

                FlowchartWindowSignals.ChangedFlowchart += OnFlowchartWindowChanged;
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

                FlowchartWindowSignals.ChangedFlowchart -= OnFlowchartWindowChanged;
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

        private static void OnFlowchartWindowChanged(Flowchart previous, Flowchart current)
        {
            if (current == null && previous == null)
            {
                return;
            }

            SetActiveFlowchart(current);
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
                SetActiveFlowchart(fromSelection);
                return fromSelection;
            }

            if (attemptSceneFallback)
            {
                Flowchart fallback = FindFlowchartInScene();
                if (fallback != null)
                {
                    SetActiveFlowchart(fallback);
                    return fallback;
                }
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
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                ResolveActiveFlowchart();
            }
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

            isCleaningUp = true;

            ToggleSubs(false);
        }

        private static bool isCleaningUp;

    }
}