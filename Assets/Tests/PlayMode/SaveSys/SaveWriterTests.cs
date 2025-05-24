using NUnit.Framework;
using UnityEngine;
using System.Collections;
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
    }
}