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
    public class SaveReaderTests : CommonTestFunctionality
    {
        [UnityTest]
        public virtual IEnumerator ReadsMetadataProperly_NONEncrypted()
        {
            yield return CommonSetup();

            saveReader.ReadEncrypted = saveWriter.WriteEncrypted = false;

            Task writeTask = saveWriter.WriteOneToDisk(writeReq);
            yield return WaitFor(writeTask);
            

            SaveMetaData expectedSaveMetaData = (SaveMetaData)writeReq.SaveMetaData;
            Task<ISaveMetaData> readTask = saveReader.ReadMetadataFromDisk(readReq);
            yield return WaitFor(readTask);
            SaveMetaData whatWeGot = (SaveMetaData) readTask.Result;
            Assert.AreEqual(expectedSaveMetaData, whatWeGot, "The save meta datas do not match.");
        }


        [UnityTest]
        public virtual IEnumerator ReadsMetadataProperly_Encrypted()
        {
            yield return CommonSetup();

            saveReader.ReadEncrypted = saveWriter.WriteEncrypted = true;

            Task writeTask = saveWriter.WriteOneToDisk(writeReq);
            yield return WaitFor(writeTask);

            SaveMetaData expectedMeta = (SaveMetaData)writeReq.SaveMetaData;

            Task<ISaveMetaData> readTask = saveReader.ReadMetadataFromDisk(readReq);
            yield return WaitFor(readTask);

            SaveMetaData whatWeGot = (SaveMetaData)readTask.Result;
            Assert.AreEqual(expectedMeta, whatWeGot, "The save meta datas do not match.");
        }

        [UnityTest]
        public virtual IEnumerator ReadsMainSaveDataProperly_NONEncrypted()
        {
            yield return CommonSetup();

            saveReader.ReadEncrypted = saveWriter.WriteEncrypted = false;

            Task writeTask = saveWriter.WriteOneToDisk(writeReq);
            yield return WaitFor(writeTask);

            CompositeSaveData expectedMainSaveData = writeReq.MainState as CompositeSaveData;
            Task<CompositeSaveData> readTask = saveReader.ReadMainSaveDataFromDisk(readReq);
            yield return WaitFor(readTask);
            CompositeSaveData whatWeGot = readTask.Result;

            Assert.AreEqual(expectedMainSaveData, whatWeGot, "The main save data was not read from disk properly.");

        }

        [UnityTest]
        public virtual IEnumerator ReadsMainSaveDataProperly_Encrypted()
        {
            yield return CommonSetup();

            saveReader.ReadEncrypted = saveWriter.WriteEncrypted = true;

            Task writeTask = saveWriter.WriteOneToDisk(writeReq);
            yield return WaitFor(writeTask);

            CompositeSaveData expectedMainSaveData = writeReq.MainState as CompositeSaveData;
            Task<CompositeSaveData> readTask = saveReader.ReadMainSaveDataFromDisk(readReq);
            yield return WaitFor(readTask);
            CompositeSaveData whatWeGot = readTask?.Result;

            Assert.AreEqual(expectedMainSaveData, whatWeGot, "The (encrypted) main save data was not read from disk properly.");

        }

        protected Encoding utf8 = Encoding.UTF8;
        protected const string fileNameFormat = "{0}_{1}.{2}";

        [Test]
        public virtual async Task ReadingMetadata_ReportsMissingFile()
        {
            await CommonSetupAsync();

            SaveReadRequest requestForNonexistentFile = new SaveReadRequest(readReq)
            {
                SlotNumber = 99
            };

            string saveFolderPath = GetAndPrepSaveFolderPath(requestForNonexistentFile);
            GetFullFilePath(requestForNonexistentFile, saveFolderPath, out string filePath);

            Assert.ThrowsAsync<FileNotFoundException>(() => saveReader.ReadMetadataFromDisk(requestForNonexistentFile));

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

        protected virtual string FilePathFormat { get { return saveReader.FilePathFormat; } }

        [Test]
        public virtual async Task ReadingMainContent_ReportsMissingFile()
        {
            await CommonSetupAsync();

            SaveReadRequest requestForNonexistentFile = new SaveReadRequest(readReq);
            requestForNonexistentFile.SlotNumber = 99;

            string saveFolderPath = GetAndPrepSaveFolderPath(requestForNonexistentFile);
            GetFullFilePath(requestForNonexistentFile, saveFolderPath, out string filePath);

            Assert.ThrowsAsync<FileNotFoundException>(() => saveReader.ReadMainSaveDataFromDisk(requestForNonexistentFile));

        }

        [UnityTest]
        public virtual IEnumerator ReadingMainContent_ReportsBadJsonOnMalformedData()
        {
            yield return CommonSetup();

            SaveReadRequest reqForMalformedFile = new SaveReadRequest(readReq);
            reqForMalformedFile.SlotNumber = 71;

            string fileNumFormatted = reqForMalformedFile.SlotNumber.ToString(saveReader.SaveNumberFormat);
            string fileName = string.Format(fileNameFormat, saveReader.SavePrefix,
                fileNumFormatted, saveReader.FileExtension);
            string filePath = FileUtils.GetPathToFile(SaveDirectoryType.DataPath, fileName, saveReader.RelativeSavePath);

            //GetFullFilePath(reqForMalformedFile, saveFolderPath, out string filePath);

            string randomJunk = "e45 yvtm8q345yfg78 ty278rty452rt34t 7864r t376 r3";

            //File.WriteAllText(filePath, randomJunk);

            Assert.ThrowsAsync<ArgumentException>(() => saveReader.ReadMainSaveDataFromDisk(reqForMalformedFile),
                "Does not throw an ArgumentException upon reading invalid content");
            //string errorMessage = string.Empty;
            //bool throwsIt = false;
            //try
            //{
            //    Task readTask = 
            //    await 
            //}
            //catch (ArgumentException ex)
            //{
            //    errorMessage = ex.Message;
            //    throwsIt = true;
            //}
            //catch (Exception ex)
            //{
            //    errorMessage = ex.Message;
            //}
            //finally
            //{
            //    bool isAboutJson = errorMessage.ToLower().Contains("json");

            //    Assert.IsTrue(isAboutJson, $"The exception message is not what was expected:\n{errorMessage}");
            //    Assert.IsTrue(throwsIt, "Does not throw an ArgumentException upon reading invalid content");


            //}

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public virtual async Task ReadingMeta_ReportsBadJsonOnMalformedData()
        {
            await CommonSetupAsync().ConfigureAwait(false);

            SaveReadRequest reqForMalformedFile = new SaveReadRequest(readReq);
            reqForMalformedFile.SlotNumber = 345;

            string saveFolderPath = GetAndPrepSaveFolderPath(reqForMalformedFile);
            GetFullFilePath(reqForMalformedFile, saveFolderPath, out string filePath);

            string randomJunk = "e45 yvtm8q345yfg78 ty278rty452rt34t 7864r t376 r3";

            await File.WriteAllTextAsync(filePath, randomJunk).ConfigureAwait(false);
            
            string errorMessage = string.Empty;
            bool throwsIt = false;
            try
            {
                await saveReader.ReadMetadataFromDisk(reqForMalformedFile);
            }
            catch (ArgumentException ex)
            {
                errorMessage = ex.Message;
                throwsIt = true;
            }
            finally
            {
                Assert.IsTrue(throwsIt, "Does not throw the expected ArgumentException upon reading invalid content");
                
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

            Task writeTask = saveWriter.WriteOneToDisk(writeReq);
            yield return WaitFor(writeTask);

            SaveReadRequest otherReadReq = new SaveReadRequest(readReq);
            otherReadReq.SlotNumber = metaBefore.SlotNumber;

            Task<ISaveMetaData> readTask = saveReader.ReadMetadataFromDisk(otherReadReq);
            yield return WaitFor(readTask);
            var metaAfter =  (SaveMetaData) readTask.Result;
            Assert.AreEqual(metaBefore, metaAfter);

        }

        [Test]
        public virtual async Task ReadMain_WrongEncryptionFlag_Throws()
        {
            saveWriter.WriteEncrypted = true;
            saveReader.ReadEncrypted = false;

            bool throwsIt = false;
            try
            {
                Task writeTask = saveWriter.WriteOneToDisk(writeReq);
                await writeTask;

                Task readTask = saveReader.ReadMainSaveDataFromDisk(readReq);
                await readTask;
            }
            catch (ArgumentException)
            {
                throwsIt = true;
            }
            finally
            {
                Assert.IsTrue(throwsIt);
            }
        }

    }
}