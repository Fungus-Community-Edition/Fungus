using Amanita.Myceliaudio;
using Amanita.SaveSys;
using Amanita.Utils;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Encoding = System.Text.Encoding;
using UnityObject = UnityEngine.Object;
using Amanita;

namespace Amanita.SaveSystemTests
{
    public abstract class CommonTestFunctionality
    {
        protected virtual string PathToTestScene => "ScenePrefabs/VarStateTests";

        [OneTimeSetUp]
        public virtual void DoOneTimeSetUp()
        {
            SaveSystem.InitPaths();

            pathToFungusManagerPrefab = AmanitaConstants.PathToAmanitaManagerPrefab;
            AmanitaManager fungusManagerPrefab = Resources.Load<AmanitaManager>(pathToFungusManagerPrefab);
            AmanitaManager fungusManager = UnityObject.Instantiate(fungusManagerPrefab);

            saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
            saveReader = ScriptableObject.CreateInstance<SaveReader>();
            encryptor = ScriptableObject.CreateInstance<Encryptor>();

            readReq = new SaveReadRequest
            {
                SlotNumber = writeReq.SlotNumber,
                BaseSaveDirectory = writeReq.BaseSaveDirectory,
            };

            waitToYield = new WaitForSeconds(waitTime);
            metaData.SaveVersion = "1.2.3";

            playAudioArgsSO = Resources.Load<PlayAudioArgsSO>(pathToAudioArgsSO);
            flowchartApplier = ScriptableObject.CreateInstance<FlowchartApplier>();
            audioApplier = ScriptableObject.CreateInstance<MyceliaudioApplier>();


        }

        protected IEnumerator WaitFor(Task writeTask)
        {
            var taskAwaitable = writeTask.ConfigureAwait(false);
            // ^We need it set up this way because otherwise, depending on 
            // the task, Unity might hang indefinitely.
            yield return taskAwaitable;
            
        }

        [SetUp]
        public virtual void DoSetUp()
        {
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

            PrepScene();
            
            RegisterSaveData();
            void RegisterSaveData()
            {
                CompositeSaveData mainSave = (CompositeSaveData)writeReq.MainState;
                mainSave.Clear();

                if (ReqFlowchart)
                {
                    flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);

                    SaveDataUnit encodedFlowchartSave = flowchartSaveData.Serialized();
                    mainSave.Add(encodedFlowchartSave);

                    IList<BlockSaveData> blockSaves = blockSaveCodec.EncodeToMultiSave(flowchart);
                    foreach (var blockSave in blockSaves)
                    {
                        SaveDataUnit saveDataUnit = blockSave.Serialized();
                        mainSave.Add(saveDataUnit);
                    }
                }

                saveDataSet = new SaveDataSet(metaData, mainSave);

            }

            SaveSystem.S.RegisterSaveDataApplier(flowchartApplier);
            SaveSystem.S.RegisterSaveDataApplier(audioApplier);
            saveWriter.DeleteBackupsPostOverwrite = true;
            LogAssert.ignoreFailingMessages = false;

        }


        protected SaveWriter saveWriter;
        protected SaveReader saveReader;
        protected Encryptor encryptor;
        protected SaveReadRequest readReq;

        WaitForSeconds waitToYield;
        private float waitTime = 0.2f;
        protected SaveMetaData metaData = new SaveMetaData();

        protected PlayAudioArgsSO playAudioArgsSO;
        protected string pathToAudioArgsSO = "testClip";
        protected FlowchartApplier flowchartApplier;
        protected MyceliaudioApplier audioApplier;

        protected FlowchartSaveCodec flowchartSaveCodec;
        protected BlockSaveCodec blockSaveCodec;

        protected virtual void PrepScene()
        {
            CreateScene();
            void CreateScene()
            {
                testScenePrefab = Resources.Load<GameObject>(PathToTestScene);
                if (testScenePrefab == null)
                    throw new Exception($"Could not load prefab at {PathToTestScene} from Resources.");

                testScene = UnityObject.Instantiate(testScenePrefab);
            }

            if (ReqFlowchart)
            {
                PrepFlowchart();
                PrepVars();
                PrepVarInitVals();
                void PrepVarInitVals()
                {
                    initNameVal = nameVar.Value;
                    initScoreVal = scoreVar.Value;
                    initIsNewPlayerVal = isNewPlayerVar.Value;
                    initFastestTimeVal = fastestTimeVar.Value;
                    initThreeDPosVal = threeDPosVar.Value;
                    initTwoDPosVal = twoDPosVar.Value;
                    initStringVal = stringVar.Value;
                }
            }

        }

        protected string pathToFungusManagerPrefab = "Prefabs/FungusManager";
        protected GameObject testScenePrefab;
        protected GameObject testScene;
        protected Flowchart flowchart;

        protected virtual bool ReqFlowchart => true;
        protected virtual void PrepFlowchart()
        {
            flowchart = testScene.GetComponentInChildren<Flowchart>(true);
            if (flowchart == null)
                throw new Exception("Flowchart component not found in test scene prefab.");

        }

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

        protected SaveWriteRequest writeReq = new SaveWriteRequest
        {
            SaveName = "TestSave",
            SlotNumber = 1,
            MainState = new CompositeSaveData(),
            SaveMetaData = new SaveMetaData(),
            BaseSaveDirectory = SaveDirectoryType.DataPath
        };


        protected virtual CompositeSaveData MainSave
        {
            get { return (CompositeSaveData) writeReq.MainState; }
        }

        protected FlowchartSaveData flowchartSaveData;
        protected SaveDataSet saveDataSet;

        protected virtual void ResetVarsToInitVals()
        {
            nameVar.Value = initNameVal;
            scoreVar.Value = initScoreVal;
            isNewPlayerVar.Value = initIsNewPlayerVal;
            fastestTimeVar.Value = initFastestTimeVal;
            threeDPosVar.Value = initThreeDPosVal;
            twoDPosVar.Value = initTwoDPosVal;
            stringVar.Value = initStringVal;
        }

        protected string relativePathForTesting = "TempSaves";
        
        protected AudioSystem AudioSys { get { return AudioSystem.S; } }

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

            ResetRelativeSavePaths();
            void ResetRelativeSavePaths()
            {
                saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
                saveReader.RelativeSavePath = saveReader.DefaultRelativeSavePath;
            }

            GetRidOfTestScene();
            void GetRidOfTestScene()
            {
                if (testScene != null)
                {
                    UnityObject.DestroyImmediate(testScene);
                }

                if (AmanitaManager.Instance != null)
                {
                    AmanitaManager.Instance.gameObject.SetActive(false);
                    UnityObject.DestroyImmediate(AmanitaManager.Instance.gameObject);
                }

                
            }
        }

        protected virtual bool ShouldDeleteTestSavesAtEnd => true;

        protected void DeleteAllTestSaves()
        {
            foreach (string root in SaveSystem.SaveDirectoryPaths.Values)
            {
                string pathToTempFolder = root; // We assume we already have the paths set based on the relative path for testing

                IList<string> pathsToTestSaves = Directory.EnumerateFiles(pathToTempFolder, "*.save",
                    SearchOption.AllDirectories).ToList();
                IList<string> pathsToTheMetaFiles = Directory.EnumerateFiles(pathToTempFolder,
                    "*.save.meta", SearchOption.AllDirectories).ToList();

                List<string> pathsForWhatToDelete = new List<string>(pathsToTestSaves);
                pathsForWhatToDelete.AddRange(pathsToTheMetaFiles);

                foreach (string filePath in pathsForWhatToDelete)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

            }
        }

        protected virtual IEnumerator CommonSetup()
        {
            yield return waitToYield;
            // The SaveSystem singleton should be set up by this point, meaning that
            // SaveDirectoryPaths should be initialized.
            PrepNewPathsForTesting();
            PrepAndRegisterSaveData();
        }

        void PrepAndRegisterSaveData()
        {
            if (flowchart == null)
            {
                flowchart = testScene.GetComponentInChildren<Flowchart>();
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

        void PrepNewPathsForTesting()
        {
            Dictionary<SaveDirectoryType, string> newPaths =
                new Dictionary<SaveDirectoryType, string>(BaseSavePaths);
            foreach (var keyEl in BaseSavePaths.Keys)
            {
                string currentVal = BaseSavePaths[keyEl];
                string newPath = Path.Combine(currentVal, relativePathForTesting);
                newPaths[keyEl] = newPath;
            }

            
            foreach (var keyEl in newPaths.Keys)
            {
                string path = newPaths[keyEl];
                SaveSystem.SaveDirectoryPaths[keyEl] = path;
            }
        }

        protected IDictionary<SaveDirectoryType, string> BaseSavePaths { get; set; } =
            new Dictionary<SaveDirectoryType, string>
            {
                { SaveDirectoryType.DataPath, Application.dataPath },
                { SaveDirectoryType.PersistentDataPath, Application.persistentDataPath },
                { SaveDirectoryType.StreamingAssetsPath, Application.streamingAssetsPath }
            };

        protected virtual async Task CommonSetupAsync()
        {
            await Task.Delay(1000).ConfigureAwait(false);

            if (UnityThreadUtil.IsMainThread)
            {
                PrepNewPathsForTesting();
                PrepAndRegisterSaveData();
            }
            else
            {
                using (var countdown = new CountdownEvent(1))
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        PrepNewPathsForTesting();
                        PrepAndRegisterSaveData();
                        countdown.Signal();
                    });

                    countdown.Wait();
                }
            }
            
        }

        protected string SavePrefix { get { return saveWriter.SavePrefix; } }
        protected string FileExtension { get { return saveWriter.FileExtension; } }

    }
}