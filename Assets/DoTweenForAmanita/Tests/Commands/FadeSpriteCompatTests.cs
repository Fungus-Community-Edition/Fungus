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
    public class FadeSpriteCompatTests : CommandTestBase<FadeSprite>
    {
        private SpriteRenderer spriteRenderer;
        private static readonly Color TargetColor = new Color(0.2f, 0.4f, 0.6f, 0.5f);

        protected override void ConfigureCommand(FadeSprite cmd)
        {
            spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.white;

            // Assign private fields via reflection
            typeof(FadeSprite).GetField("spriteRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, spriteRenderer);
            typeof(FadeSprite).GetField("_duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, new FloatData(Duration));
            typeof(FadeSprite).GetField("_targetColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, new ColorData(TargetColor));
            typeof(FadeSprite).GetField("fadeTweener", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, ScriptableObject.CreateInstance<AmaniDoTweenAdapter>());
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
            typeof(FadeSprite).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(command, true);

            yield return RunBlockAndWait();
            AssertFinalState();
        }

        // --------------------
        // waitUntilFinished = false
        // --------------------
        [UnityTest]
        public IEnumerator NoWait_ContinuesImmediately_AndChangesColor()
        {
            typeof(FadeSprite).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(command, false);

            bool continued = false;
            command.StartedContinue += OnFadeStartedContinue;
            void OnFadeStartedContinue(Command c)
            {
                continued = true;
                command.StartedContinue -= OnFadeStartedContinue;
            }

            flowchart.StartCoroutine(block.Execute());

            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            yield return new WaitForSeconds(Duration + 0.05f);
            AssertFinalState();
        }
    }
}