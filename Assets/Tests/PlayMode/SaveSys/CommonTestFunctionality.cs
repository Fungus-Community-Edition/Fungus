using AtMycelia.Amanita;
using AtMycelia.Amanita.Myceliaudio;
using AtMycelia.SaveSys;
using AtMycelia.Amanita.VScripting;
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
using AtMycelia;
using AtMycelia.Amanita.SaveSys;

namespace SaveSystemTests
{
    /// <summary>
    /// Base test harness. Provides optional setup of:
    /// SaveSystem (writer/reader/codecs), Scene prefab, Flowchart + variables.
    /// Override the Req* flags in subclasses to trim what is initialized.
    /// </summary>
    public abstract class CommonTestFunctionality
    {
        // ---- Feature Flags (override to reduce setup cost) ----
        protected virtual bool ReqSaveSystem => true;
        protected virtual bool ReqSceneLoad => true;
        protected virtual bool ReqFlowchart => true;
        protected virtual bool ShouldIgnoreFailingLogMessagesByDefault => false;
        protected virtual bool ShouldDeleteTestSavesAtEnd => true;

        // ---- Resource Paths ----
        protected virtual string PathToTestScene => "ScenePrefabs/VarStateTests";
        protected string pathToAmanitaManagerPrefab = "Prefabs/AmanitaManager";
        protected string pathToAudioArgsSO = "testClip";

        // ---- Core Objects / Singletons ----
        protected AmanitaManager ammyManager;
        protected ISaveManager saveManager;
        protected SaveWriter saveWriter;
        protected SaveReader saveReader;
        protected Encryptor encryptor;
        protected SaveStorageSettings testStorageSettings;

        // ---- Codecs / Appliers ----
        protected FlowchartSaveCodec flowchartSaveCodec;
        protected BlockSaveCodec blockSaveCodec;
        protected FlowchartApplier flowchartApplier;
        protected MyceliaudioApplier audioApplier;

        // ---- Scene / Flowchart ----
        protected GameObject testScenePrefab;
        protected GameObject testScene;
        protected Flowchart flowchart;

        // ---- Flowchart Variables ----
        protected IVariable<string> nameVar;
        protected IVariable<int> scoreVar;
        protected IVariable<bool> isNewPlayerVar;
        protected IVariable<float> fastestTimeVar;
        protected IVariable<Vector3> threeDPosVar;
        protected IVariable<Vector2> twoDPosVar;
        protected IVariable<string> stringVar;
        protected IVariable<Transform> transformVar;

        // ---- Initial Variable Values ----
        protected string initNameVal;
        protected int initScoreVal;
        protected bool initIsNewPlayerVal;
        protected float initFastestTimeVal;
        protected Vector3 initThreeDPosVal;
        protected Vector2 initTwoDPosVal;
        protected string initStringVal;

        // ---- Save Data ----
        protected SaveMetaData metaData = new SaveMetaData();
        protected SaveWriteRequest writeReq = new SaveWriteRequest
        {
            SaveName = "TestSave",
            SlotNumber = 1,
            MainState = new CompositeSaveData(),
            SaveMetaData = new SaveMetaData(),
            BaseSaveDirectory = SaveDirectoryType.DataPath
        };
        protected SaveReadRequest readReq;
        protected FlowchartSaveData flowchartSaveData;
        protected SaveDataSet saveDataSet;

        // ---- Utilities ----
        protected readonly fsSerializer serializer = new fsSerializer();
        protected readonly fsSerializer serializerForTest = new fsSerializer();
        protected readonly DefaultSavePathResolver testPathResolver = new DefaultSavePathResolver("TestSaves");
        protected readonly DefaultSavePathResolver otherTestPathResolver = new DefaultSavePathResolver();
        protected readonly IList<string> saveFilePathsForCleanup = new List<string>();
        protected readonly List<UnityObj> toDestroyInTearDown = new List<UnityObj>();
        protected readonly IList<Flowchart> testOnlyFlowcharts = new List<Flowchart>();
        protected readonly IList<VariableSourceAsset> testOnlyVarSourceAssets = new List<VariableSourceAsset>();
        protected PlayAudioArgsSO playAudioArgsSO;
        protected WaitForSeconds waitToYield;
        private readonly float waitTime = 0.2f;

        // ---- Convenience Properties ----
        protected virtual CompositeSaveData MainSave => (CompositeSaveData)writeReq.MainState;
        protected string SavePrefix => saveWriter != null ? saveWriter.SavePrefix : string.Empty;
        protected string FileExtension => saveWriter != null ? saveWriter.FileExtension : string.Empty;
        protected AudioSystem AudioSys => AudioSystem.S;

        private ManualResetEventSlim saveSysInstallEvent;

        [OneTimeSetUp]
        public virtual void DoOneTimeSetUp()
        {
            waitToYield = new WaitForSeconds(waitTime);

            if (ReqSceneLoad)
            {
                testScenePrefab = Resources.Load<GameObject>(PathToTestScene);
                if (testScenePrefab == null)
                    throw new Exception($"Could not load prefab at {PathToTestScene} from Resources.");
            }
        }

        [SetUp]
        public virtual void DoSetUp()
        {
            // Note that this can run after the bootstrapper's done its whole initialization thing, so...
            LogAssert.ignoreFailingMessages = false;
            PlayerPrefs.DeleteAll();
            SaveSystemBootstrapper.ResetStaticsForTest();
            DestroyExistingAmanitaManagerIfAny();
            ResetSingletonStaticsForSetUp();

            SaveSystemBootstrapper.Enabled = ReqSaveSystem;
            if (ReqSaveSystem)
            {
                saveSysInstallEvent = new ManualResetEventSlim(false);
                SaveSysSignals.BaseSaveSysInstallationComplete += OnBaseSaveSysInstallationComplete;
                SetupSaveSystemAndDependencies();
                metaData.SaveVersion = "1.2.3";
                InitReadRequestFromWriteRequest();
            }

            LoadSupplementaryAppliersAndAssets();
            LoadCodecsIfNeeded();

            if (ReqSceneLoad)
            {
                LoadAndPrepScene();
                if (ReqFlowchart)
                {
                    EnsureFlowchartAndVariables();
                    CaptureInitialVariableValues();
                    BuildInitialInMemorySaveData();
                }
            }

            if (ReqSaveSystem)
            {
                // For tests that touch writer behavior often.
                saveWriter.DeleteBackupsPostOverwrite = true;
            }

            LogAssert.ignoreFailingMessages = ShouldIgnoreFailingLogMessagesByDefault;
            RegisterForTeardown();
        }

        // ---- Setup Helpers ----
        private void DestroyExistingAmanitaManagerIfAny()
        {
            if (AmanitaManager.S != null)
            {
                UnityObj.DestroyImmediate(AmanitaManager.S.gameObject);
            }
        }

        protected virtual void ResetSingletonStaticsForSetUp()
        {
            Flowchart.ResetStaticsForTest();
            AmanitaManager.ResetStaticsForTest();
            AudioSystem.ResetStaticsForTest();
        }

        protected virtual void ResetSingletonStaticsForTearDown()
        {
            //SaveSystem.ResetStaticsForTest(); // Leave this for one time tear down
            SaveSystemBootstrapper.ResetStaticsForTest();
            Flowchart.ResetStaticsForTest();
            AmanitaManager.ResetStaticsForTest();
            AudioSystem.ResetStaticsForTest();
        }

        private void SetupSaveSystemAndDependencies()
        {
            ammyManager = AmanitaManager.EnsureExists();
            ammyManager.Init();

            if (AmanitaManager.S != ammyManager)
                Debug.LogError("AmanitaManager.S was not set correctly!");

            testStorageSettings = ScriptableObject.CreateInstance<SaveStorageSettings>();
            testStorageSettings.RelativePath = testPathResolver.RelativePath;

            testPathResolver.StorageSettings = testStorageSettings;
            otherTestPathResolver.StorageSettings = testStorageSettings;

            saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
            saveReader = ScriptableObject.CreateInstance<SaveReader>();
            saveWriter.StorageSettings = saveReader.StorageSettings = testStorageSettings;
            encryptor = ScriptableObject.CreateInstance<Encryptor>();

            var testInstaller = new TestSaveSystemInstaller
            {
                StorageSettings = testStorageSettings,
                SaveReaderOverride = saveReader,
                SaveWriterOverride = saveWriter
            };

            SaveSystemBootstrapper.Installer = testInstaller;
            SaveSystemBootstrapper.InstallContext = null;

            SaveSystem.SavePathResolver = testPathResolver;
        }

        private void InitReadRequestFromWriteRequest()
        {
            readReq = new SaveReadRequest
            {
                SlotNumber = writeReq.SlotNumber,
                BaseSaveDirectory = writeReq.BaseSaveDirectory
            };
        }

        private void LoadSupplementaryAppliersAndAssets()
        {
            playAudioArgsSO = Resources.Load<PlayAudioArgsSO>(pathToAudioArgsSO);
            flowchartApplier = ScriptableObject.CreateInstance<FlowchartApplier>();
            audioApplier = ScriptableObject.CreateInstance<MyceliaudioApplier>();
        }

        private void LoadCodecsIfNeeded()
        {
            if (!ReqFlowchart) return;
            flowchartSaveCodec = ScriptableObject.CreateInstance<FlowchartSaveCodec>();
            blockSaveCodec = ScriptableObject.CreateInstance<BlockSaveCodec>();
        }

        private void LoadAndPrepScene()
        {
            testScene = UnityObj.Instantiate(testScenePrefab);
        }

        private void EnsureFlowchartAndVariables()
        {
            flowchart = testScene.GetComponentInChildren<Flowchart>(true);
            if (flowchart == null)
                throw new Exception("Flowchart component not found in test scene prefab.");
            flowchart.AlwaysKeepGuid = false;
            flowchart.gameObject.SetActive(true);
            flowchart.Refresh();

            // Variables
            nameVar = (IVariable<string>)flowchart.GetVariable("name");
            scoreVar = (IVariable<int>)flowchart.GetVariable("score");
            isNewPlayerVar = (IVariable<bool>)flowchart.GetVariable("newPlayer");
            fastestTimeVar = (IVariable<float>)flowchart.GetVariable("fastestTimeInSeconds");
            threeDPosVar = (IVariable<Vector3>)flowchart.GetVariable("threeDPos");
            twoDPosVar = (IVariable<Vector2>)flowchart.GetVariable("twoDPos");

            stringVar = flowchart.AddNewVariable("someStringVar", "Hello, World!");
            transformVar = (IVariable<Transform>)flowchart.GetVariable("someTrans");
        }

        private void CaptureInitialVariableValues()
        {
            initNameVal = nameVar.Value;
            initScoreVal = scoreVar.Value;
            initIsNewPlayerVal = isNewPlayerVar.Value;
            initFastestTimeVal = fastestTimeVar.Value;
            initThreeDPosVal = threeDPosVar.Value;
            initTwoDPosVal = twoDPosVar.Value;
            initStringVal = stringVar.Value;
        }

        private void BuildInitialInMemorySaveData()
        {
            if (!ReqFlowchart) return;

            MainSave.Clear();

            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            MainSave.Add(flowchartSaveData);

            IList<BlockSaveData> blockSaves = blockSaveCodec.EncodeToMultiSave(flowchart);
            foreach (var blockSave in blockSaves)
            {
                MainSave.Add(blockSave);
            }

            saveDataSet = new SaveDataSet(metaData, MainSave);
        }

        private void RegisterForTeardown()
        {
            // Always track created objects (null checks later)
            toDestroyInTearDown.Add(testScene);
            toDestroyInTearDown.Add(flowchartApplier);
            toDestroyInTearDown.Add(audioApplier);
            toDestroyInTearDown.Add(flowchartSaveCodec);
            toDestroyInTearDown.Add(blockSaveCodec);
            toDestroyInTearDown.Add(encryptor);
            toDestroyInTearDown.Add(saveWriter);
            toDestroyInTearDown.Add(saveReader);
            toDestroyInTearDown.Add(testStorageSettings);
            if (AmanitaManager.S != null)
            {
                toDestroyInTearDown.Add(AmanitaManager.S.gameObject);
            }

            // Flowchart may spawn an EventSystem
            foreach (var evt in UnityObj.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            {
                toDestroyInTearDown.Add(evt.gameObject);
            }
        }

        // ---- Variable Reset Helper ----
        protected virtual void ResetVarsToInitVals()
        {
            if (!ReqFlowchart) return;

            nameVar.Value = initNameVal;
            scoreVar.Value = initScoreVal;
            isNewPlayerVar.Value = initIsNewPlayerVal;
            fastestTimeVar.Value = initFastestTimeVal;
            threeDPosVar.Value = initThreeDPosVal;
            twoDPosVar.Value = initTwoDPosVal;
            stringVar.Value = initStringVal;
        }

        // ---- Coroutine / Async Common Setup (Flowchart only) ----
        protected virtual IEnumerator CommonSetup()
        {
            yield return waitToYield;
            //PrepNewPathsForTesting();
            PrepAndRegisterSaveData();
        }

        protected virtual async Task CommonSetupAsync()
        {
            await Task.Delay(CommonSetupDelay).ConfigureAwait(false);

            if (!ReqFlowchart) return;

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
            //saveReader.PathResolver = saveWriter.PathResolver = testPathResolver;
            if (ReqSaveSystem)
            {
                while (!SaveSystem.FullyInitted)
                {
                    await Task.Delay(50).ConfigureAwait(false);
                }
            }
        }

        protected virtual int CommonSetupDelay => 200;

        protected virtual void PrepAndRegisterSaveData()
        {
            if (!ReqFlowchart) return;
            if (flowchart == null && testScene != null)
                flowchart = testScene.GetComponentInChildren<Flowchart>();

            if (flowchart == null) throw new Exception("No Flowchart found in the scene.");

            RegisterTestFlowchart(flowchart);

            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            MainSave.Add(flowchartSaveData);

            foreach (var blockSave in blockSaveCodec.EncodeToMultiSave(flowchart))
            {
                MainSave.Add(blockSave);
            }
        }

        protected virtual void RegisterTestFlowchart(Flowchart testFc)
        {
            testOnlyFlowcharts.Add(testFc);
        }

        // ---- Path Prep ----
        protected virtual void OnBaseSaveSysInstallationComplete()
        {
            //PrepNewPathsForTesting();
            saveSysInstallEvent?.Set();
        }

        // ---- Teardown ----
        [TearDown]
        public virtual void DoTearDown()
        {
            SaveSystemBootstrapper.Enabled = true; // Re-enable in case a test disabled it, to ensure proper teardown of the save system.
            if (ReqSaveSystem)
            {
                DeleteAllTestSaves();
                SaveSysSignals.BaseSaveSysInstallationComplete -= OnBaseSaveSysInstallationComplete;
                if (SaveSystem.SaveManager != null)
                    SaveSystem.ClearSaveDataAppliers();
            }

            ResetSingletonStaticsForSetUp();
            UnregisterTestOnlyUids();
            
            CleanupTrackedSaveFiles();
            DestroyRegisteredObjects();
            
            writeReq.MainState = new CompositeSaveData();
            testOnlyFlowcharts.Clear();
            testOnlyVarSourceAssets.Clear();

            saveSysInstallEvent = null;
        }

        protected virtual void CleanupTrackedSaveFiles()
        {
            foreach (string path in saveFilePathsForCleanup)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            saveFilePathsForCleanup.Clear();
        }

        private void DestroyRegisteredObjects()
        {
            foreach (var obj in toDestroyInTearDown)
            {
                if (obj != null && obj is not ScriptableObject)
                    UnityObj.DestroyImmediate(obj);
                else if (obj is ScriptableObject)
                    UnityObj.Destroy(obj); 
                    // ^These are in-memory ones; it should be fine to use regular Destroy,
                    // allowing their OnDisabled methods to trigger and do other cleanup.
            }

            toDestroyInTearDown.Clear();

            // Null out references
            ammyManager = null;
            flowchart = null;
            testScene = null;
            flowchartApplier = null;
            audioApplier = null;
            flowchartSaveCodec = null;
            blockSaveCodec = null;
            encryptor = null;
            saveWriter = null;
            saveReader = null;
            saveManager = null;
        }

        [OneTimeTearDown]
        public virtual void DoOneTimeTearDown()
        {
            if (ShouldDeleteTestSavesAtEnd && ReqSaveSystem && SaveSystem.SaveManager != null)
            {
                DeleteAllTestSaves();
            }

            SaveSystem.ResetStaticsForTest();
            ResetRelativeSavePaths();
            DestroyResidualSceneAndManager();
        }

        private void ResetRelativeSavePaths()
        {
            if (saveWriter != null)
                saveWriter.RelativeSavePath = saveWriter.DefaultRelativeSavePath;
            if (saveReader != null)
                saveReader.RelativeSavePath = saveReader.DefaultRelativeSavePath;
        }

        private void DestroyResidualSceneAndManager()
        {
            if (testScene != null)
                UnityObj.DestroyImmediate(testScene);

            if (AmanitaManager.S != null)
            {
                AmanitaManager.S.gameObject.SetActive(false);
                UnityObj.DestroyImmediate(AmanitaManager.S.gameObject);
            }
        }

        // ---- GUID Handling for Test-Only Assets ----
        protected virtual void RegisterTestOnlyVsa(VariableSourceAsset testVsa)
        {
            testOnlyVarSourceAssets.Add(testVsa);
            testVsa.UniqueId = $"FakeTestVsaID_{testOnlyVarSourceAssets.Count}";
            testVsa.AlwaysKeepGuid = false;
        }

        protected virtual void UnregisterTestOnlyUids()
        {
            foreach (var fc in testOnlyFlowcharts)
            {
                fc.OnTearDown();
            }

            foreach (var fc in UnityObj.FindObjectsByType<Flowchart>(FindObjectsSortMode.None))
            {
                fc.OnTearDown();
            }

        }

        // ---- Save File Deletion ----
        protected void DeleteAllTestSaves()
        {
            if (SaveSystem.SaveManager == null) return;

            IList<string> folderPaths = new string[]
            {
                SaveSystem.GetSaveDirectory(SaveDirectoryType.DataPath),
                SaveSystem.GetSaveDirectory(SaveDirectoryType.PersistentDataPath),
                testPathResolver.GetSaveFolderPath(SaveDirectoryType.DataPath),
                testPathResolver.GetSaveFolderPath(SaveDirectoryType.PersistentDataPath),
            };

            foreach (string root in folderPaths)
            {
                if (!Directory.Exists(root)) continue;

                var allPaths = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                    .Where(path => path.EndsWith(".save") ||
                                path.EndsWith(".save.meta") ||
                                path.EndsWith(".save.bak") ||
                                path.EndsWith(".save.bak.meta"))
                    .ToList();

                foreach (var filePath in allPaths)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            }
        }

        // ---- Misc Helpers ----
        protected IEnumerator WaitFor(Task task)
        {
            var awaitable = task.ConfigureAwait(false);
            yield return awaitable;
        }
    }
}