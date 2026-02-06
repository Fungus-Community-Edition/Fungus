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

        public static Action<Vector2, Event> EmptySpaceLeftMouseDown = delegate { };
        public static Action<Vector2, Event> EmptySpaceLeftMouseUp = delegate { };
        public static Action<Vector2, Event> LeftMouseUp = delegate { };

        public static Action<Vector2> EmptySpaceClicked = delegate { };
        public static Action<Flowchart, Flowchart> ChangedFlowchart = delegate { };
        public static Action WindowPanned = delegate { };

        public static Action<Vector2, Event> LeftMouseDragStarted = delegate { };
        public static Action<Vector2, Event> LeftMouseDragged = delegate { };
        public static Action<Vector2, Event> LeftMouseDragEnded = delegate { };

        public static Action<Vector2, Event> RightMouseDragStarted = delegate { };
        public static Action<Vector2, Event> RightMouseDragged = delegate { };
        public static Action<Vector2, Event> RightMouseDragEnded = delegate { };
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

    public interface IEmptySpaceLeftMouseDownResponder
    {
        void OnEmptySpaceLeftMouseDown(Vector2 pos, Event evt);
    }

    public interface ILeftMouseUpResponder
    {
        void OnLeftMouseUp(Vector2 pos, Event evt);
    }

    public interface IEmptySpaceLeftMouseUpResponder
    {
        void OnEmptySpaceLeftMouseUp(Vector2 pos, Event evt);
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
        void OnPostBlockDeletion(uint blockId);
    }

    public interface IPostMultiBlockDeletionResponder
    {
        void OnPostMultiBlockDeletion(IList<uint> blockIds);
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

    public interface ILeftMouseDragStartResponder
    {
        void OnLeftMouseDragStarted(Vector2 startPos, Event evt);
    }

    public interface ILeftMouseDragResponder
    {
        void OnLeftMouseDragged(Vector2 direction, Event evt);
    }

    public interface ILeftMouseDragEndResponder
    {
        void OnLeftMouseDragEnded(Vector2 endPos, Event evt);
    }

    public interface IRightMouseDragStartResponder
    {
        void OnRightMouseDragStarted(Vector2 startPos, Event evt);
    }

    public interface IRightMouseDragResponder
    {
        void OnRightMouseDragged(Vector2 direction, Event evt);
    }

    public interface IRightMouseDragEndResponder
    {
        void OnRightMouseDragEnded(Vector2 endPos, Event evt);
    }

}