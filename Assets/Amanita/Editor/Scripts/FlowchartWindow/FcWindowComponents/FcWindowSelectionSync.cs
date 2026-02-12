using System;
using System.Linq;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// To keep the FlowchartWindow and BlockInspector synced with the last Flowchart selected.
    /// </summary>
    public class FcWindowSelectionSync : IFcWindowComponent, IDisposable
    {
        public virtual void Initialize(IFlowchartViewHost window)
        {
            _window = window;
            _blockInspectorSync = new BlockInspectorSynchronization(
                () => Flowchart,
                block =>
                {
                    if (block != null)
                    {
                        _window.SelectBlock(block);
                    }
                });

            DeregisterCallbacks();
            ListenForEvents();
        }

        protected virtual void DeregisterCallbacks()
        {
            BlockSignals.BlockCreated -= OnBlockCreated;
            BlockSignals.BlockLeftClicked -= OnBlockClicked;
            FlowchartWindowSignals.EmptySpaceLeftClicked -= OnEmptySpaceClicked;
            FlowchartWindowSignals.ChangedFlowchart -= OnFlowchartChanged;
        }

        protected virtual void ListenForEvents()
        {
            BlockSignals.BlockCreated += OnBlockCreated;
            BlockSignals.BlockLeftClicked += OnBlockClicked;
            FlowchartWindowSignals.EmptySpaceLeftClicked += OnEmptySpaceClicked;
            FlowchartWindowSignals.ChangedFlowchart += OnFlowchartChanged;
        }

        protected virtual void OnBlockCreated(Block block)
        {
            _blockInspectorSync?.HandleBlockCreated(block);
        }

        protected virtual void OnBlockClicked(Block block, Event inputEvent)
        {
            _blockInspectorSync?.HandleBlockClicked(block);
        }

        protected virtual void OnEmptySpaceClicked(PointerEventInfo info)
        {
            _blockInspectorSync?.HandleEmptySpaceClicked();
        }

        protected virtual void OnFlowchartChanged(Flowchart prevFlowchart, Flowchart currentFlowchart)
        {
            _skipNextEditorUpdate = true;
            _blockInspectorSync?.ResetLastShownBlock();
        }

        protected bool _skipNextEditorUpdate;
        protected BlockInspectorSynchronization _blockInspectorSync;

        protected IFlowchartViewHost _window;

        public virtual void OnEditorUpdate()
        {
            if (Flowchart == null)
            {
                return;
            }

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

            _blockInspectorSync?.SyncInspectorWithSelectionIfNeeded();
        }

        protected virtual BlockInspector BlockInspector
        {
            get => BlockInspectorManager.Inspector;
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

        public virtual void Dispose()
        {
            BlockSignals.BlockCreated -= OnBlockCreated;
            BlockSignals.BlockLeftClicked -= OnBlockClicked;

            _blockInspectorSync = null;
            _prevVarCount = 0;
        }

    }
}
