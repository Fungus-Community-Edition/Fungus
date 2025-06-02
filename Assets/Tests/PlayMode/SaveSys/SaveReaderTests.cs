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

            pathToEncoder = "SaveEncoders/BlockSaveEncoder";
            blockSaveEncoder = Resources.Load<BlockSaveEncoder>(pathToEncoder);

            CompositeSaveData compSave = (CompositeSaveData)writeReq.MainState;


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
            writeReq.MainState = new CompositeSaveData { };
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
            SaveDataUnit encodedFlowchartSave = flowchartSaveData.Serialized();
            mainSave.Add(encodedFlowchartSave);

            IList<BlockSaveData> blockSaves = blockSaveEncoder.EncodeToMultiSave(flowchart);
            foreach (var blockSave in blockSaves)
            {
                SaveDataUnit saveDataUnit = blockSave.Serialized();
                mainSave.Add(saveDataUnit);
            }

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

        [UnityTest]
        public virtual IEnumerator ReadingMetadata_ReportsMissingFile()
        {
            yield return CommonSetup();

            SaveReadRequest requestForNonexistentFile = new SaveReadRequest(readReq);
            requestForNonexistentFile.SlotNumber = 99;

            string saveFolderPath = GetAndPrepSaveFolderPath(requestForNonexistentFile);
            GetFullFilePath(requestForNonexistentFile, saveFolderPath, out string filePath);

            Assert.Throws<FileNotFoundException>(() => saveReader.ReadMetadataFromDisk(requestForNonexistentFile));

        }

        protected virtual string GetAndPrepSaveFolderPath(SaveReadRequest request)
        {
            string saveFolder = SaveSystem.SaveDirectoryPaths[request.BaseSaveDirectory];
            bool thereIsRelativePathToConsider = RelativeSavePath.Count() > 0;
            if (thereIsRelativePathToConsider)
            {
                saveFolder = Path.Combine(saveFolder, RelativeSavePath);
            }

            Directory.CreateDirectory(saveFolder); // In case it doesn't exist.
            return saveFolder;
        }

        protected virtual string RelativeSavePath { get { return saveReader.RelativeSavePath; } }

        protected virtual void GetFullFilePath(SaveReadRequest request, string saveFolderPath,
            out string filePath)
        {
            string fileName = string.Format(fileNameFormat, SavePrefix, request.SlotNumber, FileExtension);
            filePath = string.Format(FilePathFormat, saveFolderPath, fileName);
        }

        protected virtual string SavePrefix { get { return saveReader.SavePrefix; } }
        protected virtual string FileExtension { get { return saveReader.FileExtension; } }
        protected virtual string FilePathFormat { get { return saveReader.FilePathFormat; } }

        [UnityTest]
        public virtual IEnumerator ReadingMainContent_ReportsMissingFile()
        {
            yield return CommonSetup();

            SaveReadRequest requestForNonexistentFile = new SaveReadRequest(readReq);
            requestForNonexistentFile.SlotNumber = 99;

            string saveFolderPath = GetAndPrepSaveFolderPath(requestForNonexistentFile);
            GetFullFilePath(requestForNonexistentFile, saveFolderPath, out string filePath);

            Assert.Throws<FileNotFoundException>(() => saveReader.ReadMainSaveDataFromDisk(requestForNonexistentFile));

        }

        [UnityTest]
        public virtual IEnumerator ReadingMainContent_ReportsBadJsonOnMalformedData()
        {
            yield return CommonSetup();

            SaveReadRequest requestForNonexistentFile = new SaveReadRequest(readReq);
            requestForNonexistentFile.SlotNumber = 71;

            string saveFolderPath = GetAndPrepSaveFolderPath(requestForNonexistentFile);
            GetFullFilePath(requestForNonexistentFile, saveFolderPath, out string filePath);

            string randomJunk = "e45 yvtm8q345yfg78 ty278rty452rt34t 7864r t376 r3";

            File.WriteAllText(filePath, randomJunk);

            string errorMessage = string.Empty;
            try
            {
                saveReader.ReadMainSaveDataFromDisk(requestForNonexistentFile);
            }
            catch (ArgumentException ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                Assert.Throws<ArgumentException>(() => saveReader.ReadMainSaveDataFromDisk(requestForNonexistentFile), "Does not throw an argument exception upon reading invalid content");
                bool isAboutJson = errorMessage.ToLower().Contains("json");

                Assert.IsTrue(isAboutJson, $"The exception message is not what was expected:\n{errorMessage}");
            }
        }

        [UnityTest]
        public virtual IEnumerator ReadingMeta_ReportsBadJsonOnMalformedData()
        {
            yield return CommonSetup();

            SaveReadRequest requestForNonexistentFile = new SaveReadRequest(readReq);
            requestForNonexistentFile.SlotNumber = 345;

            string saveFolderPath = GetAndPrepSaveFolderPath(requestForNonexistentFile);
            GetFullFilePath(requestForNonexistentFile, saveFolderPath, out string filePath);

            string randomJunk = "e45 yvtm8q345yfg78 ty278rty452rt34t 7864r t376 r3";

            File.WriteAllText(filePath, randomJunk);

            string errorMessage = string.Empty;
            try
            {
                saveReader.ReadMetadataFromDisk(requestForNonexistentFile);
            }
            catch (ArgumentException ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                Assert.Throws<ArgumentException>(() => saveReader.ReadMetadataFromDisk(requestForNonexistentFile), "Does not throw an argument exception upon reading invalid content");
                bool isAboutJson = errorMessage.ToLower().Contains("json");

                Assert.IsTrue(isAboutJson, $"The exception message is not what was expected:\n{errorMessage}");
            }
        }

    }
}