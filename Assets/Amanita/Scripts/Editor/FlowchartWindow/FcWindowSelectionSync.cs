using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// To keep the FlowchartWindow and BlockInspector synced with the last Flowchart selected.
    /// </summary>
    public class FcWindowSelectionSync : IFcWindowComponent
    {
        public virtual void Initialize(FlowchartWindow window)
        {
            _window = window;
        }

        protected FlowchartWindow _window;

        public virtual void OnEditorUpdate()
        {
            var fc = _window.Flowchart;

            if (fc == null)
            {
                return;
            }

            // If you switched flowcharts, we bail out
            if (_window.HandleFlowchartSelectionChange())
            {
                return;
            }

            if (fc.VariableCount != prevVarCount)
            {
                prevVarCount = fc.VariableCount;
                _window.Repaint();
            }

            UpdateStaleFlagsAndRepaintAsNeeded();
            void UpdateStaleFlagsAndRepaintAsNeeded()
            {
                // These flags can get set to true by the BlockInspector and CommandEditor
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

        }

        protected int prevVarCount;
        public void OnToolbarGUI() { }
        public void OnGUI(DrawBlockContext d, FlowchartContext f) { }
        public void OnInspectorGUI() { }

        
        public virtual void OnInspectorUpdate()
        {
            var fc = _window.Flowchart;

            if (fc == null || AnyNullBlocks())
            {
                _window.UpdateBlockCollection();
                _window.Repaint();
                return;
            }
            
            GameObject selectedGO = Selection.activeGameObject;
            bool flowchartIsSelected = selectedGO != null &&
                selectedGO.GetComponent<Flowchart>() != null;
            if (flowchartIsSelected)
            {
                // To reduce conflicts with selecting assets and such, we only force the Inspector to
                // focus on a Block when the current selected GameObject has an FC
                ShowBlockInspector(fc, fc.SelectedBlock);
            }

        }
        bool AnyNullBlocks() => _window.blocks.Any(b => b == null);

        protected virtual void ShowBlockInspector(Flowchart flowchart, Block block)
        {
            if (flowchart == null)
            {
                return;
            }

            if (block != null)
            {
                _window.SelectBlock(block);
            }
            else
            {
                flowchart.ClearSelectedBlocks();
                flowchart.ClearSelectedCommands();
            }

            CreateOrReuseBlockInspectorSO();
            void CreateOrReuseBlockInspectorSO()
            {
                if (FlowchartWindow.blockInspector == null)
                {
                    FlowchartWindow.blockInspector = ScriptableObject
                        .CreateInstance<BlockInspector>();
                    FlowchartWindow.blockInspector.hideFlags = HideFlags.DontSave;
                }
                if (flowchart.SelectedBlock != null)
                {
                    Selection.activeObject = FlowchartWindow.blockInspector;
                    EditorUtility.SetDirty(FlowchartWindow.blockInspector);
                }
            
            }

            SetBlockInspectorToTheRightBlock();
            void SetBlockInspectorToTheRightBlock()
            {
                var blockInspector = FlowchartWindow.blockInspector;
                bool wasAlreadyShowingThisBlock = blockInspector != null && blockInspector.block == block;
                if (!wasAlreadyShowingThisBlock)
                {
                    // ^We need this check to make sure that when a Command is selected in the 
                    // Inspector, it's not immediately unselected
                    flowchart.ClearSelectedCommands();
                }

                //Block prevSelectedBlock = flowchart.SelectedBlock;
                //flowchart.SelectedBlock = block;
                //if (prevSelectedBlock != flowchart.SelectedBlock)
                //{
                //    flowchart.ClearSelectedCommands();
                //}

                if (block != null && block.ActiveCommand != null)
                {
                    flowchart.AddSelectedCommand(block.ActiveCommand);
                    FlowchartWindow.blockInspector.block = block;
                }

            }

        }


    }
}
