using Amanita;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils; // for equality comparers
using Type = System.Type;
using System.Reflection;

namespace CommandCompat
{
    public class LTI_FadeToViewCompatTests : LTI_CommandTestBase<FadeToView>
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
            Type cmdType = typeof(FadeToView);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            cmdType.GetField("targetCamera", flags)
                .SetValue(cmd, cameraGO);
            cmdType.GetField("targetView", flags)
                .SetValue(cmd, targetView);
            cmdType.GetField("duration", flags)
                .SetValue(cmd, Duration);
            cmdType.GetField("waitUntilFinished", flags)
                .SetValue(cmd, true);

            // Inject AmaniLeanTweenAdapter for all tweeners
            cmdType.GetField("doFadeTween", flags)
                .SetValue(cmd, adapter);
            cmdType.GetField("doOrthoSizeTween", flags)
                .SetValue(cmd, adapter);
            cmdType.GetField("doPosTween", flags)
                .SetValue(cmd, adapter);
            cmdType.GetField("doRotTween", flags)
                .SetValue(cmd, adapter);
        }

        protected override void AssertFinalState()
        {
            var vec3Comparer = new Vector3EqualityComparer(Epsilon);
            var quatComparer = new QuaternionEqualityComparer(Epsilon);

            // Not going to worry about the z pos here
            Vector3 expectedPos = targetView.transform.position;
            expectedPos.z = cameraGO.transform.position.z;

            Assert.That(cameraGO.transform.position, Is.EqualTo(expectedPos).Using(vec3Comparer), "Position mismatch");
            Assert.That(cameraGO.transform.rotation, Is.EqualTo(targetView.transform.rotation).Using(quatComparer), "Rotation mismatch");
            Assert.AreEqual(targetView.ViewSize, cameraGO.orthographicSize, Epsilon, "Ortho size mismatch");
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            Object.DestroyImmediate(cameraGO);
            Object.DestroyImmediate(targetView.gameObject);
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