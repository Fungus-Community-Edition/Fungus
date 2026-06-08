using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils; // for equality comparers
using AtMycelia.Amanita;
using AtMycelia.Amanita.VScripting;

namespace VScriptingTests.Commands
{
    public class FadeToViewTests : CommandTestBase<FadeToView>
    {
        private Camera cameraGO;
        private View targetView;

        protected override void ConfigureCommand(FadeToView cmd)
        {
            // Camera setup
            var camGO = new GameObject("TestCamera");

            cameraGO = camGO.AddComponent<Camera>();
            cameraGO.transform.position = Vector3.zero;
            cameraGO.transform.rotation = Quaternion.identity;
            cameraGO.orthographicSize = 5f;

            // Target view setup
            var viewGO = new GameObject("TargetView");
            targetView = viewGO.AddComponent<View>();
            targetView.transform.position = new Vector3(8f, 3f, -12f);
            targetView.transform.rotation = Quaternion.Euler(10f, 30f, 0f);
            targetView.ViewSize = 2.5f;

            // Assign private fields
            typeof(FadeToView).GetField("targetCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, cameraGO);
            typeof(FadeToView).GetField("targetView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, targetView);
            typeof(FadeToView).GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, _duration);
            typeof(FadeToView).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, true);

            // Null tweeners → default adapter
            typeof(FadeToView).GetField("fadeTweener", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, null);
            typeof(FadeToView).GetField("orthoSizeTweener", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, null);
            typeof(FadeToView).GetField("posTweener", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, null);
            typeof(FadeToView).GetField("rotTweener", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, null);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            Object.DestroyImmediate(cameraGO);
            Object.Destroy(targetView);
        }

        protected override void AssertFinalState()
        {
            var vec3Comparer = new Vector3EqualityComparer(_epsilon);
            var quatComparer = new QuaternionEqualityComparer(_epsilon);

            // Not going to worry about the z pos here
            Vector3 expectedPos = targetView.transform.position;
            expectedPos.z = cameraGO.transform.position.z;

            Assert.That(cameraGO.transform.position, Is.EqualTo(expectedPos).Using(vec3Comparer), "Position mismatch");
            Assert.That(cameraGO.transform.rotation, Is.EqualTo(targetView.transform.rotation).Using(quatComparer), "Rotation mismatch");
            Assert.AreEqual(targetView.ViewSize, cameraGO.orthographicSize, _epsilon, "Ortho size mismatch");
        }

        [UnityTest]
        public IEnumerator WaitUntilFinished_FadesAndMovesToView()
        {
            yield return RunBlockAndWait();
            AssertFinalState();
        }

        [UnityTest]
        public IEnumerator NoWait_ContinuesImmediately_AndFadesAndMoves()
        {
            typeof(FadeToView).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_command, false);

            bool continued = false;
            _command.StartedContinue += _ => continued = true;

            _flowchart.ExecuteBlock(_block);

            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            yield return new WaitForSeconds(_duration + 0.05f);
            AssertFinalState();
        }
    }
}