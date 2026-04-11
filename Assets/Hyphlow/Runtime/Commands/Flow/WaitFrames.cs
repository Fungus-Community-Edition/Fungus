using UnityEngine;
using System.Collections;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Waits for a number of frames before executing the next command in the block.
    /// </summary>
    [CommandInfo("Flow", 
                 "Wait Frames", 
                 "Waits for a number of frames before executing the next command in the block.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class WaitFrames : Command
    {
        [Tooltip("Number of frames to wait for")]
        [SerializeField] protected IntegerData frameCount = new IntegerData(1);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(frameCount);
        }

        protected virtual IEnumerator WaitForFrames()
        {
            int count = frameCount.Value;
            while (count > 0)
            {
                yield return new WaitForEndOfFrame();
                count--;
            }

            Continue();
        }

        #region Public members

        public override void OnEnter()
        {
            StartCoroutine(WaitForFrames());
        }

        public override string GetSummary()
        {
            return frameCount.Value.ToString() + " frames";
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return frameCount.integerRef == variable || base.HasReference(variable);
        }

        #endregion
    }
}
