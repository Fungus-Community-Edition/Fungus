using System.Collections;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Call TransitionTo on given Snapshot.
    /// </summary>
    [CommandInfo("Audio",
                 "Mixer Snapshot Transition To",
                     "Call TransitionTo on given Snapshot.")]
    [AddComponentMenu("")]
    public class AudioMixerSnapshotTransitionTo : Command
    {
        [SerializeField] protected AudioMixerSnapshotData snapshot;
        [SerializeField] protected FloatData timeToReach;

        [Tooltip("Wait for the transition to complete before continuing.")]
        [SerializeField] protected bool waitUntilFinished = false;

        public override void OnEnter()
        {
            snapshot.Value.TransitionTo(timeToReach.Value);

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
            yield return new WaitForSeconds(timeToReach.Value);
            Continue();
        }

        public override string GetSummary()
        {
            if (snapshot.Value == null)
                return "Error: no snapshot set";

            var retval =  snapshot.Value.name + " in " + timeToReach.Value.ToString();

            if (waitUntilFinished)
                retval += " waits";

            return retval;
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return snapshot.audioMixerSnapshotRef == variable ||
                timeToReach.floatRef == variable ||
                base.HasReference(variable);
        }
    }
}