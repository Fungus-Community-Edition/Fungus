using System.Collections;
using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Encoding = System.Text.Encoding;
using UnityObject = UnityEngine.Object;
using System;
using System.Threading;

namespace Amanita.SaveSystemTests
{
    public class SaveWriterTests : CommonTestFunctionality
    {
        
        [Test]
        public virtual async Task WritesSaveToDisk_BaseDataPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;
            saveWriter.RelativeSavePath = "";
            // ^Since it might get set to null by other tests, we need to reset it

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
            string fullPath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory, fileName, relativePath);

            Task<bool> writeTask = saveWriter.WriteOneToDisk(writeArgs);
            yield return WaitFor(writeTask);

            bool fileWasWritten = File.Exists(fullPath);
            Assert.IsTrue(fileWasWritten, "Save file was not created.");
        }

        string debugSaveFolder;

        protected virtual async Task CommonSaveWriteTestAsync(SaveWriteRequest writeArgs,
            string relativePath = "")
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                relativePath = saveWriter.RelativeSavePath;
            }

            string formattedSaveNum = writeArgs.SlotNumber.ToString(SaveNumberFormat);
            string fileName = string.Format(FileNameFormat, SavePrefix,
                formattedSaveNum, FileExtension);
            string fullPath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory, fileName, relativePath);

            Task<bool> writeTask = saveWriter.WriteOneToDisk(writeArgs);
            await writeTask.ConfigureAwait(false);

            bool fileWasWritten = File.Exists(fullPath);
            Assert.IsTrue(fileWasWritten, "Save file was not created.");
        }


        protected string SaveNumberFormat { get { return saveWriter.SaveNumberFormat; } }

        #region Successful writes
        [Test]
        public virtual async Task WritesSaveToDisk_BasePersistentDataPath()
        {
            saveWriter.RelativeSavePath = "";
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            
            // ^Since it might get set to null by other tests, we need to reset it
            await CommonSaveWriteTestAsync(writeArgs);
        }

        [Test]
        public virtual async Task WritesSaveToDisk_BaseStreamingAssetsPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            
            // ^Since it might get set to null by other tests, we need to reset it
            await CommonSaveWriteTestAsync(writeArgs);
        }

        // We can worry about PlayerPrefs later, if we need to.

        [Test]
        public virtual async Task WritesSaveToDisk_BaseDataPath_RelativePathIncluded()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;
            await CommonSaveWriteTestAsync(writeArgs, saveWriter.RelativeSavePath);
        }

        [Test]
        public virtual async Task WritesSaveToDisk_BasePersistentDataPath_RelativePathIncluded()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            await CommonSaveWriteTestAsync(writeArgs, saveWriter.RelativeSavePath);
        }

        [Test]
        public virtual async Task WritesSaveToDisk_BaseStreamingAssetsPath_RelativePathIncluded()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;

            await CommonSaveWriteTestAsync(writeArgs, saveWriter.RelativeSavePath);
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
                MainState = null, // Intentionally null to test rejection.
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            Assert.ThrowsAsync<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData),
                "Expected ArgumentNullException when trying to write null save data.");

            writeArgsWithNullSaveData.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.ThrowsAsync<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData),
                "Expected ArgumentNullException when trying to write null save data.");

            writeArgsWithNullSaveData.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            Assert.ThrowsAsync<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData),
                "Expected ArgumentNullException when trying to write null save data.");
        }

        [Test]
        public virtual async Task WritesSaveToDisk_AnyPath_RejectNegativeSlotNumber()
        {
            await CommonSetupAsync();
            SaveWriteRequest writeArgsWithBadSlotNumber = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = -1, // Intentionally negative to test rejection.
                MainState = new CompositeSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
            writeArgsWithBadSlotNumber.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
            writeArgsWithBadSlotNumber.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
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
                BaseSaveDirectory = (SaveDirectoryType)999 // Intentionally invalid to test rejection.
            };
            Assert.ThrowsAsync<System.ArgumentException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadBaseDirectory),
                "Expected ArgumentException when trying to write with invalid base directory.");
        }

        // BaseSaveDirectoryType is an enum, so we can't really test it with a null value.

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
                new SaveWriteRequest
                {
                    SaveName = "TestSave3",
                    SlotNumber = 2,
                    MainState = new CompositeSaveData(),
                    SaveMetaData = new SaveMetaData(),
                    BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath
                }
            };

        [Test]
        public virtual async Task WriteAllToDisk_PartialSuccess()
        {
            // Let's say the first one fails for some reason
            IList<SaveWriteRequest> withOneNull = new List<SaveWriteRequest>(multipleThingsToWrite)
            {
                [1] = null // Intentionally null to simulate failure
            };
            bool threwIt = false;
            try
            {
                Task writeTask = saveWriter.WriteAllToDisk(withOneNull);
                await writeTask.ConfigureAwait(false);
            }
            catch (NullReferenceException e)
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
            Assert.ThrowsAsync<System.NullReferenceException>(() => saveWriter.WriteAllToDisk(null),
                "Expected NullReferenceException when trying to write a null list of SaveWriteArgs.");
        }

        [Test]
        public virtual async Task VerifyFileContent_NONEncrypted_AfterWrite()
        {
            await CommonSetupAsync();
            saveWriter.WriteEncrypted = false;

            SaveWriteRequest writeArgs = new SaveWriteRequest
            {
                SaveName = "TestSaveContentVerification",
                SlotNumber = 0,
                MainState = MainSave,
                SaveMetaData = new SaveMetaData { },
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string expectedMetaDataJson = JsonUtility.ToJson(writeArgs.SaveMetaData, true);
            string expectedMainSaveDataJson = JsonUtility.ToJson(writeArgs.MainState, true);

            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}" +
                $"{expectedMainSaveDataJson}{SaveDiskAccessor.CompletionMarker}";
            await CommonSaveWriteTestAsync(writeArgs);

            string jsonText = await ReadAndVerifyContent();
            Task<string> ReadAndVerifyContent()
            {
                string filePath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber, saveWriter);
                
                return File.ReadAllTextAsync(filePath, utf8);
            }

            // Verify the content
            Assert.AreEqual(expectedJsonText, jsonText, "File content does not match the expected JSON text.");
        }

        [Test]
        public virtual async Task VerifyFileContent_Encrypted_AfterWrite()
        {
            await CommonSetupAsync();

            saveWriter.WriteEncrypted = true;

            SaveWriteRequest writeArgs = new SaveWriteRequest
            {
                SaveName = "TestSaveEncrypted",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string expectedMetaDataJson = JsonUtility.ToJson(writeArgs.SaveMetaData, true);
            string expectedMainSaveDataJson = JsonUtility.ToJson(writeArgs.MainState, true);

            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}" +
                $"{expectedMainSaveDataJson}{SaveDiskAccessor.CompletionMarker}";
            byte key = 0xAA;
            byte[] expectedEncryptedData = utf8.GetBytes(expectedJsonText)
                .Select(b => (byte)(b ^ key)).ToArray(); // Simple XOR encryption for testing

            await CommonSaveWriteTestAsync(writeArgs);

            string fileNumFormatted = writeArgs.SlotNumber.ToString(SaveNumberFormat);
            string saveFolder = FileUtils.GetPathToFolder(writeArgs.BaseSaveDirectory, saveWriter.RelativeSavePath);
            
            string filePath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber, saveWriter);
            string stringDataToWrite = string.Empty;

            DecideDirectoriesAndSuch();
            void DecideDirectoriesAndSuch()
            {
                Directory.CreateDirectory(saveFolder); // In case it doesn't exist.

                SaveData saveData = (SaveData)writeArgs.MainState;
                stringDataToWrite = JsonUtility.ToJson(saveData, true);
            }
            string decryptedString = string.Empty;

            byte[] encryptedData = await File.ReadAllBytesAsync(filePath);
            byte[] decryptedData = encryptedData.Select(b => (byte)(b ^ key)).ToArray();
            decryptedString = utf8.GetString(decryptedData);

            Debug.Log($"Decrypted string:\n{decryptedString}");
            // Verify the content
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
                Debug.Log("SaveWriterTest: Responding to AmanitaSaveWritten event for multiple writes.");
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
            
            // ^Since it might get set to null by other tests, we need to reset it
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }

            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            // No writes, so the event should not be invoked
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked without any writes.");

        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullWriteArgs()
        {
            
            // ^Since it might get set to null by other tests, we need to reset it
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool respondedWhenItShouldnt = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                respondedWhenItShouldnt = true;
            }

            Assert.ThrowsAsync<System.NullReferenceException>(() => saveWriter.WriteOneToDisk(null),
                "Expected NullReferenceException when trying to write null SaveWriteArgs.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(respondedWhenItShouldnt, "AmanitaSaveWritten event was invoked with null write args.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullSaveData()
        {
            
            // ^Since it might get set to null by other tests, we need to reset it
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
                MainState = null, // Intentionally null to test rejection.
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            Assert.ThrowsAsync<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData),
                "Expected ArgumentNullException when trying to write null save data.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(respondedWhenItShouldnt, "AmanitaSaveWritten event was invoked with null save data.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNegativeSlotNumber()
        {
            
            // ^Since it might get set to null by other tests, we need to reset it
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }
            SaveWriteRequest writeArgsWithBadSlotNumber = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = -1, // Intentionally negative to test rejection.
                MainState = new CompositeSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with negative slot number.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectInvalidBaseDirectory()
        {
            
            // ^Since it might get set to null by other tests, we need to reset it
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
                BaseSaveDirectory = (SaveDirectoryType)999 // Intentionally invalid to test rejection.
            };
            Assert.ThrowsAsync<System.ArgumentException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadBaseDirectory),
                "Expected ArgumentException when trying to write with invalid base directory.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with invalid base directory.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullList()
        {
            
            // ^Since it might get set to null by other tests, we need to reset it
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }
            Assert.ThrowsAsync<System.NullReferenceException>(() => saveWriter.WriteAllToDisk(null),
                "Expected NullReferenceException when trying to write a null list of SaveWriteArgs.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with null list of SaveWriteArgs.");
        }

        [Test]
        public virtual async Task DirectoryCreation_OnWrite()
        {
            
            // ^Since it might get set to null by other tests, we need to reset it
            SaveWriteRequest writeArgsForSaveDirectoryCreation = new SaveWriteRequest
            {
                SaveName = "TestSaveDirectoryCreation",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            SaveDirectoryType baseSaveDirectory = writeArgsForSaveDirectoryCreation.BaseSaveDirectory;
            string relativePath = saveWriter.RelativeSavePath;
            string saveFolder = FileUtils.GetPathToFolder(baseSaveDirectory, relativePath);

            // ^This is the directory we expect to be created
            // Ensure the directory does not exist before writing. We want to test directory creation.
            if (Directory.Exists(saveFolder))
            {
                Directory.Delete(saveFolder, true);
            }

            bool directoryWasErased = !Directory.Exists(saveFolder);
            Assert.IsTrue(directoryWasErased, "Directory should not exist before writing.");
            await saveWriter.WriteOneToDisk(writeArgsForSaveDirectoryCreation);
            Assert.IsTrue(Directory.Exists(saveFolder), "Directory was not created after writing.");
        }

        // Failsafe tests

        [Test]
        public async Task Failsafe_CreatesBackupBeforeOverwrite_Encrypted()
        {
            LogAssert.ignoreFailingMessages = true;

            saveWriter.WriteEncrypted = true;
            await CommonFailsafeTest_KeepBackups();

            string filePath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber, saveWriter);
            string backupPath = filePath + saveWriter.BackupFileExtension;

            Assert.IsTrue(File.Exists(filePath), "Initial save file should exist.");
            Assert.IsFalse(File.Exists(backupPath), "Backup file should not exist before overwrite.");

            // Overwrite: simulate by writing again
            await saveWriter.WriteOneToDisk(writeArgs);

            // Backup exist after successful write since we set the writer to NOT delete backups on overwrite
            Assert.IsTrue(File.Exists(backupPath), "Backup file should exist when writer is set to NOT delete them");
        }

        protected virtual async Task CommonFailsafeTest_KeepBackups()
        {
            await CommonSetupAsync();
            
            saveWriter.DeleteBackupsPostOverwrite = false;
            // ^So we can test the backup creation
            await saveWriter.WriteOneToDisk(writeArgs); // Initial write to create the save file
        }

        [Test]
        public async Task Failsafe_CreatesBackupBeforeOverwrite_NONEncrypted()
        {
            LogAssert.ignoreFailingMessages = true;

            saveWriter.WriteEncrypted = false;
            await CommonFailsafeTest_KeepBackups();

            string filePath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber, saveWriter);
            string backupPath = filePath + saveWriter.BackupFileExtension;

            Assert.IsTrue(File.Exists(filePath), "Initial save file should exist.");
            Assert.IsFalse(File.Exists(backupPath), "Backup file should not exist before overwrite.");

            // Overwrite: simulate by writing again
            await saveWriter.WriteOneToDisk(writeArgs);

            // Backup exist after successful write since we set the writer to NOT delete backups on overwrite
            Assert.IsTrue(File.Exists(backupPath), "Backup file should exist when writer is set to NOT delete them");
        }

        [Test]
        public async Task Failsafe_BackupRetainedOnWriteFailure_Encrypted()
        {
            LogAssert.ignoreFailingMessages = true;

            saveWriter.WriteEncrypted = true;
            await CommonFailsafeTest_KeepBackups();

            string filePath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory,
                writeArgs.SlotNumber, saveWriter);
            string backupPath = filePath + saveWriter.BackupFileExtension;

            // Simulate write failure by locking the file. We're not going for a hard lock
            // here
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

                // Backup should exist after failed write
                Assert.IsTrue(writeFailed, "Write should fail when file is locked.");
                Assert.IsTrue(File.Exists(backupPath), "Backup file should be retained after failed write.");
            }

            // Clean up backup for other tests
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }

        [Test]
        public async Task Failsafe_BackupRetainedOnWriteFailure_NONEncrypted()
        {
            LogAssert.ignoreFailingMessages = true;

            saveWriter.WriteEncrypted = false;
            await CommonFailsafeTest_KeepBackups();

            string filePath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory,
                writeArgs.SlotNumber, saveWriter);
            string backupPath = filePath + saveWriter.BackupFileExtension;

            // Simulate write failure by locking the file. We're not going for a hard lock
            // here
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

                // Backup should exist after failed write
                Assert.IsTrue(writeFailed, "Write should fail when file is locked.");
                Assert.IsTrue(File.Exists(backupPath), "Backup file should be retained after failed write.");
            }

            // Clean up backup for other tests
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }


        [Test]
        public async Task Failsafe_LogsOnWriteFailure_Encrypted()
        {
            await CommonSetupAsync();
            saveWriter.WriteEncrypted = true;

            // Write initial save
            await saveWriter.WriteOneToDisk(writeArgs);

            string filePath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber, saveWriter);

            // Simulate write failure by locking the file
            using (var fileLock = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                string expectedBackupFilePath = filePath + saveWriter.BackupFileExtension;
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
            saveWriter.WriteEncrypted = false;

            // Write initial save
            await saveWriter.WriteOneToDisk(writeArgs);

            string filePath = FileUtils.GetPathToFile(writeArgs.BaseSaveDirectory, writeArgs.SlotNumber, saveWriter);

            // Simulate write failure by locking the file
            using (var fileLock = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                string expectedBackupFilePath = filePath + saveWriter.BackupFileExtension;
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
        public async Task AfterWrite_SuccessCompletionMarkerAdded()
        {
            // We want to add a success marker after writing a save so that on startup, we can easily check
            // for any corrupted saves.

            // Just jotting down some ideas for it here:
            // Add a specific line at the end of the file that indicates success. Of course, 
            // this line should not be part of the actual save data. The reader and writer
            // should be aware of this line when doing their thing.
            // The writer adds it, the reader checks for it.

            await CommonSetupAsync();
            Assert.Ignore();
        }

    }

}