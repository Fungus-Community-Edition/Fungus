using AtMycelia.Amanita;
using AtMycelia.Hyphlow;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AtMycelia.Amanita.VScripting;

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
            var durationData = (FloatData)_cmdType.GetField("_duration", _flags).GetValue(cmd);
            durationData.Value = _duration;

            var targetAlphaData = (FloatData)_cmdType.GetField("_targetAlpha", _flags).GetValue(cmd);
            targetAlphaData.Value = 0.75f;

            var waitUntilFinishedData = (BooleanData)_cmdType.GetField("_waitUntilFinished", _flags).GetValue(cmd);
            waitUntilFinishedData.Value = true;

            _cmdType.GetField("fadeTweener", _flags)
                .SetValue(cmd, null); // triggers default adapter
        }

        protected override void AssertFinalState()
        {
            Assert.AreEqual(0.75f, cameraManager.ScreenOpacity, _epsilon, "Fade alpha mismatch");
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
            var waitUntilFinishedData = (BooleanData)_cmdType.GetField("_waitUntilFinished", _flags).GetValue(_command);
            waitUntilFinishedData.Value = false;

            bool continued = false;
            _command.StartedContinue += _ => continued = true;

            _flowchart.StartCoroutine(_block.Execute());

            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            yield return new WaitForSeconds(_duration + 0.05f);
            AssertFinalState();
        }
    }
}