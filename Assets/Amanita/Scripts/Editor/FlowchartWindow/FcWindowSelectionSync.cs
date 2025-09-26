using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// To keep the FlowchartWindow and BlockInspector synced with the last Flowchart selected.
    /// </summary>
    public class FcWindowSelectionSync : IFcWindowComponent, IDisposable
    {
        public virtual void Initialize(IFlowchartHost window)
        {
            _window = window;
            DeregisterCallbacks();
            ListenForEvents();
        }

        protected virtual void DeregisterCallbacks()
        {
            BlockSignals.BlockCreated -= OnBlockCreated;
            BlockSignals.BlockClicked -= OnBlockClicked;
            FlowchartWindowSignals.EmptySpaceClicked -= OnEmptySpaceClicked;
            FlowchartWindowSignals.ChangedFlowchart -= OnFlowchartChanged;
        }

        protected virtual void ListenForEvents()
        {
            BlockSignals.BlockCreated += OnBlockCreated;
            BlockSignals.BlockClicked += OnBlockClicked;
            FlowchartWindowSignals.EmptySpaceClicked += OnEmptySpaceClicked;
            FlowchartWindowSignals.ChangedFlowchart += OnFlowchartChanged;
        }

        protected virtual void OnBlockCreated(Block block)
        {
            _lastShownBlock = block;
            SelectBlockAndShowInspector(block);
        }

        protected void SelectBlockAndShowInspector(Block block)
        {
            if (Flowchart == null) return;

            // Avoid redundant inspector rebuilds
            bool inspectorIsActive = BlockInspector != null && Selection.activeObject == BlockInspector;
            bool inspectorAlreadyShowingThisBlock = BlockInspector != null && BlockInspector.block == block;
            if (inspectorIsActive && inspectorAlreadyShowingThisBlock)
            {
                return;
            }

            // Update Flowchart selection
            if (block != null)
            {
                _window.SelectBlock(block);
                ShowBlockInspector(block);
            }
            else
            {
                Flowchart.ClearSelectedBlocks();
            }

            _lastShownBlock = block;
        }

        protected Block _lastShownBlock;

        protected virtual void OnBlockClicked(Block block, Event inputEvent)
        {
            _lastShownBlock = block;
            ShowBlockInspector(block);
        }

        protected virtual void OnEmptySpaceClicked()
        {
            _lastShownBlock = null;
            SelectBlockAndShowInspector(null);
            if (Flowchart != null && Selection.activeGameObject != Flowchart.gameObject)
            {
                Selection.activeGameObject = Flowchart.gameObject;
            }
            
        }

        protected virtual void OnFlowchartChanged(Flowchart prevFlowchart, Flowchart currentFlowchart)
        {
            _skipNextEditorUpdate = true;
        }

        protected bool _skipNextEditorUpdate; // So we know when to bail in OnEditorUpdate

        protected IFlowchartHost _window;

        public virtual void OnEditorUpdate()
        {
            if (Flowchart == null)
            {
                return;
            }

            // If you switched flowcharts, we bail out
            if (_skipNextEditorUpdate)
            {
                _skipNextEditorUpdate = false;
                return;
            }

            if (Flowchart.VariableCount != _prevVarCount)
            {
                _prevVarCount = Flowchart.VariableCount;
                _window.Repaint();
            }

            UpdateStaleFlagsAndRepaintAsNeeded();
            void UpdateStaleFlagsAndRepaintAsNeeded()
            {
                // These flags can get set to true by the BlockInspector and CommandEditor
                if (Flowchart.SelectedCommandsStale)
                {
                    Flowchart.SelectedCommandsStale = false;
                    _window.Repaint();
                }

                if (CommandEditor.SelectedCommandDataStale)
                {
                    CommandEditor.SelectedCommandDataStale = false;
                    _window.Repaint();
                }

                if (BlockEditor.SelectedBlockDataStale)
                {
                    BlockEditor.SelectedBlockDataStale = false;
                    _window.Repaint();
                }

                if (FlowchartEditor.FlowchartDataStale)
                {
                    FlowchartEditor.FlowchartDataStale = false;
                    _window.Repaint();
                }
            }

        }

        protected int _prevVarCount;
        public void OnToolbarGUI() { }
        public void OnGUI(DrawBlockContext d, FlowchartContext f) { }
        public void OnInspectorGUI() { }

        public virtual void OnInspectorUpdate()
        {
            if (Flowchart == null || AnyNullBlocks())
            {
                _window.UpdateBlockCollection();
                _window.Repaint();
                return;
            }
            
            if (ShouldShowInspector())
            {
                _lastShownBlock = Flowchart.SelectedBlock;
                ShowBlockInspector(_lastShownBlock);
            }

            bool ShouldShowInspector()
            {
                GameObject selectedGO = Selection.activeGameObject;
                
                bool flowchartIsSelected = selectedGO != null &&
                    selectedGO.GetComponent<Flowchart>() != null;

                bool changedBlockSelection = Flowchart.SelectedBlock != _lastShownBlock;

                bool alreadyShowingSelectedBlock = BlockInspector != null &&
                    BlockInspector.block == Flowchart.SelectedBlock;

                bool result = flowchartIsSelected && changedBlockSelection && !alreadyShowingSelectedBlock;
                return result;
            }

        }

        protected virtual BlockInspector BlockInspector
        {
            get => FlowchartWindow.blockInspector;
            set => FlowchartWindow.blockInspector = value;
        }
        protected virtual Flowchart Flowchart
        {
            get
            {
                Flowchart result = null;
                if (_window != null)
                {
                    result = _window.Flowchart;
                }
                return result;
            }
        }

        bool AnyNullBlocks() => _window.Blocks.Any(b => b == null);

        protected virtual void ShowBlockInspector(Block block)
        {
            bool alreadyShowingThatBlock = BlockInspector != null &&
                BlockInspector.block == block &&
                Selection.activeObject == BlockInspector;
            if (alreadyShowingThatBlock)
            {
                return; // No change — skip inspector rebuild
            }

            if (Flowchart == null)
            {
                return;
            }

            if (block != null)
            {
                _window.SelectBlock(block);
            }
            else
            {
                Flowchart.ClearSelectedBlocks();
                Flowchart.ClearSelectedCommands();
            }

            CreateOrReuseBlockInspectorSO();
            void CreateOrReuseBlockInspectorSO()
            {
                if (BlockInspector == null)
                {
                    Debug.Log($"Creating new BlockInspector in FcWindowSelectionSync's CreateOrReuseBlockInspectorSO method");
                    BlockInspector = ScriptableObject.CreateInstance<BlockInspector>();
                    BlockInspector.hideFlags = HideFlags.DontSave;
                    EditorUtility.SetDirty(BlockInspector);
                }
                if (Flowchart.SelectedBlock != null && Selection.activeObject != BlockInspector)
                {
                    Debug.Log($"Right before changing active object to the inspector in FcWindowSelectionSync's CreateOrReuseBlockInspectorSO");
                    Selection.activeObject = BlockInspector;
                }
            }

            SetBlockInspectorToTheRightBlock();
            void SetBlockInspectorToTheRightBlock()
            {
                bool wasAlreadyShowingThisBlock = BlockInspector != null && BlockInspector.block == block;
                if (!alreadyShowingThatBlock)
                {
                    // ^We need this check to make sure that when a Command is selected in the 
                    // Inspector, it's not immediately unselected
                    Flowchart.ClearSelectedCommands();
                }

                if (block != null)
                {
                    BlockInspector.block = block;
                    if (block.ActiveCommand != null)
                    {
                        Flowchart.AddSelectedCommand(block.ActiveCommand);
                    }
                }
                if (block != null && block.ActiveCommand != null)
                {
                    Flowchart.AddSelectedCommand(block.ActiveCommand);
                }

            }

        }

        public virtual void Dispose()
        {
            BlockSignals.BlockCreated -= OnBlockCreated;
            BlockSignals.BlockClicked -= OnBlockClicked;

            _lastShownBlock = null;
            _prevVarCount = 0;
        }

    }
}
