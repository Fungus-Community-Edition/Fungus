using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.TestTools;
using Encoding = System.Text.Encoding;

namespace Amanita.SaveSystemTests
{
    public class FileIOIntegrationTests : CommonTestFunctionality
    {
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

        
    }

}