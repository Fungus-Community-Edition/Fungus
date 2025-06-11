using Amanita.Myceliaudio;
using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Encoding = System.Text.Encoding;
using UnityObject = UnityEngine.Object;

namespace Amanita.SaveSystemTests
{
    public abstract class CommonTestFunctionality
    {
        protected virtual string PathToTestScene => "ScenePrefabs/VarStateTests";

        [OneTimeSetUp]
        public virtual void DoOneTimeSetUp()
        {
            SaveSystem.InitPaths();

            saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
            saveReader = ScriptableObject.CreateInstance<SaveReader>();
            encryptor = ScriptableObject.CreateInstance<Encryptor>();

            readReq = new SaveReadRequest
            {
                SlotNumber = writeReq.SlotNumber,
                BaseSaveDirectory = writeReq.BaseSaveDirectory,
            };

            PrepNewPathsForTesting();
            void PrepNewPathsForTesting()
            {
                baseSavePaths = new Dictionary<SaveDirectoryType, string>(SaveSystem.SaveDirectoryPaths);
                Dictionary<SaveDirectoryType, string> newPaths = new Dictionary<SaveDirectoryType, string>(SaveSystem.SaveDirectoryPaths);
                foreach (var keyEl in SaveSystem.SaveDirectoryPaths.Keys)
                {
                    string currentVal = SaveSystem.SaveDirectoryPaths[keyEl];
                    string newPath = Path.Combine(currentVal, relativePathForTesting);
                    newPaths[keyEl] = newPath;
                }

                foreach (var keyEl in newPaths.Keys)
                {
                    string path = newPaths[keyEl];
                    SaveSystem.SaveDirectoryPaths[keyEl] = path;
                }
            }

            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            saveReader.RelativeSavePath = saveReader.DefaultRelativeSavePath;

            waitToYield = new WaitForSeconds(waitTime);
            metaData.SaveVersion = "1.2.3";

            playAudioArgsSO = Resources.Load<PlayAudioArgsSO>(pathToAudioArgsSO);
            flowchartApplier = ScriptableObject.CreateInstance<FlowchartApplier>();
            audioApplier = ScriptableObject.CreateInstance<MyceliaudioApplier>();


        }

        protected IEnumerator WaitFor(Task writeTask)
        {
            yield return new WaitUntil(() => writeTask.IsCompleted);
        }


        protected IDictionary<SaveDirectoryType, string> baseSavePaths;

        protected SaveWriteRequest writeReq = new SaveWriteRequest
        {
            SaveName = "TestSave",
            SlotNumber = 0,
            MainState = new CompositeSaveData(),
            SaveMetaData = new SaveMetaData(),
            BaseSaveDirectory = SaveDirectoryType.DataPath
        };

        //[SetUp]
        public virtual void DoSetUp()
        {
            
            PrepScene();
            LoadCodecs();
            void LoadCodecs()
            {
                string pathToCodec = "SaveCodecs/FlowchartSaveCodec";
                //flowchartSaveCodec = Resources.Load<FlowchartSaveCodec>(pathToCodec);
                flowchartSaveCodec = ScriptableObject.CreateInstance<FlowchartSaveCodec>();

                pathToCodec = "SaveCodecs/BlockSaveCodec";
                //blockSaveCodec = Resources.Load<BlockSaveCodec>(pathToCodec);
                blockSaveCodec = ScriptableObject.CreateInstance<BlockSaveCodec>(); // We want to ensure we have a fresh instance for each test
            }
            RegisterSaveData();
            void RegisterSaveData()
            {
                CompositeSaveData mainSave = (CompositeSaveData)writeReq.MainState;
                mainSave.Clear();

                flowchartSaveCodec.ToMakeFrom = flowchart;
                flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);

                SaveDataUnit encodedFlowchartSave = flowchartSaveData.Serialized();
                mainSave.Add(encodedFlowchartSave);

                IList<BlockSaveData> blockSaves = blockSaveCodec.EncodeToMultiSave(flowchart);
                foreach (var blockSave in blockSaves)
                {
                    SaveDataUnit saveDataUnit = blockSave.Serialized();
                    mainSave.Add(saveDataUnit);
                }

                saveDataSet = new SaveDataSet(metaData, mainSave);

            }
        }

        protected virtual CompositeSaveData MainSave
        {
            get { return (CompositeSaveData) writeReq.MainState; }
        }

        protected virtual void PrepScene()
        {
            testScenePrefab = Resources.Load<GameObject>(PathToTestScene);
            if (testScenePrefab == null)
                throw new Exception($"Could not load prefab at {PathToTestScene} from Resources.");

            testScene = UnityObject.Instantiate(testScenePrefab);
            flowchart = testScene.GetComponentInChildren<Flowchart>(true);
            if (flowchart == null)
                throw new Exception("Flowchart component not found in test scene prefab.");

            PrepVars();
            initNameVal = nameVar.Value;
            initScoreVal = scoreVar.Value;
            initIsNewPlayerVal = isNewPlayerVar.Value;
            initFastestTimeVal = fastestTimeVar.Value;
            initThreeDPosVal = threeDPosVar.Value;
            initTwoDPosVal = twoDPosVar.Value;
            initStringVal = stringVar.Value;
        }

        protected GameObject testScenePrefab;
        protected GameObject testScene;

        protected Flowchart flowchart;
        protected FlowchartSaveCodec flowchartSaveCodec;
        protected FlowchartSaveData flowchartSaveData;
        protected BlockSaveCodec blockSaveCodec;

        protected SaveWriter saveWriter;
        protected SaveReader saveReader;
        protected SaveReadRequest readReq;
        float waitTime = 0.2f;
        WaitForSeconds waitToYield;
        protected string relativePathForTesting = "TempSaves";
        protected Encryptor encryptor;
        protected SaveDataSet saveDataSet;
        protected SaveMetaData metaData = new SaveMetaData();
        protected PlayAudioArgsSO playAudioArgsSO;
        protected string pathToAudioArgsSO = "testClip";
        protected FlowchartApplier flowchartApplier;
        protected MyceliaudioApplier audioApplier;
        protected AudioSystem AudioSys { get { return AudioSystem.S; } }

        protected StringVariable nameVar = null;
        protected IntegerVariable scoreVar = null;
        protected BooleanVariable isNewPlayerVar = null;
        protected FloatVariable fastestTimeVar = null;
        protected Vector3Variable threeDPosVar = null;
        protected Vector2Variable twoDPosVar = null;
        protected StringVariable stringVar = null;
        protected TransformVariable transformVar = null;

        protected string initNameVal;
        protected int initScoreVal;
        protected bool initIsNewPlayerVal;
        protected float initFastestTimeVal;
        protected Vector3 initThreeDPosVal;
        protected Vector2 initTwoDPosVal;
        protected string initStringVal;

        protected virtual void PrepVars()
        {
            nameVar = (StringVariable)flowchart.GetVariable("name");
            scoreVar = (IntegerVariable)flowchart.GetVariable("score");
            isNewPlayerVar = (BooleanVariable)flowchart.GetVariable("newPlayer");
            fastestTimeVar = (FloatVariable)flowchart.GetVariable("fastestTimeInSeconds");
            threeDPosVar = (Vector3Variable)flowchart.GetVariable("threeDPos");
            twoDPosVar = (Vector2Variable)flowchart.GetVariable("twoDPos");

            stringVar = flowchart.gameObject.AddComponent<StringVariable>();
            stringVar.Value = "Hello, World!";
            flowchart.Variables.Add(stringVar);

            transformVar = (TransformVariable)flowchart.GetVariable("someTrans");
        }

        protected virtual void ResetVarsToInitVals()
        {

        }

        [TearDown]
        public virtual void DoTearDown()
        {
            writeReq.MainState = new CompositeSaveData { };
            UnityObject.DestroyImmediate(testScene);

            if (SaveSystem.S != null)
            {
                UnityObject.DestroyImmediate(SaveSystem.S.gameObject);
            }
        }

        [OneTimeTearDown]
        public virtual void DoOneTimeTearDown()
        {
            if (ShouldDeleteTestSavesAtEnd)
            {
                DeleteAllTestSaves();
            }
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            saveReader.RelativeSavePath = saveReader.DefaultRelativeSavePath;
            if (testScene != null)
            {
                UnityObject.DestroyImmediate(testScene);
            }
        }

        protected virtual bool ShouldDeleteTestSavesAtEnd => true;

        protected void DeleteAllTestSaves()
        {
            foreach (string root in SaveSystem.SaveDirectoryPaths.Values)
            {
                string pathToTempFolder = root; // We assume we already have the paths set based on the relative path for testing

                IList<string> junk = Directory.EnumerateFiles(pathToTempFolder, "*.save",
                    SearchOption.AllDirectories).ToList();
                IList<string> junkMetas = Directory.EnumerateFiles(pathToTempFolder, "*.save.meta", SearchOption.AllDirectories).ToList();

                List<string> allJunk = new List<string>(junk);
                allJunk.AddRange(junkMetas);

                foreach (string file in allJunk)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }

            }
            saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
        }

        protected virtual async Task CommonSetupAsync()
        {
            DoSetUp();
            await Task.Delay(1000).ConfigureAwait(false);

            PrepAndRegisterSaveData();
            void PrepAndRegisterSaveData()
            {
                if (flowchart == null)
                {
                    flowchart = UnityObject.FindFirstObjectByType<Flowchart>();
                    if (flowchart == null)
                    {
                        throw new Exception("No Flowchart found in the scene.");
                    }
                }
                flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
                // ^We are expecting the flowchart encoder to use the block encoder as a sub

                CompositeSaveData mainSave = (CompositeSaveData)writeReq.MainState;
                SaveDataUnit encodedFlowchartSave = flowchartSaveData.Serialized();
                mainSave.Add(encodedFlowchartSave);

                IList<BlockSaveData> blockSaves = blockSaveCodec.EncodeToMultiSave(flowchart);
                foreach (var blockSave in blockSaves)
                {
                    SaveDataUnit saveDataUnit = blockSave.Serialized();
                    mainSave.Add(saveDataUnit);
                }
            }


        }

        protected virtual IEnumerator CommonSetup()
        {
            DoSetUp();
            yield return waitToYield;

            PrepAndRegisterSaveData();
            void PrepAndRegisterSaveData()
            {
                if (flowchart == null)
                {
                    flowchart = UnityObject.FindFirstObjectByType<Flowchart>();
                    if (flowchart == null)
                    {
                        throw new Exception("No Flowchart found in the scene.");
                    }
                }
                flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
                // ^We are expecting the flowchart encoder to use the block encoder as a sub

                CompositeSaveData mainSave = (CompositeSaveData)writeReq.MainState;
                SaveDataUnit encodedFlowchartSave = flowchartSaveData.Serialized();
                mainSave.Add(encodedFlowchartSave);

                IList<BlockSaveData> blockSaves = blockSaveCodec.EncodeToMultiSave(flowchart);
                foreach (var blockSave in blockSaves)
                {
                    SaveDataUnit saveDataUnit = blockSave.Serialized();
                    mainSave.Add(saveDataUnit);
                }
            }

        }

        protected string SavePrefix { get { return saveWriter.SavePrefix; } }
        protected string FileExtension { get { return saveWriter.FileExtension; } }

    }
}