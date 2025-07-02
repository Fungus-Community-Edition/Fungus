using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using AmanitaSaveManager = Amanita.SaveSys.SaveManager;
using Encoding = System.Text.Encoding;
using UnityObject = UnityEngine.Object;


namespace Amanita.SaveSystemTests
{
    // Notes for async tests:
    // Verify that after an asynchronous operation completes
    // (e.g., saving or deletion), the file system and
    // internal state (e.g., occupied slots) are updated appropriately.
    //
    // Although exact timing might be fuzzy, you can check that asynchronous
    // operations don’t lead to race conditions (for example, by kicking off
    // multiple async save operations concurrently and then verifying
    // that all the data is correctly saved).
    // If someone might trigger multiple save or delete operations
    // concurrently, check that the internal state remains consistent
    // (a concurrency or race condition test).

    // Integration with Encoders/Decoders
    // Test scenarios where:
    // - Multiple encoders are registered
    // - An encoder fails (simulate or mock an encoder exception) and verify that
    // SaveManager handles or bubbles up that error gracefully.

    // Error Conditions and Recovery
    // Tests that simulate I/O failures can be invaluable:
    // File System Errors: Use dependency injection or mocks (if possible) to
    // simulate scenarios like disk full, file permission errors, or corrupted files
    // Data Consistency on Failure: Ensure that if a write fails midway,
    // SaveManager doesn’t leave partially written (and corrupted) states or
    // misregister occupied slots.

    // Round-Trip Consistency
    // Write a save to disk, then load it back, and compare the in-memory state to
    // confirm that serialization/deserialization works accurately. This ensures that
    // the data integrity holds through the entire cycle.

    public class SaveManagerTests : CommonTestFunctionality
    {
        protected override bool ShouldDeleteTestSavesAtEnd => true;
        protected override string PathToTestScene => "ScenePrefabs/SaveSysMonoBehaviourTests";

        [TearDown]
        public override void DoTearDown()
        {
            manager?.ClearSaveData();

            base.DoTearDown();
        }

        public override void DoSetUp()
        {
            base.DoSetUp();

            // We expect the installer to have finished injecting the dependencies by the time
            // time the scene is prepped.
            // We need to make sure that the scene is set up before the
            // manager is
            manager = (AmanitaSaveManager)saveSys.SaveManager;

            readReq.BaseSaveDirectory = manager.SaveDirType;
        }

        protected AmanitaSaveManager manager;

        [TestCaseSource(nameof(TestSlotNumSource))]
        public virtual async Task WritingToSlots(int slot)
        {
            await CommonSetupAsync();

            readReq.SlotNumber = slot;
            string expectedPath = saveReader.GetSavePath(readReq);
            Task saveTask = manager.SaveTo(slot);

            await saveTask.ConfigureAwait(false);

            bool itWasWritten = File.Exists(expectedPath);
            Assert.IsTrue(itWasWritten, $"Save at slot {slot} does not exist");

        }

        public static IEnumerable<int> TestSlotNumSource()
        {
            foreach (var slotNum in testSlotNums)
            {
                yield return slotNum;
            }

        }

        protected static IList<int> testSlotNums = new List<int>() { 1, 2, 4 };


        [UnityTest]
        public IEnumerator WritingToSlots_HandleInvalidSlotNums()
        {
            yield return CommonSetup();

            SaveWriteRequest invalidReq = new SaveWriteRequest(writeReq);

            foreach (int slot in invalidSlotNums)
            {
                string expectedLogMessage = $"Cannot register or write a save with a negative slot number.";
                LogAssert.Expect(LogType.Warning, expectedLogMessage);
                Task saveTask = manager.SaveTo(slot);
                yield return new WaitUntil(() => saveTask.IsCompleted);
            }

        }

        protected static IList<int> invalidSlotNums = new int[] { -1, -3, -325, -12, -47 };

        [TestCaseSource(nameof(TestSlotNumSource))]
        public virtual async Task RegisteringWrittenSaves_Separate(int slot)
        {
            await WritingToSlots(slot);

            var occupiedSlots = manager.GetOccupiedSlots();
            IList<int> justThatOneSlot = new int[] { slot };
            bool success = occupiedSlots.SequenceEqual(justThatOneSlot);
            Assert.IsTrue(success, "Save Manager did not register the slots properly.");

        }

        [Test]
        public virtual async Task RegisteringWrittenSaves_Together()
        {
            foreach (var slot in testSlotNums)
            {
                await WritingToSlots(slot);
            }

            var occupiedSlots = manager.GetOccupiedSlots();
            bool success = occupiedSlots.SequenceEqual(testSlotNums);
            Assert.IsTrue(success, "Save Manager did not register the slots properly.");

        }

        [UnityTest]
        public IEnumerator DeletingSlots_HandlingEmptySlots()
        {
            DeleteAllTestSaves(); // So we can be sure all slots are empty
            yield return CommonSetup();

            foreach (int slot in testSlotNums)
            {
                string expectedLogMessage = $"Cannot delete save in slot {slot} because it does not exist.";
                LogAssert.Expect(LogType.Warning, expectedLogMessage);
                manager.DeleteSave(slot);
            }
        }

        [UnityTest]
        public IEnumerator DeletingSlots_HandlingInvalidSlotNums()
        {
            yield return CommonSetup();

            foreach (int slot in invalidSlotNums)
            {
                string expectedLogMessage = $"Cannot delete a save with a negative slot number.";
                LogAssert.Expect(LogType.Warning, expectedLogMessage);
                manager.DeleteSave(slot);
            }
        }

        [UnityTest]
        public IEnumerator LoadingSlots_HandleInvalidSlotNums()
        {
            yield return CommonSetup();

            foreach (int slot in invalidSlotNums)
            {
                string expectedLogMessage = $"Cannot load a save with a negative slot number.";
                LogAssert.Expect(LogType.Warning, expectedLogMessage);
                Task<CompositeSaveData> loadTask = manager.LoadMain(slot);
                yield return new WaitUntil(() => loadTask.IsCompleted);
            }
        }

        [TestCaseSource(nameof(TestSlotNumSource))]
        public virtual async Task LoadingSlots_CorrectGameStateApplied(int slot)
        {
            await CommonSetupAsync();

            string expectedNameVarValue;
            int expectedScoreVarValue;
            bool expectedIsNewPlayerVarValue;
            float expectedFastestTimeVarValue;
            Vector3 expectedThreeDPosVarValue;
            Vector2 expectedTwoDPosVarValue;
            string expectedStringVarValue;

            expectedNameVarValue = nameVar.Value;
            expectedScoreVarValue = scoreVar.Value;
            expectedIsNewPlayerVarValue = isNewPlayerVar.Value;
            expectedFastestTimeVarValue = fastestTimeVar.Value;
            expectedThreeDPosVarValue = threeDPosVar.Value;
            expectedTwoDPosVarValue = twoDPosVar.Value;
            expectedStringVarValue = stringVar.Value;

            await manager.SaveTo(slot);
            ChangeGameState();

            CompositeSaveData mainState = await manager.LoadMain(slot, false);
            // ^We need to make sure to avoid loading the scene here. That will just make this test run
            // again, which is not what we want.
            Assert.IsNotNull(mainState, $"Main save data is null after loading slot {slot}.");

            // Fungus always has one Flowchart it initializes: one for global variables. Thus, when fetching
            // a flowchart save from mainState, we might not get the one we're looking for.
            // Hence the need to search all the FC saves and find the one we want.
            IList<SaveDataUnit> fcUnits = mainState.GetMulti<FlowchartSaveData>();
            Assert.IsNotEmpty(fcUnits, $"No Flowchart save data found in main state for slot {slot}.");
            IList<SaveData> baseDecodedDatas = flowchartSaveCodec.DecodeMultiFrom(fcUnits);
            IList<FlowchartSaveData> flowchartSaves = baseDecodedDatas
                .Where(d => d is FlowchartSaveData)
                .Cast<FlowchartSaveData>()
                .ToList();
            Assert.IsNotEmpty(flowchartSaves, $"No Flowchart save data decoded from main state for slot {slot}.");

            FlowchartSaveData hasStateWeWantToCheck = flowchartSaves.FirstOrDefault(fc => fc.FlowchartName == flowchart.name);
            Assert.IsNotNull(hasStateWeWantToCheck, $"Flowchart save data for {flowchart.name} not found in main state for slot {slot}.");

            CheckGameState(slot, hasStateWeWantToCheck);

            void ChangeGameState()
            {
                nameVar.Value = "New Name After Save";
                scoreVar.Value += 260;
                isNewPlayerVar.Value = !isNewPlayerVar.Value;
                fastestTimeVar.Value += 123.45f;
                threeDPosVar.Value += new Vector3(10, 20, 30);
                twoDPosVar.Value += new Vector2(5, 10);
                stringVar.Value = "New String Value After Save";
            }

            void CheckGameState(int slot, FlowchartSaveData flowchartSave)
            {
                string actualNameVarValue = flowchartSave.GetVarValue<string>(nameVar.Key);
                Assert.AreEqual(expectedNameVarValue, actualNameVarValue,
                    $"Name variable value mismatch for slot {slot}.");

                int actualScoreVarValue = flowchartSave.GetVarValue<int>(scoreVar.Key);
                Assert.AreEqual(expectedScoreVarValue, actualScoreVarValue,
                    $"Score variable value mismatch for slot {slot}.");

                bool actualIsNewPlayerVarValue = flowchartSave.GetVarValue<bool>(isNewPlayerVar.Key);
                Assert.AreEqual(expectedIsNewPlayerVarValue, actualIsNewPlayerVarValue,
                    $"IsNewPlayer variable value mismatch for slot {slot}.");

                float actualFastestTimeVarValue = flowchartSave.GetVarValue<float>(fastestTimeVar.Key);
                Assert.AreEqual(expectedFastestTimeVarValue, actualFastestTimeVarValue,
                    $"FastestTime variable value mismatch for slot {slot}.");

                Vector3 actualThreeDPosVarValue = flowchartSave.GetVarValue<Vector3>(threeDPosVar.Key);
                Assert.AreEqual(expectedThreeDPosVarValue, actualThreeDPosVarValue,
                    $"3D Position variable value mismatch for slot {slot}.");

                Vector2 actualTwoDPosVarValue = flowchartSave.GetVarValue<Vector2>(twoDPosVar.Key);
                Assert.AreEqual(expectedTwoDPosVarValue, actualTwoDPosVarValue,
                    $"2D Position variable value mismatch for slot {slot}.");

                string actualStringVarValue = flowchartSave.GetVarValue<string>(stringVar.Key);
                Assert.AreEqual(expectedStringVarValue, actualStringVarValue,
                    $"String variable value mismatch for slot {slot}.");
            }

        }

        protected IEnumerator SaveTo(int slot)
        {
            Task saveTask = manager.SaveTo(slot);
            yield return new WaitUntil(() => saveTask.IsCompleted);
            if (saveTask.IsFaulted)
            {
                Assert.Fail($"Failed to save to slot {slot}: {saveTask.Exception}");
            }
        }

        protected IEnumerator Load(int slot)
        {
            loadTask = manager.LoadMain(slot, false);
            yield return new WaitUntil(() => loadTask.IsCompleted);
            if (loadTask.IsFaulted)
            {
                Assert.Fail($"Failed to load slot {slot}: {loadTask.Exception}");
            }
        }

        protected Task<CompositeSaveData> loadTask;
        protected FlowchartSaveData flowchartSave;

        [UnityTest]
        public IEnumerator ReturningSlotsBasedOnWriteOrder()
        {
            yield return CommonSetup();
            Task slotWriteTask = WriteToSlotsAsync();
            yield return new WaitUntil(() => slotWriteTask.IsCompleted);

            bool success = manager.GetOccupiedSlots().SequenceEqual(testSlotNums);
            Assert.IsTrue(success, "Save Manager did not return the correct slots after writing.");
        }

        protected async Task WriteToSlotsAsync()
        {
            foreach (int slot in testSlotNums)
            {
                await manager.SaveTo(slot);
            }
        }

        [UnityTest]
        public IEnumerator OverwritingSlots()
        {
            yield return CommonSetup();

            // These expected values are the ones we want to overwrite the 
            // old initial values with
            string expectedNameVarValue = nameVar.Value + "tcvw4fyw789g8";
            int expectedScoreVarValue = scoreVar.Value + 128;
            bool expectedIsNewPlayerVarValue = isNewPlayerVar.Value;
            float expectedFastestTimeVarValue = fastestTimeVar.Value / 2;
            Vector3 expectedThreeDPosVarValue = threeDPosVar.Value + new Vector3(11, 22, 33);
            Vector2 expectedTwoDPosVarValue = twoDPosVar.Value + new Vector2(2, 8);
            string expectedStringVarValue = stringVar.Value + "eoty vwg8";

            Task writeTask = WriteToSlotsAsync(); // The slots that we will be overwriting
            yield return new WaitUntil(() => writeTask.IsCompleted);

            foreach (int slot in testSlotNums)
            {
                ChangeGameStateToExpected();
                void ChangeGameStateToExpected()
                {
                    nameVar.Value = expectedNameVarValue;
                    scoreVar.Value = expectedScoreVarValue;
                    isNewPlayerVar.Value = expectedIsNewPlayerVarValue;
                    fastestTimeVar.Value = expectedFastestTimeVarValue;
                    threeDPosVar.Value = expectedThreeDPosVarValue;
                    twoDPosVar.Value = expectedTwoDPosVarValue;
                    stringVar.Value = expectedStringVarValue;
                }
                
                Task saveTask = manager.SaveTo(slot);
                yield return new WaitUntil(() => saveTask.IsCompleted);

                // Now to fetch the save data and check that it matches the expected values
                CompositeSaveData mainState = manager.GetMainFrom(slot);
                IList<SaveDataUnit> allFcUnits = mainState.GetMulti<FlowchartSaveData>();
                IList<FlowchartSaveData> allFcSaves = flowchartSaveCodec.DecodeMultiFrom(allFcUnits)
                    .Where((res) => res is FlowchartSaveData)
                    .Cast<FlowchartSaveData>()
                    .ToList();

                FlowchartSaveData flowchartSave = allFcSaves.Where((data) => data.FlowchartName == flowchart.name)
                    .FirstOrDefault();
                Assert.IsNotNull(flowchartSave, $"Couldn't find the right flowchart save data for slot {slot}");

                CheckTheValues();
                void CheckTheValues()
                {
                    string actualNameVarValue = flowchartSave.GetVarValue<string>(nameVar.Key);
                    Assert.AreEqual(expectedNameVarValue, actualNameVarValue, $"Name variable value mismatch for slot {slot}.");

                    int actualScoreVarValue = flowchartSave.GetVarValue<int>(scoreVar.Key);
                    Assert.AreEqual(expectedScoreVarValue, actualScoreVarValue, $"Score variable value mismatch for slot {slot}.");

                    bool actualIsNewPlayerVarValue = flowchartSave.GetVarValue<bool>(isNewPlayerVar.Key);
                    Assert.AreEqual(expectedIsNewPlayerVarValue, actualIsNewPlayerVarValue, $"IsNewPlayer variable value mismatch for slot {slot}.");

                    float actualFastestTimeVarValue = flowchartSave.GetVarValue<float>(fastestTimeVar.Key);
                    Assert.AreEqual(expectedFastestTimeVarValue, actualFastestTimeVarValue, $"FastestTime variable value mismatch for slot {slot}.");

                    Vector3 actualThreeDPosVarValue = flowchartSave.GetVarValue<Vector3>(threeDPosVar.Key);
                    Assert.AreEqual(expectedThreeDPosVarValue, actualThreeDPosVarValue, $"3D Position variable value mismatch for slot {slot}.");

                    Vector2 actualTwoDPosVarValue = flowchartSave.GetVarValue<Vector2>(twoDPosVar.Key);
                    Assert.AreEqual(expectedTwoDPosVarValue, actualTwoDPosVarValue, $"2D Position variable value mismatch for slot {slot}.");

                    string actualStringVarValue = flowchartSave.GetVarValue<string>(stringVar.Key);
                    Assert.AreEqual(expectedStringVarValue, actualStringVarValue, $"String variable value mismatch for slot {slot}.");
                }

                SetExpectedValuesForNextIteration();
                void SetExpectedValuesForNextIteration()
                {
                    expectedNameVarValue += "next";
                    expectedScoreVarValue += 1000;
                    expectedIsNewPlayerVarValue = !expectedIsNewPlayerVarValue;
                    expectedFastestTimeVarValue += 10f;
                    expectedThreeDPosVarValue += new Vector3(1, 2, 3);
                    expectedTwoDPosVarValue += new Vector2(1, 2);
                    expectedStringVarValue += "next string value";
                }


            }

        }

        [UnityTest]
        public IEnumerator DeletingSlots()
        {
            yield return CommonSetup();
            Task writeTask = WriteToSlotsAsync();
            writeTask.ConfigureAwait(false);
            yield return new WaitUntil(() => writeTask.IsCompleted);

            foreach (int slot in testSlotNums)
            {
                manager.DeleteSave(slot);
                bool doesItExist = File.Exists(saveReader.GetSavePath(readReq));
                Assert.IsFalse(doesItExist, $"Save at slot {slot} was not deleted.");
            }
            var occupiedSlots = manager.GetOccupiedSlots();
            Assert.IsEmpty(occupiedSlots, "Save Manager did not clear the occupied slots after deletion.");

        }
    }
}