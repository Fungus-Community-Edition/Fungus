using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amanita.SaveSys;
using Amanita.SaveSys.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;

namespace SaveSystemTests
{
    public class SaveSlotUIManagerTests : CommonTestFunctionality
    {
        protected override string PathToTestScene => "ScenePrefabs/SaveSlotUITestScene";
        protected override bool ReqFlowchart => false;

        protected override IEnumerator CommonSetup()
        {
            yield return base.CommonSetup();
            slotUiManager = UnityObj.FindFirstObjectByType<SaveSlotUIManager>();
            Assert.IsNotNull(slotUiManager, "SaveSlotUIManager not found in test scene during setup.");
        }

        private SaveSlotUIManager slotUiManager;

        [UnityTest]
        public IEnumerator InitializesExpectedNumberOfSlots_EqualsInitialSlotCount()
        {
            // Ensure scene is loaded by base setup
            yield return CommonSetup();
            yield return null; // allow Awake/OnEnable to run

            // Read private 'initialSlotCount' via reflection
            var initialSlotCountField = slotUiManagerType.GetField("initialSlotCount", nonPublicInstanceFlags);
            Assert.IsNotNull(initialSlotCountField, "Could not reflect 'initialSlotCount' field.");

            var initialSlotCount = (int)initialSlotCountField.GetValue(slotUiManager);

            // Read protected '_slotUis' via reflection
            var slotUisField = slotUiManagerType.GetField("_slotUis", nonPublicInstanceFlags);
            Assert.IsNotNull(slotUisField, "Could not reflect '_slotUis' field.");

            var slotUis = (IList<SaveSlotViewComposer>)slotUisField.GetValue(slotUiManager);
            Assert.IsNotNull(slotUis, "_slotUis was null.");
            Assert.AreEqual(initialSlotCount, slotUis.Count, "Slot UI count did not match initialSlotCount.");
        }

        private static readonly Type slotUiManagerType = typeof(SaveSlotUIManager);
        private static readonly BindingFlags nonPublicInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        [UnityTest]
        public IEnumerator PassesMetasToSlots_OnSaveMetasReadOnInit()
        {
            yield return CommonSetup();
            yield return null; // allow Awake/OnEnable subscriptions

            // Access slots
            var slotUisField = slotUiManagerType.GetField("_slotUis", nonPublicInstanceFlags);
            Assert.IsNotNull(slotUisField, "Could not reflect '_slotUis' field.");

            var slotUis = (IList<SaveSlotViewComposer>)slotUisField.GetValue(slotUiManager);
            Assert.IsNotNull(slotUis, "_slotUis was null.");
            Assert.Greater(slotUis.Count, 0, "No slot UIs were initialized.");

            // Build a meta list. Shorter, equal, and longer cases can be validated; start with equal count
            var metaList = new List<ISaveMetaData>();
            for (int i = 0; i < slotUis.Count; i++)
            {
                metaList.Add(new SaveMetaData { SaveVersion = $"v{i}" });
            }

            // Raise the init signal
            SaveSysSignals.SaveMetasReadOnInit?.Invoke(metaList);

            // Give a frame for handlers (though it's synchronous, keep consistent with playmode timing)
            yield return null;

            // Assert each slot.Meta matches the provided list
            for (int i = 0; i < slotUis.Count; i++)
            {
                var slot = slotUis[i];
                Assert.IsNotNull(slot, $"Slot {i} was null.");
                Assert.AreEqual(metaList[i], slot.Meta, $"Slot {i} did not receive the expected meta.");
            }

            // Also test shorter meta list results in trailing nulls
            var shorterList = metaList.Take(Math.Max(1, slotUis.Count - 1)).ToList();
            SaveSysSignals.SaveMetasReadOnInit?.Invoke(shorterList);
            yield return null;

            for (int i = 0; i < slotUis.Count; i++)
            {
                var expected = i < shorterList.Count ? shorterList[i] : null;
                Assert.AreEqual(expected, slotUis[i].Meta, $"Slot {i} meta mismatch after shorter list dispatch.");
            }
        }
    }
}