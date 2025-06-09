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
using Encoding = System.Text.Encoding;
using UnityObject = UnityEngine.Object;
using AmanitaSaveManager = Amanita.SaveSys.SaveManager;
using System.ComponentModel;


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

        [UnityTest]
        public virtual IEnumerator WritingToSlots_HandleInvalidSlotNums()
        {
            yield return CommonSetup();

            SaveWriteRequest invalidReq = new SaveWriteRequest(writeReq);

            foreach (int slot in invalidSlotNums)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    manager.SaveTo(slot).GetAwaiter().GetResult();
                });
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

        [UnityTest]
        public virtual IEnumerator DeletionFromSlots_HandlingEmptySlots()
        {
            yield return CommonSetup();

            // Best to make sure that the system silently just does nothing in these
            // cases
            Assert.Ignore("");
        }

        [UnityTest]
        public virtual IEnumerator DeletionFromSlots_HandlingInvalidSlotNums()
        {
            yield return CommonSetup();

            foreach (int slot in invalidSlotNums)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    manager.DeleteSave(slot).GetAwaiter().GetResult();
                });
            }
        }

        [UnityTest]
        public virtual IEnumerator LoadingFromSlots_HandleInvalidSlotNums()
        {
            yield return CommonSetup();

            foreach (int slot in invalidSlotNums)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    manager.LoadMain(slot).GetAwaiter().GetResult();
                });
            }

        }

        [Test]
        public virtual void ReturningCorrectPathsToSaves()
        {
            Assert.Ignore();
        }

        [Test]
        public virtual void ReturningCorrectSlots()
        {
            // Make sure that things are returned in the corrected order, and that
            // the order correctly reflects changes after consecutive operations
            // like writes and deletions.
            Assert.Ignore();
        }

        [UnityTest]
        public virtual IEnumerator OverwritingSlots()
        {
            yield return CommonSetup();
            
            // After overwriting, make sure to load it and check that the game state
            // reflects the newer save.
            Assert.Ignore();
        }


    }
}