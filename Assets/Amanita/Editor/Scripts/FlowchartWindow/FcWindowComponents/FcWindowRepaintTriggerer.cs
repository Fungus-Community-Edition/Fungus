using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles when to get the new flowchart window to repaint.
    /// </summary>
    public sealed class FcWindowRepaintTriggerer : IFlowchartWindowModule, IFlowchartChangeResponder,
        IBlockSelectionResponder, IVariableAddResponder, IVariableRemoveResponder, 
        IPostBlockDeletionResponder
    {
        public int Priority { get; set; } = 0;
        public void Initialize(FlowchartWindowUitk window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            owner = window;
            isDisposed = false;
        }

        private FlowchartWindowUitk owner;
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

            EditorApplication.delayCall += () => owner?.Repaint();
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

        public void OnPostMultiBlockDeletion(IList<uint> blockIds)
        {
            TriggerRepaint();
        }

        public void OnPostBlockDeletion(uint blockId)
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