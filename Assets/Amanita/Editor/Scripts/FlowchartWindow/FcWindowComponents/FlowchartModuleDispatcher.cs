using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public interface IFlowchartWindowModule : IDisposable
    {
        void Initialize(FlowchartWindowUitk window);
    }

    public sealed class FlowchartModuleDispatcher
    {
        private readonly List<IFlowchartWindowModule> modules = new();
        private readonly Dictionary<Type, IList> responderBuckets = new();

        public void AddModule(IFlowchartWindowModule module)
        {
            modules.Add(module);

            #region Mouse Events
            AddResponder<ILeftClickResponder>(module);
            AddResponder<IRightClickResponder>(module);
            AddResponder<IDoubleClickResponder>(module);

            AddResponder<ILeftMouseDragStartResponder>(module);
            AddResponder<ILeftMouseDragResponder>(module);
            AddResponder<ILeftMouseDragEndResponder>(module);
            AddResponder<ILeftMouseUpResponder>(module);

            AddResponder<IRightMouseDragStartResponder>(module);
            AddResponder<IRightMouseDragResponder>(module);
            AddResponder<IRightMouseDragEndResponder>(module);

            AddResponder<IEmptySpaceClickResponder>(module);
            AddResponder<IEmptySpaceLeftMouseDownResponder>(module);
            AddResponder<IEmptySpaceLeftMouseUpResponder>(module);

            AddResponder<IScrollWheelMoveResponder>(module);
            AddResponder<IScrollWheelDragResponder>(module);

            #endregion

            #region Block Events
            AddResponder<IBlockCreatedResponder>(module);
            AddResponder<IPreBlockDeletionResponder>(module);
            AddResponder<IPostBlockDeletionResponder>(module);

            AddResponder<IBlockClickResponder>(module);
            AddResponder<IBlockSelectionResponder>(module);
            AddResponder<IMultiBlockSelectionResponder>(module);
            AddResponder<IBlockDeselectionResponder>(module);
            AddResponder<IMultiBlockDeselectionResponder>(module);

            AddResponder<IBlocksCopiedResponder>(module);
            
            #endregion

            AddResponder<IFlowchartChangeResponder>(module);
            
            AddResponder<ICommandSelectionResponder>(module);

            AddResponder<IWindowPanResponder>(module);

            AddResponder<IVariableAddResponder>(module);
            AddResponder<IVariableRemoveResponder>(module);

        }

        public void RemoveModule(IFlowchartWindowModule module)
        {
            modules.Remove(module);

            #region Mouse Events
            RemoveResponder<ILeftClickResponder>(module);
            RemoveResponder<IRightClickResponder>(module);
            RemoveResponder<IDoubleClickResponder>(module);

            RemoveResponder<ILeftMouseDragStartResponder>(module);
            RemoveResponder<ILeftMouseDragResponder>(module);
            RemoveResponder<ILeftMouseDragEndResponder>(module);
            RemoveResponder<ILeftMouseUpResponder>(module);

            RemoveResponder<IRightMouseDragStartResponder>(module);
            RemoveResponder<IRightMouseDragResponder>(module);
            RemoveResponder<IRightMouseDragEndResponder>(module);

            RemoveResponder<IEmptySpaceClickResponder>(module);
            RemoveResponder<IEmptySpaceLeftMouseDownResponder>(module);
            RemoveResponder<IEmptySpaceLeftMouseUpResponder>(module);

            RemoveResponder<IScrollWheelMoveResponder>(module);
            RemoveResponder<IScrollWheelDragResponder>(module);

            #endregion

            #region Block Events
            RemoveResponder<IBlockCreatedResponder>(module);
            RemoveResponder<IPreBlockDeletionResponder>(module);
            RemoveResponder<IPostBlockDeletionResponder>(module);

            RemoveResponder<IBlockClickResponder>(module);
            RemoveResponder<IBlockSelectionResponder>(module);
            RemoveResponder<IMultiBlockSelectionResponder>(module);
            RemoveResponder<IBlockDeselectionResponder>(module);
            RemoveResponder<IMultiBlockDeselectionResponder>(module);

            RemoveResponder<IBlocksCopiedResponder>(module);
            #endregion

            RemoveResponder<IFlowchartChangeResponder>(module);

            RemoveResponder<ICommandSelectionResponder>(module);

            RemoveResponder<IWindowPanResponder>(module);

            RemoveResponder<IVariableAddResponder>(module);
            RemoveResponder<IVariableRemoveResponder>(module);

            module.Dispose();
        }

        public void ClearModules()
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].Dispose();
            }

            modules.Clear();
            responderBuckets.Clear();
        }

        #region Notifiers

        #region Mouse Notifiers
        public void NotifyLeftClick(Vector2 position) =>
            Broadcast<ILeftClickResponder>(res => res.OnLeftClick(position));

        public void NotifyRightClick(Vector2 position) =>
            Broadcast<IRightClickResponder>(res => res.OnRightClick(position));

        public void NotifyDoubleClick(Vector2 position) =>
            Broadcast<IDoubleClickResponder>(res => res.OnDoubleClick(position));

        public void NotifyLeftMouseDragStarted(Vector2 startPosition, Event guiEvent) =>
            Broadcast<ILeftMouseDragStartResponder>(res => res.OnLeftMouseDragStarted(startPosition, guiEvent));

        public void NotifyLeftMouseDragged(Vector2 currentPosition, Event guiEvent) =>
            Broadcast<ILeftMouseDragResponder>(res => res.OnLeftMouseDragged(currentPosition, guiEvent));

        public void NotifyLeftMouseDragEnded(Vector2 endPosition, Event guiEvent) =>
            Broadcast<ILeftMouseDragEndResponder>(res => res.OnLeftMouseDragEnded(endPosition, guiEvent));

        public void NotifyLeftMouseUp(Vector2 position, Event evt) =>
            Broadcast<ILeftMouseUpResponder>(res => res.OnLeftMouseUp(position, evt));

        public void NotifyRightMouseDragStarted(Vector2 startPosition, Event guiEvent) =>
            Broadcast<IRightMouseDragStartResponder>(res => res.OnRightMouseDragStarted(startPosition, guiEvent));

        public void NotifyRightMouseDragged(Vector2 currentPosition, Event guiEvent) =>
            Broadcast<IRightMouseDragResponder>(res => res.OnRightMouseDragged(currentPosition, guiEvent));

        public void NotifyRightMouseDragEnded(Vector2 endPosition, Event guiEvent) =>
            Broadcast<IRightMouseDragEndResponder>(res => res.OnRightMouseDragEnded(endPosition, guiEvent));

        public void NotifyEmptySpaceClicked(Vector2 position) =>
            Broadcast<IEmptySpaceClickResponder>(res => res.OnEmptySpaceClicked(position));

        public void NotifyEmptySpaceLeftMouseDown(Vector2 position, Event evt) =>
            Broadcast<IEmptySpaceLeftMouseDownResponder>(res => res.OnEmptySpaceLeftMouseDown(position, evt));

        public void NotifyEmptySpaceLeftMouseUp(Vector2 position, Event evt) =>
            Broadcast<IEmptySpaceLeftMouseUpResponder>(res => res.OnEmptySpaceLeftMouseUp(position, evt));

        public void NotifyScrollWheelMoved() =>
            Broadcast<IScrollWheelMoveResponder>(res => res.OnScrollWheelMoved());

        public void NotifyScrollWheelDragged(Vector2 direction) =>
            Broadcast<IScrollWheelDragResponder>(res => res.OnScrollWheelDragged(direction));
        #endregion

        #region Block Notifiers
        public void NotifyBlockCreated(Block block) =>
            Broadcast<IBlockCreatedResponder>(res => res.OnBlockCreated(block));

        public void NotifyPreBlockDeleted(Block block) =>
            Broadcast<IPreBlockDeletionResponder>(res => res.OnPreBlockDeletion(block));

        public void NotifyPreMultiBlockDeleted(IList<Block> blocks) =>
            Broadcast<IPreBlockDeletionResponder>(res => res.OnPreBlockDeletion(blocks));

        public void NotifyPostBlockDeleted(uint blockId) =>
            Broadcast<IPostBlockDeletionResponder>(res => res.OnPostBlockDeletion(blockId));

        public void NotifyPostMultiBlockDeleted(IList<uint> blockIds) =>
            Broadcast<IPostMultiBlockDeletionResponder>(res => res.OnPostMultiBlockDeletion(blockIds));

        public void NotifyBlockClicked(Block block, Event evt) =>
            Broadcast<IBlockClickResponder>(res => res.OnBlockClicked(block, evt));

        public void NotifyBlockSelected(Block block) =>
            Broadcast<IBlockSelectionResponder>(res => res.OnBlockSelected(block));

        public void NotifyMultiBlocksSelected(IList<Block> blocks) =>
            Broadcast<IMultiBlockSelectionResponder>(res => res.OnMultiBlocksSelected(blocks));

        public void NotifyBlockDeselected(Block block) =>
            Broadcast<IBlockDeselectionResponder>(res => res.OnBlockDeselected(block));

        public void NotifyMultiBlocksDeselected(IList<Block> blocks) =>
            Broadcast<IMultiBlockDeselectionResponder>(res => res.OnMultiBlocksDeselected(blocks));

        public void NotifyBlocksCopied(IList<Block> copiedBlocks) =>
            Broadcast<IBlocksCopiedResponder>(res => res.OnBlocksCopied(copiedBlocks));

        #endregion

        public void NotifyFlowchartChanged(Flowchart prev, Flowchart current) =>
            Broadcast<IFlowchartChangeResponder>(res => res.OnFlowchartChanged(prev, current));

        public void NotifyCommandSelected(Command command) =>
            Broadcast<ICommandSelectionResponder>(res => res.OnCommandSelected(command));

        public void NotifyWindowPanned() =>
            Broadcast<IWindowPanResponder>(res => res.OnWindowPanned());

        #endregion

        private void AddResponder<TResponder>(IFlowchartWindowModule module)
            where TResponder : class
        {
            if (module is not TResponder responder)
            {
                return;
            }

            List<TResponder> bucket = GetOrCreateBucket<TResponder>();
            bucket.Add(responder);
        }

        private void RemoveResponder<TResponder>(IFlowchartWindowModule module)
            where TResponder : class
        {
            if (module is not TResponder responder)
            {
                return;
            }

            Type key = typeof(TResponder);
            if (!responderBuckets.TryGetValue(key, out IList bucket))
            {
                return;
            }

            List<TResponder> typedBucket = (List<TResponder>)bucket;
            typedBucket.Remove(responder);

            if (typedBucket.Count == 0)
            {
                responderBuckets.Remove(key);
            }
        }

        private List<TResponder> GetOrCreateBucket<TResponder>()
            where TResponder : class
        {
            Type key = typeof(TResponder);
            if (!responderBuckets.TryGetValue(key, out IList bucket))
            {
                var newBucket = new List<TResponder>();
                responderBuckets[key] = newBucket;
                return newBucket;
            }

            return (List<TResponder>)bucket;
        }

        private void Broadcast<TResponder>(Action<TResponder> action)
            where TResponder : class
        {
            if (!responderBuckets.TryGetValue(typeof(TResponder), out IList bucket))
            {
                return;
            }

            List<TResponder> typedBucket = (List<TResponder>)bucket;
            for (int i = 0; i < typedBucket.Count; i++)
            {
                action(typedBucket[i]);
            }
        }
    }
}