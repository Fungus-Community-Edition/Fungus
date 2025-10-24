using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Encoding = System.Text.Encoding;
using Amanita.FSExt;

namespace SaveSystemTests
{
    public class SaveWriterTests : CommonTestFunctionality
    {
        [Test]
        public virtual async Task WritesSaveToDisk_BaseDataPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;
            saveWriter.RelativeSavePath = "";

            await CommonSaveWriteTestAsync(writeArgs);
        }

        protected SaveWriteRequest writeArgs = new SaveWriteRequest
        {
            SaveName = "TestSave",
            SlotNumber = 0,
            MainState = new CompositeSaveData(),
            SaveMetaData = new SaveMetaData(),
            BaseSaveDirectory = SaveDirectoryType.DataPath
        };

        protected string FileNameFormat { get { return saveWriter.FileNameFormat; } }

        protected virtual IEnumerator CommonSaveWriteTest(SaveWriteRequest writeArgs,
            string relativePath = "")
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                relativePath = saveWriter.RelativeSavePath;
            }

            string formattedSaveNum = writeArgs.SlotNumber.ToString(SaveNumberFormat);
            string fileName = string.Format(FileNameFormat, SavePrefix,
                formattedSaveNum, FileExtension);
            string fullPath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);

            Task<bool> writeTask = saveWriter.WriteOneToDisk(writeArgs);
            yield return WaitFor(writeTask);

            bool fileWasWritten = File.Exists(fullPath);
            Assert.IsTrue(fileWasWritten, "Save file was not created.");
            if (fileWasWritten)
            {
                File.Delete(fullPath);
            }
        }

        string debugSaveFolder;

        protected virtual async Task CommonSaveWriteTestAsync(SaveWriteRequest writeArgs)
        {
            string fullPath = saveSys.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);

            Task<bool> writeTask = saveWriter.WriteOneToDisk(writeArgs);
            await writeTask.ConfigureAwait(false);

            bool fileWasWritten = File.Exists(fullPath);
            saveFilePathsForCleanup.Add(fullPath);
            Assert.IsTrue(fileWasWritten, "Save file was not created.");
        }

        protected string SaveNumberFormat { get { return saveWriter.SaveNumberFormat; } }

        #region Successful writes
        [Test]
        public virtual async Task WritesSaveToDisk_BasePersistentDataPath()
        {
            saveWriter.RelativeSavePath = "";
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            await CommonSaveWriteTestAsync(writeArgs);
        }

        [Test]
        public virtual async Task WritesSaveToDisk_BaseDataPath_RelativePathIncluded()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;
            await CommonSaveWriteTestAsync(writeArgs);
        }

        [Test]
        public virtual async Task WritesSaveToDisk_BasePersistentDataPath_RelativePathIncluded()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            await CommonSaveWriteTestAsync(writeArgs);
        }

        #endregion

        #region Rejection tests
        [Test]
        public virtual async Task WritesSaveToDisk_AnyPath_RejectNullSaveData()
        {
            await CommonSetupAsync();

            SaveWriteRequest writeArgsWithNullSaveData = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                MainState = null,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            Assert.ThrowsAsync<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData));

            writeArgsWithNullSaveData.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.ThrowsAsync<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData));
        }

        [Test]
        public virtual async Task WritesSaveToDisk_AnyPath_RejectNegativeSlotNumber()
        {
            await CommonSetupAsync();
            SaveWriteRequest writeArgsWithBadSlotNumber = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = -1,
                MainState = new CompositeSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber));
            writeArgsWithBadSlotNumber.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber));
        }

        [Test]
        public virtual async Task WritesSaveToDisk_AnyPath_RejectInvalidBaseDirectory()
        {
            await CommonSetupAsync();
            SaveWriteRequest writeArgsWithBadBaseDirectory = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                BaseSaveDirectory = (SaveDirectoryType)999
            };
            Assert.ThrowsAsync<System.ArgumentException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadBaseDirectory));
        }

        #endregion

        [Test]
        public virtual async Task WriteAllToDisk_AllSuccessful()
        {
            await CommonSetupAsync();
            Task<bool> writeTask = saveWriter.WriteAllToDisk(multipleThingsToWrite);
            await writeTask.ConfigureAwait(false);
            bool allWritten = writeTask.Result;
            Assert.IsTrue(allWritten, "Not all saves were written successfully.");
        }

        protected IList<SaveWriteRequest> multipleThingsToWrite = new List<SaveWriteRequest>
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
        public virtual async Task WriteAllToDisk_PartialSuccess()
        {
            IList<SaveWriteRequest> withOneNull = new List<SaveWriteRequest>(multipleThingsToWrite)
            {
                [1] = null
            };
            bool threwIt = false;
            try
            {
                Task writeTask = saveWriter.WriteAllToDisk(withOneNull);
                await writeTask.ConfigureAwait(false);
            }
            catch
            {
                threwIt = true;
            }
            finally
            {
                Assert.IsTrue(threwIt, "Expected NullReferenceException when trying to write a null SaveWriteArgs.");
            }
        }

        [Test]
        public virtual void WriteAllToDisk_RejectNullList()
        {
            Assert.ThrowsAsync<System.NullReferenceException>(() => saveWriter.WriteAllToDisk(null));
        }

        [Test]
        public virtual async Task VerifyFileContent_NONEncrypted_AfterWrite()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            SaveWriteRequest writeArgs = new SaveWriteRequest
            {
                SaveName = "TestSaveContentVerification",
                SlotNumber = 0,
                MainState = MainSave,
                SaveMetaData = new SaveMetaData { },
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string expectedMetaDataJson = JsonUtility.ToJson(writeArgs.SaveMetaData, true);
            string expectedMainSaveDataJson = serializerForTest.ToJson(writeArgs.MainState, true);

            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}" +
                $"{expectedMainSaveDataJson}{SaveDiskAccessor.CompletionMarker}";
            await CommonSaveWriteTestAsync(writeArgs);

            string jsonText = await ReadAndVerifyContent();
            Task<string> ReadAndVerifyContent()
            {
                string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
                saveFilePathsForCleanup.Add(filePath);
                return File.ReadAllTextAsync(filePath, utf8);
            }

            Assert.AreEqual(expectedJsonText, jsonText, "File content does not match the expected JSON text.");
        }

        [Test]
        public virtual async Task VerifyFileContent_Encrypted_AfterWrite()
        {
            await CommonSetupAsync();

            saveWriter.ExpectEncryption = true;

            SaveWriteRequest writeArgs = new SaveWriteRequest
            {
                SaveName = "TestSaveEncrypted",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string expectedMetaDataJson = JsonUtility.ToJson(writeArgs.SaveMetaData, true);
            string expectedMainSaveDataJson = serializerForTest.ToJson(writeArgs.MainState, true);

            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}" +
                $"{expectedMainSaveDataJson}{SaveDiskAccessor.CompletionMarker}";
            byte key = 0xAA;
            byte[] expectedEncryptedData = utf8.GetBytes(expectedJsonText)
                .Select(b => (byte)(b ^ key)).ToArray();

            await CommonSaveWriteTestAsync(writeArgs);

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            saveFilePathsForCleanup.Add(filePath);

            byte[] encryptedData = await File.ReadAllBytesAsync(filePath);
            byte[] decryptedData = encryptedData.Select(byteElem => (byte)(byteElem ^ key)).ToArray();
            string decryptedString = utf8.GetString(decryptedData);

            Assert.AreEqual(expectedJsonText, decryptedString, "Decrypted content does not match the expected JSON text.");
        }

        protected static Encoding utf8 = Encoding.UTF8;

        [Test]
        public virtual async Task EventInvocation_AmanitaSaveWritten()
        {
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
                Assert.IsNotNull(writeResults.SaveData, "Save data should not be null.");
                Assert.IsNotNull(writeResults.FilePath, "File path should not be null.");
                Assert.IsNotNull(writeResults.FileName, "File name should not be null.");
            }

            Task<bool> writeTask = saveWriter.WriteOneToDisk(writeArgs);
            await writeTask.ConfigureAwait(false);
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsTrue(responded, "AmanitaSaveWritten event was not invoked after writing save data.");
        }

        [Test]
        public virtual async Task EventInvocation_AmanitaSaveWritten_MultipleWrites()
        {
            await CommonSetupAsync();
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;

            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
                Assert.IsNotNull(writeResults.SaveData, "Save data should not be null.");
                Assert.IsNotNull(writeResults.FilePath, "File path should not be null.");
                Assert.IsNotNull(writeResults.FileName, "File name should not be null.");
            }

            Task<bool> writeTask = saveWriter.WriteOneToDisk(writeArgs);
            await writeTask.ConfigureAwait(false);
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsTrue(responded, "AmanitaSaveWritten event was not invoked after writing multiple saves.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_NoWrites()
        {
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }

            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked without any writes.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullWriteArgs()
        {
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool respondedWhenItShouldnt = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                respondedWhenItShouldnt = true;
            }

            Assert.ThrowsAsync<System.NullReferenceException>(() => saveWriter.WriteOneToDisk(null));
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(respondedWhenItShouldnt, "AmanitaSaveWritten event was invoked with null write args.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullSaveData()
        {
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool respondedWhenItShouldnt = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                respondedWhenItShouldnt = true;
            }

            SaveWriteRequest writeArgsWithNullSaveData = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                MainState = null,
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            Assert.ThrowsAsync<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData));
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(respondedWhenItShouldnt, "AmanitaSaveWritten event was invoked with null save data.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNegativeSlotNumber()
        {
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }
            SaveWriteRequest writeArgsWithBadSlotNumber = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = -1,
                MainState = new CompositeSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber));
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with negative slot number.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectInvalidBaseDirectory()
        {
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }
            SaveWriteRequest writeArgsWithBadBaseDirectory = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                BaseSaveDirectory = (SaveDirectoryType)999
            };
            Assert.ThrowsAsync<System.ArgumentException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadBaseDirectory));
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with invalid base directory.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullList()
        {
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }
            Assert.ThrowsAsync<System.NullReferenceException>(() => saveWriter.WriteAllToDisk(null));
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with null list of SaveWriteArgs.");
        }

        [Test]
        public virtual async Task DirectoryCreation_OnWrite()
        {
            SaveWriteRequest writeArgsForSaveDirectoryCreation = new SaveWriteRequest
            {
                SaveName = "TestSaveDirectoryCreation",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            SaveDirectoryType baseSaveDirectory = writeArgsForSaveDirectoryCreation.BaseSaveDirectory;
            string saveFolder = saveWriter.GetSaveFolderPath(baseSaveDirectory);

            if (Directory.Exists(saveFolder))
            {
                Directory.Delete(saveFolder, true);
            }

            bool directoryWasErased = !Directory.Exists(saveFolder);
            Assert.IsTrue(directoryWasErased, "Directory should not exist before writing.");
            await saveWriter.WriteOneToDisk(writeArgsForSaveDirectoryCreation);
            Assert.IsTrue(Directory.Exists(saveFolder), "Directory was not created after writing.");

            if (Directory.Exists(saveFolder))
            {
                Directory.Delete(saveFolder, true);
            }
        }

        // Failsafe tests

        [Test]
        public async Task Failsafe_CreatesBackupBeforeOverwrite_Encrypted()
        {
            LogAssert.ignoreFailingMessages = true;

            saveWriter.ExpectEncryption = true;
            await CommonFailsafeTest_EraseBackups();

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;
            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);

            Assert.IsTrue(File.Exists(filePath), "Initial save file should exist.");
            Assert.IsFalse(File.Exists(backupPath), "Backup file should not exist before overwrite.");

            saveWriter.DeleteBackupsPostOverwrite = false;
            await saveWriter.WriteOneToDisk(writeArgs);

            Assert.IsTrue(File.Exists(backupPath), "Backup file should exist when writer is set to NOT delete them");
        }

        protected virtual async Task CommonFailsafeTest_KeepBackups()
        {
            await CommonSetupAsync();
            saveWriter.DeleteBackupsPostOverwrite = false;
            await saveWriter.WriteOneToDisk(writeArgs);
        }

        protected virtual async Task CommonFailsafeTest_EraseBackups()
        {
            await CommonSetupAsync();

            saveWriter.DeleteBackupsPostOverwrite = true;
            await saveWriter.WriteOneToDisk(writeArgs);
        }

        [Test]
        public async Task Failsafe_CreatesBackupBeforeOverwrite_NONEncrypted()
        {
            LogAssert.ignoreFailingMessages = true;

            saveWriter.ExpectEncryption = false;
            await CommonFailsafeTest_EraseBackups();

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;
            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);

            Assert.IsTrue(File.Exists(filePath), "Initial save file should exist.");
            Assert.IsFalse(File.Exists(backupPath), "Backup file should not exist before overwrite.");

            saveWriter.DeleteBackupsPostOverwrite = false;
            await saveWriter.WriteOneToDisk(writeArgs);

            Assert.IsTrue(File.Exists(backupPath), "Backup file should exist when writer is set to NOT delete them");
        }

        [Test]
        public async Task Failsafe_BackupRetainedOnWriteFailure_Encrypted()
        {
            LogAssert.ignoreFailingMessages = true;

            saveWriter.ExpectEncryption = true;
            await CommonFailsafeTest_KeepBackups();

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;
            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);

            using (var fileLock = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bool writeFailed = false;
                try
                {
                    await saveWriter.WriteOneToDisk(writeArgs);
                }
                catch (IOException)
                {
                    writeFailed = true;
                }

                Assert.IsTrue(writeFailed, "Write should fail when file is locked.");
                Assert.IsTrue(File.Exists(backupPath), "Backup file should be retained after failed write.");
            }
        }

        [Test]
        public async Task Failsafe_BackupRetainedOnWriteFailure_NONEncrypted()
        {
            LogAssert.ignoreFailingMessages = true;

            saveWriter.ExpectEncryption = false;
            await CommonFailsafeTest_KeepBackups();

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            string backupPath = filePath + saveWriter.BackupFileExtension;
            saveFilePathsForCleanup.Add(filePath);
            saveFilePathsForCleanup.Add(backupPath);

            using (var fileLock = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bool writeFailed = false;
                try
                {
                    await saveWriter.WriteOneToDisk(writeArgs);
                }
                catch (IOException)
                {
                    writeFailed = true;
                }

                Assert.IsTrue(writeFailed, "Write should fail when file is locked.");
                Assert.IsTrue(File.Exists(backupPath), "Backup file should be retained after failed write.");
            }
        }

        [Test]
        public async Task Failsafe_LogsOnWriteFailure_Encrypted()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = true;

            await saveWriter.WriteOneToDisk(writeArgs);

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            saveFilePathsForCleanup.Add(filePath);

            using (var fileLock = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                string expectedBackupFilePath = filePath + saveWriter.BackupFileExtension;
                saveFilePathsForCleanup.Add(expectedBackupFilePath);
                string expectedErrorMessage = $"Could not move file {filePath} to backup {expectedBackupFilePath}." +
                                    $"\nException: The process cannot access the file because it is being used by another process.";
                LogAssert.Expect(LogType.Error, expectedErrorMessage);

                try
                {
                    await saveWriter.WriteOneToDisk(writeArgs);
                }
                catch
                {
                    // Expected
                }
            }
        }

        [Test]
        public async Task Failsafe_LogsOnWriteFailure_NONEncrypted()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            await saveWriter.WriteOneToDisk(writeArgs);

            string filePath = saveWriter.GetSaveFilePath(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber);
            saveFilePathsForCleanup.Add(filePath);

            using (var fileLock = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                string expectedBackupFilePath = filePath + saveWriter.BackupFileExtension;
                saveFilePathsForCleanup.Add(expectedBackupFilePath);
                string expectedErrorMessage = $"Could not move file {filePath} to backup {expectedBackupFilePath}." +
                                    $"\nException: The process cannot access the file because it is being used by another process.";
                LogAssert.Expect(LogType.Error, expectedErrorMessage);

                try
                {
                    await saveWriter.WriteOneToDisk(writeArgs);
                }
                catch
                {
                    // Expected
                }
            }
        }

        protected override int CommonSetupDelay
        {
            get
            {
                return 250;
            }
        }

        [Test]
        public async Task OverwritesFile_WhenWritingToSameSlotTwice()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            // First write: simple data (remove SaveDataUnit; use test SaveData)
            var firstData = new CompositeSaveData();
            firstData.Add(new RawStringSaveData("{\"value\":\"first\"}"));

            var firstWriteArgs = new SaveWriteRequest
            {
                SaveName = "OverwriteTest",
                SlotNumber = 5,
                MainState = firstData,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(firstWriteArgs);

            string filePath = saveWriter.GetSaveFilePath(firstWriteArgs.BaseSaveDirectory, firstWriteArgs.SlotNumber);
            saveFilePathsForCleanup.Add(filePath);
            string firstContent = await File.ReadAllTextAsync(filePath);

            var secondData = new CompositeSaveData();
            secondData.Add(new RawStringSaveData("{\"value\":\"second\"}"));

            var secondWriteArgs = new SaveWriteRequest
            {
                SaveName = "OverwriteTest",
                SlotNumber = 5,
                MainState = secondData,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(secondWriteArgs);

            string secondContent = await File.ReadAllTextAsync(filePath);

            Assert.AreNotEqual(firstContent, secondContent, "File content was not overwritten.");
            Assert.IsTrue(secondContent.Contains("second"), "Overwritten file does not contain new data.");
        }

        [Test]
        public async Task WriteOneToDisk_LogsError_WhenFileIsLocked_ReadAllowed()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            var writeArgsLocked = new SaveWriteRequest
            {
                SaveName = "LockedFileTest",
                SlotNumber = 6,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(writeArgsLocked);

            string filePath = saveWriter.GetSaveFilePath(writeArgsLocked.BaseSaveDirectory, writeArgsLocked.SlotNumber);
            saveFilePathsForCleanup.Add(filePath);

            using (var fileLock = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                string expectedError = $"Could not move file {filePath} to backup {filePath + saveWriter.BackupFileExtension}.";
                bool threwRightError = false;
                bool writeFailed = false;
                LogAssert.Expect(LogType.Error, new Regex("Could not move file .* to backup .*"));

                try
                {
                    await saveWriter.WriteOneToDisk(writeArgsLocked);
                }
                catch (IOException ioe)
                {
                    threwRightError = ioe.Message.Contains(expectedError);
                    writeFailed = true;
                }

                Assert.IsTrue(threwRightError, "Wrong error thrown when trying to access locked file.");
                Assert.IsTrue(writeFailed, "Expected write to fail when file is locked.");
            }
        }

        [Test]
        public async Task WriteOneToDisk_LogsError_WhenFileIsLocked_FullLock()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            var writeArgsLocked = new SaveWriteRequest
            {
                SaveName = "LockedFileTest",
                SlotNumber = 6,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            await saveWriter.WriteOneToDisk(writeArgsLocked);

            string filePath = saveWriter.GetSaveFilePath(writeArgsLocked.BaseSaveDirectory, writeArgsLocked.SlotNumber);
            saveFilePathsForCleanup.Add(filePath);

            LogAssert.ignoreFailingMessages = true;

            using (var fileLock = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                LogAssert.Expect(LogType.Error, new Regex("Could not move file .* to backup .*"));
                bool writeFailed = false;

                try
                {
                    await saveWriter.WriteOneToDisk(writeArgsLocked);
                }
                catch
                {
                    writeFailed = true;
                }

                Assert.IsTrue(writeFailed, "Expected write to fail when file is locked.");
            }
        }

        [Test]
        public async Task WritesLargeSaveData_Successfully()
        {
            await CommonSetupAsync();
            saveWriter.ExpectEncryption = false;

            var largeData = new CompositeSaveData();
            for (int i = 0; i < 10000; i++)
            {
                largeData.Add(new IndexSaveData(i));
            }

            var largeWriteArgs = new SaveWriteRequest
            {
                SaveName = "LargeSaveTest",
                SlotNumber = 7,
                MainState = largeData,
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            await saveWriter.WriteOneToDisk(largeWriteArgs);

            string filePath = saveWriter.GetSaveFilePath(largeWriteArgs.BaseSaveDirectory, largeWriteArgs.SlotNumber);

            Assert.IsTrue(File.Exists(filePath), "Large save file was not created.");
            saveFilePathsForCleanup.Add(filePath);

            string fileContent = await File.ReadAllTextAsync(filePath);

            string unescaped = Regex.Unescape(fileContent);
            Assert.IsTrue(unescaped.Contains("\"index\":9999"), "Large save file does not contain expected data.");
        }
    }

}