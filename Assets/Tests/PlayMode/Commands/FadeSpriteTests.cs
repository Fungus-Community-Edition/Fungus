using AtMycelia.Hyphlow;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AtMycelia.Amanita.VScripting;

namespace VScriptingTests.Commands
{
    public class FadeSpriteTests : CommandTestBase<FadeSprite>
    {
        private SpriteRenderer spriteRenderer;
        private static readonly Color TargetColor = new Color(0.2f, 0.4f, 0.6f, 0.5f);

        protected override void ConfigureCommand(FadeSprite cmd)
        {
            spriteRenderer = _go.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.white;

            // Assign private fields via reflection
            _cmdType.GetField("spriteRenderer", _flags)
                .SetValue(cmd, spriteRenderer);
            _cmdType.GetField("duration", _flags)
                .SetValue(cmd, new FloatData(_duration));
            _cmdType.GetField("targetColor", _flags)
                .SetValue(cmd, new ColorData(TargetColor));
            _cmdType.GetField("fadeTweener", _flags)
                .SetValue(cmd, null); // triggers default adapter
        }

        protected override void AssertFinalState()
        {
            var actual = spriteRenderer.color;
            Assert.AreEqual(TargetColor.r, actual.r, _epsilon, "R channel mismatch");
            Assert.AreEqual(TargetColor.g, actual.g, _epsilon, "G channel mismatch");
            Assert.AreEqual(TargetColor.b, actual.b, _epsilon, "B channel mismatch");
            Assert.AreEqual(TargetColor.a, actual.a, _epsilon, "A channel mismatch");
        }

        // --------------------
        // waitUntilFinished = true
        // --------------------
        [UnityTest]
        public IEnumerator WaitUntilFinished_ChangesColor()
        {
            _cmdType.GetField("waitUntilFinished", _flags)
                .SetValue(_command, new BooleanData(true));

            yield return RunBlockAndWait();
            AssertFinalState();
        }

        // --------------------
        // waitUntilFinished = false
        // --------------------
        [UnityTest]
        public IEnumerator NoWait_ContinuesImmediately_AndChangesColor()
        {
            _cmdType.GetField("waitUntilFinished", _flags)
                .SetValue(_command, new BooleanData(false));

            bool continued = false;
            _command.StartedContinue += OnFadeStartedContinue;
            void OnFadeStartedContinue(Command c)
            {
                continued = true;
                _command.StartedContinue -= OnFadeStartedContinue;
            }

            _flowchart.StartCoroutine(_block.Execute());

            // Continue should be called immediately
            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            // Tween should still run in background
            yield return new WaitForSeconds(_duration + 0.05f);
            AssertFinalState();
        }
    }
}