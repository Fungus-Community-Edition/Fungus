using AtMycelia.Amanita;
using AtMycelia.Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

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

            // Assign variable-backed fields via reflection
            var durationData = (FloatData)cmdType.GetField("_duration", flags).GetValue(cmd);
            durationData.Value = Duration;

            var targetAlphaData = (FloatData)cmdType.GetField("_targetAlpha", flags).GetValue(cmd);
            targetAlphaData.Value = 0.75f;

            var waitUntilFinishedData = (BooleanData)cmdType.GetField("_waitUntilFinished", flags).GetValue(cmd);
            waitUntilFinishedData.Value = true;

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
            var waitUntilFinishedData = (BooleanData)cmdType.GetField("_waitUntilFinished", flags).GetValue(command);
            waitUntilFinishedData.Value = false;

            bool continued = false;
            command.StartedContinue += _ => continued = true;

            flowchart.StartCoroutine(block.Execute());

            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            yield return new WaitForSeconds(Duration + 0.05f);
            AssertFinalState();
        }
    }
}