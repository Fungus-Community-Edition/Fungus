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

namespace Amanita.SaveSystemTests
{
    public class SaveWriterTests : CommonTestFunctionality
    {
        
        [UnityTest]
        public virtual IEnumerator WritesSaveToDisk_BaseDataPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;
            saveWriter.RelativeSavePath = "";
            // ^Since it might get set to null by other tests, we need to reset it

            yield return CommonSaveWriteTest(writeArgs);
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

            string fileName = string.Format(FileNameFormat, SavePrefix,
                writeArgs.SlotNumber.ToString(SaveNumberFormat), FileExtension);
            string baseDirectory = SaveSystem.SaveDirectoryPaths[writeArgs.BaseSaveDirectory];
            string fullPath; // So we can judge the results

            fullPath = Path.Combine(baseDirectory, relativePath, fileName);
            
            Task<bool> writeTask = saveWriter.WriteOneToDisk(writeArgs);
            yield return WaitFor(writeTask);

            bool fileWasWritten = System.IO.File.Exists(fullPath);
            Assert.IsTrue(fileWasWritten, "Save file was not created.");
        }

        string debugSaveFolder;

        protected string SaveNumberFormat { get { return saveWriter.SaveNumberFormat; } }

        #region Successful writes
        [UnityTest]
        public virtual IEnumerator WritesSaveToDisk_BasePersistentDataPath()
        {
            saveWriter.RelativeSavePath = "";
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            
            // ^Since it might get set to null by other tests, we need to reset it
            yield return CommonSaveWriteTest(writeArgs);
        }

        [UnityTest]
        public virtual IEnumerator WritesSaveToDisk_BaseStreamingAssetsPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            
            // ^Since it might get set to null by other tests, we need to reset it
            yield return CommonSaveWriteTest(writeArgs);
        }

        // We can worry about PlayerPrefs later, if we need to.

        [UnityTest]
        public virtual IEnumerator WritesSaveToDisk_BaseDataPath_RelativePathIncluded()
        {
            
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;

            yield return CommonSaveWriteTest(writeArgs, saveWriter.RelativeSavePath);
        }

        [UnityTest]
        public virtual IEnumerator WritesSaveToDisk_BasePersistentDataPath_RelativePathIncluded()
        {
            
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            yield return CommonSaveWriteTest(writeArgs, saveWriter.RelativeSavePath);
        }

        [UnityTest]
        public virtual IEnumerator WritesSaveToDisk_BaseStreamingAssetsPath_RelativePathIncluded()
        {
            
            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;

            yield return CommonSaveWriteTest(writeArgs, saveWriter.RelativeSavePath);
        }

        #endregion

        #region Rejection tests
        [UnityTest]
        public virtual IEnumerator WritesSaveToDisk_AnyPath_RejectNullSaveData()
        {
            yield return null;

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

        [UnityTest]
        public virtual IEnumerator WritesSaveToDisk_AnyPath_RejectNegativeSlotNumber()
        {
            yield return null;
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

        [UnityTest]
        public virtual IEnumerator WritesSaveToDisk_AnyPath_RejectInvalidBaseDirectory()
        {
            yield return null;
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

        [UnityTest]
        public virtual IEnumerator WriteAllToDisk_AllSuccessful()
        {
            yield return null;
            Task<bool> writeTask = saveWriter.WriteAllToDisk(multipleThingsToWrite);
            yield return WaitFor(writeTask);
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

        // Now a test for file content verification would be nice, but that would require reading the file back and checking its contents. Let's set that test to be ignored for now.
        [UnityTest]
        public virtual IEnumerator VerifyFileContent_NONEncrypted_AfterWrite()
        {
            // ^Since it might get set to null by other tests, we need to reset it
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

            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}";
            yield return CommonSaveWriteTest(writeArgs);

            string jsonText = ReadAndVerifyContent();
            string ReadAndVerifyContent()
            {
                string saveFolder = SaveSystem.SaveDirectoryPaths[writeArgs.BaseSaveDirectory];
                if (saveWriter.RelativeSavePath.Count() > 0)
                {
                    saveFolder = Path.Combine(saveFolder, saveWriter.RelativeSavePath);
                }
                string fileName = string.Format(FileNameFormat, saveWriter.SavePrefix, writeArgs.SlotNumber.ToString(SaveNumberFormat), saveWriter.FileExtension);
                string filePath = Path.Combine(saveFolder, fileName);
                // Read the file content
                return File.ReadAllText(filePath, utf8);
            }

            // Verify the content
            Assert.AreEqual(expectedJsonText, jsonText, "File content does not match the expected JSON text.");
        }

        [UnityTest]
        public virtual IEnumerator VerifyFileContent_Encrypted_AfterWrite()
        {
            
            
            // ^Since it might get set to null by other tests, we need to reset it
            saveWriter.WriteEncrypted = true; // Set to true to test encrypted writing

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

            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}";
            byte key = 0xAA;
            byte[] expectedEncryptedData = utf8.GetBytes(expectedJsonText)
                .Select(b => (byte)(b ^ key)).ToArray(); // Simple XOR encryption for testing

            yield return CommonSaveWriteTest(writeArgs);

            string saveFolder = string.Empty, stringDataToWrite = string.Empty,
                fileName = string.Empty, filePath = string.Empty,
                savePrefix = saveWriter.SavePrefix, fileExtension = saveWriter.FileExtension,
                filePathFormat = saveWriter.FilePathFormat;
            string relativeSavePath = saveWriter.RelativeSavePath;
            DecideDirectoriesAndSuch();
            void DecideDirectoriesAndSuch()
            {
                saveFolder = SaveSystem.SaveDirectoryPaths[writeArgs.BaseSaveDirectory];

                if (relativeSavePath.Count() > 0)
                {
                    saveFolder = Path.Combine(saveFolder, relativeSavePath);
                }
                Directory.CreateDirectory(saveFolder); // In case it doesn't exist.

                SaveData saveData = (SaveData)writeArgs.MainState;
                stringDataToWrite = JsonUtility.ToJson(saveData, true);
                // ^Might want to write a float array in the future, but for now, we just write the JSON string.
                fileName = string.Format(FileNameFormat, savePrefix, writeArgs.SlotNumber.ToString(SaveNumberFormat), fileExtension);
                filePath = string.Format(filePathFormat, saveFolder, fileName);
            }
            string decryptedString = string.Empty;

            Task readTask = ReadAndDecrypt();
            async Task ReadAndDecrypt()
            {
                Task <byte[]> readTask = File.ReadAllBytesAsync(filePath);
                byte[] encryptedData = await readTask;
                byte[] decryptedData = encryptedData.Select(b => (byte)(b ^ key))
                    .ToArray();
                decryptedString = utf8.GetString(decryptedData);
            }
            yield return WaitFor(readTask);

            // Verify the content
            Assert.AreEqual(expectedJsonText, decryptedString, "Decrypted content does not match the expected JSON text.");

        }

        protected static Encoding utf8 = Encoding.UTF8;

        [UnityTest]
        public virtual IEnumerator EventInvocation_AmanitaSaveWritten()
        {
            
            // ^Since it might get set to null by other tests, we need to reset it

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
            yield return WaitFor(writeTask);
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsTrue(responded, "AmanitaSaveWritten event was not invoked after writing save data.");

        }

        [UnityTest]
        public virtual IEnumerator EventInvocation_AmanitaSaveWritten_MultipleWrites()
        {
            
            // ^Since it might get set to null by other tests, we need to reset it
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
            yield return WaitFor(writeTask);
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
        public virtual void DirectoryCreation_OnWrite()
        {
            
            // ^Since it might get set to null by other tests, we need to reset it
            SaveWriteRequest writeArgsForSaveDirectoryCreation = new SaveWriteRequest
            {
                SaveName = "TestSaveDirectoryCreation",
                SlotNumber = 0,
                MainState = new CompositeSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            SaveDirectoryType saveDirectoryType = writeArgsForSaveDirectoryCreation.BaseSaveDirectory;
            string saveFolder = SaveSystem.SaveDirectoryPaths[saveDirectoryType];
            if (saveWriter.RelativeSavePath.Count() > 0)
            {
                saveFolder = Path.Combine(saveFolder, saveWriter.RelativeSavePath);
            }
            // ^This is the directory we expect to be created
            // Ensure the directory does not exist before writing. We want to test directory creation.
            if (Directory.Exists(saveFolder))
            {
                Directory.Delete(saveFolder, true);
            }

            bool directoryWasErased = !Directory.Exists(saveFolder);
            Assert.IsTrue(directoryWasErased, "Directory should not exist before writing.");
            saveWriter.WriteOneToDisk(writeArgsForSaveDirectoryCreation);
            Assert.IsTrue(Directory.Exists(saveFolder), "Directory was not created after writing.");
        }

        [Test]
        public virtual async Task AtomicOperation_TempSaveWhenOverwriting()
        {
            Assert.Ignore();
        }
    
    }
}