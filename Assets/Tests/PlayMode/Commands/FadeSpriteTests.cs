using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace VScriptingTests.Commands
{
    public class FadeSpriteTests : CommandTestBase<FadeSprite>
    {
        private SpriteRenderer spriteRenderer;
        private static readonly Color TargetColor = new Color(0.2f, 0.4f, 0.6f, 0.5f);

        protected override void ConfigureCommand(FadeSprite cmd)
        {
            spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.white;

            // Assign private fields via reflection
            cmdType.GetField("spriteRenderer", flags)
                .SetValue(cmd, spriteRenderer);
            cmdType.GetField("duration", flags)
                .SetValue(cmd, new FloatData(Duration));
            cmdType.GetField("targetColor", flags)
                .SetValue(cmd, new ColorData(TargetColor));
            cmdType.GetField("fadeTweener", flags)
                .SetValue(cmd, null); // triggers default adapter
        }

        protected override void AssertFinalState()
        {
            var actual = spriteRenderer.color;
            Assert.AreEqual(TargetColor.r, actual.r, Epsilon, "R channel mismatch");
            Assert.AreEqual(TargetColor.g, actual.g, Epsilon, "G channel mismatch");
            Assert.AreEqual(TargetColor.b, actual.b, Epsilon, "B channel mismatch");
            Assert.AreEqual(TargetColor.a, actual.a, Epsilon, "A channel mismatch");
        }

        // --------------------
        // waitUntilFinished = true
        // --------------------
        [UnityTest]
        public IEnumerator WaitUntilFinished_ChangesColor()
        {
            cmdType.GetField("waitUntilFinished", flags)
                .SetValue(command, new BooleanData(true));

            yield return RunBlockAndWait();
            AssertFinalState();
        }

        // --------------------
        // waitUntilFinished = false
        // --------------------
        [UnityTest]
        public IEnumerator NoWait_ContinuesImmediately_AndChangesColor()
        {
            cmdType.GetField("waitUntilFinished", flags)
                .SetValue(command, new BooleanData(false));

            bool continued = false;
            command.StartedContinue += OnFadeStartedContinue;
            void OnFadeStartedContinue(Command c)
            {
                continued = true;
                command.StartedContinue -= OnFadeStartedContinue;
            }

            flowchart.StartCoroutine(block.Execute());

            // Continue should be called immediately
            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            // Tween should still run in background
            yield return new WaitForSeconds(Duration + 0.05f);
            AssertFinalState();
        }
    }
}