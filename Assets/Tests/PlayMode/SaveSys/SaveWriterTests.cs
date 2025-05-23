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
        [Ignore("")]
        public virtual void WritesSaveToDisk()
        {
            string savePath = Application.persistentDataPath + "/save.json";
            MyceliaudioSaveData saveData = new MyceliaudioSaveData();
            AmanitaSaveData mainSaveData = new AmanitaSaveData();
            Assert.IsTrue(System.IO.File.Exists(savePath), "Save file was not created.");
        }
    }
}