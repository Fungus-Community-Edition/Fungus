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

            RegisterResponder<ILeftClickResponder>(module);
            RegisterResponder<IRightClickResponder>(module);
            RegisterResponder<IDoubleClickResponder>(module);
            RegisterResponder<IScrollWheelMoveResponder>(module);
            RegisterResponder<IScrollWheelDragResponder>(module);
            RegisterResponder<IEmptySpaceClickResponder>(module);
            RegisterResponder<IFlowchartChangeResponder>(module);
            RegisterResponder<IBlocksCopiedResponder>(module);
            RegisterResponder<IPreBlockDeletionResponder>(module);
            RegisterResponder<IBlockSelectionResponder>(module);
            RegisterResponder<ICommandSelectionResponder>(module);
            RegisterResponder<IWindowPanResponder>(module);

            RegisterResponder<IBlockCreatedResponder>(module);
            RegisterResponder<IPostBlockDeletionResponder>(module);
            RegisterResponder<IBlockClickResponder>(module);

        }

        public void RemoveModule(IFlowchartWindowModule module)
        {
            modules.Remove(module);

            UnregisterResponder<ILeftClickResponder>(module);
            UnregisterResponder<IRightClickResponder>(module);
            UnregisterResponder<IDoubleClickResponder>(module);
            UnregisterResponder<IScrollWheelMoveResponder>(module);
            UnregisterResponder<IScrollWheelDragResponder>(module);
            UnregisterResponder<IEmptySpaceClickResponder>(module);
            UnregisterResponder<IFlowchartChangeResponder>(module);
            UnregisterResponder<IBlocksCopiedResponder>(module);
            UnregisterResponder<IPreBlockDeletionResponder>(module);
            UnregisterResponder<IBlockSelectionResponder>(module);
            UnregisterResponder<ICommandSelectionResponder>(module);
            UnregisterResponder<IWindowPanResponder>(module);

            UnregisterResponder<IBlockCreatedResponder>(module);
            UnregisterResponder<IPostBlockDeletionResponder>(module);
            UnregisterResponder<IBlockClickResponder>(module);

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
        public void NotifyLeftClick(Vector2 position) =>
            Broadcast<ILeftClickResponder>(res => res.OnLeftClick(position));

        public void NotifyRightClick(Vector2 position) =>
            Broadcast<IRightClickResponder>(res => res.OnRightClick(position));

        public void NotifyDoubleClick(Vector2 position) =>
            Broadcast<IDoubleClickResponder>(res => res.OnDoubleClick(position));

        public void NotifyScrollWheelMoved() =>
            Broadcast<IScrollWheelMoveResponder>(res => res.OnScrollWheelMoved());

        public void NotifyScrollWheelDragged(Vector2 direction) =>
            Broadcast<IScrollWheelDragResponder>(res => res.OnScrollWheelDragged(direction));

        public void NotifyEmptySpaceClicked(Vector2 position) =>
            Broadcast<IEmptySpaceClickResponder>(res => res.OnEmptySpaceClicked(position));

        public void NotifyFlowchartChanged(Flowchart prev, Flowchart current) =>
            Broadcast<IFlowchartChangeResponder>(res => res.OnFlowchartChanged(prev, current));

        public void NotifyBlocksCopied(IList<Block> copiedBlocks) =>
            Broadcast<IBlocksCopiedResponder>(res => res.OnBlocksCopied(copiedBlocks));

        public void NotifyPreBlockDeletion(IList<Block> blocksToDelete) =>
            Broadcast<IPreBlockDeletionResponder>(res => res.OnPreBlockDeletion(blocksToDelete));

        public void NotifyBlockSelected(Block block) =>
            Broadcast<IBlockSelectionResponder>(res => res.OnBlockSelected(block));

        public void NotifyCommandSelected(Command command) =>
            Broadcast<ICommandSelectionResponder>(res => res.OnCommandSelected(command));

        public void NotifyWindowPanned() =>
            Broadcast<IWindowPanResponder>(res => res.OnWindowPanned());

        public void NotifyBlockCreated(Block block) =>
            Broadcast<IBlockCreatedResponder>(res => res.OnBlockCreated(block));

        public void NotifyPreBlockDeleted(Block block) =>
            Broadcast<IPreBlockDeletionResponder>(res => res.OnPreBlockDeletion(block));

        public void NotifyPreMultiBlockDeleted(IList<Block> blocks) =>
            Broadcast<IPreBlockDeletionResponder>(res => res.OnPreBlockDeletion(blocks));

        public void NotifyPostBlockDeleted(Block block) =>
            Broadcast<IPostBlockDeletionResponder>(res => res.OnPostBlockDeletion(block));

        public void NotifyPostMultiBlockDeleted(IList<Block> blocks) =>
            Broadcast<IPostBlockDeletionResponder>(res => res.OnPostMultiBlockDeletion(blocks));

        public void NotifyBlockClicked(Block block, Event evt) =>
            Broadcast<IBlockClickResponder>(res => res.OnBlockClicked(block, evt));
        #endregion

        private void RegisterResponder<TResponder>(IFlowchartWindowModule module)
            where TResponder : class
        {
            if (module is not TResponder responder)
            {
                return;
            }

            List<TResponder> bucket = GetOrCreateBucket<TResponder>();
            bucket.Add(responder);
        }

        private void UnregisterResponder<TResponder>(IFlowchartWindowModule module)
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