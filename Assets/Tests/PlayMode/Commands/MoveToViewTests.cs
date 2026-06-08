using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils; // for equality comparers
using AtMycelia.Amanita;
using AtMycelia.Amanita.VScripting;

namespace VScriptingTests.Commands
{
    public class MoveToViewTests : CommandTestBase<MoveToView>
    {
        private Camera cameraGO;
        private View targetView;

        protected override void ConfigureCommand(MoveToView cmd)
        {
            var camGO = new GameObject("TestCamera");
            cameraGO = camGO.AddComponent<Camera>();
            cameraGO.transform.position = Vector3.zero;
            cameraGO.transform.rotation = Quaternion.identity;
            cameraGO.orthographicSize = 5f;

            var viewGO = new GameObject("TargetView");
            targetView = viewGO.AddComponent<View>();
            targetView.transform.position = new Vector3(10f, 5f, -20f);
            targetView.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
            targetView.ViewSize = 3f;

            typeof(MoveToView).GetField("targetCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, cameraGO);
            typeof(MoveToView).GetField("targetView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, targetView);
            typeof(MoveToView).GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, _duration);
            typeof(MoveToView).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, true);

            typeof(MoveToView).GetField("orthoSizeTweener", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, null);
            typeof(MoveToView).GetField("posTweener", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, null);
            typeof(MoveToView).GetField("rotTweener", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, null);
        }

        [UnityTest]
        public IEnumerator WaitUntilFinished_MovesCameraToView()
        {
            yield return RunBlockAndWait();
            AssertFinalState();
        }

        protected override void AssertFinalState()
        {
            var vec3Comparer = new Vector3EqualityComparer(_epsilon);
            var quatComparer = new QuaternionEqualityComparer(_epsilon);

            // Let's not worry about the z pos. By default, the CameraManager doesn't pan
            // with the z pos in mind
            Vector3 expectedPos = targetView.transform.position;
            expectedPos.z = cameraGO.transform.position.z;

            Assert.That(cameraGO.transform.position, Is.EqualTo(expectedPos).Using(vec3Comparer), "Position mismatch");
            Assert.That(cameraGO.transform.rotation, Is.EqualTo(targetView.transform.rotation).Using(quatComparer), "Rotation mismatch");
            Assert.AreEqual(targetView.ViewSize, cameraGO.orthographicSize, _epsilon, "Ortho size mismatch");
        }

        [UnityTest]
        public IEnumerator NoWait_ContinuesImmediately_AndMovesCamera()
        {
            typeof(MoveToView).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
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