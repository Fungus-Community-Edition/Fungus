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

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [Test]
        public virtual void WritesSaveToDisk_BasePathInAssetsFolder()
        {
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                SaveDirectory = SaveDirectoryType.DataPath
            };

            string savePrefix = saveWriter.SavePrefix;
            string fileExtension = saveWriter.FileExtension;
            string fileName = string.Format(fileNameFormat, savePrefix,
                writeArgs.SlotNumber, fileExtension);
            string baseDirectory = SaveSystem.SaveDirectoryPaths[writeArgs.SaveDirectory];
            string fullPath = System.IO.Path.Combine(baseDirectory, fileName);
            saveWriter.WriteOneToDisk(writeArgs);
            Assert.IsTrue(System.IO.File.Exists(fullPath), "Save file was not created.");
        }

        protected const string fileNameFormat = "{0}_0{1}.{2}";

        [Test]
        public virtual void WritesSaveToDisk_BasePersistentDataPath()
        {
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                SaveDirectory = SaveDirectoryType.PersistentDataPath
            };
            string savePrefix = saveWriter.SavePrefix;
            string fileExtension = saveWriter.FileExtension;
            string fileName = string.Format(fileNameFormat, savePrefix,
                writeArgs.SlotNumber, fileExtension);
            string baseDirectory = SaveSystem.SaveDirectoryPaths[writeArgs.SaveDirectory];
            string fullPath = System.IO.Path.Combine(baseDirectory, fileName);
            saveWriter.WriteOneToDisk(writeArgs);
            Assert.IsTrue(System.IO.File.Exists(fullPath), "Save file was not created.");
        }

        [Test]
        public virtual void WritesSaveToDisk_BaseStreamingAssetsPath()
        {
            SaveWriteArgs writeArgs = new SaveWriteArgs
            {
                SaveName = "TestSave",
                SlotNumber = 0,
                SaveData = new AmanitaSaveData(),
                SaveDirectory = SaveDirectoryType.StreamingAssetsPath
            };
            string savePrefix = saveWriter.SavePrefix;
            string fileExtension = saveWriter.FileExtension;
            string fileName = string.Format(fileNameFormat, savePrefix,
                writeArgs.SlotNumber, fileExtension);
            string baseDirectory = SaveSystem.SaveDirectoryPaths[writeArgs.SaveDirectory];
            string fullPath = System.IO.Path.Combine(baseDirectory, fileName);
            saveWriter.WriteOneToDisk(writeArgs);
            Assert.IsTrue(System.IO.File.Exists(fullPath), "Save file was not created.");
        }
    }
}