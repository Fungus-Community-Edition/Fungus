using UnityEngine;
using System.Collections;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// The block will execute when the game starts playing.
    /// </summary>
    [EventHandlerInfo("",
                      "Game Started",
                      "The block will execute when the game starts playing.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class GameStarted : EventHandler
    {
        [Tooltip("Wait for a number of frames after startup before executing the Block. Can help fix startup order issues.")]
        [SerializeField] protected int waitForFrames = 1;

        public virtual void Trigger()
        {
            StartCoroutine(GameStartCoroutine());
        }

        protected virtual IEnumerator GameStartCoroutine()
        {
            int frameCount = waitForFrames;
            while (frameCount > 0)
            {
                yield return new WaitForEndOfFrame();
                frameCount--;
            }

            ExecuteBlock();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (waitForFrames <= 0)
            {
                waitForFrames = 1;
            }
        }
    }
}
