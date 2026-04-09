using AtMycelia.Hyphlow.EditorUtils;
using UnityEngine;

namespace AtMycelia.Amanita.EditorUtils
{
    public interface IUGUIEventHandler
    {
        /// <summary>
        /// Try to consume this Event. Returns true if it did something.
        /// </summary>
        bool Handle(Event eventToHandle, FlowchartContext ctx);
    }
}