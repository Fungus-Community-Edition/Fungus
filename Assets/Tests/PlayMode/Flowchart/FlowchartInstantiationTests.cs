using Amanita;
using Amanita.VScripting;
using Amanita.VScripting.EventHandlers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;
using System.Reflection;
using Type = System.Type;
using System.Linq;

namespace VScriptingTests.FlowchartLifecycle
{
    public class FlowchartInstantiationTests
    {
        [UnityTest]
        public IEnumerator Flowchart_AssignsUniqueId_OnEnable()
        {
            // Arrange
            // Act: activate and wait a frame for Awake/OnEnable to run
            yield return null;

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(testFc.UniqueId), "Flowchart should have a non-empty UniqueId after OnEnable.");
        }

        [SetUp]
        public void Setup()
        {
            AmanitaManager.EnsureExists();
            AmanitaManager.S.Init();
            fcHolder = new GameObject("TestFlowchart_InstantiationTestHolder");
            toDestroyOnTearDown.Add(fcHolder);
            testFc = fcHolder.AddComponent<Flowchart>();
            testFc.IsTestOnly = true;
            Block blockAdded = testFc.CreateBlock(new Vector2(0, 0));
            TestGameStarted testGameStarted = fcHolder.AddComponent<TestGameStarted>();
            blockAdded._EventHandler = testGameStarted;
            testGameStarted.ParentBlock = blockAdded;

        }

        private GameObject fcHolder;
        private Flowchart testFc;
        private readonly IList<UnityObj> toDestroyOnTearDown = new List<UnityObj>();

        [TearDown]
        public void TearDown()
        {
            EventSystem evSys = UnityObj.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            // ^Might've been created by the fc during the test
            toDestroyOnTearDown.Add(evSys.gameObject);

            if (testFc != null)
            {
                testFc.OnTearDown();
            }

            foreach (var obj in toDestroyOnTearDown)
            {
                if (obj != null)
                {
                    UnityObj.Destroy(obj);
                }
            }

            toDestroyOnTearDown.Clear();
            fcHolder = null;
            testFc = null;
        }

        private readonly Type fcType = typeof(Flowchart);

        [UnityTest]
        public IEnumerator Flowchart_RegistersInCachedFlowcharts_OnEnable()
        {
            // Arrange
            yield return null;

            // Assert
            var flowcharts = AmanitaManager.S.FlowchartsInScene;
            Assert.IsTrue(flowcharts.Contains(testFc), "Flowchart should be present in CachedFlowcharts after OnEnable.");

        }

        [UnityTest]
        public IEnumerator Flowchart_RemovesFromCachedFlowcharts_OnDisableOrDestroy()
        {
            yield return null;
            var cachedFcs = AmanitaManager.S.FlowchartsInScene;
            Assert.IsTrue(cachedFcs.Contains(testFc), 
                "Precondition failed: Flowchart not added to cache.");

            // Act: disable first to trigger OnDisable, then destroy to ensure cleanup
            fcHolder.SetActive(false);
            yield return null;
            Assert.IsFalse(cachedFcs.Contains(testFc), 
                "Flowchart should be removed from CachedFlowcharts on OnDisable.");

            // Re-enable to re-add, then destroy to verify removal via OnDestroy/cleanup
            fcHolder.SetActive(true);
            yield return null;
            Assert.IsTrue(cachedFcs.Contains(testFc), 
                "Precondition failed: Flowchart not re-added to cache.");

            testFc.OnTearDown(); // ensure proper cleanup
            UnityObj.Destroy(fcHolder);
            yield return null; // allow destroy to complete

            Assert.IsFalse(cachedFcs.Contains(testFc), 
                "Flowchart should be removed from CachedFlowcharts after destruction.");
        }

        [UnityTest]
        public IEnumerator Flowchart_UIModelOwner_IsSet_OnAwake()
        {
            // Arrange
            yield return null;

            // Assert: Awake should assign UIModel.Owner to this GameObject
            Assert.IsNotNull(testFc.UIModel, "Flowchart.UIModel should not be null after Awake.");
            Assert.AreEqual(testFc.gameObject, testFc.UIModel.Owner, "Flowchart should register itself as UIModel.Owner in Awake.");

        }

        [UnityTest]
        public IEnumerator Flowchart_Ensures_EventSystem_InScene()
        {
            // Arrange
            yield return null;

            // Act
            yield return null;

            // Assert: Flowchart.CheckEventSystem should ensure an EventSystem exists and is active
            var eventSystem = UnityObj.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            Assert.IsNotNull(eventSystem, "Flowchart should ensure an EventSystem exists in the scene.");
            toDestroyOnTearDown.Add(eventSystem.gameObject);
            Assert.IsTrue(eventSystem.gameObject.activeSelf, "EventSystem should be active after Flowchart initialization.");

        }

        private class TestGameStarted : GameStarted
        {
            public static int TriggerCount;
            public override void Trigger()
            {
                TriggerCount++;
                base.Trigger();
            }
        }

        [UnityTest]
        public IEnumerator Flowchart_Triggers_GameStarted_Blocks_OnStart()
        {
            // Arrange
            TestGameStarted.TriggerCount = 0;

            // Given the timing of when we set up the TestGameStarted block, we need to force 
            // the Flowchart to invoke Start() again so it will kick off GameStarted coroutine.
            // Start() is protected, so call via reflection.
            Type fcType = typeof(Flowchart);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            MethodInfo startMethod = fcType.GetMethod("Start", flags);
            Assert.IsNotNull(startMethod, "Could not reflect Flowchart.Start().");
            startMethod.Invoke(testFc, null);

            // Given the timing of when we set up the TestGameStarted block, we need to force 
            // Act: enable and wait for Flowchart.Start + coroutine to run
            // Wait until AmanitaManager reports fully initialized (Flowchart waits for this before triggering)
            int guard = 0;
            while ((AmanitaManager.S == null || !AmanitaManager.S.IsFullyInitted) && guard++ < 120)
            {
                yield return null;
            }
            // Allow the coroutine to trigger handlers
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.GreaterOrEqual(TestGameStarted.TriggerCount, 1,
                "GameStarted event handlers should be triggered when Flowchart starts.");

        }
    }
}