using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using Encoding = System.Text.Encoding;
using UnityObject = UnityEngine.Object;

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

            SaveMetaData expectedSaveMetaData = (SaveMetaData)writeReq.SaveMetaData;

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
        public virtual void ReadsMainSaveDataProperly_NONEncrypted()
        {
            saveWriter.WriteEncrypted = false;
            saveWriter.WriteOneToDisk(writeReq);

            AmanitaSaveData expectedMainSaveData = writeReq.MainSaveData as AmanitaSaveData;
            AmanitaSaveData whatWeGot = saveReader.ReadMainSaveDataFromDisk(readReq);

            Assert.AreEqual(expectedMainSaveData, whatWeGot, "The main save data was not read from disk properly.");

        }

        [Test]
        public virtual void ReadsMainSaveDataProperly_Encrypted()
        {
            saveWriter.WriteEncrypted = true;
            saveWriter.WriteOneToDisk(writeReq);

            AmanitaSaveData expectedMainSaveData = writeReq.MainSaveData as AmanitaSaveData;
            AmanitaSaveData whatWeGot = saveReader.ReadMainSaveDataFromDisk(readReq);

            Assert.AreEqual(expectedMainSaveData, whatWeGot, "The (encrypted) main save data was not read from disk properly.");

        }

        protected Encoding utf8 = Encoding.UTF8;
        protected const string fileNameFormat = "{0}_0{1}.{2}";

    }
}