using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Encoding = System.Text.Encoding;

namespace SaveSystemTests
{
    public class SaveReaderTests : CommonTestFunctionality
    {
        // Needs SaveSystem for path resolution and writer/reader; no scene/flowchart needed.
        protected override bool ReqSaveSystem => true;
        protected override bool ReqSceneLoad => false;
        protected override bool ReqFlowchart => false;

        [Test]
        public virtual async Task ReadingMeta_Success_NONEncrypted()
        {
            await CommonSetupAsync().ConfigureAwait(false);
            saveReader.ExpectEncryption = saveWriter.ExpectEncryption = false;

            await CommonMetadataReadTestAsync().ConfigureAwait(false);
        }

        [Test]
        public virtual async Task ReadingMeta_Success_Encrypted()
        {
            await CommonSetupAsync().ConfigureAwait(false);
            saveReader.ExpectEncryption = saveWriter.ExpectEncryption = true;

            await CommonMetadataReadTestAsync().ConfigureAwait(false);
        }

        protected virtual IEnumerator CommonMetadataReadTest()
        {
            Task writeTask = saveWriter.WriteOneToDiskAsync(writeReq);
            yield return WaitFor(writeTask);

            var expectedMeta = (SaveMetaData)writeReq.SaveMetaData;

            Task<ISaveMetaData> readTask = saveReader.ReadMetadataFromDiskAsync(readReq);
            yield return WaitFor(readTask);

            var whatWeGot = (SaveMetaData)readTask.Result;
            Assert.AreEqual(expectedMeta, whatWeGot, "The save meta datas do not match.");
        }

        protected virtual async Task CommonMetadataReadTestAsync()
        {
            await saveWriter.WriteOneToDiskAsync(writeReq).ConfigureAwait(false);

            var expectedMeta = (SaveMetaData)writeReq.SaveMetaData;

            var meta = await saveReader.ReadMetadataFromDiskAsync(readReq).ConfigureAwait(false);
            var whatWeGot = (SaveMetaData)meta;

            Assert.AreEqual(expectedMeta, whatWeGot, "The save meta datas do not match.");
        }

        [Test]
        public virtual async Task ReadingMain_Success_NONEncrypted()
        {
            await CommonSetupAsync().ConfigureAwait(false);

            saveReader.ExpectEncryption = saveWriter.ExpectEncryption = false;

            await saveWriter.WriteOneToDiskAsync(writeReq).ConfigureAwait(false);

            var expectedMainSaveData = writeReq.MainState as CompositeSaveData;
            var result = await saveReader.ReadMainSaveDataFromDiskAsync(readReq).ConfigureAwait(false);

            Assert.AreEqual(expectedMainSaveData, result, "The main save data was not read from disk properly.");
        }

        [Test]
        public virtual async Task ReadingMain_Success_Encrypted()
        {
            await CommonSetupAsync().ConfigureAwait(false);

            saveReader.ExpectEncryption = saveWriter.ExpectEncryption = true;

            await saveWriter.WriteOneToDiskAsync(writeReq).ConfigureAwait(false);

            var expectedMainSaveData = writeReq.MainState as CompositeSaveData;
            var result = await saveReader.ReadMainSaveDataFromDiskAsync(readReq).ConfigureAwait(false);

            Assert.AreEqual(expectedMainSaveData, result, "The (encrypted) main save data was not read from disk properly.");
        }

        protected Encoding utf8 = Encoding.UTF8;
        protected const string fileNameFormat = "{0}_{1}.{2}";

        [Test]
        public virtual async Task ReadingMeta_Fail_ReportsMissingFile()
        {
            await CommonSetupAsync();

            var requestForNonexistentFile = new SaveReadRequest(readReq) { SlotNumber = 99 };

            string saveFolderPath = GetAndPrepSaveFolderPath(requestForNonexistentFile);
            GetFullFilePath(requestForNonexistentFile, saveFolderPath, out _);

            Assert.ThrowsAsync<FileNotFoundException>(() => saveReader.ReadMetadataFromDiskAsync(requestForNonexistentFile));
        }

        protected virtual string GetAndPrepSaveFolderPath(SaveReadRequest request)
        {
            string saveFolder = SaveSystem.S.GetSaveDirectory(request.BaseSaveDirectory);
            if (!string.IsNullOrEmpty(RelativeSavePath))
            {
                saveFolder = Path.Combine(saveFolder, RelativeSavePath);
            }

            Directory.CreateDirectory(saveFolder);
            return saveFolder;
        }

        protected virtual string RelativeSavePath => saveReader.RelativeSavePath;

        protected virtual void GetFullFilePath(SaveReadRequest request, string saveFolderPath, out string filePath)
        {
            string fileName = string.Format(fileNameFormat, SavePrefix, request.SlotNumber.ToString(saveReader.SaveNumberFormat), FileExtension);
            filePath = string.Format(FilePathFormat, saveFolderPath, fileName);
        }

        protected virtual string FilePathFormat => saveReader.FilePathFormat;

        [Test]
        public virtual async Task ReadingMain_Fail_ReportsMissingFile()
        {
            await CommonSetupAsync().ConfigureAwait(false);

            var requestForNonexistentFile = new SaveReadRequest(readReq) { SlotNumber = 99 };

            string saveFolderPath = GetAndPrepSaveFolderPath(requestForNonexistentFile);
            GetFullFilePath(requestForNonexistentFile, saveFolderPath, out _);

            Assert.ThrowsAsync<FileNotFoundException>(() => saveReader.ReadMainSaveDataFromDiskAsync(requestForNonexistentFile));
        }

        [Test]
        public virtual async Task ReadingMain_Fail_ReportsBadJsonOnMalformedData()
        {
            await CommonSetupAsync().ConfigureAwait(false);
            saveReader.ExpectEncryption = saveWriter.ExpectEncryption = false;

            var reqForMalformedFile = new SaveReadRequest(readReq) { SlotNumber = 71 };

            string fileNumFormatted = reqForMalformedFile.SlotNumber.ToString(saveReader.SaveNumberFormat);
            string fileName = string.Format(fileNameFormat, saveReader.SavePrefix, fileNumFormatted, saveReader.FileExtension);
            string filePath = saveReader.GetSaveFilePath(fileName, SaveDirectoryType.DataPath);

            string randomJunk = "e45 yvtm8q345yfg78 ty278rty452rt34t 7864r t376 r3";
            await File.WriteAllTextAsync(filePath, randomJunk);

            var readTask = saveReader.ReadMainSaveDataFromDiskAsync(reqForMalformedFile);

            Assert.ThrowsAsync<IOException>(() => readTask, "Does not throw an IOException upon reading invalid content");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public virtual async Task ReadingMeta_Fail_ReportsBadJsonOnMalformedData()
        {
            await CommonSetupAsync().ConfigureAwait(false);

            var reqForMalformedFile = new SaveReadRequest(readReq) { SlotNumber = 345 };

            string fileNumFormatted = reqForMalformedFile.SlotNumber.ToString(saveReader.SaveNumberFormat);
            string fileName = string.Format(fileNameFormat, saveReader.SavePrefix, fileNumFormatted, saveReader.FileExtension);
            string filePath = saveReader.GetSaveFilePath(fileName, SaveDirectoryType.DataPath);

            string randomJunk = "e45 yvtm8q345yfg78 ty278rty452rt34t 7864r t376 r3";

            await File.WriteAllTextAsync(filePath, randomJunk).ConfigureAwait(false);

            try
            {
                await saveReader.ReadMetadataFromDiskAsync(reqForMalformedFile).ConfigureAwait(false);
                Assert.Fail("Does not throw the expected IOException upon reading invalid content");
            }
            catch (IOException)
            {
                // Expected
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Test]
        public virtual void RecognizesRequiredBaseSaveDirectories()
        {
            var copyReq = new SaveReadRequest(readReq) { BaseSaveDirectory = SaveDirectoryType.DataPath };

            string pathFound = saveReader.GetSavePath(copyReq);
            StringAssert.StartsWith(Application.dataPath, pathFound,
                $"App data path to save {readReq.SlotNumber} not recognized correctly. It's instead recognized as {pathFound}");

            copyReq.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            pathFound = saveReader.GetSavePath(copyReq);
            StringAssert.StartsWith(Application.persistentDataPath, pathFound,
                $"App persistent data path to save {readReq.SlotNumber} not recognized correctly. It's instead recognized as {pathFound}");
        }

        [Test]
        public virtual void KnowsCorrectSaveFileNamesForPaths()
        {
            var copyReq = new SaveReadRequest(readReq);

            IList<int> validSlotNumbers = new[] { 1, 6, 12, 33, 64 };
            foreach (int slotNumber in validSlotNumbers)
            {
                copyReq.SlotNumber = slotNumber;

                string path = saveReader.GetSavePath(copyReq);
                string expectedEnd = string.Format(fileNameFormat, saveReader.SavePrefix, slotNumber.ToString(saveReader.SaveNumberFormat), saveReader.FileExtension);

                StringAssert.EndsWith(expectedEnd, path, $"File name for slot {copyReq.SlotNumber} is wrong.");
            }
        }

        [Test]
        public virtual async Task ReadingMeta_Success_NonDefaultMetaInput()
        {
            await CommonSetupAsync().ConfigureAwait(false);

            // Modify the meta on the request and verify it round-trips
            var metaBefore = (SaveMetaData)writeReq.SaveMetaData;
            metaBefore.SaveName = "BlastOff";
            metaBefore.TimeStamp = new DateTime(2025, 12, 31).ToUniversalTime();

            saveWriter.ExpectEncryption = saveReader.ExpectEncryption = false;

            await saveWriter.WriteOneToDiskAsync(writeReq).ConfigureAwait(false);

            var otherReadReq = new SaveReadRequest(readReq) { SlotNumber = metaBefore.SlotNumber };
            var metaAfter = (SaveMetaData)await saveReader.ReadMetadataFromDiskAsync(otherReadReq).ConfigureAwait(false);

            Assert.AreEqual(metaBefore, metaAfter);
        }

        protected override int CommonSetupDelay => 250;
    }
}