using Amanita;
using Amanita.DOTweenIntegration;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Reflection;

namespace CommandCompat
{
    public class FadeScreenCompatTests : CommandTestBase<FadeScreen>
    {
        private CameraManager cameraManager;

        protected override void ConfigureCommand(FadeScreen command)
        {
            // Ensure CameraManager exists
            cameraManager = AmanitaManager.S.CameraManager;
            cameraManager.ScreenFadeTexture = null;

            // Assign private fields via reflection
            Type fadeScreenType = typeof(FadeScreen);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            fadeScreenType.GetField("duration", flags)
                .SetValue(command, Duration);
            fadeScreenType.GetField("targetAlpha", flags)
                .SetValue(command, 0.75f);
            fadeScreenType.GetField("waitUntilFinished", flags)
                .SetValue(command, true);
            fadeScreenType.GetField("fadeTweener", flags)
                .SetValue(command, adapter);
            fadeScreenType.GetField("doFadeTween", flags).
                SetValue(command, adapter);
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
            typeof(FadeScreen).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
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