using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Central authority for creating, showing, and clearing the hidden BlockInspector ScriptableObject.
    /// Automatically reacts to Flowchart/Block selection signals so every editor surface
    /// stays in sync without relying on FlowchartWindow’s static field directly.
    /// </summary>
    [InitializeOnLoad]
    public static class BlockInspectorManager
    {
        static BlockInspectorManager()
        {
            ListenForEvents();
        }

        private static void ListenForEvents()
        {
            BlockSignals.BlockSelected += OnBlockSelected;
            BlockSignals.BlockDeselected += OnBlockDEselected;
            BlockSignals.MultiBlocksSelected += OnMultiBlocksSelected;

            FlowchartWindowSignals.EmptySpaceLeftClicked += OnEmptySpaceLeftClicked;
            FlowchartWindowSignals.ChangedFlowchart += OnFlowchartChanged;

            AssemblyReloadEvents.beforeAssemblyReload += DisposeInspector;
            EditorApplication.quitting += DisposeInspector;
        }

        private static void OnBlockSelected(Block block)
        {
            Show(block);
        }

        public static void Show(Block block)
        {
            if (block == null)
            {
                Clear();
                return;
            }

            Flowchart flowchart = block.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }

            TrackedFlowchart = flowchart;
            ShowInspectorFor(flowchart, block);
        }

        public static void Clear()
        {
            ClearInternal(trackedFlowchart);
        }

        private static Flowchart trackedFlowchart;

        /// <summary>
        /// Clears the BlockInspector and resets selection state.
        /// </summary>
        private static void ClearInternal(Flowchart flowchart)
        {
            if (inspectorInstance != null)
            {
                inspectorInstance._block = null;
            }

            if (flowchart != null)
            {
                flowchart.ClearSelectedCommands();

                if (flowchart.gameObject != null)
                {
                    Selection.activeGameObject = flowchart.gameObject;
                    // ^To keep the inspector focused on the Flowchart itself
                }
            }

            lastShownBlock = null;
            InspectorTargetChanged(null);
        }

        private static BlockInspector inspectorInstance;
        private static Block lastShownBlock;
        public static event Action<Block> InspectorTargetChanged = delegate { };


        private static Flowchart TrackedFlowchart
        {
            set => trackedFlowchart = value;
        }

        private static void ShowInspectorFor(Flowchart flowchart, Block block)
        {
            if (flowchart == null || block == null)
            {
                return;
            }

            BlockInspector inspector = EnsureInspector();

            bool inspectorIsActive = Selection.activeObject == inspector;
            bool inspectorAlreadyShowing = inspector._block == block;

            if (!inspectorAlreadyShowing)
            {
                flowchart.ClearSelectedCommands();
                inspector._block = block;

                if (block.ActiveCommand != null)
                {
                    flowchart.AddSelectedCommand(block.ActiveCommand);
                }
            }

            if (Selection.activeObject != inspector)
            {
                Selection.activeObject = inspector;
            }

            lastShownBlock = block;
            InspectorTargetChanged(block);
        }

        private static BlockInspector EnsureInspector()
        {
            if (inspectorInstance == null)
            {
                inspectorInstance = ScriptableObject.CreateInstance<BlockInspector>();
                inspectorInstance.hideFlags = HideFlags.DontSave;
                EditorUtility.SetDirty(inspectorInstance);
            }

            return inspectorInstance;
        }

        public static Flowchart CurrentFlowchart => trackedFlowchart;

        public static BlockInspector Inspector => EnsureInspector();

        public static Block LastShownBlock => lastShownBlock;

        private static void OnBlockDEselected(Block block)
        {
            Flowchart flowchart = block != null ? 
                block.GetFlowchart() : 
                trackedFlowchart;
            if (flowchart == null)
            {
                Clear();
                return;
            }

            if (flowchart.SelectedBlockCount == 0)
            {
                ClearInternal(flowchart);
                return;
            }

            Block selectedBlock = GetPrimarySelectedBlock(flowchart);
            if (selectedBlock != null)
            {
                ShowInspectorFor(flowchart, selectedBlock);
            }
            else
            {
                ClearInternal(flowchart);
            }
        }

        private static Block GetPrimarySelectedBlock(Flowchart flowchart)
        {
            if (flowchart == null || flowchart.UIModel == null)
            {
                return null;
            }

            return flowchart.UIModel.SelectedBlock;
        }

        private static void OnMultiBlocksSelected(IList<Block> blocks)
        {
            if (blocks == null)
            {
                Clear();
                return;
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                Block candidate = blocks[i];
                if (candidate != null)
                {
                    Show(candidate);
                    return;
                }
            }

            Clear();
        }

        private static void OnEmptySpaceLeftClicked(PointerEventInfo _)
        {
            ClearInternal(trackedFlowchart);
        }

        private static void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            TrackedFlowchart = next;

            if (next == null)
            {
                ClearInternal(null);
                return;
            }

            Block selectedBlock = GetPrimarySelectedBlock(next);
            if (selectedBlock != null)
            {
                ShowInspectorFor(next, selectedBlock);
            }
            else
            {
                ClearInternal(next);
            }
        }

        private static void DisposeInspector()
        {
            if (inspectorInstance != null)
            {
                ScriptableObject.DestroyImmediate(inspectorInstance);
                inspectorInstance = null;
            }

            trackedFlowchart = null;
            lastShownBlock = null;
        }
    }
}