using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class FlowchartWindowInputHandler : IInputProcessor, IDisposable
    {
        public static readonly float RightClickTolerance = 5f;
        public static readonly float MinZoomValue = 0.25f;
        public static readonly float MaxZoomValue = 1f;
        public static readonly float GridLineSpacingSize = 120;
        public static readonly float GridObjectSnap = 20;

        public FlowchartWindowInputHandler(params IUGUIEventHandler[] subhandlers)
        {
            this.subhandlers = subhandlers;
        }

        protected IList<IUGUIEventHandler> subhandlers;

        public virtual bool Process(Event currentEv, FlowchartContext flowchartCtx)
        {
            foreach (var elem in subhandlers)
                if (elem.Handle(currentEv, flowchartCtx))
                    return true;
            return false;

        }

        protected static readonly int leftMouseButton = 0;
        protected FlowchartContext currentContext;

        public virtual void Dispose()
        {
            for (var i = 0; i < subhandlers.Count; i++)
            {
                var disposableHandler = subhandlers[i] as IDisposable;
                disposableHandler?.Dispose();
            }
        }

    }

}