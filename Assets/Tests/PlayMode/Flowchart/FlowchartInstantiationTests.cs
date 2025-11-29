using Amanita;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace VScriptingTests.FlowchartLifecycle
{
    public class FlowchartInstantiationTests
    {
        [UnityTest]
        public IEnumerator Flowchart_AssignsUniqueId_OnEnable()
        {
            // Arrange
            CommonSetup();

            var go = new GameObject("Test_Flowchart_UniqueId");
            var fc = go.AddComponent<Flowchart>();

            // Act: activate and wait a frame for Awake/OnEnable to run
            go.SetActive(true);
            yield return null;

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(fc.UniqueId), "Flowchart should have a non-empty UniqueId after OnEnable.");
        }

        private void CommonSetup()
        {
            AmanitaManager.EnsureExists();
        }

        [UnityTest]
        public IEnumerator Flowchart_RegistersInCachedFlowcharts_OnEnable()
        {
            // Arrange
            CommonSetup();

            var go = new GameObject("Test_Flowchart_Cache_Add");
            var fc = go.AddComponent<Flowchart>();

            // Act
            go.SetActive(true);
            yield return null;

            // Assert
            Assert.IsTrue(Flowchart.CachedFlowcharts.Contains(fc), "Flowchart should be present in CachedFlowcharts after OnEnable.");
        }

        [UnityTest]
        public IEnumerator Flowchart_RemovesFromCachedFlowcharts_OnDisableOrDestroy()
        {
            // Arrange
            CommonSetup();

            var go = new GameObject("Test_Flowchart_Cache_Remove");
            var fc = go.AddComponent<Flowchart>();

            go.SetActive(true);
            yield return null;
            Assert.IsTrue(Flowchart.CachedFlowcharts.Contains(fc), "Precondition failed: Flowchart not added to cache.");

            // Act: disable first to trigger OnDisable, then destroy to ensure cleanup
            go.SetActive(false);
            yield return null;
            Assert.IsFalse(Flowchart.CachedFlowcharts.Contains(fc), "Flowchart should be removed from CachedFlowcharts on OnDisable.");

            // Re-enable to re-add, then destroy to verify removal via OnDestroy/cleanup
            go.SetActive(true);
            yield return null;
            Assert.IsTrue(Flowchart.CachedFlowcharts.Contains(fc), "Precondition failed: Flowchart not re-added to cache.");

            Object.Destroy(go);
            yield return null; // allow destroy to complete

            Assert.IsFalse(Flowchart.CachedFlowcharts.Contains(fc), "Flowchart should be removed from CachedFlowcharts after destruction.");
        }

        [UnityTest]
        public IEnumerator Flowchart_UIModelOwner_IsSet_OnAwake()
        {
            // Arrange
            CommonSetup();

            var go = new GameObject("Test_Flowchart_UIModelOwner");
            var fc = go.AddComponent<Flowchart>();

            // Act
            go.SetActive(true);
            yield return null;

            // Assert: Awake should assign UIModel.Owner to this GameObject
            Assert.IsNotNull(fc.UIModel, "Flowchart.UIModel should not be null after Awake.");
            Assert.AreEqual(go, fc.UIModel.Owner, "Flowchart should register itself as UIModel.Owner in Awake.");
        }
    }
}