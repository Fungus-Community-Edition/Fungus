using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class FlowchartWindowSignals
    {
        public static Action<Vector2> LeftClicked = delegate { };
        public static Action<Vector2> RightClicked = delegate { };
        public static Action<Vector2> DoubleClicked = delegate { };

        public static Action ScrollWheelMoved = delegate { };

        public static Action<Vector2> EmptySpaceClicked = delegate { };
        public static Action<Flowchart, Flowchart> ChangedFlowchart = delegate { };
        public static Action<IList<Block>> BlocksCopied = delegate { };
        public static Action<IList<Block>> PreBlockDeletion = delegate { };
        public static Action<Block> BlockSelected = delegate { };
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