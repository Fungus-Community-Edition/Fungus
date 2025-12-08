using Amanita;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace CommandCompat
{
    public class LTI_FadeScreenCompatTests : LTI_CommandTestBase<FadeScreen>
    {
        private CameraManager cameraManager;

        protected override void ConfigureCommand(FadeScreen command)
        {
            // Ensure CameraManager exists and reset fade texture for deterministic behaviour
            cameraManager = AmanitaManager.S.CameraManager;
            cameraManager.ScreenFadeTexture = null;

            // Assign private fields via reflection
            CmdType.GetField("duration", flags)
                .SetValue(command, Duration);
            CmdType.GetField("targetAlpha", flags)
                .SetValue(command, 0.75f);
            CmdType.GetField("waitUntilFinished", flags)
                .SetValue(command, true);
            CmdType.GetField("fadeTweener", flags)
                .SetValue(command, adapter);
            CmdType.GetField("doFade", flags)
                .SetValue(command, adapter);
        }

        protected override void AssertFinalState()
        {
            Assert.AreEqual(0.75f, cameraManager.ScreenOpacity, Epsilon, "Fade alpha mismatch");
        }

        [UnityTest]
        public IEnumerator WaitUntilFinished_FadesScreen()
        {
            yield return RunBlockAndWait();
            AssertFinalState();
        }

        [UnityTest]
        public IEnumerator NoWait_ContinuesImmediately_AndFadesScreen()
        {
            CmdType.GetField("waitUntilFinished", flags)
                .SetValue(command, false);

            bool continued = false;
            command.StartedContinue += _ => continued = true;

            flowchart.ExecuteBlock(block);

            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            yield return new WaitForSeconds(Duration + 0.05f);
            AssertFinalState();
        }
    }
}