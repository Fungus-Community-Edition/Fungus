using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Amanita;

namespace VScriptingTests.Commands
{
    public class FadeScreenTests : CommandTestBase<FadeScreen>
    {
        private CameraManager cameraManager;

        protected override void ConfigureCommand(FadeScreen cmd)
        {
            // Ensure CameraManager exists
            cameraManager = AmanitaManager.S.CameraManager;
            cameraManager.ScreenFadeTexture = null;

            // Assign private fields via reflection
            cmdType.GetField("duration", flags)
                .SetValue(cmd, Duration);
            cmdType.GetField("targetAlpha", flags)
                .SetValue(cmd, 0.75f);
            cmdType.GetField("waitUntilFinished", flags)
                .SetValue(cmd, true);
            cmdType.GetField("fadeTweener", flags)
                .SetValue(cmd, null); // triggers default adapter
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
            cmdType.GetField("waitUntilFinished", flags)
                .SetValue(command, false);

            bool continued = false;
            command.StartedContinue += _ => continued = true;

            flowchart.StartCoroutine(block.Execute());

            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            yield return new WaitForSeconds(Duration + 0.05f);
            AssertFinalState();
        }
    }
}