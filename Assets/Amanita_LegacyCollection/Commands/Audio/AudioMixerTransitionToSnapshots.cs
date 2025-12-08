using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Amanita.VScripting
{
    /// <summary>
    /// Calls the mix TransitionToSnapshots.
    /// </summary>
    [CommandInfo("Audio",
                 "Mixer Transition To Snapshots",
                     "Calls the mix TransitionToSnapshots.")]
    [AddComponentMenu("")]
    public class AudioMixerTransitionToSnapshots : Command, ICollectionCompatible
    {
        [SerializeField] protected AudioMixerData mixer;
        [SerializeField] protected AudioMixerSnapshot[] snapShots;
        [SerializeField] protected float[] floatArr;

        [Tooltip("Optional, if set values will be copied into floatArr before TransitionToSnapshots is called")]
        [SerializeField] protected CollectionData floatCollection;

        [SerializeField] protected FloatData timeToTransition;

        [Tooltip("Wait for the transition to complete before continuing.")]
        [SerializeField] protected bool waitUntilFinished = false;

        public override void OnEnter()
        {
            if (floatCollection.Value != null)
            {
                System.Array.Resize(ref floatArr, snapShots.Length);
                floatCollection.Value.CopyTo(floatArr, 0);
            }

            mixer.Value.TransitionToSnapshots(snapShots, floatArr, timeToTransition.Value);

            if (waitUntilFinished)
            {
                StartCoroutine(WaitForTransition());
            }
            else
            {
                Continue();
            }
        }

        protected IEnumerator WaitForTransition()
        {
            yield return new WaitForSeconds(timeToTransition.Value);
            Continue();
        }

        public override string GetSummary()
        {
            if (mixer.Value == null)
                return "Error: no mixer set";

            var retval =  mixer.Value.name + " " + snapShots.Length.ToString() + " in " + timeToTransition.Value.ToString() + "s";

            if (waitUntilFinished)
                retval += " waits";

            return retval;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(mixer.VarRef, variable) ||
                ReferenceEquals(floatCollection.VarRef, variable) ||
                ReferenceEquals(timeToTransition.VarRef, variable) ||
                base.HasReference(variable);
        }

        public bool IsVarCompatibleWithCollection(Variable variableInQuestion, string compatibleWith)
        {
            if (compatibleWith == "floatCollection")
                return floatCollection.Value != null && floatCollection.Value.IsElementCompatible(variableInQuestion);
            else
                return true;
        }
    }
}