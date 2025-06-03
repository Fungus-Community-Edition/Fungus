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

        [OneTimeSetUp]
        public virtual void DoOneTimeSetUp()
        {
            SaveSystem.InitPaths();
            
            saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
            saveReader = ScriptableObject.CreateInstance<SaveReader>();
            readReq = new SaveReadRequest
            {
                SlotNumber = writeReq.SlotNumber,
                BaseSaveDirectory = writeReq.BaseSaveDirectory,
            };

            waitToYield = new WaitForSeconds(waitTime);
            string pathToCodec = "SaveCodecs/FlowchartSaveCodec";
            flowchartSaveCodec = Resources.Load<FlowchartSaveCodec>(pathToCodec);

            pathToCodec = "SaveCodecs/BlockSaveCodec";
            blockSaveCodec = Resources.Load<BlockSaveCodec>(pathToCodec);

            CompositeSaveData compSave = (CompositeSaveData)writeReq.MainState;

            saveWriter.RelativeSavePath = relativePathForTesting;
            saveReader.RelativeSavePath = relativePathForTesting;
        }

        [SetUp]
        public virtual void DoSetUp()
        {
            PrepScene();
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
        protected FlowchartSaveCodec flowchartSaveCodec;
        protected FlowchartSaveData flowchartSaveData;
        protected BlockSaveCodec blockSaveCodec;

        protected SaveWriter saveWriter;
        protected SaveReader saveReader;
        protected SaveReadRequest readReq;
        float waitTime = 0.2f;
        WaitForSeconds waitToYield;
        protected string relativePathForTesting = "TempSaves";

        [TearDown]
        public virtual void DoTearDown()
        {
            writeReq.MainState = new CompositeSaveData { };
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [OneTimeTearDown]
        public virtual void DoOneTimeTearDown()
        {
            DeleteAllTestSaves();
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            saveReader.RelativeSavePath = saveReader.DefaultRelativeSavePath;
            if (varStateTestScene != null)
            {
                UnityObject.DestroyImmediate(varStateTestScene);
            }
        }

        protected void DeleteAllTestSaves()
        {
            foreach (string root in SaveSystem.SaveDirectoryPaths.Values)
            {
                string pathToTempFolder = Path.Combine(root, relativePathForTesting);

                IList<string> junk = Directory.EnumerateFiles(pathToTempFolder, "*.save",
                    SearchOption.AllDirectories).ToList();
                IList<string> junkMetas = Directory.EnumerateFiles(pathToTempFolder, "*.save.meta", SearchOption.AllDirectories).ToList();

                List<string> allJunk = new List<string>(junk);
                allJunk.AddRange(junkMetas);

                foreach (string file in allJunk)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }

            }
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
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
            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            // ^We are expecting the flowchart encoder to use the block encoder as a sub

            CompositeSaveData mainSave = (CompositeSaveData)writeReq.MainState;
            SaveDataUnit encodedFlowchartSave = flowchartSaveData.Serialized();
            mainSave.Add(encodedFlowchartSave);

            IList<BlockSaveData> blockSaves = blockSaveCodec.EncodeToMultiSave(flowchart);
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
        protected const string fileNameFormat = "{0}_{1}.{2}";

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
            string fileName = string.Format(fileNameFormat, SavePrefix, request.SlotNumber.ToString(saveReader.SaveNumberFormat), FileExtension);
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

            SaveReadRequest reqForMalformedFile = new SaveReadRequest(readReq);
            reqForMalformedFile.SlotNumber = 71;

            string saveFolderPath = GetAndPrepSaveFolderPath(reqForMalformedFile);
            GetFullFilePath(reqForMalformedFile, saveFolderPath, out string filePath);

            string randomJunk = "e45 yvtm8q345yfg78 ty278rty452rt34t 7864r t376 r3";

            File.WriteAllText(filePath, randomJunk);

            string errorMessage = string.Empty;
            try
            {
                saveReader.ReadMainSaveDataFromDisk(reqForMalformedFile);
            }
            catch (ArgumentException ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                Assert.Throws<ArgumentException>(() => saveReader.ReadMainSaveDataFromDisk(reqForMalformedFile), "Does not throw an argument exception upon reading invalid content");
                bool isAboutJson = errorMessage.ToLower().Contains("json");

                Assert.IsTrue(isAboutJson, $"The exception message is not what was expected:\n{errorMessage}");

                File.Delete(filePath);
            }
        }

        [UnityTest]
        public virtual IEnumerator ReadingMeta_ReportsBadJsonOnMalformedData()
        {
            yield return CommonSetup();

            SaveReadRequest reqForMalformedFile = new SaveReadRequest(readReq);
            reqForMalformedFile.SlotNumber = 345;

            string saveFolderPath = GetAndPrepSaveFolderPath(reqForMalformedFile);
            GetFullFilePath(reqForMalformedFile, saveFolderPath, out string filePath);

            string randomJunk = "e45 yvtm8q345yfg78 ty278rty452rt34t 7864r t376 r3";

            File.WriteAllText(filePath, randomJunk);
            
            string errorMessage = string.Empty;
            try
            {
                saveReader.ReadMetadataFromDisk(reqForMalformedFile);
            }
            catch (ArgumentException ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                
                Assert.Throws<ArgumentException>(() => saveReader.ReadMetadataFromDisk(reqForMalformedFile), "Does not throw an argument exception upon reading invalid content");
                bool isAboutJson = errorMessage.ToLower().Contains("json");

                Assert.IsTrue(isAboutJson, $"The exception message is not what was expected:\n{errorMessage}");
                File.Delete(filePath);
            }
        }

        [Test]
        public virtual void RecognizesRequiredBaseSaveDirectories()
        {
            SaveReadRequest copyReq = new SaveReadRequest(readReq);
            copyReq.BaseSaveDirectory = SaveDirectoryType.DataPath;

            string pathFound = saveReader.GetSavePath(copyReq);
            StringAssert.StartsWith(Application.dataPath, pathFound, $"App data path to save {readReq.SlotNumber} not recognized correctly. It's instead recognized as {pathFound}");

            copyReq.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            pathFound = saveReader.GetSavePath(copyReq);
            StringAssert.StartsWith(Application.persistentDataPath, pathFound, $"App persistent data path to save {readReq.SlotNumber} not recognized correctly. It's instead recognized as {pathFound}");

            copyReq.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            pathFound = saveReader.GetSavePath(copyReq);
            StringAssert.StartsWith(Application.streamingAssetsPath, pathFound, $"App streaming data path to save {readReq.SlotNumber} not recognized correctly. It's instead recognized as {pathFound}");

        }



        // No need for these two tests below. The json checks basically cover what
        // these two would.
        //[Test]
        //public virtual IEnumerator ReportUnexpectedlyEncryptedFiles()
        //{
        //    yield return CommonSetup();

        //    Assert.Ignore();
        //}
        //[UnityTest]
        //public virtual IEnumerator ReportUnexpectedlyUnencryptedFiles()
        //{
        //    yield return CommonSetup();

        //    saveReader.ReadEncrypted = true;
        //    saveWriter.WriteEncrypted = false;
        //    saveWriter.WriteOneToDisk(writeReq);

        //    Assert.Ignore();

        //}


        [Test]
        public virtual void KnowsCorrectSaveFileNamesForPaths()
        {
            SaveReadRequest copyReq = new SaveReadRequest(readReq);

            IList<int> validSlotNumbers = new int[] { 1, 6, 12, 33, 64 };
            foreach (int slotNumber in validSlotNumbers)
            {
                copyReq.SlotNumber = slotNumber;

                string path = saveReader.GetSavePath(copyReq);
                string expectedEnd = string.Format(fileNameFormat, saveReader.SavePrefix, slotNumber.ToString(saveReader.SaveNumberFormat), saveReader.FileExtension);

                StringAssert.EndsWith(expectedEnd, path, $"File name for slot {copyReq.SlotNumber} is wrong.");
            }
            
        }

        [UnityTest]

        public virtual IEnumerator ReadsNonDefaultMetadata()
        {
            yield return CommonSetup();
            SaveWriteRequest withCustomMeta = new SaveWriteRequest(writeReq);
            SaveMetaData metaBefore = (SaveMetaData)withCustomMeta.SaveMetaData;
            metaBefore.Name = "BlastOff";
            metaBefore.TimeStamp = new DateTime(2025, 12, 31).ToUniversalTime();

            saveWriter.WriteEncrypted = saveReader.ReadEncrypted = false;
            saveWriter.WriteOneToDisk(writeReq);

            SaveReadRequest otherReadReq = new SaveReadRequest(readReq);
            otherReadReq.SlotNumber = metaBefore.SlotNumber;

            var metaAfter = (SaveMetaData)saveReader.ReadMetadataFromDisk(otherReadReq);
            Assert.AreEqual(metaBefore, metaAfter);

        }

        [Test]
        public void ReadMain_WrongEncryptionFlag_Throws()
        {
            saveWriter.WriteEncrypted = true;
            saveReader.ReadEncrypted = false;

            saveWriter.WriteOneToDisk(writeReq);

            Assert.Throws<ArgumentException>(() =>
                saveReader.ReadMainSaveDataFromDisk(readReq)
            );
        }

    }
}