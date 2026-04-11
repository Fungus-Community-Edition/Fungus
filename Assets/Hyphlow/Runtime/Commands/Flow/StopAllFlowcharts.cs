using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Stops execution of all Flowcharts in the scene.
    /// </summary>
    [CommandInfo("Flow",
                 "Stop All Flowcharts",
                 "Stops execution of all Blocks in all Flowcharts in the scene.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class StopAllFlowcharts : Command
    {
        [Tooltip("Flowcharts with any of these GameObjects as their target will not be stopped.")]
        [SerializeField] private GameObjectData[] _exceptions;

        [Tooltip("Whether to exclude Flowcharts that are set to NOT be destroyed on load.")]
        [SerializeField] private BooleanData _excludePersistentFlowcharts = new BooleanData(true);

        public override void OnEnter()
        {
            base.OnEnter();

            var inScene = FlowchartRegistry.GetSceneFlowcharts();
            for (int i = 0; i < inScene.Count; i++)
            {
                var fChart = inScene[i];
                if (ShouldStopFlowchart(fChart))
                {
                    fChart.StopAllBlocks();
                }
            }

            if (!ShouldStopFlowchart(this.GetFlowchart()))
            {
                Continue();
            }
        }

        private bool ShouldStopFlowchart(Flowchart flowchart)
        {
            if (_excludePersistentFlowcharts.Value && flowchart.gameObject.scene.name == null)
            {
                return false;
            }

            for (int i = 0; i < _exceptions.Length; i++)
            {
                var exception = _exceptions[i];
                if (exception.Value == flowchart.gameObject)
                {
                    return false;
                }
            }
            return true;
        }
    }
}