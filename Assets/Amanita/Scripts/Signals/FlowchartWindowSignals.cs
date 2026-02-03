using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class FlowchartWindowSignals
    {
        /// <summary>
        /// Invoked when the user left-clicks inside the flowchart window just once.
        /// </summary>
        public static Action<Vector2> LeftClicked = delegate { };
        public static Action<Vector2> RightClicked = delegate { };

        /// <summary>
        /// Invoked when the user double-left-clicks inside the flowchart window.
        /// </summary>
        public static Action<Vector2> DoubleClicked = delegate { };

        public static Action ScrollWheelMoved = delegate { };

        /// <summary>
        /// Invoked when the user drags the scroll wheel inside the flowchart window. The argument
        /// passed is the mouse movement since the last event.
        /// </summary>
        public static Action<Vector2> ScrollWheelDragged = delegate { };

        public static Action<Vector2> EmptySpaceClicked = delegate { };
        public static Action<Flowchart, Flowchart> ChangedFlowchart = delegate { };
        public static Action<IList<Block>> BlocksCopied = delegate { };
        public static Action<Command> CommandSelected = delegate { };
        public static Action WindowPanned = delegate { };
    }

    // Interfaces for subscribing to flowchart window signals
    public interface ILeftClickResponder
    {
        void OnLeftClick(Vector2 position);
    }

    public interface IRightClickResponder
    {
        void OnRightClick(Vector2 position);
    }

    public interface IDoubleClickResponder
    {
        void OnDoubleClick(Vector2 position);
    }

    public interface IScrollWheelMoveResponder
    {
        void OnScrollWheelMoved();
    }

    public interface IScrollWheelDragResponder
    {
        void OnScrollWheelDragged(Vector2 direction);
    }

    public interface IEmptySpaceClickResponder
    {
        void OnEmptySpaceClicked(Vector2 pos);
    }

    public interface IFlowchartChangeResponder
    {
        void OnFlowchartChanged(Flowchart oldFc, Flowchart newFc);
    }

    public interface IBlocksCopiedResponder
    {
        void OnBlocksCopied(IList<Block> blocks);
    }

    public interface IPreBlockDeletionResponder
    {
        void OnPreBlockDeletion(IList<Block> blocks);
        void OnPreBlockDeletion(Block block);
    }

    public interface IPostBlockDeletionResponder
    {
        void OnPostMultiBlockDeletion(IList<Block> blocks);
        void OnPostBlockDeletion(Block block);
    }

    public interface IBlockSelectionResponder
    {
        void OnBlockSelected(Block block);
    }

    public interface ICommandSelectionResponder
    {
        void OnCommandSelected(Command command);
    }

    public interface IWindowPanResponder
    {
        void OnWindowPanned();
    }

}