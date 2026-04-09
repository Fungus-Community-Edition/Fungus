using System;
using System.Collections.Generic;
using UnityEditor;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Handles when to get the new flowchart window to repaint.
    /// </summary>
    public sealed class FcWindowRepaintTriggerer : IFlowchartWindowModule, IFlowchartChangeResponder,
        IBlockSelectionResponder, IVariableAddResponder, IVariableRemoveResponder, 
        IPostBlockDeletionResponder
    {
        public int Priority { get; set; } = 0;
        public void Initialize(FlowchartWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            owner = window;
            isDisposed = false;
        }

        private FlowchartWindow owner;
        private bool isDisposed;

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            TriggerRepaint();
        }

        private void TriggerRepaint()
        {
            // Without this func, we'd have a lot more boilerplate in the other event responses.
            if (isDisposed)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (owner != null)
                {
                    owner.Repaint();
                }
            };
        }

        public void OnVariableAdded(Flowchart addedTo, IVariable variable)
        {
            TriggerRepaint();
        }

        public void OnVariableRemoved(Flowchart removedFrom, IVariable variable)
        {
            TriggerRepaint();
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            owner = null;
        }

        public void OnPostMultiBlockDeletion(IList<short> blockIds)
        {
            TriggerRepaint();
        }

        public void OnPostBlockDeletion(ushort blockId)
        {
            TriggerRepaint();
        }

        public void OnBlockSelected(Block block)
        {
            TriggerRepaint();
        }

        public void OnMultiBlocksSelected(IList<Block> blocks)
        {
            TriggerRepaint();
        }
    }
}