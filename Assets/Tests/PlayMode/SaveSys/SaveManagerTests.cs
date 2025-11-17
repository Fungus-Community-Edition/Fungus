using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using AmanitaSaveManager = Amanita.SaveSys.SaveManager;

namespace SaveSystemTests
{
    public class SaveManagerTests : CommonTestFunctionality
    {
        // Needs full environment: SaveSystem + scene + flowchart.
        protected override string PathToTestScene => "ScenePrefabs/SaveSysMonoBehaviourTests";
        protected override bool ReqSaveSystem => true;
        protected override bool ReqSceneLoad => true;
        protected override bool ReqFlowchart => true;
        protected override bool ShouldDeleteTestSavesAtEnd => true;

        protected AmanitaSaveManager manager;

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            manager = (AmanitaSaveManager) saveSys.SaveManager;
            readReq.BaseSaveDirectory = manager.SaveDirType;
        }

        [TearDown]
        public override void DoTearDown()
        {
            manager?.ClearSaveData();
            base.DoTearDown();
        }

        protected static readonly IList<int> testSlotNums = new List<int> { 1, 2, 4 };
        protected static readonly IList<int> invalidSlotNums = new[] { -1, -3, -325, -12, -47 };

        public static IEnumerable<int> TestSlotNumSource()
        {
            foreach (var slot in testSlotNums) yield return slot;
        }

        [TestCaseSource(nameof(TestSlotNumSource))]
        public async Task WritingToSlots(int slot)
        {
            await CommonSetupAsync();

            readReq.SlotNumber = slot;
            string expectedPath = saveReader.GetSavePath(readReq);

            await manager.SaveTo(slot).ConfigureAwait(false);

            Assert.IsTrue(File.Exists(expectedPath), $"Save at slot {slot} does not exist.");
        }

        [UnityTest]
        public IEnumerator WritingToSlots_HandleInvalidSlotNums()
        {
            yield return CommonSetup();

            foreach (int slot in invalidSlotNums)
            {
                LogAssert.Expect(LogType.Warning,
                    "Cannot register or write a save with a negative slot number.");
                var task = manager.SaveTo(slot);
                yield return new WaitUntil(() => task.IsCompleted);
            }
        }

        [TestCaseSource(nameof(TestSlotNumSource))]
        public async Task RegisteringWrittenSaves_Separate(int slot)
        {
            await WritingToSlots(slot);
            var occupiedSlots = manager.GetOccupiedSlots();
            Assert.IsTrue(occupiedSlots.SequenceEqual(new[] { slot }),
                "Save Manager did not register the slot properly.");
        }

        [Test]
        public async Task RegisteringWrittenSaves_Together()
        {
            foreach (var slot in testSlotNums)
                await WritingToSlots(slot);

            var occupiedSlots = manager.GetOccupiedSlots();
            Assert.IsTrue(occupiedSlots.SequenceEqual(testSlotNums),
                "Save Manager did not register the slots properly.");
        }

        [UnityTest]
        public IEnumerator DeletingSlots_HandlingEmptySlots()
        {
            DeleteAllTestSaves();
            yield return CommonSetup();

            foreach (int slot in testSlotNums)
            {
                LogAssert.Expect(LogType.Warning,
                    $"Cannot delete save in slot {slot} because it does not exist.");
                manager.DeleteSave(slot);
            }
        }

        [UnityTest]
        public IEnumerator DeletingSlots_HandlingInvalidSlotNums()
        {
            yield return CommonSetup();

            foreach (int slot in invalidSlotNums)
            {
                LogAssert.Expect(LogType.Warning,
                    "Cannot delete a save with a negative slot number.");
                manager.DeleteSave(slot);
            }
        }

        [UnityTest]
        public IEnumerator LoadingSlots_HandleInvalidSlotNums()
        {
            yield return CommonSetup();

            foreach (int slot in invalidSlotNums)
            {
                LogAssert.Expect(LogType.Warning,
                    "Cannot load a save with a negative slot number.");
                var task = manager.LoadMain(slot);
                yield return new WaitUntil(() => task.IsCompleted);
            }
        }

        [TestCaseSource(nameof(TestSlotNumSource))]
        public async Task LoadingSlots_CorrectGameStateApplied(int slot)
        {
            await CommonSetupAsync();

            // Capture pre-save expected values
            string expectedName = nameVar.Value;
            int expectedScore = scoreVar.Value;
            bool expectedIsNew = isNewPlayerVar.Value;
            float expectedFastest = fastestTimeVar.Value;
            Vector3 expectedThreeD = threeDPosVar.Value;
            Vector2 expectedTwoD = twoDPosVar.Value;
            string expectedString = stringVar.Value;

            await manager.SaveTo(slot);
            MutateGameState(); // So that after loading, we can check if the correct state was restored.
            void MutateGameState()
            {
                nameVar.Value = "Changed";
                scoreVar.Value += 260;
                isNewPlayerVar.Value = !isNewPlayerVar.Value;
                fastestTimeVar.Value += 1.23f;
                threeDPosVar.Value += new Vector3(10, 20, 30);
                twoDPosVar.Value += new Vector2(5, 10);
                stringVar.Value = "Changed string";
            }

            CompositeSaveData mainState = await manager.LoadMain(slot, loadScene: false);
            Assert.IsNotNull(mainState, $"Main save data is null after loading slot {slot}.");

            var flowchartSaves = mainState.GetMulti<FlowchartSaveData>();
            Assert.IsNotEmpty(flowchartSaves, $"No Flowchart save data found for slot {slot}.");

            FlowchartSaveData fcSave =
                flowchartSaves.FirstOrDefault(fChart => fChart.FlowchartName == flowchart.name);
            Assert.IsNotNull(fcSave, $"Flowchart save data for {flowchart.name} not found in slot {slot}.");

            AssertThatTheCorrectStateWasLoaded();
            void AssertThatTheCorrectStateWasLoaded()
            {                
                Assert.AreEqual(expectedName, fcSave.GetVarValue<string>(nameVar.Key));
                Assert.AreEqual(expectedScore, fcSave.GetVarValue<int>(scoreVar.Key));
                Assert.AreEqual(expectedIsNew, fcSave.GetVarValue<bool>(isNewPlayerVar.Key));
                Assert.AreEqual(expectedFastest, fcSave.GetVarValue<float>(fastestTimeVar.Key));
                Assert.AreEqual(expectedThreeD, fcSave.GetVarValue<Vector3>(threeDPosVar.Key));
                Assert.AreEqual(expectedTwoD, fcSave.GetVarValue<Vector2>(twoDPosVar.Key));
                Assert.AreEqual(expectedString, fcSave.GetVarValue<string>(stringVar.Key));
            }

        }

        [UnityTest]
        public IEnumerator ReturningSlotsBasedOnWriteOrder()
        {
            yield return CommonSetup();
            var writeTask = WriteToSlotsAsync();
            yield return new WaitUntil(() => writeTask.IsCompleted);

            Assert.IsTrue(manager.GetOccupiedSlots().SequenceEqual(testSlotNums),
                "Save Manager did not return the correct slots after writing.");
        }

        [UnityTest]
        public IEnumerator OverwritingSlots()
        {
            yield return CommonSetup();

            // Prepare initial writes
            var initialWrite = WriteToSlotsAsync();
            yield return new WaitUntil(() => initialWrite.IsCompleted);

            // Starting expected values
            string expectedName = nameVar.Value + "_ovr";
            int expectedScore = scoreVar.Value + 128;
            bool expectedIsNew = isNewPlayerVar.Value;
            float expectedFastest = fastestTimeVar.Value / 2f;
            Vector3 expectedThreeD = threeDPosVar.Value + new Vector3(11, 22, 33);
            Vector2 expectedTwoD = twoDPosVar.Value + new Vector2(2, 8);
            string expectedString = stringVar.Value + "_str";

            foreach (int slot in testSlotNums)
            {
                ApplyExpected();
                var saveTask = manager.SaveTo(slot);
                yield return new WaitUntil(() => saveTask.IsCompleted);

                var mainState = manager.GetMainFrom(slot);
                var fcSave = mainState.GetMulti<FlowchartSaveData>()
                    .FirstOrDefault(d => d.FlowchartName == flowchart.name);
                Assert.IsNotNull(fcSave, $"Missing Flowchart save for slot {slot}.");

                Assert.AreEqual(expectedName, fcSave.GetVarValue<string>(nameVar.Key));
                Assert.AreEqual(expectedScore, fcSave.GetVarValue<int>(scoreVar.Key));
                Assert.AreEqual(expectedIsNew, fcSave.GetVarValue<bool>(isNewPlayerVar.Key));
                Assert.AreEqual(expectedFastest, fcSave.GetVarValue<float>(fastestTimeVar.Key));
                Assert.AreEqual(expectedThreeD, fcSave.GetVarValue<Vector3>(threeDPosVar.Key));
                Assert.AreEqual(expectedTwoD, fcSave.GetVarValue<Vector2>(twoDPosVar.Key));
                Assert.AreEqual(expectedString, fcSave.GetVarValue<string>(stringVar.Key));

                AdvanceExpected();
            }

            void ApplyExpected()
            {
                nameVar.Value = expectedName;
                scoreVar.Value = expectedScore;
                isNewPlayerVar.Value = expectedIsNew;
                fastestTimeVar.Value = expectedFastest;
                threeDPosVar.Value = expectedThreeD;
                twoDPosVar.Value = expectedTwoD;
                stringVar.Value = expectedString;
            }

            void AdvanceExpected()
            {
                expectedName += "_next";
                expectedScore += 1000;
                expectedIsNew = !expectedIsNew;
                expectedFastest += 10f;
                expectedThreeD += new Vector3(1, 2, 3);
                expectedTwoD += new Vector2(1, 2);
                expectedString += "_next";
            }
        }

        [UnityTest]
        public IEnumerator DeletingSlots()
        {
            yield return CommonSetup();
            var writeTask = WriteToSlotsAsync();
            yield return new WaitUntil(() => writeTask.IsCompleted);

            foreach (int slot in testSlotNums)
            {
                manager.DeleteSave(slot);
                readReq.SlotNumber = slot;
                bool exists = File.Exists(saveReader.GetSavePath(readReq));
                Assert.IsFalse(exists, $"Save at slot {slot} was not deleted.");
            }

            Assert.IsEmpty(manager.GetOccupiedSlots(),
                "Save Manager did not clear occupied slots after deletion.");
        }

        protected async Task WriteToSlotsAsync()
        {
            foreach (int slot in testSlotNums)
                await manager.SaveTo(slot);
        }

        protected override int CommonSetupDelay => 250;
    }
}