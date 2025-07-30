using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Amanita.SaveSystemTests
{
    public class FileIOIntegrationTests : CommonTestFunctionality
    {
        public override void DoSetUp()
        {
            base.DoSetUp();
            saveReaderFallback = new TestSaveReader();
        }

        protected TestSaveReader saveReaderFallback;
        // ^For when we need to avoid hangs from async calls (what with the quirks with the test runner)

        [Test]
        public async Task SmallData_RoundTrip()
        {
            var data = new CompositeSaveData();
            data.Add(new SaveDataUnit("Foo", "{\"x\":42}"));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "SmallRT",
                SlotNumber = 1,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(writeReq);

            var readReq = new SaveReadRequest
            {
                SlotNumber = 1,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            var result = await saveReader.ReadMainSaveDataFromDisk(readReq);

            Assert.IsTrue(data.Equals(result));
        }

        [TestCase(42, SaveDirectoryType.DataPath)]
        [TestCase(55, SaveDirectoryType.PersistentDataPath)]
        [TestCase(99, SaveDirectoryType.DataPath)]
        [TestCase(39, SaveDirectoryType.StreamingAssetsPath)]
        [Test]
        public async Task Overwrite_Behavior_CreatesAndDeletesBackup(int slotNumber, SaveDirectoryType baseDir)
        {
            var meta = new SaveMetaData();

            // Ensure fresh test directory
            string savePath = FileUtils.GetPathToFile(baseDir, slotNumber, saveWriter);
            string backupPath = savePath + saveWriter.BackupFileExtension;
            if (File.Exists(savePath)) File.Delete(savePath);
            if (File.Exists(backupPath)) File.Delete(backupPath);

            // STEP 1 — Write initial data
            var originalData = new CompositeSaveData();
            originalData.Add(new SaveDataUnit("State", "{\"value\":1}"));

            var firstWrite = new SaveWriteRequest
            {
                SlotNumber = slotNumber,
                BaseSaveDirectory = baseDir,
                SaveMetaData = meta,
                MainState = originalData
            };
            await saveWriter.WriteOneToDisk(firstWrite);

            Assert.IsTrue(File.Exists(savePath), "Initial file was not created.");

            // STEP 2 — Overwrite with new data
            var newData = new CompositeSaveData();
            newData.Add(new SaveDataUnit("State", "{\"value\":999}"));

            var secondWrite = new SaveWriteRequest
            {
                SlotNumber = slotNumber,
                BaseSaveDirectory = baseDir,
                SaveMetaData = meta,
                MainState = newData
            };
            saveWriter.DeleteBackupsPostOverwrite = false; // to allow checking the backup
            await saveWriter.WriteOneToDisk(secondWrite);

            Assert.IsTrue(File.Exists(savePath), "Overwritten file was not created.");
            Assert.IsTrue(File.Exists(backupPath), "Backup file was not created during overwrite.");

            // Optional: Verify that the backup contains the original content
            var backupContent = await File.ReadAllTextAsync(backupPath);
            string unescaped = Regex.Unescape(backupContent);
            Assert.IsTrue(unescaped.Contains("\"value\":1"), "Backup file did not preserve original content.");

            // STEP 3 — Write again with deletion enabled
            saveWriter.DeleteBackupsPostOverwrite = true;
            await saveWriter.WriteOneToDisk(secondWrite); // trigger overwrite

            Assert.IsFalse(File.Exists(backupPath), "Backup file was not deleted after overwrite with cleanup enabled.");
        }


        [TestCase(1, SaveDirectoryType.DataPath)]
        [TestCase(5, SaveDirectoryType.PersistentDataPath)]
        [TestCase(99, SaveDirectoryType.DataPath)]
        [TestCase(39, SaveDirectoryType.StreamingAssetsPath)]
        public async Task SmallData_RoundTrip_VariedSlots(int slotNumber, SaveDirectoryType dirType)
        {
            var data = new CompositeSaveData();
            data.Add(new SaveDataUnit("Foo", "{\"x\":42}"));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "RT_Varied",
                SlotNumber = slotNumber,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = dirType
            };
            await saveWriter.WriteOneToDisk(writeReq);

            var readReq = new SaveReadRequest
            {
                SlotNumber = slotNumber,
                BaseSaveDirectory = dirType
            };
            var result = await saveReader.ReadMainSaveDataFromDisk(readReq);

            Assert.IsTrue(data.Equals(result));
        }

        [Test]
        public async Task EncryptedData_RoundTrip()
        {
            saveWriter.WriteEncrypted = true;
            saveReader.ReadEncrypted = true;

            var data = new CompositeSaveData();
            data.Add(new SaveDataUnit("Foo", "{\"x\":123}"));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "EncryptedRT",
                SlotNumber = 2,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(writeReq);

            var readReq = new SaveReadRequest
            {
                SlotNumber = 2,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            var result = await saveReader.ReadMainSaveDataFromDisk(readReq);

            Assert.IsTrue(data.Equals(result), "Encrypted round-trip did not preserve data.");
        }

        [Test]
        public async Task EncryptedMetaData_RoundTrip()
        {
            saveWriter.WriteEncrypted = true;
            saveReader.ReadEncrypted = true;

            var meta = new SaveMetaData();
            meta.SaveName = "EncryptedMetaTest";

            var data = new CompositeSaveData();
            data.Add(new SaveDataUnit("Bar", "{\"y\":456}"));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "EncryptedMetaRT",
                SlotNumber = 3,
                MainState = data,
                SaveMetaData = meta,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(writeReq);

            var readReq = new SaveReadRequest
            {
                SlotNumber = 3,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            var result = await saveReader.ReadMetadataFromDisk(readReq);

            Assert.AreEqual(meta.SaveName, result.SaveName, "Encrypted metadata round-trip did not preserve SaveName.");
        }

        [Test]
        public async Task EncryptedFile_Corruption_Throws()
        {
            saveWriter.WriteEncrypted = true;
            saveReaderFallback.ReadEncrypted = true;

            var data = new CompositeSaveData();
            data.Add(new SaveDataUnit("Foo", "{\"x\":999}"));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "CorruptTest",
                SlotNumber = 4,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(writeReq);

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
                await saveReaderFallback.ReadMainSaveDataFromDisk(readReq);
            }, assertErrorMessage);
        }

        [Test, TestCaseSource(nameof(UnicodeTestCases))]
        public async Task EncryptedUnicodeData_RoundTrip(string unicodeString)
        {
            saveWriter.WriteEncrypted = true;
            saveReader.ReadEncrypted = true;

            var data = new CompositeSaveData();
            data.Add(new SaveDataUnit("Unicode", unicodeString));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "UnicodeRT",
                SlotNumber = 5,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(writeReq);

            var readReq = new SaveReadRequest
            {
                SlotNumber = 5,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            var result = await saveReader.ReadMainSaveDataFromDisk(readReq);

            Assert.IsTrue(result.Units.Any(u => u.Content == unicodeString), $"Unicode data '{unicodeString}' was not preserved in encrypted round-trip.");
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
            saveWriter.WriteEncrypted = false;
            var data = new CompositeSaveData();
            data.Add(new SaveDataUnit("Foo", "{\"x\":42}"));

            var writeReq = new SaveWriteRequest
            {
                SaveName = "FlagMismatch",
                SlotNumber = 6,
                MainState = data,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(writeReq);

            // Try to read as encrypted
            TestSaveReader saveReader = saveReaderFallback;
            saveReader.ReadEncrypted = true;
            var readReq = new SaveReadRequest
            {
                SlotNumber = 6,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string assertErrorMessage = "Reading unencrypted file as encrypted did not throw.";
            Assert.ThrowsAsync<ArgumentException>(async () => await saveReader.ReadMainSaveDataFromDisk(readReq).ConfigureAwait(false),
                assertErrorMessage);
        }

    }

}