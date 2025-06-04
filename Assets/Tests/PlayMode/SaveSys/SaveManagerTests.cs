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


namespace Amanita.SaveSystemTests
{
    public class SaveManagerTests : CommonTestFunctionality
    {
        protected override string PathToTestScene => "ScenePrefabs/SaveSysMonoBehaviourTests";
        public override void DoOneTimeSetUp()
        {
            base.DoOneTimeSetUp();
            readReq.BaseSaveDirectory = manager.SaveDirType;

            // Since we're working with a manager other than the one belonging to the SaveSystem singleton
            manager.SaveWriter = saveWriter;
            manager.SaveReader = saveReader;
            manager.RegisterMainEncoder(flowchartSaveCodec);
        }

        protected AmanitaSaveManager manager = new AmanitaSaveManager();

        [UnityTest]
        public virtual IEnumerator WritesSaveToAppropriateSlots()
        {
            yield return CommonSetup();

            foreach (int slot in testSlotNums)
            {
                manager.RegisterAndWriteSave(slot);
                readReq.SlotNumber = slot;
                string expectedPath = saveReader.GetSavePath(readReq);

                bool itWasWritten = File.Exists(expectedPath);
                Assert.IsTrue(itWasWritten, $"Save at slot {slot} does not exist");
            }

        }

        protected IList<int> testSlotNums = new List<int>() { 0, 2, 4, 6, 8, 16, 32, };

        [UnityTest]
        public virtual IEnumerator RegistersWrittenSavesProperly()
        {
            yield return WritesSaveToAppropriateSlots();

            var occupiedSlots = manager.GetOccupiedSlots();
            bool success = occupiedSlots.SequenceEqual(testSlotNums);
            Assert.IsTrue(success, "Save Manager did not register the slots properly.");

        }
    }
}