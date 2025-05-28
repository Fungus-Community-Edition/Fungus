using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Amanita.SaveSys;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using System.Linq;
using UnityEngine.TestTools;

namespace Amanita.SaveSystemTests
{
    public class SaveReaderTests
    {
        protected string toVarStateTests = "ScenePrefabs/VarStateTests";

        [SetUp]
        public virtual void DoSetUp()
        {
            SaveSystem.InitPaths();
            PrepScene();
            saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
            saveReader = ScriptableObject.CreateInstance<SaveReader>();
            readReq = new SaveReadRequest
            {
                SlotNumber = writeReq.SlotNumber,
                BaseSaveDirectory = writeReq.BaseSaveDirectory,
            };
        }

        protected virtual void PrepScene()
        {
            varStateTestPrefab = Resources.Load<GameObject>(toVarStateTests);
            varStateTestScene = UnityObject.Instantiate(varStateTestPrefab);
            flowchart = varStateTestScene.GetComponentInChildren<Flowchart>();
        }

        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;

        protected Flowchart flowchart;
        protected SaveWriter saveWriter;
        protected SaveReader saveReader;
        protected SaveReadRequest readReq;

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [Test]
        public virtual void ReadsMetadataProperly()
        {
            saveWriter.WriteOneToDisk(writeReq);

            SaveMetaData expectedSaveMetaData = writeReq.SaveMetaData;

            SaveMetaData whatWeGot = saveReader.ReadMetadataFromDisk(readReq);
            Assert.AreEqual(expectedSaveMetaData, whatWeGot, "The save meta datas do not match.");
        }

        protected SaveWriteRequest writeReq = new SaveWriteRequest
        {
            SaveName = "TestSave",
            SlotNumber = 0,
            MainSaveData = new AmanitaSaveData(),
            BaseSaveDirectory = SaveDirectoryType.DataPath
        };

        [Test]
        public virtual void ReadsMainSaveDataProperly()
        {
            saveWriter.WriteOneToDisk(writeReq);

            AmanitaSaveData expectedMainSaveData = writeReq.MainSaveData as AmanitaSaveData;

            AmanitaSaveData whatWeGot = saveReader.ReadMainSaveDataFromDisk(readReq);
            Assert.AreEqual(expectedMainSaveData, whatWeGot, "The main save data was not read from disk properly.");

        }

    }
}