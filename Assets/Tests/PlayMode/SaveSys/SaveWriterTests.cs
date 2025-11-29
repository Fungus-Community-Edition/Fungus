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
using Amanita.FSExt;
using FullSerializer;
using Amanita;
using UnityEngine.TestTools;

namespace SaveSystemTests
{
    public class SaveWriterTests : CommonTestFunctionality
    {
        // Only need SaveSystem core (writer/reader). No scene or flowchart variables.
        protected override bool ReqSaveSystem => true;
        protected override bool ReqSceneLoad => false;
        protected override bool ReqFlowchart => false;
        protected override bool ShouldDeleteTestSavesAtEnd => true;

        protected SaveWriteRequest writeArgs;

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            // Fresh request per test to avoid cross‑test mutation
            writeArgs = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            saveWriter.RelativeSavePath = "";
        }

        protected string FileNameFormat => saveWriter.FileNameFormat;
        protected string SaveNumberFormat => saveWriter.SaveNumberFormat;

        // ------------- Core helpers ------------

        protected virtual async Task CommonSaveWriteTestAsync(SaveWriteRequest args)
        {
            string fullPath = saveSys.GetSaveFilePath(args.BaseSaveDirectory, args.SlotNumber);
            await saveWriter.WriteOneToDiskAsync(args);
            bool fileWasWritten = File.Exists(fullPath);
            saveFilePathsForCleanup.Add(fullPath);
            Assert.IsTrue(fileWasWritten, "Save file was not created.");
        }

        // ------------- Successful writes ------------

        [Test]
        public async Task WritesSaveToDisk_BaseDataPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;
            await CommonSaveWriteTestAsync(writeArgs);
        }

        [Test]
        public async Task WritesSaveToDisk_BasePersistentDataPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            await CommonSaveWriteTestAsync(writeArgs);
        }

        [Test]
        public async Task WritesSaveToDisk_BaseDataPath_RelativePathIncluded()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;
            await CommonSaveWriteTestAsync(writeArgs);
        }

        [Test]
        public async Task WritesSaveToDisk_BasePersistentDataPath_RelativePathIncluded()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            await CommonSaveWriteTestAsync(writeArgs);
        }

        // ------------- Rejection tests ------------

        [Test]
        public async Task WritesSaveToDisk_AnyPath_RejectNullSaveData()
        {
            await CommonSetupAsync();
            var bad = new SaveWriteRequest
            {
                SaveName = "Bad",
                SlotNumber = 0,
                MainState = null,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.ThrowsAsync<ArgumentNullException>(() => saveWriter.WriteOneToDiskAsync(bad));
            bad.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.ThrowsAsync<ArgumentNullException>(() => saveWriter.WriteOneToDiskAsync(bad));
        }

        [Test]
        public async Task WritesSaveToDisk_AnyPath_RejectNegativeSlotNumber()
        {
            await CommonSetupAsync();
            var bad = new SaveWriteRequest
            {
                SaveName = "Bad",
                SlotNumber = -1,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDiskAsync(bad));
            bad.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDiskAsync(bad));
        }

        [Test]
        public async Task WritesSaveToDisk_AnyPath_RejectInvalidBaseDirectory()
        {
            await CommonSetupAsync();
            var bad = new SaveWriteRequest
            {
                SaveName = "Bad",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = (SaveDirectoryType)999
            };
            Assert.ThrowsAsync<ArgumentException>(() => saveWriter.WriteOneToDiskAsync(bad));
        }

        // ------------- Multi-write ------------

        protected IList<SaveWriteRequest> multipleThingsToWrite => new List<SaveWriteRequest>
        {
            new SaveWriteRequest
            {
                SaveName = "TestSave1",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            },
            new SaveWriteRequest
            {
                SaveName = "TestSave2",
                SlotNumber = 1,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.PersistentDataPath
            },
        };

        [Test]
        public async Task WriteAllToDisk_AllSuccessful()
        {
            await CommonSetupAsync();
            bool allWritten = await saveWriter.WriteAllToDiskAsync(multipleThingsToWrite);
            Assert.IsTrue(allWritten, "Not all saves were written successfully.");
        }

        [Test]
        public async Task WriteAllToDisk_PartialSuccess()
        {
            var withNull = new List<SaveWriteRequest>(multipleThingsToWrite) { [1] = null };
            bool threw = false;
            try
            {
                await saveWriter.WriteAllToDiskAsync(withNull);
            }
            catch
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Expected exception when list contains null request.");
        }

        [Test]
        public void WriteAllToDisk_RejectNullList()
        {
            Assert.ThrowsAsync<NullReferenceException>(() => saveWriter.WriteAllToDiskAsync(null));
        }

        // ------------- Content verification ------------

        [Test]
        public async Task VerifyFileContent_NONEncrypted_AfterWrite()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            var args = new SaveWriteRequest
            {
                SaveName = "ContentVerification",
                SlotNumber = 2,
                MainState = MainSave,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string expectedMetaJson = serializer.ToJson(args.SaveMetaData, true);
            string expectedMainJson = serializerForTest.ToJson(args.MainState, true);
            string expectedAll = $"{expectedMetaJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainJson}{SaveDiskAccessor.CompletionMarker}";

            await CommonSaveWriteTestAsync(args);

            string path = saveWriter.GetSaveFilePath(args.BaseSaveDirectory, args.SlotNumber);
            saveFilePathsForCleanup.Add(path);
            string actual = await File.ReadAllTextAsync(path);
            Assert.AreEqual(expectedAll, actual);
        }

        [Test]
        public async Task VerifyFileContent_Encrypted_AfterWrite()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = true;

            var args = new SaveWriteRequest
            {
                SaveName = "EncryptedVerification",
                SlotNumber = 3,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            fsSerializer ser = AmanitaManager.DefaultSerializer;
            string expectedMetaJson = ser.ToJson(args.SaveMetaData, true);
            string expectedMainJson = serializerForTest.ToJson(args.MainState, true);
            string expectedPlain = $"{expectedMetaJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainJson}{SaveDiskAccessor.CompletionMarker}";

            byte key = 0xAA;
            byte[] expectedEncrypted = System.Text.Encoding.UTF8
                .GetBytes(expectedPlain)
                .Select(b => (byte)(b ^ key))
                .ToArray();

            await CommonSaveWriteTestAsync(args);

            string path = saveWriter.GetSaveFilePath(args.BaseSaveDirectory, args.SlotNumber);
            saveFilePathsForCleanup.Add(path);
            byte[] encrypted = await File.ReadAllBytesAsync(path);
            byte[] decrypted = encrypted.Select(b => (byte)(b ^ key)).ToArray();
            string decryptedStr = System.Text.Encoding.UTF8.GetString(decrypted);
            Assert.AreEqual(expectedPlain, decryptedStr);
        }

        // ------------- Events ------------

        [Test]
        public async Task EventInvocation_AmanitaSaveWritten()
        {
            bool responded = false;
            void Handler(SaveWriteResults r)
            {
                responded = true;
                Assert.IsNotNull(r.SaveData);
                Assert.IsNotNull(r.FilePath);
                Assert.IsNotNull(r.FileName);
            }
            SaveSysSignals.AmanitaSaveWritten += Handler;
            await saveWriter.WriteOneToDiskAsync(writeArgs);
            SaveSysSignals.AmanitaSaveWritten -= Handler;
            Assert.IsTrue(responded);
        }

        [Test]
        public async Task EventInvocation_AmanitaSaveWritten_MultipleWrites()
        {
            bool responded = false;
            void Handler(SaveWriteResults r) => responded = true;
            SaveSysSignals.AmanitaSaveWritten += Handler;
            await saveWriter.WriteOneToDiskAsync(writeArgs);
            await saveWriter.WriteOneToDiskAsync(writeArgs);
            SaveSysSignals.AmanitaSaveWritten -= Handler;
            Assert.IsTrue(responded);
        }

        [Test]
        public void EventInvocation_AmanitaSaveWritten_NoWrites()
        {
            bool responded = false;
            void Handler(SaveWriteResults r) => responded = true;
            SaveSysSignals.AmanitaSaveWritten += Handler;
            SaveSysSignals.AmanitaSaveWritten -= Handler;
            Assert.IsFalse(responded);
        }

        [Test]
        public void EventInvocation_AmanitaSaveWritten_RejectNullWriteArgs()
        {
            bool responded = false;
            void Handler(SaveWriteResults r) => responded = true;
            SaveSysSignals.AmanitaSaveWritten += Handler;
            Assert.ThrowsAsync<NullReferenceException>(() => saveWriter.WriteOneToDiskAsync(null));
            SaveSysSignals.AmanitaSaveWritten -= Handler;
            Assert.IsFalse(responded);
        }

        [Test]
        public void EventInvocation_AmanitaSaveWritten_RejectNullSaveData()
        {
            bool responded = false;
            void Handler(SaveWriteResults r) => responded = true;
            var bad = new SaveWriteRequest
            {
                SaveName = "Bad",
                SlotNumber = 0,
                MainState = null,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            SaveSysSignals.AmanitaSaveWritten += Handler;
            Assert.ThrowsAsync<ArgumentNullException>(() => saveWriter.WriteOneToDiskAsync(bad));
            SaveSysSignals.AmanitaSaveWritten -= Handler;
            Assert.IsFalse(responded);
        }

        [Test]
        public void EventInvocation_AmanitaSaveWritten_RejectNegativeSlotNumber()
        {
            bool responded = false;
            void Handler(SaveWriteResults r) => responded = true;
            var bad = new SaveWriteRequest
            {
                SaveName = "Bad",
                SlotNumber = -1,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            SaveSysSignals.AmanitaSaveWritten += Handler;
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDiskAsync(bad));
            SaveSysSignals.AmanitaSaveWritten -= Handler;
            Assert.IsFalse(responded);
        }

        [Test]
        public void EventInvocation_AmanitaSaveWritten_RejectInvalidBaseDirectory()
        {
            bool responded = false;
            void Handler(SaveWriteResults r) => responded = true;
            var bad = new SaveWriteRequest
            {
                SaveName = "Bad",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = (SaveDirectoryType)999
            };
            SaveSysSignals.AmanitaSaveWritten += Handler;
            Assert.ThrowsAsync<ArgumentException>(() => saveWriter.WriteOneToDiskAsync(bad));
            SaveSysSignals.AmanitaSaveWritten -= Handler;
            Assert.IsFalse(responded);
        }

        [Test]
        public void EventInvocation_AmanitaSaveWritten_RejectNullList()
        {
            bool responded = false;
            void Handler(SaveWriteResults r) => responded = true;
            SaveSysSignals.AmanitaSaveWritten += Handler;
            Assert.ThrowsAsync<NullReferenceException>(() => saveWriter.WriteAllToDiskAsync(null));
            SaveSysSignals.AmanitaSaveWritten -= Handler;
            Assert.IsFalse(responded);
        }

        // ------------- Directory creation ------------

        [Test]
        public async Task DirectoryCreation_OnWrite()
        {
            var args = new SaveWriteRequest
            {
                SaveName = "DirCreate",
                SlotNumber = 8,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            string folder = saveWriter.GetSaveFolderPath(args.BaseSaveDirectory);
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
            Assert.IsFalse(Directory.Exists(folder), "Directory unexpectedly exists pre-write.");
            await saveWriter.WriteOneToDiskAsync(args);
            Assert.IsTrue(Directory.Exists(folder), "Directory was not created.");
        }

        // ------------- Failsafe / backup tests ------------

        protected virtual async Task CommonFailsafeTest_KeepBackups()
        {
            await CommonSetupAsync();
            saveWriter.DeleteBackupsPostOverwrite = false;
            await saveWriter.WriteOneToDiskAsync(writeArgs);
        }

        protected virtual async Task CommonFailsafeTest_EraseBackups()
        {
            await CommonSetupAsync();
            saveWriter.DeleteBackupsPostOverwrite = true;
            await saveWriter.WriteOneToDiskAsync(writeArgs);
        }

        [Test]
        public async Task Failsafe_CreatesBackupBeforeOverwrite_Encrypted()
        {
            LogAssert.ignoreFailingMessages = true;
            saveWriter.ExpectEncryption = true;
            await CommonFailsafeTest_EraseBackups();

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;
            Assert.IsTrue(File.Exists(filePath));
            Assert.IsFalse(File.Exists(backupPath));

            saveWriter.DeleteBackupsPostOverwrite = false;
            await saveWriter.WriteOneToDiskAsync(writeArgs);
            Assert.IsTrue(File.Exists(backupPath));
            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);
        }

        [Test]
        public async Task Failsafe_CreatesBackupBeforeOverwrite_NONEncrypted()
        {
            LogAssert.ignoreFailingMessages = true;
            saveWriter.ExpectEncryption = false;
            await CommonFailsafeTest_EraseBackups();

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;
            Assert.IsTrue(File.Exists(filePath));
            Assert.IsFalse(File.Exists(backupPath));

            saveWriter.DeleteBackupsPostOverwrite = false;
            await saveWriter.WriteOneToDiskAsync(writeArgs);
            Assert.IsTrue(File.Exists(backupPath));
            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);
        }

        [Test]
        public async Task Failsafe_BackupRetainedOnWriteFailure_Encrypted()
        {
            LogAssert.ignoreFailingMessages = true;
            saveWriter.ExpectEncryption = true;
            await CommonFailsafeTest_KeepBackups();

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;

            using (File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bool failed = false;
                try { await saveWriter.WriteOneToDiskAsync(writeArgs); }
                catch (IOException) { failed = true; }
                Assert.IsTrue(failed);
                Assert.IsTrue(File.Exists(backupPath));
            }

            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);
        }

        [Test]
        public async Task Failsafe_BackupRetainedOnWriteFailure_NONEncrypted()
        {
            LogAssert.ignoreFailingMessages = true;
            saveWriter.ExpectEncryption = false;
            await CommonFailsafeTest_KeepBackups();

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;

            using (File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bool failed = false;
                try { await saveWriter.WriteOneToDiskAsync(writeArgs); }
                catch (IOException) { failed = true; }
                Assert.IsTrue(failed);
                Assert.IsTrue(File.Exists(backupPath));
            }

            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);
        }

        [Test]
        public async Task Failsafe_LogsOnWriteFailure_Encrypted()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = true;
            await saveWriter.WriteOneToDiskAsync(writeArgs);

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;

            using (File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                LogAssert.Expect(LogType.Error, new Regex("Could not move file .* to backup .*"));
                try { await saveWriter.WriteOneToDiskAsync(writeArgs); } catch { }
            }

            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);
        }

        [Test]
        public async Task Failsafe_LogsOnWriteFailure_NONEncrypted()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;
            await saveWriter.WriteOneToDiskAsync(writeArgs);

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;

            using (File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                LogAssert.Expect(LogType.Error, new Regex("Could not move file .* to backup .*"));
                try { await saveWriter.WriteOneToDiskAsync(writeArgs); } catch { }
            }

            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);
        }

        // ------------- Overwrite behavior ------------

        [Test]
        public async Task OverwritesFile_WhenWritingToSameSlotTwice()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            var firstData = new CompositeSaveData();
            firstData.Add(new RawStringSaveData("{\"value\":\"first\"}"));
            var firstArgs = new SaveWriteRequest
            {
                SaveName = "OverwriteTest",
                SlotNumber = 5,
                MainState = firstData,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(firstArgs);
            string path = saveWriter.GetSaveFilePath(firstArgs.BaseSaveDirectory, firstArgs.SlotNumber);
            string firstContent = await File.ReadAllTextAsync(path);

            var secondData = new CompositeSaveData();
            secondData.Add(new RawStringSaveData("{\"value\":\"second\"}"));
            var secondArgs = new SaveWriteRequest
            {
                SaveName = "OverwriteTest",
                SlotNumber = 5,
                MainState = secondData,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(secondArgs);
            string secondContent = await File.ReadAllTextAsync(path);

            saveFilePathsForCleanup.Add(path);
            Assert.AreNotEqual(firstContent, secondContent);
            Assert.IsTrue(secondContent.Contains("second"));
        }

        // ------------- Locked file error paths ------------

        [Test]
        public async Task WriteOneToDisk_LogsError_WhenFileIsLocked_ReadAllowed()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            var args = new SaveWriteRequest
            {
                SaveName = "LockedFileTest",
                SlotNumber = 6,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(args);
            string path = saveWriter.GetSaveFilePath(args.BaseSaveDirectory, args.SlotNumber);

            using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                LogAssert.Expect(LogType.Error, new Regex("Could not move file .* to backup .*"));
                bool failed = false;
                try { await saveWriter.WriteOneToDiskAsync(args); }
                catch (IOException) { failed = true; }
                Assert.IsTrue(failed);
            }
            saveFilePathsForCleanup.Add(path);
        }

        [Test]
        public async Task WriteOneToDisk_LogsError_WhenFileIsLocked_FullLock()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            var args = new SaveWriteRequest
            {
                SaveName = "LockedFileTest",
                SlotNumber = 6,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDiskAsync(args);
            string path = saveWriter.GetSaveFilePath(args.BaseSaveDirectory, args.SlotNumber);

            LogAssert.ignoreFailingMessages = true;
            using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                LogAssert.Expect(LogType.Error, new Regex("Could not move file .* to backup .*"));
                bool failed = false;
                try { await saveWriter.WriteOneToDiskAsync(args); } catch { failed = true; }
                Assert.IsTrue(failed);
            }
            saveFilePathsForCleanup.Add(path);
        }

        // ------------- Large data -------------

        [Test]
        public async Task WritesLargeSaveData_Successfully()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            var large = new CompositeSaveData();
            for (int i = 0; i < 10000; i++)
                large.Add(new IndexSaveData(i));

            var args = new SaveWriteRequest
            {
                SaveName = "LargeSaveTest",
                SlotNumber = 7,
                MainState = large,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            await saveWriter.WriteOneToDiskAsync(args);
            string path = saveWriter.GetSaveFilePath(args.BaseSaveDirectory, args.SlotNumber);
            Assert.IsTrue(File.Exists(path));
            saveFilePathsForCleanup.Add(path);

            string content = await File.ReadAllTextAsync(path);
            string unescaped = Regex.Unescape(content);
            Assert.IsTrue(unescaped.Contains("\"index\": 9999"), "Large save file does not contain expected data.");
        }

        protected override int CommonSetupDelay => 250;
    }
}