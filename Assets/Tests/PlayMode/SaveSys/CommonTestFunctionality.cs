using Amanita;
using Amanita.Myceliaudio;
using Amanita.SaveSys;
using Amanita.Utils;
using Amanita.VScripting;
using FullSerializer;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;

namespace SaveSystemTests
{
    public abstract class CommonTestFunctionality
    {
        protected virtual string PathToTestScene => "ScenePrefabs/VarStateTests";

        [OneTimeSetUp]
        public virtual void DoOneTimeSetUp()
        {
            waitToYield = new WaitForSeconds(waitTime);
            testScenePrefab = Resources.Load<GameObject>(PathToTestScene);
            if (testScenePrefab == null)
                throw new Exception($"Could not load prefab at {PathToTestScene} from Resources.");
        }

        protected SaveSystem saveSys;
        protected fsSerializer serializerForTest = new fsSerializer();

        [SetUp]
        public virtual void DoSetUp()
        {
            SaveSysSignals.BaseSaveSysInstallationComplete += OnBaseSaveSysInstallationComplete;
            PlayerPrefs.DeleteAll();
            if (AmanitaManager.S != null)
            {
                UnityObj.DestroyImmediate(AmanitaManager.S.gameObject);
            }

            ResetSingletonStatics();

            PrepAmanitaManagerAndItsSubmodules();
            void PrepAmanitaManagerAndItsSubmodules()
            {
                pathToAmanitaManagerPrefab = AmanitaConstants.PathToAmanitaManagerPrefab;
                AmanitaManager amanitaManagerPrefab = Resources.Load<AmanitaManager>(pathToAmanitaManagerPrefab);
                ammyManager = UnityObj.Instantiate(amanitaManagerPrefab);
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
                blockSaveCodec = ScriptableObject.CreateInstance<BlockSaveCodec>();
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
                        mainSave.Add(flowchartSaveData);

                        IList<BlockSaveData> blockSaves = blockSaveCodec.EncodeToMultiSave(flowchart);
                        foreach (var blockSave in blockSaves)
                        {
                            mainSave.Add(blockSave);
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

                IList<EventSystem> possiblyMadeByFlowchart = UnityObj.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

                foreach (var eventSys in possiblyMadeByFlowchart)
                {
                    toDestroyInTearDown.Add(eventSys.gameObject);
                }
            }
        }

        protected readonly IList<string> saveFilePathsForCleanup = new List<string>();
        protected SaveStorageSettings storageSettings;
        protected readonly List<UnityObj> toDestroyInTearDown = new List<UnityObj>();
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
            testScene = UnityObj.Instantiate(testScenePrefab);
            
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
            SaveSysSignals.BaseSaveSysInstallationComplete -= OnBaseSaveSysInstallationComplete;
            SaveSystem.S.ClearSaveDataAppliers();
            ResetSingletonStatics();
            UnregisterTestOnlyUids();
            DeleteAllTestSaves();
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
                        UnityObj.DestroyImmediate(obj);
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
            testOnlyFlowcharts.Clear();
            testOnlyVarSourceAssets.Clear();
        }

        protected virtual void RegisterTestOnlyVsa(VariableSourceAsset testVsa)
        {
            testOnlyVarSourceAssets.Add(testVsa);
            testVsa.UniqueId = $"FakeTestVsaID_{testOnlyVarSourceAssets.Count}";
        }

        protected virtual void UnregisterTestOnlyUids()
        {
            var fcUidRegistry = AmanitaManager.GetOrAddGuidRegistryFor<Flowchart>();
            foreach (var fc in testOnlyFlowcharts)
            {
                fcUidRegistry.RemoveGuid(fc.UniqueId);
            }

            // In case we missed any others
            foreach (var fc in UnityObj.FindObjectsByType<Flowchart>(FindObjectsSortMode.None))
            {
                fcUidRegistry.RemoveGuid(fc.UniqueId);
            }

            var vsaUidRegistry = AmanitaManager.GetOrAddGuidRegistryFor<VariableSourceAsset>();
            foreach (var vsa in testOnlyVarSourceAssets)
            {
                vsaUidRegistry.RemoveGuid(vsa.UniqueId);
            }
        }

        protected readonly IList<Flowchart> testOnlyFlowcharts = new List<Flowchart>();
        protected readonly IList<VariableSourceAsset> testOnlyVarSourceAssets = new List<VariableSourceAsset>();
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
                    UnityObj.DestroyImmediate(testScene);
                }

                if (AmanitaManager.S != null)
                {
                    AmanitaManager.S.gameObject.SetActive(false);
                    UnityObj.DestroyImmediate(AmanitaManager.S.gameObject);
                }
            }
        }

        protected virtual bool ShouldDeleteTestSavesAtEnd => true;

        protected readonly DefaultSavePathResolver testPathResolver = new DefaultSavePathResolver("TestSaves");
        protected readonly DefaultSavePathResolver otherTestPathResolver = new DefaultSavePathResolver();
        protected void DeleteAllTestSaves()
        {
            IList<string> folderPaths = new string[]
            {
                saveSys.GetSaveDirectory(SaveDirectoryType.DataPath),
                saveSys.GetSaveDirectory(SaveDirectoryType.PersistentDataPath),

                testPathResolver.GetSaveFolderPath(SaveDirectoryType.DataPath),
                testPathResolver.GetSaveFolderPath(SaveDirectoryType.PersistentDataPath),
            };

            foreach (string root in folderPaths)
            {
                string pathToTempFolder = root;
                if (!Directory.Exists(pathToTempFolder))
                {
                    continue;
                }

                IList<string> pathsToTestSaves = Directory.EnumerateFiles(pathToTempFolder, "*.save",
                    SearchOption.AllDirectories).ToList();
                IList<string> pathsToTheMetaFiles = Directory.EnumerateFiles(pathToTempFolder,
                    "*.save.meta", SearchOption.AllDirectories).ToList();
                IList<string> pathsToBakFiles = Directory.EnumerateFiles(pathToTempFolder,
                    "*.save.bak", SearchOption.AllDirectories).ToList();
                IList<string> pathsToBakMetaFiles = Directory.EnumerateFiles(pathToTempFolder,
                    "*.save.bak.meta", SearchOption.AllDirectories).ToList();

                List<string> pathsForWhatToDelete = new List<string>(pathsToTestSaves);
                pathsForWhatToDelete.AddRange(pathsToTheMetaFiles);
                pathsForWhatToDelete.AddRange(pathsToBakFiles);
                pathsForWhatToDelete.AddRange(pathsToBakMetaFiles);

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

            PrepNewPathsForTesting();
            PrepAndRegisterSaveData();
        }

        protected virtual void RegisterTestFlowchart(Flowchart testFc)
        {
            testOnlyFlowcharts.Add(testFc);
            // No need to give it a fake unique ID here, since Flowcharts get themselves such when they see
            // that they're in a test context.
        }

        protected virtual void PrepAndRegisterSaveData()
        {
            // But without getting it written to disk. Working purely in memory here.
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

                RegisterTestFlowchart(flowchart);

                flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
                mainSave.Add(flowchartSaveData);

                IList<BlockSaveData> blockSaves = blockSaveCodec.EncodeToMultiSave(flowchart);
                foreach (var blockSave in blockSaves)
                {
                    mainSave.Add(blockSave);
                }
            }
        }

        protected virtual async Task CommonSetupAsync()
        {
            await Task.Delay(CommonSetupDelay).ConfigureAwait(false);

            if (UnityThreadUtil.IsMainThread)
            {
                PrepAndRegisterSaveData();
            }
            else
            {
                using (var countdown = new CountdownEvent(1))
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        PrepAndRegisterSaveData();
                        countdown.Signal();
                    });

                    countdown.Wait();
                }
            }
        }

        protected virtual void OnBaseSaveSysInstallationComplete()
        {
            PrepNewPathsForTesting();
            
        }

        protected virtual void PrepNewPathsForTesting()
        {
            testPathResolver.RelativePath = "TestSaves";
            // ^Need to make sure, since it prioritizes the internal storage settings
            saveSys = SaveSystem.S;
            saveSys.SavePathResolver = testPathResolver;
        }

        protected virtual int CommonSetupDelay
        {
            get
            {
                return 200; // Milliseconds
            }
        }

        protected string SavePrefix { get { return saveWriter.SavePrefix; } }
        protected string FileExtension { get { return saveWriter.FileExtension; } }

        protected IEnumerator WaitFor(Task writeTask)
        {
            var taskAwaitable = writeTask.ConfigureAwait(false);
            yield return taskAwaitable;
        }

    }

}