using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Amanita.EditorUtils
{
    public class FcWindowSelectionSync : IFcWindowComponent
    {
        FlowchartWindow _window;

        public void Initialize(FlowchartWindow window)
        {
            _window = window;
        }

        public void OnEditorUpdate()
        {
            var fc = _window.Flowchart;
            if (fc == null) return;

            // If you switched flowcharts, we bail out
            if (_window.HandleFlowchartSelectionChange())
                return;

            // detect variable‐count change
            if (fc.VariableCount != prevVarCount)
            {
                prevVarCount = fc.VariableCount;

                _window.Repaint();
            }

            // these flags get set by the BlockInspector and CommandEditor
            if (fc.SelectedCommandsStale)
            {
                fc.SelectedCommandsStale = false;

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

        protected int prevVarCount;
        public void OnToolbarGUI() { }
        public void OnCanvasGUI(DrawBlockContext d, FlowchartContext f) { }
        public void OnInspectorGUI() { }

        // — Helpers carried over from FlowchartWindow — 
        private void ShowBlockInspector(Flowchart flowchart, Block block)
        {
            _window.SelectBlock(block);

            CreateOrReuseBlockInspectorSO();
            void CreateOrReuseBlockInspectorSO()
            {
                if (FlowchartWindow.blockInspector == null)
                {
                    FlowchartWindow.blockInspector = ScriptableObject
                        .CreateInstance<BlockInspector>();
                    FlowchartWindow.blockInspector.hideFlags = HideFlags.DontSave;
                }
                Selection.activeObject = FlowchartWindow.blockInspector;
                EditorUtility.SetDirty(FlowchartWindow.blockInspector);
            }

            Block prevSelectedBlock = flowchart.SelectedBlock;
            flowchart.SelectedBlock = block;
            if (prevSelectedBlock != flowchart.SelectedBlock)
            {
                flowchart.ClearSelectedCommands();
            }

            if (block.ActiveCommand != null)
                flowchart.AddSelectedCommand(block.ActiveCommand);

            FlowchartWindow.blockInspector.block = block;
        }

        public virtual void OnInspectorUpdate()
        {
            var fc = _window.Flowchart;

            // if flowchart got cleared or blocks have gone null, rebuild
            if (fc == null || AnyNullBlocks())
            {
                _window.UpdateBlockCollection();
                _window.Repaint();
                return;
            }

            // if nothing in the Scene is selected but a block is selected in the flowchart,
            // make sure the BlockInspector SO is visible and pointing at it
            if (Selection.activeGameObject == null && fc.SelectedBlock != null)
            {
                ShowBlockInspector(fc, (Block)fc.SelectedBlock);
            }

        }
        bool AnyNullBlocks() => _window.blocks.Any(b => b == null);

    }
}