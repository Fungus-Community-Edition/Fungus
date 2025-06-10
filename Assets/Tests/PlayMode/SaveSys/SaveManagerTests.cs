using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        //protected override bool ShouldDeleteTestSavesAtEnd => false;
        protected override string PathToTestScene => "ScenePrefabs/SaveSysMonoBehaviourTests";

        [OneTimeSetUp]
        public override void DoOneTimeSetUp()
        {
            base.DoOneTimeSetUp();
        }

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();

            // We need to make sure that the scene is set up before the
            // manager is
            FileSaveRepository saveRepository = new FileSaveRepository();
            saveRepository.Init(saveReader, saveWriter);
            // Since we're working with a manager other than the one belonging to the SaveSystem singleton
            manager = new AmanitaSaveManager(saveRepository);
            //manager.SaveWriter = saveWriter;
            //manager.SaveReader = saveReader;
            manager.RegisterMainCodec(flowchartSaveCodec);

            readReq.BaseSaveDirectory = manager.SaveDirType;
        }

        protected AmanitaSaveManager manager;

        [UnityTest]
        public virtual IEnumerator WritingToSlots()
        {
            yield return CommonSetup();

            foreach (int slot in testSlotNums)
            {
                readReq.SlotNumber = slot;
                string expectedPath = saveReader.GetSavePath(readReq);
                Task saveTask = manager.SaveTo(slot);

                yield return new WaitUntil(() => saveTask.IsCompleted);


                bool itWasWritten = File.Exists(expectedPath);
                Assert.IsTrue(itWasWritten, $"Save at slot {slot} does not exist");
            }

        }

        protected IList<int> testSlotNums = new List<int>() { 0, 2, 4, 6, 8, 16, 32, };

        [Test]
        public virtual async Task WritingToSlots_HandleInvalidSlotNums()
        {
            await CommonSetupAsync();

            SaveWriteRequest invalidReq = new SaveWriteRequest(writeReq);

            foreach (int slot in invalidSlotNums)
            {
                string expectedLogMessage = $"Cannot register or write a save with a negative slot number.";
                LogAssert.Expect(LogType.Warning, expectedLogMessage);
                await manager.SaveTo(slot);
            }

        }

        IList<int> invalidSlotNums = new int[] { -1, -3, -325, -12, -47 };

        [UnityTest]
        public virtual IEnumerator RegisteringWrittenSaves()
        {
            yield return WritingToSlots();

            var occupiedSlots = manager.GetOccupiedSlots();
            bool success = occupiedSlots.SequenceEqual(testSlotNums);
            Assert.IsTrue(success, "Save Manager did not register the slots properly.");

        }

        [Test]
        public virtual async Task DeletingSlots_HandlingEmptySlots()
        {
            DeleteAllTestSaves(); // So we can be sure all slots are empty
            await CommonSetupAsync();

            foreach (int slot in testSlotNums)
            {
                string expectedLogMessage = $"Cannot delete save in slot {slot} because it does not exist.";
                LogAssert.Expect(LogType.Warning, expectedLogMessage);
                manager.DeleteSave(slot);
            }
        }

        [UnityTest]
        public virtual IEnumerator DeletingSlots_HandlingInvalidSlotNums()
        {
            yield return CommonSetup();

            foreach (int slot in invalidSlotNums)
            {
                string expectedLogMessage = $"Cannot delete a save with a negative slot number.";
                LogAssert.Expect(LogType.Warning, expectedLogMessage);
                manager.DeleteSave(slot);
            }
        }

        [Test]
        public virtual async Task LoadingSlots_HandleInvalidSlotNums()
        {
            await CommonSetupAsync();

            foreach (int slot in invalidSlotNums)
            {
                string expectedLogMessage = $"Cannot load a save with a negative slot number.";
                LogAssert.Expect(LogType.Warning, expectedLogMessage);
                await manager.LoadMain(slot);
            }
        }

        [Test]
        public virtual async Task LoadingSlots_TransitionsToCorrectScene()
        {
            Assert.Ignore();

        }

        [Test]
        public virtual async Task LoadingSlots_CorrectGameStateApplied()
        {
            await CommonSetupAsync();

            SaveSystem.S.RegisterSaveDataApplier(flowchartApplier);
            SaveSystem.S.RegisterSaveDataApplier(audioApplier);

            foreach (var slot in testSlotNums)
            {
                // Need these to help check if the right game state is applied
                string expectedNameVarValue = nameVar.Value;
                int expectedScoreVarValue = scoreVar.Value;
                bool expectedIsNewPlayerVarValue = isNewPlayerVar.Value;
                float expectedFastestTimeVarValue = fastestTimeVar.Value;
                Vector3 expectedThreeDPosVarValue = threeDPosVar.Value;
                Vector2 expectedTwoDPosVarValue = twoDPosVar.Value;
                string expectedStringVarValue = stringVar.Value;

                // To help us see if the state's loaded correctly
                await manager.SaveTo(slot);

                // Change the game state to something else
                // so we can check if the loading works correctly
                nameVar.Value = "New Name After Save";
                scoreVar.Value = 9999;
                isNewPlayerVar.Value = false;
                fastestTimeVar.Value = 123.45f;
                threeDPosVar.Value = new Vector3(10, 20, 30);
                twoDPosVar.Value = new Vector2(5, 10);
                stringVar.Value = "New String Value After Save";

                readReq.SlotNumber = slot;
                CompositeSaveData mainState = await manager.LoadMain(slot);
                Assert.IsNotNull(mainState, "Main save data is null after loading.");
                Assert.AreEqual(nameVar.Value, expectedNameVarValue,
                    $"Name variable value mismatch after loading slot {slot}.");
                Assert.AreEqual(scoreVar.Value, expectedScoreVarValue,
                    $"Score variable value mismatch after loading slot {slot}.");
                Assert.AreEqual(isNewPlayerVar.Value, expectedIsNewPlayerVarValue,
                    $"IsNewPlayer variable value mismatch after loading slot {slot}.");
                Assert.AreEqual(fastestTimeVar.Value, expectedFastestTimeVarValue,
                    $"FastestTime variable value mismatch after loading slot {slot}.");
                Assert.AreEqual(threeDPosVar.Value, expectedThreeDPosVarValue,
                    $"3D Position variable value mismatch after loading slot {slot}.");
                Assert.AreEqual(twoDPosVar.Value, expectedTwoDPosVarValue,
                    $"2D Position variable value mismatch after loading slot {slot}.");
                Assert.AreEqual(stringVar.Value, expectedStringVarValue,
                    $"String variable value mismatch after loading slot {slot}.");
            }
            
        }


        [Test]
        public virtual async Task ReturningSlotsBasedOnWriteOrder()
        {
            await WriteToSlotsAsync();

            bool success = manager.GetOccupiedSlots().SequenceEqual(testSlotNums);
            Assert.IsTrue(success, "Save Manager did not return the correct slots after writing.");
        }

        protected virtual async Task WriteToSlotsAsync()
        {
            foreach (int slot in testSlotNums)
            {
                await manager.SaveTo(slot);
            }
        }

        [Test]
        public virtual async Task OverwritingSlots()
        {
            await CommonSetupAsync();
            await WriteToSlotsAsync();

            // Now change the game state before we overwriting the saves
            // This is to ensure that the save data is different from the previous saves
            // and that the overwriting works as expected.

            string expectedNameVarValue = nameVar.Value + "3we4to789y3458t";
            int expectedScoreVarValue = scoreVar.Value + 1000;
            bool expectedIsNewPlayerVarValue = !isNewPlayerVar.Value;
            float expectedFastestTimeVarValue = fastestTimeVar.Value + 10f;
            Vector3 expectedThreeDPosVarValue = threeDPosVar.Value + new Vector3(1, 2, 3);
            Vector2 expectedTwoDPosVarValue = twoDPosVar.Value + new Vector2(1, 2);
            string expectedStringVarValue = stringVar.Value + "new string value";

            foreach (int slot in testSlotNums)
            {
                ChangeGameState();
                await manager.SaveTo(slot);

                // Now to fetch the save data and check that it matches the expected values

                CompositeSaveData mainState = manager.GetMainFrom(slot);
                SaveDataUnit forFlowchart = mainState.GetSingle<FlowchartSaveData>();

                FlowchartSaveData flowchartSave = (FlowchartSaveData) flowchartSaveCodec.DecodeFrom(forFlowchart);

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

            void ChangeGameState()
            {
                nameVar.Value = expectedNameVarValue;
                scoreVar.Value = expectedScoreVarValue;
                isNewPlayerVar.Value = expectedIsNewPlayerVarValue;
                fastestTimeVar.Value = expectedFastestTimeVarValue;
                threeDPosVar.Value = expectedThreeDPosVarValue;
                twoDPosVar.Value = expectedTwoDPosVarValue;
                stringVar.Value = expectedStringVarValue;
            }


        }

        [Test]
        public virtual async Task DeletingSlots()
        {
            await CommonSetupAsync();
            await WriteToSlotsAsync();
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