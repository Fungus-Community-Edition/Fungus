using UnityEngine;

namespace Amanita.VScripting.EventHandlers
{
    /// <summary>
    /// The block will execute when the Flowchart game object is enabled.
    /// </summary>
    [EventHandlerInfo("Scene",
                      "Flowchart Enabled",
                      "The block will execute when the Flowchart game object is enabled.")]
    [AddComponentMenu("")]
    public class FlowchartEnabled : EventHandler
    {   
        protected override void OnEnable()
        {
            base.OnEnable();
            // Blocks use coroutines to schedule command execution, but Unity's coroutines are
            // sometimes unreliable when enabling / disabling objects.
            // To workaround this we execute the block on the next frame.
            Invoke(nameof(DoEvent), 0);
        }

        protected virtual void DoEvent()
        {
            ExecuteBlock();
        }
    }
}
