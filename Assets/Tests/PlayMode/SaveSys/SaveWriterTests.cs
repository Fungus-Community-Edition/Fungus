using Amanita.Myceliaudio;
using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Encoding = System.Text.Encoding;
using UnityObject = UnityEngine.Object;

namespace Amanita.SaveSystemTests
{
    public class SaveWriterTests
    {
        protected string toVarStateTests = "ScenePrefabs/VarStateTests";

        [SetUp]
        public virtual void DoSetUp()
        {
            PrepScene();
            PrepVars();
            flowchartSaveEncoder = ScriptableObject.CreateInstance<FlowchartSaveEncoder>();
            flowchartSaveData = flowchartSaveEncoder.EncodeToSave(flowchart);
            flowchartApplier = ScriptableObject.CreateInstance<FlowchartApplier>();
            saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
            saveWriter.RelativeSavePath = string.Empty;
            SaveSystem.InitPaths();

            flowchartSaveEncoder.ToMakeFrom = flowchart;
            SaveDataUnit unit = flowchartSaveData.Serialized();
            AmanitaSaveData mainSaveData = new AmanitaSaveData()
            {

            };
            writeArgs = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
        }

        protected FlowchartSaveEncoder flowchartSaveEncoder;
        protected FlowchartApplier flowchartApplier;
        protected FlowchartSaveData flowchartSaveData = null;

        protected virtual void PrepScene()
        {
            varStateTestPrefab = Resources.Load<GameObject>(toVarStateTests);
            varStateTestScene = UnityObject.Instantiate(varStateTestPrefab);
            playAudioArgsSO = Resources.Load<PlayAudioArgsSO>(pathToAudioArgsSO);
            flowchart = varStateTestScene.GetComponentInChildren<Flowchart>();
            audioSys = AudioSystem.S;
            applier = ScriptableObject.CreateInstance<MyceliaudioApplier>();
        }

        protected Flowchart flowchart;
        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;


        protected PlayAudioArgsSO playAudioArgsSO;
        protected string pathToAudioArgsSO = "testClip";

        protected PlayAudioArgs audioArgs;
        protected AudioSystem audioSys;
        protected MyceliaudioApplier applier;
        protected SaveWriter saveWriter;

        protected virtual void PrepVars()
        {
            nameVar = (StringVariable)flowchart.GetVariable("name");
            scoreVar = (IntegerVariable)flowchart.GetVariable("score");
            newPlayerVar = (BooleanVariable)flowchart.GetVariable("newPlayer");
            fastestTimeVar = (FloatVariable)flowchart.GetVariable("fastestTimeInSeconds");
            threeDPosVar = (Vector3Variable)flowchart.GetVariable("threeDPos");
            twoDPosVar = (Vector2Variable)flowchart.GetVariable("twoDPos");

            stringVar = flowchart.gameObject.AddComponent<StringVariable>();
            stringVar.Value = "Hello, World!";
            flowchart.Variables.Add(stringVar);

            transformVar = (TransformVariable)flowchart.GetVariable("someTrans");
        }

        protected StringVariable nameVar = null;
        protected IntegerVariable scoreVar = null;
        protected BooleanVariable newPlayerVar = null;
        protected FloatVariable fastestTimeVar = null;
        protected Vector3Variable threeDPosVar = null;
        protected Vector2Variable twoDPosVar = null;
        protected StringVariable stringVar = null;
        protected TransformVariable transformVar = null;

        protected string SavePrefix { get { return saveWriter.SavePrefix; } }
        protected string FileExtension { get { return saveWriter.FileExtension; } }

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [Test]
        public virtual void WritesSaveToDisk_BaseDataPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it

            CommonSaveWriteTest(writeArgs);
        }

        protected SaveWriteRequest writeArgs = new SaveWriteRequest
        {
            SaveName = "TestSave",
            SlotNumber = 0,
            SaveData = new AmanitaSaveData(),
            SaveMetaData = new SaveMetaData(),
            BaseSaveDirectory = SaveDirectoryType.DataPath
        };

        protected const string fileNameFormat = "{0}_0{1}.{2}";

        protected virtual void CommonSaveWriteTest(SaveWriteRequest writeArgs, string relativePath = "")
        {
            string fileName = string.Format(fileNameFormat, SavePrefix,
                writeArgs.SlotNumber, FileExtension);
            string baseDirectory = SaveSystem.SaveDirectoryPaths[writeArgs.BaseSaveDirectory];
            string fullPath; // So we can judge the results

            if (string.IsNullOrEmpty(relativePath))
            {
                fullPath = System.IO.Path.Combine(baseDirectory, fileName);
            }
            else
            {
                fullPath = System.IO.Path.Combine(baseDirectory, relativePath, fileName);
            }

            saveWriter.WriteOneToDisk(writeArgs);

            bool fileWasWritten = System.IO.File.Exists(fullPath);
            Assert.IsTrue(fileWasWritten, "Save file was not created.");
        }

        #region Successful writes
        [Test]
        public virtual void WritesSaveToDisk_BasePersistentDataPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            CommonSaveWriteTest(writeArgs);
        }

        [Test]
        public virtual void WritesSaveToDisk_BaseStreamingAssetsPath()
        {
            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            CommonSaveWriteTest(writeArgs);
        }

        // We can worry about PlayerPrefs later, if we need to.

        [Test]
        public virtual void WritesSaveToDisk_BaseDataPath_RelativePathIncluded()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            writeArgs.BaseSaveDirectory = SaveDirectoryType.DataPath;

            CommonSaveWriteTest(writeArgs, saveWriter.RelativeSavePath);
        }

        [Test]
        public virtual void WritesSaveToDisk_BasePersistentDataPath_RelativePathIncluded()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            CommonSaveWriteTest(writeArgs, saveWriter.RelativeSavePath);
        }

        [Test]
        public virtual void WritesSaveToDisk_BaseStreamingAssetsPath_RelativePathIncluded()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;

            CommonSaveWriteTest(writeArgs, saveWriter.RelativeSavePath);
        }

        #endregion

        #region Rejection tests
        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectNullSaveData()
        {
            SaveWriteRequest writeArgsWithNullSaveData = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = null, // Intentionally null to test rejection.
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData),
                "Expected ArgumentNullException when trying to write null save data.");

            writeArgsWithNullSaveData.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData),
                "Expected ArgumentNullException when trying to write null save data.");

            writeArgsWithNullSaveData.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData),
                "Expected ArgumentNullException when trying to write null save data.");
        }

        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectNullOrEmptySaveName()
        {
            SaveWriteRequest writeArgsWithBadSaveName = new SaveWriteRequest
            {
                SaveName = null, // Intentionally null to test rejection.
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSaveName),
                "Expected ArgumentNullException when trying to write with null save name.");
            writeArgsWithBadSaveName.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSaveName),
                "Expected ArgumentNullException when trying to write with null save name.");
            writeArgsWithBadSaveName.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSaveName),
                "Expected ArgumentNullException when trying to write with null save name.");
        }

        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectNegativeSlotNumber()
        {
            SaveWriteRequest writeArgsWithBadSlotNumber = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = -1, // Intentionally negative to test rejection.
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.Throws<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
            writeArgsWithBadSlotNumber.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.Throws<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
            writeArgsWithBadSlotNumber.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            Assert.Throws<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
        }

        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectInvalidBaseDirectory()
        {
            SaveWriteRequest writeArgsWithBadBaseDirectory = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = (SaveDirectoryType)999 // Intentionally invalid to test rejection.
            };
            Assert.Throws<System.ArgumentException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadBaseDirectory),
                "Expected ArgumentException when trying to write with invalid base directory.");
        }

        // BaseSaveDirectoryType is an enum, so we can't really test it with a null value.

        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectNullOrEmptyRelativePath()
        {
            saveWriter.RelativeSavePath = null; // Intentionally null to test rejection.
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write with null relative path.");
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write with null relative path.");
            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write with null relative path.");
        }
        #endregion

        [Test]
        public virtual void WriteAllToDisk_AllSuccessful()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;

            bool allWritten = saveWriter.WriteAllToDisk(multipleThingsToWrite);
            Assert.IsTrue(allWritten, "Not all saves were written successfully.");
        }

        protected IList<SaveWriteRequest> multipleThingsToWrite = new List<SaveWriteRequest>
            {
                new SaveWriteRequest
                {
                    SaveName = "TestSave1",
                    SlotNumber = 0,
                    SaveData = new AmanitaSaveData(),
                    SaveMetaData = new SaveMetaData(),
                    BaseSaveDirectory = SaveDirectoryType.DataPath
                },
                new SaveWriteRequest
                {
                    SaveName = "TestSave2",
                    SlotNumber = 1,
                    SaveData = new AmanitaSaveData(),
                    SaveMetaData = new SaveMetaData(),
                    BaseSaveDirectory = SaveDirectoryType.PersistentDataPath
                },
                new SaveWriteRequest
                {
                    SaveName = "TestSave3",
                    SlotNumber = 2,
                    SaveData = new AmanitaSaveData(),
                    SaveMetaData = new SaveMetaData(),
                    BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath
                }
            };

        [Test]
        public virtual void WriteAllToDisk_PartialSuccess()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;

            // Let's say the first one fails for some reason
            IList<SaveWriteRequest> withOneNull = new List<SaveWriteRequest>(multipleThingsToWrite);
            withOneNull[1] = null; // Intentionally null to simulate failure
            Assert.Throws<System.NullReferenceException>(() => saveWriter.WriteAllToDisk(withOneNull),
                "Expected NullReferenceException when trying to write a null SaveWriteArgs.");
        }

        [Test]
        public virtual void WriteAllToDisk_RejectNullList()
        {
            // It is implemented, though. Take a look at the SaveWriter class.
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;

            Assert.Throws<System.NullReferenceException>(() => saveWriter.WriteAllToDisk(null),
                "Expected NullReferenceException when trying to write a null list of SaveWriteArgs.");
        }

        // Now a test for file content verification would be nice, but that would require reading the file back and checking its contents. Let's set that test to be ignored for now.
        [Test]
        public virtual void VerifyFileContent_NONEncrypted_AfterWrite()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            saveWriter.WriteEncrypted = false;

            SaveWriteRequest writeArgs = new SaveWriteRequest
            {
                SaveName = "TestSaveContentVerification",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                SaveMetaData = new SaveMetaData { },
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string expectedMetaDataJson = JsonUtility.ToJson(writeArgs.SaveMetaData, true);
            string expectedMainSaveDataJson = JsonUtility.ToJson(writeArgs.SaveData, true);

            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}";
            CommonSaveWriteTest(writeArgs);

            string jsonText = ReadAndVerifyContent();
            string ReadAndVerifyContent()
            {
                string saveFolder = SaveSystem.SaveDirectoryPaths[writeArgs.BaseSaveDirectory];
                if (saveWriter.RelativeSavePath.Count() > 0)
                {
                    saveFolder = Path.Combine(saveFolder, saveWriter.RelativeSavePath);
                }
                string fileName = string.Format(fileNameFormat, saveWriter.SavePrefix, writeArgs.SlotNumber, saveWriter.FileExtension);
                string filePath = Path.Combine(saveFolder, fileName);
                // Read the file content
                return File.ReadAllText(filePath, utf8);
            }

            // Verify the content
            Assert.AreEqual(expectedJsonText, jsonText, "File content does not match the expected JSON text.");
        }

        [Test]
        public virtual void VerifyFileContent_Encrypted_AfterWrite()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            saveWriter.WriteEncrypted = true; // Set to true to test encrypted writing

            SaveWriteRequest writeArgs = new SaveWriteRequest
            {
                SaveName = "TestSaveEncrypted",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            string expectedMetaDataJson = JsonUtility.ToJson(writeArgs.SaveMetaData, true);
            string expectedMainSaveDataJson = JsonUtility.ToJson(writeArgs.SaveData, true);

            string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}";
            byte key = 0xAA;
            byte[] expectedEncryptedData = utf8.GetBytes(expectedJsonText)
                .Select(b => (byte)(b ^ key)).ToArray(); // Simple XOR encryption for testing

            CommonSaveWriteTest(writeArgs);

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

                SaveData saveData = writeArgs.SaveData;
                stringDataToWrite = JsonUtility.ToJson(saveData, true);
                // ^Might want to write a float array in the future, but for now, we just write the JSON string.
                fileName = string.Format(fileNameFormat, savePrefix, writeArgs.SlotNumber, fileExtension);
                filePath = string.Format(filePathFormat, saveFolder, fileName);
            }
            string decryptedString = string.Empty;
            ReadAndDecrypt();
            void ReadAndDecrypt()
            {
                // Read the encrypted file
                byte[] encryptedData = File.ReadAllBytes(filePath);
                // Decrypt it
                byte[] decryptedData = encryptedData.Select(b => (byte)(b ^ key)).ToArray();
                // Convert it back to string
                decryptedString = utf8.GetString(decryptedData);
            }

            // Verify the content
            Assert.AreEqual(expectedJsonText, decryptedString, "Decrypted content does not match the expected JSON text.");

        }

        protected static Encoding utf8 = Encoding.UTF8;

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
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

            saveWriter.WriteOneToDisk(writeArgs);
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsTrue(responded, "AmanitaSaveWritten event was not invoked after writing save data.");

        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_MultipleWrites()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
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
            saveWriter.WriteAllToDisk(multipleThingsToWrite);
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsTrue(responded, "AmanitaSaveWritten event was not invoked after writing multiple saves.");

        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_NoWrites()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
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
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool respondedWhenItShouldnt = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                respondedWhenItShouldnt = true;
            }

            Assert.Throws<System.NullReferenceException>(() => saveWriter.WriteOneToDisk(null),
                "Expected NullReferenceException when trying to write null SaveWriteArgs.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(respondedWhenItShouldnt, "AmanitaSaveWritten event was invoked with null write args.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullSaveData()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
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
                SaveData = null, // Intentionally null to test rejection.
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithNullSaveData),
                "Expected ArgumentNullException when trying to write null save data.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(respondedWhenItShouldnt, "AmanitaSaveWritten event was invoked with null save data.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullOrEmptySaveName()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }
            SaveWriteRequest writeArgsWithBadSaveName = new SaveWriteRequest
            {
                SaveName = null, // Intentionally null to test rejection.
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSaveName),
                "Expected ArgumentNullException when trying to write with null save name.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with null or empty save name.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNegativeSlotNumber()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
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
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.Throws<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadSlotNumber),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with negative slot number.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectInvalidBaseDirectory()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
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
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = (SaveDirectoryType)999 // Intentionally invalid to test rejection.
            };
            Assert.Throws<System.ArgumentException>(() => saveWriter.WriteOneToDisk(writeArgsWithBadBaseDirectory),
                "Expected ArgumentException when trying to write with invalid base directory.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with invalid base directory.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullOrEmptyRelativePath()
        {
            saveWriter.RelativeSavePath = null; // Intentionally null to test rejection.
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write with null relative path.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with null or empty relative path.");
        }

        [Test]
        public virtual void EventInvocation_AmanitaSaveWritten_RejectNullList()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            SaveSysSignals.AmanitaSaveWritten += OnAmanitaSaveWritten;
            bool responded = false;
            void OnAmanitaSaveWritten(SaveWriteResults writeResults)
            {
                responded = true;
            }
            Assert.Throws<System.NullReferenceException>(() => saveWriter.WriteAllToDisk(null),
                "Expected NullReferenceException when trying to write a null list of SaveWriteArgs.");
            SaveSysSignals.AmanitaSaveWritten -= OnAmanitaSaveWritten;
            Assert.IsFalse(responded, "AmanitaSaveWritten event was invoked with null list of SaveWriteArgs.");
        }

        [Test]
        public virtual void DirectoryCreation_OnWrite()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            SaveWriteRequest writeArgsForSaveDirectoryCreation = new SaveWriteRequest
            {
                SaveName = "TestSaveDirectoryCreation",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
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
    }
}