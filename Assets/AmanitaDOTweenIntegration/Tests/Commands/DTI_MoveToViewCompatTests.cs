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
    // DTI is short for DOTween Integration
    public class DTI_MoveToViewCompatTests : DTI_CommandTestBase<MoveToView>
    {
        private Camera cameraGO;
        private View targetView;

        protected override void ConfigureCommand(MoveToView cmd)
        {
            var camGO = new GameObject("TestCamera");
            cameraGO = camGO.AddComponent<Camera>();
            cameraGO.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            cameraGO.orthographicSize = 5f;

            var viewGO = new GameObject("TargetView");
            targetView = viewGO.AddComponent<View>();
            targetView.transform.position = new Vector3(10f, 5f, -20f);
            targetView.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
            targetView.ViewSize = 3f;

            Type cmdType = typeof(MoveToView);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            cmdType.GetField("targetCamera", flags)
                .SetValue(cmd, cameraGO);
            cmdType.GetField("targetView", flags)
                .SetValue(cmd, targetView);
            cmdType.GetField("duration", flags)
                .SetValue(cmd, Duration);
            cmdType.GetField("waitUntilFinished", flags)
                .SetValue(cmd, true);

            cmdType.GetField("orthoSizeTweener", flags)
                .SetValue(cmd, adapter);
            cmdType.GetField("posTweener", flags)
                .SetValue(cmd, adapter);
            cmdType.GetField("rotTweener", flags)
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

            // Let's not worry about the z pos. By default, the CameraManager doesn't pan
            // with the z pos in mind
            Vector3 expectedPos = targetView.transform.position;
            expectedPos.z = cameraGO.transform.position.z;

            Assert.That(cameraGO.transform.position, Is.EqualTo(expectedPos).Using(vec3Comparer), "Position mismatch");
            Assert.That(cameraGO.transform.rotation, Is.EqualTo(targetView.transform.rotation).Using(quatComparer), "Rotation mismatch");
            Assert.AreEqual(targetView.ViewSize, cameraGO.orthographicSize, Epsilon, "Ortho size mismatch");
        }

        [UnityTest]
        public IEnumerator WaitUntilFinished_MovesCameraToView()
        {
            yield return RunBlockAndWait();
            AssertFinalState();
        }

        [UnityTest]
        public IEnumerator NoWait_ContinuesImmediately_AndMovesCamera()
        {
            typeof(MoveToView).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
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