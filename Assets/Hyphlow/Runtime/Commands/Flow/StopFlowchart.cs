using UnityEngine;
using System.Collections.Generic;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Stops execution of all Blocks in a Flowchart.
    /// </summary>
    [CommandInfo("Flow", 
                 "Stop Flowchart", 
                 "Stops execution of all Blocks in a Flowchart")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class StopFlowchart : Command
    {       
        [Tooltip("Stop all executing Blocks in the Flowchart that contains this command")]
        [SerializeField] protected bool stopParentFlowchart;

        [Tooltip("Stop all executing Blocks in a list of target Flowcharts")]
        [SerializeField] protected List<Flowchart> targetFlowcharts = new List<Flowchart>();

        #region Public members

        public override void OnEnter()
        {
            var flowchart = GetFlowchart();

            for (int i = 0; i < targetFlowcharts.Count; i++)
            {
                var f = targetFlowcharts[i];
                f.StopAllBlocks();
            }

            //current block and command logic doesn't require it in this order but it makes sense to
            // stop everything but yourself first
            if (stopParentFlowchart)
            {
                flowchart.StopAllBlocks();
            }

            //you might not be stopping this flowchart so keep going
            Continue();
        }

        public override bool IsReorderableArray(string propertyName)
        {
            if (propertyName == "targetFlowcharts")
            {
                return true;
            }

            return false;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        #endregion
    }
}