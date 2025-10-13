using Amanita.Myceliaudio;
using Amanita.SaveSys;
using Amanita.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObject = UnityEngine.Object;
using UnityEngine.EventSystems;
using Amanita.VScripting;
using Amanita;
using FullSerializer;

namespace SaveSystemTests
{
    public abstract class CommonTestFunctionality
    {
        protected virtual string PathToTestScene => "ScenePrefabs/VarStateTests";

        [OneTimeSetUp]
        public virtual void DoOneTimeSetUp()
        {
            waitToYield = new WaitForSeconds(waitTime);
        }

        protected SaveSystem saveSys;

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
            PlayerPrefs.DeleteAll();
            //SaveStorageSettings saveStorageSettings = new SaveStorageSettings();
            if (AmanitaManager.S != null)
            {
                UnityObject.DestroyImmediate(AmanitaManager.S.gameObject);
            }

            ResetSingletonStatics();

            PrepAmanitaManagerAndItsSubmodules();
            void PrepAmanitaManagerAndItsSubmodules()
            {
                pathToAmanitaManagerPrefab = AmanitaConstants.PathToAmanitaManagerPrefab;
                AmanitaManager amanitaManagerPrefab = Resources.Load<AmanitaManager>(pathToAmanitaManagerPrefab);
                ammyManager = UnityObject.Instantiate(amanitaManagerPrefab);
                AmanitaManager.S = ammyManager;
                ammyManager.Init();

                // 3. Defensive check
                if (AmanitaManager.S != ammyManager)
                    Debug.LogError("AmanitaManager.S was not set correctly!");

                saveSys = ammyManager.GetComponentInChildren<SaveSystem>();
                SaveSystem.S = saveSys;
                SaveSystemInstaller installer = ammyManager.GetComponentInChildren<SaveSystemInstaller>();
                SaveSystemInstaller.S = installer;

                saveManager = saveSys.SaveManager;
                storageSettings = ScriptableObject.CreateInstance<SaveStorageSettings>();
                storageSettings.RelativePath = "TestSaves";
                saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
                saveReader = ScriptableObject.CreateInstance<SaveReader>();
                saveWriter.StorageSettings = saveReader.StorageSettings = storageSettings;
                otherTestPathResolver.StorageSettings = storageSettings;
                encryptor = ScriptableObject.CreateInstance<Encryptor>();
            }

            metaData.SaveVersion = "1.2.3";

            readReq = new SaveReadRequest
            {
                SlotNumber = writeReq.SlotNumber,
                BaseSaveDirectory = writeReq.BaseSaveDirectory,
            };

            playAudioArgsSO = Resources.Load<PlayAudioArgsSO>(pathToAudioArgsSO);
            flowchartApplier = ScriptableObject.CreateInstance<FlowchartApplier>();
            audioApplier = ScriptableObject.CreateInstance<MyceliaudioApplier>();

            LoadCodecs();
            void LoadCodecs()
            {
                flowchartSaveCodec = ScriptableObject.CreateInstance<FlowchartSaveCodec>();
                BuiltinVarSaveCodec builtinCodec = new BuiltinVarSaveCodec();
                flowchartSaveCodec.RegisterVarCodec(builtinCodec);
                flowchartApplier.RegisterVarCodec(builtinCodec);
                blockSaveCodec = ScriptableObject.CreateInstance<BlockSaveCodec>(); // We want to ensure we have a fresh instance for each test
            }
            
            writeReq = new SaveWriteRequest
            {
                SaveName = "TestSave",
                SlotNumber = 1,
                MainState = new CompositeSaveData(),
                SaveMetaData = new SaveMetaData(),
                BaseSaveDirectory = SaveDirectoryType.DataPath
            };

            if (ReqSceneLoad)
            {
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

                saveWriter.DeleteBackupsPostOverwrite = true;
            }

            LogAssert.ignoreFailingMessages = ShouldIgnoreFailingLogMessagesByDefault;

            RegisterWhatToDestroyInTearDown();
            void RegisterWhatToDestroyInTearDown()
            {
                toDestroyInTearDown.Add(AmanitaManager.S.gameObject);

                toDestroyInTearDown.Add(saveWriter);
                toDestroyInTearDown.Add(saveReader);
                toDestroyInTearDown.Add(storageSettings);
                toDestroyInTearDown.Add(encryptor);

                toDestroyInTearDown.Add(flowchartApplier);
                toDestroyInTearDown.Add(audioApplier);
                toDestroyInTearDown.Add(flowchartSaveCodec);
                toDestroyInTearDown.Add(blockSaveCodec);

                toDestroyInTearDown.Add(testScene);

                IList<EventSystem> possiblyMadeByFlowchart = UnityObject.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

                toDestroyInTearDown.AddRange(possiblyMadeByFlowchart);
            }
            
        }

        protected readonly IList<string> saveFilePathsForCleanup = new List<string>();
        protected SaveStorageSettings storageSettings;
        protected readonly List<UnityObject> toDestroyInTearDown = new List<UnityObject>();
        protected virtual void ResetSingletonStatics()
        {
            SaveSystem.ResetStaticsForTest();
            SaveSystemInstaller.ResetStaticsForTest();
            Flowchart.ResetStaticsForTest();
            AmanitaManager.ResetStaticsForTest();
            AudioSystem.ResetStaticsForTest();
        }

        protected virtual bool ReqSceneLoad => true;
        protected virtual bool ShouldIgnoreFailingLogMessagesByDefault => false;
        protected SaveWriter saveWriter;
        protected SaveReader saveReader;
        protected Encryptor encryptor;
        protected SaveReadRequest readReq;
        protected AmanitaManager ammyManager;
        protected WaitForSeconds waitToYield;
        private float waitTime = 0.2f; // Note that CoreLockMode starts at the 1-second mark
        protected SaveMetaData metaData = new SaveMetaData();

        protected PlayAudioArgsSO playAudioArgsSO;
        protected string pathToAudioArgsSO = "testClip";
        protected FlowchartApplier flowchartApplier;
        protected MyceliaudioApplier audioApplier;
        protected readonly fsSerializer serializer = new fsSerializer();
        protected FlowchartSaveCodec flowchartSaveCodec;
        protected BlockSaveCodec blockSaveCodec;
        protected ISaveManager saveManager;

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

        protected string pathToAmanitaManagerPrefab = "Prefabs/AmanitaManager";
        protected GameObject testScenePrefab;
        protected GameObject testScene;
        protected Flowchart flowchart;

        protected virtual bool ReqFlowchart => true;
        protected virtual void PrepFlowchart()
        {
            flowchart = testScene.GetComponentInChildren<Flowchart>(true);
            if (flowchart == null)
                throw new Exception("Flowchart component not found in test scene prefab.");
            flowchart.gameObject.SetActive(true);
        }

        protected virtual void PrepVars()
        {
            nameVar = (IVariable<string>)flowchart.GetVariable("name");
            scoreVar = (IVariable<int>)flowchart.GetVariable("score");
            isNewPlayerVar = (IVariable<bool>)flowchart.GetVariable("newPlayer");
            fastestTimeVar = (IVariable<float>)flowchart.GetVariable("fastestTimeInSeconds");
            threeDPosVar = (IVariable<Vector3>)flowchart.GetVariable("threeDPos");
            twoDPosVar = (IVariable<Vector2>)flowchart.GetVariable("twoDPos");

            flowchart.AddNewVariable<string, StringVariable>("someStringVar", "Hello, World!");
            stringVar = flowchart.GetVariable("someStringVar") as IVariable<string>;
            transformVar = (IVariable<Transform>)flowchart.GetVariable("someTrans");
        }

        protected IVariable<string> nameVar = null;
        protected IVariable<int> scoreVar = null;
        protected IVariable<bool> isNewPlayerVar = null;
        protected IVariable<float> fastestTimeVar = null;
        protected IVariable<Vector3> threeDPosVar = null;
        protected IVariable<Vector2> twoDPosVar = null;
        protected IVariable<string> stringVar = null;
        protected IVariable<Transform> transformVar = null;

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

        protected AudioSystem AudioSys { get { return AudioSystem.S; } }

        [TearDown]
        public virtual void DoTearDown()
        {
            SaveSystem.S.ClearSaveDataAppliers();
            ResetSingletonStatics();

            CleanupSaveFiles();
            void CleanupSaveFiles()
            {
                foreach (string path in saveFilePathsForCleanup)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }

                saveFilePathsForCleanup.Clear();
            }

            DestroyGameObjects();
            void DestroyGameObjects()
            {
                foreach (var obj in toDestroyInTearDown)
                {
                    if (obj != null)
                    {
                        UnityObject.DestroyImmediate(obj);
                    }
                }

                ammyManager = null;
                flowchart = null;
                testScene = null;
                saveWriter = null;
                saveReader = null;
                flowchartApplier = null;
                encryptor = null;
                audioApplier = null;
                flowchartSaveCodec = null;
                blockSaveCodec = null;
                saveSys = null;
                saveManager = null;
                toDestroyInTearDown.Clear();

            }

            writeReq.MainState = new CompositeSaveData { };
            
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

                if (AmanitaManager.S != null)
                {
                    AmanitaManager.S.gameObject.SetActive(false);
                    UnityObject.DestroyImmediate(AmanitaManager.S.gameObject);
                }

                
            }
        }

        protected virtual bool ShouldDeleteTestSavesAtEnd => true;

        protected readonly TestSavePathResolver testPathResolver = new TestSavePathResolver();
        protected readonly DefaultSavePathResolver otherTestPathResolver = new DefaultSavePathResolver();
        protected void DeleteAllTestSaves()
        {
            // We want to go for both the default and test paths
            IList<string> folderPaths = new string[]
            {
                saveSys.GetSaveDirectory(SaveDirectoryType.DataPath),
                saveSys.GetSaveDirectory(SaveDirectoryType.PersistentDataPath),

                testPathResolver.GetSaveFolderPath(SaveDirectoryType.DataPath),
                testPathResolver.GetSaveFolderPath(SaveDirectoryType.PersistentDataPath),
            };

            foreach (string root in folderPaths)
            {
                string pathToTempFolder = root; // We assume we already have the paths set based on the relative path for testing
                if (!Directory.Exists(pathToTempFolder))
                {
                    continue;
                }

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

        protected virtual void PrepAndRegisterSaveData()
        {
            CompositeSaveData mainSave = (CompositeSaveData)writeReq.MainState;

            if (ReqFlowchart)
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

        void PrepNewPathsForTesting()
        {
            // We need the writers, readers, and system as a whole to use the same resolver
            saveSys.SavePathResolver = testPathResolver;
            saveWriter.PathResolver = testPathResolver;
            saveReader.PathResolver = testPathResolver;
        }

        public class TestSavePathResolver : IConfigurableSaveSlotPathResolver<SaveDirectoryType>
        {
            public TestSavePathResolver(string relativePath = "TempSaves", string fileExtension = "save")
            {
                RelativePath = relativePath;
                FileExtension = fileExtension;
            }
            public string RelativePath { get; protected set; }
            public string FileExtension { get; protected set; }

            public string NumberFormat => "D2";

            string IConfigurableSaveSlotPathResolver.NumberFormat
            {
                get => NumberFormat;
                set
                {
                    // We don't allow changing this
                }
            }
            string IConfigurableSavePathResolver.RelativePath { get => RelativePath; set => RelativePath = value; }
            string IConfigurableSavePathResolver.FileExtension { get => FileExtension; set => FileExtension = value; }

            public string GetSaveFolderPath(SaveDirectoryType input)
            {
                string basePath;
                switch (input)
                {
                    case SaveDirectoryType.DataPath:
                    case SaveDirectoryType.InTheBalls:
                        basePath = Application.dataPath; break;
                    case SaveDirectoryType.PersistentDataPath:
                        basePath = Application.persistentDataPath; break;
                    default:
                        throw new ArgumentOutOfRangeException($"Input of type {input} is not supported.");
                }

                string result = Path.Combine(basePath, RelativePath);
                return result;
            }
            public string GetSaveFolderPath(object input)
            {
                if (input is SaveDirectoryType type)
                {
                    return GetSaveFolderPath(type);
                }
                else
                {
                    throw new ArgumentException($"Input must be of type {typeof(SaveDirectoryType)}");
                }
            }
            public string GetSaveFilePath(string fileName, SaveDirectoryType input)
            {
                string result = Path.Combine(GetSaveFolderPath(input),
                    $"{fileName}.{FileExtension}");
                return result;
            }
            public string GetSaveFilePath(string fileName, object input)
            {
                if (input is SaveDirectoryType type)
                {
                    return GetSaveFilePath(fileName, type);
                }
                else
                {
                    throw new ArgumentException($"Input must be of type {typeof(SaveDirectoryType)}");
                }
            }

            public string GetSaveFilePath(SaveDirectoryType input, int slotNumber)
            {
                string fileName = GetSaveFileName(slotNumber);
                string result = GetSaveFilePath($"saveData_slot{slotNumber}", input);
                return result;
            }

            public string GetSaveFileName(int slotNumber)
            {
                string path = GetSaveFilePath(SaveDirectoryType.PersistentDataPath, slotNumber);
                string result = Path.GetFileNameWithoutExtension(path);
                return result;
            }

            public string GetSaveFilePath(object input, int slotNumber)
            {
                string result = GetSaveFilePath((SaveDirectoryType)input, slotNumber);
                return result;
            }
        }

        protected IDictionary<SaveDirectoryType, string> BaseSavePaths { get; set; } =
            new Dictionary<SaveDirectoryType, string>
            {
                { SaveDirectoryType.DataPath, Application.dataPath },
                { SaveDirectoryType.PersistentDataPath, Application.persistentDataPath },
            };

        protected virtual async Task CommonSetupAsync()
        {
            await Task.Delay(CommonSetupDelay).ConfigureAwait(false);

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

        protected virtual int CommonSetupDelay
        {
            get
            {
                return 250; // Milliseconds
            }
        }

        protected string SavePrefix { get { return saveWriter.SavePrefix; } }
        protected string FileExtension { get { return saveWriter.FileExtension; } }

    }
}