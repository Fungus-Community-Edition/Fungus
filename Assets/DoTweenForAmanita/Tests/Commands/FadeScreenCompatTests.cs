using Amanita;
using Amanita.ThirdPartyInt.DGDOTween;
using Amanita.Tweening;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace CommandCompat
{
    public class FadeScreenCompatTests : CommandTestBase<FadeScreen>
    {
        private CameraManager cameraManager;

        protected override void ConfigureCommand(FadeScreen cmd)
        {
            // Ensure CameraManager exists
            cameraManager = AmanitaManager.S.CameraManager;
            cameraManager.ScreenFadeTexture = null;

            // Assign private fields via reflection
            typeof(FadeScreen).GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, Duration);
            typeof(FadeScreen).GetField("targetAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, 0.75f);
            typeof(FadeScreen).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, true);
            typeof(FadeScreen).GetField("fadeTweener", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, ScriptableObject.CreateInstance<AmaniDoTweenAdapter>());
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

            flowchart.StartCoroutine(block.Execute());

            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            yield return new WaitForSeconds(Duration + 0.05f);
            AssertFinalState();
        }
    }
}