using Amanita.Myceliaudio;
using Amanita.SaveSys;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObject = UnityEngine.Object;
using UnityEngine.EventSystems;
using Amanita.VScripting;
using Amanita;

namespace VScriptingTests.MuscariableTests
{
    public abstract class MuscariableWithSceneTestsCommon : MuscariableTestsCommon
    {
        protected virtual string PathToTestScene => "ScenePrefabs/VarStateTests";

        [OneTimeSetUp]
        public virtual void DoOneTimeSetUp()
        {
            waitToYield = new WaitForSeconds(waitTime);
        }

        [SetUp]
        public virtual void DoSetUp()
        {
            PlayerPrefs.DeleteAll();
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

                SaveSystemInstaller installer = ammyManager.GetComponentInChildren<SaveSystemInstaller>();
                SaveSystemInstaller.S = installer;

            }

            if (ReqSceneLoad)
            {
                PrepScene();
            }

            LogAssert.ignoreFailingMessages = ShouldIgnoreFailingLogMessagesByDefault;
        }

        protected virtual void ResetSingletonStatics()
        {
            SaveSystem.ResetStaticsForTest();
            SaveSystemInstaller.ResetStaticsForTest();
            Flowchart.ResetStaticsForTest();
            AmanitaManager.ResetStaticsForTest();
            AudioSystem.ResetStaticsForTest();
        }

        GameObject toUndoDontDestroyOnLoad;
        protected virtual bool ReqSceneLoad => true;
        protected virtual bool ShouldIgnoreFailingLogMessagesByDefault => false;
        protected AmanitaManager ammyManager;
        protected WaitForSeconds waitToYield;
        private float waitTime = 0.2f; // Note that CoreLockMode starts at the 1-second mark

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

        }

        protected virtual void PrepVars()
        {
            
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

        [TearDown]
        public virtual void DoTearDown()
        {
            ResetSingletonStatics();

            DestroyGameObjects();
            void DestroyGameObjects()
            {
                UnityObject.DestroyImmediate(testScene);
                testScene = null;
                DestroyEventSystems();
                void DestroyEventSystems()
                {
                    EventSystem[] possiblyMadeByFlowchart = UnityObject.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

                    foreach (var elem in possiblyMadeByFlowchart)
                    {
                        UnityObject.DestroyImmediate(elem.gameObject);
                    }
                }

                if (ammyManager != null)
                {
                    UnityObject.DestroyImmediate(ammyManager.gameObject);
                    // ^This should also destroy the submodules
                    ammyManager = null;
                }

            }

        }

        [OneTimeTearDown]
        public virtual void DoOneTimeTearDown()
        {
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

        protected virtual async Task CommonSetupAsync()
        {
            await Task.Delay(CommonSetupDelay).ConfigureAwait(false);


        }

        protected virtual int CommonSetupDelay
        {
            get
            {
                return 250; // Milliseconds
            }
        }


    }
}