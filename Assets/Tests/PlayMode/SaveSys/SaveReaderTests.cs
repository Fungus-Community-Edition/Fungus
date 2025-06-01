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

            waitToYield = new WaitForSeconds(waitTime);
            string pathToEncoder = "SaveEncoders/FlowchartSaveEncoder";
            flowchartSaveEncoder = Resources.Load<FlowchartSaveEncoder>(pathToEncoder);
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
        protected FlowchartSaveEncoder flowchartSaveEncoder;
        protected FlowchartSaveData flowchartSaveData;
        protected BlockSaveEncoder blockSaveEncoder;

        protected SaveWriter saveWriter;
        protected SaveReader saveReader;
        protected SaveReadRequest readReq;
        float waitTime = 0.1f;
        WaitForSeconds waitToYield;

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }


        [UnityTest]
        public virtual IEnumerator ReadsMetadataProperly_NONEncrypted()
        {
            yield return CommonSetup();

            saveReader.ReadEncrypted = saveWriter.WriteEncrypted = false;
            saveWriter.WriteOneToDisk(writeReq);

            SaveMetaData expectedSaveMetaData = (SaveMetaData)writeReq.SaveMetaData;

            SaveMetaData whatWeGot = (SaveMetaData) saveReader.ReadMetadataFromDisk(readReq);
            Assert.AreEqual(expectedSaveMetaData, whatWeGot, "The save meta datas do not match.");
        }

        protected virtual IEnumerator CommonSetup()
        {
            yield return waitToYield;
            flowchartSaveData = flowchartSaveEncoder.EncodeToSave(flowchart);
            // ^We are expecting the flowchart encoder to use the block encoder as a sub

            CompositeSaveData mainSave = (CompositeSaveData)writeReq.MainState;
            mainSave.Add(flowchartSaveData.Serialized());
        }

        protected SaveWriteRequest writeReq = new SaveWriteRequest
        {
            SaveName = "TestSave",
            SlotNumber = 0,
            MainState = new CompositeSaveData(),
            SaveMetaData = new SaveMetaData(),
            BaseSaveDirectory = SaveDirectoryType.DataPath
        };


        [UnityTest]
        public virtual IEnumerator ReadsMetadataProperly_Encrypted()
        {
            yield return CommonSetup();

            saveReader.ReadEncrypted = saveWriter.WriteEncrypted = true;
            saveWriter.WriteOneToDisk(writeReq);

            SaveMetaData expectedMeta = (SaveMetaData)writeReq.SaveMetaData;

            SaveMetaData whatWeGot = (SaveMetaData)(saveReader.ReadMetadataFromDisk(readReq));
            Assert.AreEqual(expectedMeta, whatWeGot, "The save meta datas do not match.");
        }

        [UnityTest]
        public virtual IEnumerator ReadsMainSaveDataProperly_NONEncrypted()
        {
            yield return CommonSetup();

            saveReader.ReadEncrypted = saveWriter.WriteEncrypted = false;
            saveWriter.WriteOneToDisk(writeReq);

            CompositeSaveData expectedMainSaveData = writeReq.MainState as CompositeSaveData;
            CompositeSaveData whatWeGot = saveReader.ReadMainSaveDataFromDisk(readReq);

            Assert.AreEqual(expectedMainSaveData, whatWeGot, "The main save data was not read from disk properly.");

        }

        [UnityTest]
        public virtual IEnumerator ReadsMainSaveDataProperly_Encrypted()
        {
            yield return CommonSetup();

            saveReader.ReadEncrypted = saveWriter.WriteEncrypted = true;
            saveWriter.WriteOneToDisk(writeReq);

            CompositeSaveData expectedMainSaveData = writeReq.MainState as CompositeSaveData;
            CompositeSaveData whatWeGot = saveReader.ReadMainSaveDataFromDisk(readReq);

            Assert.AreEqual(expectedMainSaveData, whatWeGot, "The (encrypted) main save data was not read from disk properly.");

        }

        protected Encoding utf8 = Encoding.UTF8;
        protected const string fileNameFormat = "{0}_0{1}.{2}";

        


    }
}