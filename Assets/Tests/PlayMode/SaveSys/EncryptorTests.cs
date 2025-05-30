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
    public class EncryptorTests
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
            SaveSystem.InitPaths();

            flowchartSaveEncoder.ToMakeFrom = flowchart;
            SaveDataUnit unit = flowchartSaveData.Serialized();
            mainSaveData.Add(unit);

            metaData.SaveVersion = "1.2.3";

            saveDataSet = new SaveDataSet(metaData, mainSaveData);
            encryptor = ScriptableObject.CreateInstance<Encryptor>();
            
        }

        protected FlowchartSaveEncoder flowchartSaveEncoder;
        protected FlowchartApplier flowchartApplier;
        protected FlowchartSaveData flowchartSaveData = null;
        protected SaveMetaData metaData = new SaveMetaData();
        protected CompositeSaveData mainSaveData = new CompositeSaveData();

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

        protected AudioSystem audioSys;
        protected MyceliaudioApplier applier;

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
        protected Encryptor encryptor;
        protected SaveDataSet saveDataSet;


        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [Test]
        public virtual void ReturnsExpectedBytes()
        {
            try
            {
                string expectedMetaDataJson = JsonUtility.ToJson(metaData, true);
                string expectedMainSaveDataJson = JsonUtility.ToJson(mainSaveData, true);

                string expectedJsonText = $"{expectedMetaDataJson}{SaveDiskAccessor.ReadWriteDelimiter}{expectedMainSaveDataJson}";

                byte key = 0xAA;
                IList<byte> expectedBytes = utf8.GetBytes(expectedJsonText)
                    .Select(b => (byte)(b ^ key))
                    .ToArray(); // Simple XOR encryption for testing

                object output = encryptor.GetOutput(saveDataSet);
                byte[] bytesWeGot = (byte[])output;

                bool success = expectedBytes.SequenceEqual(bytesWeGot);
                Assert.IsTrue(success, "Got the wrong set of encrypted bytes.");
            }
            catch (System.Exception ex)
            {
                Debug.Log("Caught exception: " + ex);
                throw;

            }
        }

        protected Encoding utf8 = Encoding.UTF8;

        [Test]
        public virtual void RejectsNullInput()
        {
            Assert.Throws<System.NullReferenceException>(() => { encryptor.GetOutput(null); },
                "Does not reject null input.");
        }

        [Test]
        public virtual void RejectsNonSaveDataSetInput()
        {
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(varStateTestScene); },
                $"Accepted a scene as input when it shouldn't.");
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(flowchartApplier); },
                "Accepted a FlowchartApplier when it shouldn't.");
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(encryptor); },
                "Accepted itself when it shouldn't.");
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(saveDataSet.Meta); },
                "Accepted the metadata itself when it should've been in another container.");
            Assert.Throws<System.ArgumentException>(() => { encryptor.GetOutput(saveDataSet.MainState); },
                "Accepted the main state itself when it should've been in another container.");
        }
    }
}