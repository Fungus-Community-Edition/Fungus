using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Encapsulates viewport input handlers (pan/zoom/reset/selection/drag) for the UITK flowchart window.
    /// </summary>
    public sealed class MainViewportManager : IFlowchartWindowModule,
        IScrollWheelDragResponder, IRightMouseDragResponder,
        IScrollWheelMoveResponder, IFlowchartChangeResponder,
        ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseDragEndResponder,
        IEmptySpaceLeftMouseDownResponder, IEmptySpaceLeftMouseUpResponder, IEmptySpaceLeftClickResponder,
        ILeftMouseDownResponder, ILeftMouseUpResponder, IBlockClickResponder, IBlockCreatedResponder
    {
        public int Priority { get; set; } = 0;

        public MainViewportManager(FlowchartContext context, float minZoom, float maxZoom)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }


            _hitDetector = new HitDetector();
            _panHandler = new PanHandler(context);
            _zoomHandler = new ZoomHandler(context, minZoom, maxZoom);
            _scrollPosResetter = new ScrollPosResetter(context);
            _boxSelectionHandler = new SelectionBoxDragTrackerUitk(context);
            _blockDragHandler = new BlockDragHandler(context);
            _singleClickBlockSelector = new SingleSelectionHandler(context);
            

            _submodules.Add(_hitDetector);
            _submodules.Add(_panHandler);
            _submodules.Add(_zoomHandler);
            _submodules.Add(_scrollPosResetter);
            _submodules.Add(_boxSelectionHandler);
            _submodules.Add(_blockDragHandler);
            _submodules.Add(_singleClickBlockSelector);
        }

        private readonly PanHandler _panHandler;
        private readonly ZoomHandler _zoomHandler;
        private readonly ScrollPosResetter _scrollPosResetter;
        private readonly SelectionBoxDragTrackerUitk _boxSelectionHandler;
        private readonly BlockDragHandler _blockDragHandler;
        private readonly SingleSelectionHandler _singleClickBlockSelector;
        private readonly HitDetector _hitDetector;

        private readonly IList<IFlowchartWindowModule> _submodules = new List<IFlowchartWindowModule>();
        public IReadOnlyList<IFlowchartWindowModule> Submodules => (IReadOnlyList<IFlowchartWindowModule>)_submodules;
        private bool _isDisposed;

        public void Initialize(FlowchartWindow window)
        {
            for (int i = 0; i < _submodules.Count; i++)
            {
                _submodules[i].Initialize(window);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            for (int i = 0; i < _submodules.Count; i++)
            {
                _submodules[i].Dispose();
            }
        }

        public void OnGUI(Event currentEvent)
        {
            _scrollPosResetter.OnGUI(currentEvent);
        }

        public void OnScrollWheelDragged(Vector2 direction)
        {
            Forward<IScrollWheelDragResponder>(r => r.OnScrollWheelDragged(direction));
        }

        public void OnRightMouseDragged(PointerEventInfo info, Event evt)
        {
            Forward<IRightMouseDragResponder>(r => r.OnRightMouseDragged(info, evt));
        }

        public void OnScrollWheelMoved()
        {
            Forward<IScrollWheelMoveResponder>(r => r.OnScrollWheelMoved());
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart current)
        {
            Forward<IFlowchartChangeResponder>(r => r.OnFlowchartChanged(previous, current));
        }

        public void OnLeftMouseDragStarted(PointerEventInfo info, Event evt)
        {
            Forward<ILeftMouseDragStartResponder>(r => r.OnLeftMouseDragStarted(info, evt));
        }

        public void OnLeftMouseDragged(PointerEventInfo info, Event evt)
        {
            Forward<ILeftMouseDragResponder>(r => r.OnLeftMouseDragged(info, evt));
        }

        public void OnLeftMouseDragEnded(PointerEventInfo info, Event evt)
        {
            Forward<ILeftMouseDragEndResponder>(r => r.OnLeftMouseDragEnded(info, evt));
        }

        public void OnEmptySpaceLeftMouseDown(PointerEventInfo info, Event evt)
        {
            Forward<IEmptySpaceLeftMouseDownResponder>(r => r.OnEmptySpaceLeftMouseDown(info, evt));
        }

        public void OnEmptySpaceLeftMouseUp(PointerEventInfo info, Event evt)
        {
            Forward<IEmptySpaceLeftMouseUpResponder>(r => r.OnEmptySpaceLeftMouseUp(info, evt));
        }

        public void OnLeftMouseDown(PointerEventInfo info)
        {
            Forward<ILeftMouseDownResponder>(r => r.OnLeftMouseDown(info));
        }

        public void OnLeftMouseUp(PointerEventInfo info, Event evt)
        {
            Forward<ILeftMouseUpResponder>(r => r.OnLeftMouseUp(info, evt));
        }

        private void Forward<TResponder>(Action<TResponder> action)
            where TResponder : class
        {
            for (int i = 0; i < _submodules.Count; i++)
            {
                if (_submodules[i] is not TResponder responder)
                {
                    continue;
                }

                action(responder);
            }
        }

        public void OnBlockClicked(Block block, Event evt)
        {
            Forward<IBlockClickResponder>(r => r.OnBlockClicked(block, evt));
        }

        public void OnBlockCreated(Block block)
        {
            Forward<IBlockCreatedResponder>(r => r.OnBlockCreated(block));
        }

        public void OnEmptySpaceLeftClicked(PointerEventInfo info)
        {
            Forward<IEmptySpaceLeftClickResponder>(r => r.OnEmptySpaceLeftClicked(info));
        }
    }
}