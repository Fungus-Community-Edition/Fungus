using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Amanita.SaveSys;
using UnityObject = UnityEngine.Object;
using UnityEngine.TestTools;
using Amanita.Myceliaudio;

namespace Amanita.SaveSystemTests
{
    public class SaveWriterTests
    {
        protected string toVarStateTests = "ScenePrefabs/VarStateTests";

        [SetUp]
        public virtual void DoSetUp()
        {
            PrepScene();
            saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
            saveWriter.RelativeSavePath = string.Empty;
            SaveSystem.InitPaths();
        }

        protected virtual void PrepScene()
        {
            varStateTestPrefab = Resources.Load<GameObject>(toVarStateTests);
            varStateTestScene = UnityObject.Instantiate(varStateTestPrefab);
            playAudioArgsSO = Resources.Load<PlayAudioArgsSO>(pathToAudioArgsSO);
            audioSys = AudioSystem.S;
            applier = ScriptableObject.CreateInstance<MyceliaudioApplier>();
        }

        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;


        protected PlayAudioArgsSO playAudioArgsSO;
        protected string pathToAudioArgsSO = "testClip";

        protected PlayAudioArgs audioArgs;
        protected AudioSystem audioSys;
        protected MyceliaudioApplier applier;
        protected SaveWriter saveWriter;

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
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it

            CommonSaveWriteTest(writeArgs);
        }

        protected const string fileNameFormat = "{0}_0{1}.{2}";

        protected virtual void CommonSaveWriteTest(SaveWriteArgs writeArgs, string relativePath = "")
        {
            string fileName = string.Format(fileNameFormat, SavePrefix,
                writeArgs.SlotNumber, FileExtension);
            string baseDirectory = SaveSystem.SaveDirectoryPaths[writeArgs.BaseSaveDirectory];
            string fullPath;

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
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.PersistentDataPath
            };
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            CommonSaveWriteTest(writeArgs);
        }

        [Test]
        public virtual void WritesSaveToDisk_BaseStreamingAssetsPath()
        {
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath
            };
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            // ^Since it might get set to null by other tests, we need to reset it
            CommonSaveWriteTest(writeArgs);
        }

        // We can worry about PlayerPrefs later, if we need to.

        [Test]
        public virtual void WritesSaveToDisk_BaseDataPath_RelativePathIncluded()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;

            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            CommonSaveWriteTest(writeArgs, saveWriter.RelativeSavePath);
        }

        [Test]
        public virtual void WritesSaveToDisk_BasePersistentDataPath_RelativePathIncluded()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;

            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.PersistentDataPath
            };

            CommonSaveWriteTest(writeArgs, saveWriter.RelativeSavePath);
        }

        [Test]
        public virtual void WritesSaveToDisk_BaseStreamingAssetsPath_RelativePathIncluded()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;

            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath
            };

            CommonSaveWriteTest(writeArgs, saveWriter.RelativeSavePath);
        }

        #endregion

        #region Rejection tests
        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectNullSaveData()
        {
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = null, // Intentionally null to test rejection.
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write null save data.");

            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write null save data.");

            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write null save data.");
        }

        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectNullOrEmptySaveName()
        {
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = null, // Intentionally null to test rejection.
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write with null save name.");
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write with null save name.");
            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            Assert.Throws<System.ArgumentNullException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentNullException when trying to write with null save name.");
        }

        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectNegativeSlotNumber()
        {
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = -1, // Intentionally negative to test rejection.
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
            Assert.Throws<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
            writeArgs.BaseSaveDirectory = SaveDirectoryType.PersistentDataPath;
            Assert.Throws<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
            writeArgs.BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath;
            Assert.Throws<System.ArgumentOutOfRangeException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentOutOfRangeException when trying to write with negative slot number.");
        }

        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectInvalidBaseDirectory()
        {
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = (SaveDirectoryType)999 // Intentionally invalid to test rejection.
            };
            Assert.Throws<System.ArgumentException>(() => saveWriter.WriteOneToDisk(writeArgs),
                "Expected ArgumentException when trying to write with invalid base directory.");
        }

        // BaseSaveDirectoryType is an enum, so we can't really test it with a null value.

        [Test]
        public virtual void WritesSaveToDisk_AnyPath_RejectNullOrEmptyRelativePath()
        {
            saveWriter.RelativeSavePath = null; // Intentionally null to test rejection.
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };
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
            // It is implemented, though. Take a look at the SaveWriter class.
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
        
            bool allWritten = saveWriter.WriteAllToDisk(multipleThingsToWrite);
            Assert.IsTrue(allWritten, "Not all saves were written successfully.");
        }

        protected IList<SaveWriteArgs> multipleThingsToWrite = new List<SaveWriteArgs>
            {
                new SaveWriteArgs
                {
                    SaveName = "TestSave1",
                    SlotNumber = 0,
                    SaveData = new AmanitaSaveData(),
                    BaseSaveDirectory = SaveDirectoryType.DataPath
                },
                new SaveWriteArgs
                {
                    SaveName = "TestSave2",
                    SlotNumber = 1,
                    SaveData = new AmanitaSaveData(),
                    BaseSaveDirectory = SaveDirectoryType.PersistentDataPath
                },
                new SaveWriteArgs
                {
                    SaveName = "TestSave3",
                    SlotNumber = 2,
                    SaveData = new AmanitaSaveData(),
                    BaseSaveDirectory = SaveDirectoryType.StreamingAssetsPath
                }
            };

        [Test]
        public virtual void WriteAllToDisk_PartialSuccess()
        {
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
        
            // Let's say the first one fails for some reason
            IList<SaveWriteArgs> withOneNull = new List<SaveWriteArgs>(multipleThingsToWrite);
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
    }
}