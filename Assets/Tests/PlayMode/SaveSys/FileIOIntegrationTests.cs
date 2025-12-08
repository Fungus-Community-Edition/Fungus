using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystemTests
{
    public class FileIOIntegrationTests : CommonTestFunctionality
    {
        // Needs SaveSystem, but not scene/flowchart.
        protected override bool ReqSceneLoad => false;
        protected override bool ReqFlowchart => false;

        public override void DoSetUp()
        {
            base.DoSetUp();
            saveReaderFallback = new TestSaveReader();
            saveReaderFallback.StorageSettings = storageSettings;
        }

        protected TestSaveReader saveReaderFallback;

        [Test]
        public async Task SmallData_RoundTrip()
        {
            var data = new CompositeSaveData();
            data.Add(new RawIntSaveData(42));

            var writeReqLocal = new SaveWriteRequest
            {
                SaveName = "SmallRT",
                SlotNumber = 1,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(writeReqLocal);
            string filePath = saveSys.GetSaveFilePath(SaveDirectoryType.DataPath, writeReqLocal.SlotNumber);
            saveFilePathsForCleanup.Add(filePath);

            var readReqLocal = new SaveReadRequest
            {
                SlotNumber = 1,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            var result = await saveReader.ReadMainSaveDataFromDiskAsync(readReqLocal);
            Assert.IsTrue(data.Equals(result));
        }

        [TestCase(42, SaveDirectoryType.DataPath)]
        [TestCase(55, SaveDirectoryType.PersistentDataPath)]
        [TestCase(99, SaveDirectoryType.DataPath)]
        [Test]
        public async Task Overwrite_Behavior_CreatesAndDeletesBackup(int slotNumber, SaveDirectoryType baseDir)
        {
            var meta = new SaveMetaData();

            // Ensure fresh test directory
            string savePath = saveWriter.GetSaveFilePath(baseDir, slotNumber);
            string backupPath = savePath + saveWriter.BackupFileExtension;
            saveFilePathsForCleanup.Add(savePath);
            saveFilePathsForCleanup.Add(backupPath);

            var originalData = new CompositeSaveData();
            originalData.Add(new RawIntSaveData(1));

            var firstWrite = new SaveWriteRequest
            {
                SlotNumber = slotNumber,
                BaseSaveDirectory = baseDir,
                SaveMetaData = meta,
                MainState = originalData
            };
            await saveWriter.WriteOneToDiskAsync(firstWrite);

            Assert.IsTrue(File.Exists(savePath), "Initial file was not created.");

            // STEP 2 — Overwrite with new data
            var newData = new CompositeSaveData();
            newData.Add(new RawIntSaveData(999));

            var secondWrite = new SaveWriteRequest
            {
                SlotNumber = slotNumber,
                BaseSaveDirectory = baseDir,
                SaveMetaData = meta,
                MainState = newData
            };
            saveWriter.DeleteBackupsPostOverwrite = false; // to allow checking the backup
            await saveWriter.WriteOneToDiskAsync(secondWrite);

            Assert.IsTrue(File.Exists(savePath), "Overwritten file was not created.");
            Assert.IsTrue(File.Exists(backupPath), "Backup file was not created during overwrite.");

            // Verify that the backup contains the original content (Value = 1)
            var backupContent = await File.ReadAllTextAsync(backupPath);
            string unescaped = Regex.Unescape(backupContent);
            Assert.IsTrue(unescaped.Contains("\"Value\": 1"), "Backup file did not preserve original content.");

            // STEP 3 — Write again with deletion enabled
            saveWriter.DeleteBackupsPostOverwrite = true;
            await saveWriter.WriteOneToDiskAsync(secondWrite); // trigger overwrite

            Assert.IsFalse(File.Exists(backupPath), "Backup file was not deleted after overwrite with cleanup enabled.");
        }

        [TestCase(1, SaveDirectoryType.DataPath)]
        [TestCase(5, SaveDirectoryType.PersistentDataPath)]
        [TestCase(99, SaveDirectoryType.DataPath)]
        public async Task SmallData_RoundTrip_VariedSlots(int slotNumber, SaveDirectoryType dirType)
        {
            var data = new CompositeSaveData();
            data.Add(new RawIntSaveData(42));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "RT_Varied",
                SlotNumber = slotNumber,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = dirType
            };
            await saveWriter.WriteOneToDiskAsync(writeReq);

            var readReq = new SaveReadRequest
            {
                SlotNumber = slotNumber,
                BaseSaveDirectory = dirType
            };
            var result = await saveReader.ReadMainSaveDataFromDiskAsync(readReq);

            Assert.IsTrue(data.Equals(result));
        }

        [Test]
        public async Task EncryptedData_RoundTrip()
        {
            saveWriter.ExpectEncryption = true;
            saveReader.ExpectEncryption = true;

            var data = new CompositeSaveData();
            data.Add(new RawIntSaveData(123));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "EncryptedRT",
                SlotNumber = 2,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(writeReq);

            var readReq = new SaveReadRequest
            {
                SlotNumber = 2,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            var result = await saveReader.ReadMainSaveDataFromDiskAsync(readReq);

            Assert.IsTrue(data.Equals(result), "Encrypted round-trip did not preserve data.");
        }

        [Test]
        public async Task EncryptedMetaData_RoundTrip()
        {
            saveWriter.ExpectEncryption = true;
            saveReader.ExpectEncryption = true;

            var meta = new SaveMetaData();
            meta.SaveName = "EncryptedMetaTest";

            var data = new CompositeSaveData();
            data.Add(new RawStringSaveData("hello meta"));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "EncryptedMetaRT",
                SlotNumber = 3,
                MainState = data,
                SaveMetaData = meta,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(writeReq);

            var readReq = new SaveReadRequest
            {
                SlotNumber = 3,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            var result = await saveReader.ReadMetadataFromDiskAsync(readReq);

            Assert.AreEqual(meta.SaveName, result.SaveName, "Encrypted metadata round-trip did not preserve SaveName.");
        }

        [Test]
        public async Task EncryptedFile_Corruption_Throws()
        {
            saveWriter.ExpectEncryption = true;
            saveReaderFallback.ExpectEncryption = true;

            var data = new CompositeSaveData();
            data.Add(new RawIntSaveData(999));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "CorruptTest",
                SlotNumber = 4,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(writeReq);

            // Corrupt the file
            var path = saveReaderFallback.GetSavePath(new SaveReadRequest { SlotNumber = 4, BaseSaveDirectory = SaveDirectoryType.DataPath });
            var bytes = File.ReadAllBytes(path);
            bytes[0] ^= 0xFF; // Flip some bits
            File.WriteAllBytes(path, bytes);

            var readReq = new SaveReadRequest
            {
                SlotNumber = 4,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string assertErrorMessage = "Corrupted encrypted file did not throw.";
            Assert.ThrowsAsync<InvalidDataException>(async () =>
            {
                await saveReaderFallback.ReadMainSaveDataFromDiskAsync(readReq);
            }, assertErrorMessage);
        }

        [Test, TestCaseSource(nameof(UnicodeTestCases))]
        public async Task EncryptedUnicodeData_RoundTrip(string unicodeString)
        {
            saveWriter.ExpectEncryption = true;
            saveReader.ExpectEncryption = true;

            var data = new CompositeSaveData();
            data.Add(new RawStringSaveData(unicodeString));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "UnicodeRT",
                SlotNumber = 5,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(writeReq);

            var readReq = new SaveReadRequest
            {
                SlotNumber = 5,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            var result = await saveReader.ReadMainSaveDataFromDiskAsync(readReq);

            Assert.IsTrue(result.Items.OfType<RawStringSaveData>().Any(u => u.Value == unicodeString),
                $"Unicode data '{unicodeString}' was not preserved in encrypted round-trip.");
        }

        public static IEnumerable<string> UnicodeTestCases()
        {
            yield return "こんにちは世界🌏 Привет мир 𝄞"; // Japanese, Russian, emoji, music symbol
            yield return "你好，世界"; // Chinese
            yield return "안녕하세요 세계"; // Korean
            yield return "مرحبا بالعالم"; // Arabic
            yield return "שלום עולם"; // Hebrew
            yield return "😀😃😄😁😆😅😂🤣"; // Emoji sequence
            yield return "Café naïve façade coöperate"; // Accented Latin characters
            yield return "𝔘𝔫𝔦𝔠𝔬𝔡𝔢 𝕋𝕖𝕤𝕥"; // Mathematical/Fraktur/Double-struck
            yield return "हैलो वर्ल्ड"; // Hindi
            yield return "Zażółć gęślą jaźń"; // Polish diacritics
        }

        [Test]
        public async Task EncryptedFlag_Mismatch_Throws()
        {
            // Write unencrypted
            saveWriter.ExpectEncryption = false;
            var data = new CompositeSaveData();
            data.Add(new RawIntSaveData(42));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "FlagMismatch",
                SlotNumber = 6,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(writeReq);

            // Try to read as encrypted
            TestSaveReader saveReader = saveReaderFallback;
            saveReader.ExpectEncryption = true;
            var readReq = new SaveReadRequest
            {
                SlotNumber = 6,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string assertErrorMessage = "Reading unencrypted file as encrypted did not throw.";
            Assert.ThrowsAsync<ArgumentException>(async () => await saveReader.ReadMainSaveDataFromDiskAsync(readReq).ConfigureAwait(false),
                assertErrorMessage);
        }
    }

    // Simple test SaveData types for round-trips
    [System.Serializable]
    public class RawIntSaveData : SaveData
    {
        [SerializeField] public int Value;

        public RawIntSaveData() { }
        public RawIntSaveData(int value) { Value = value; }

    }

    
}