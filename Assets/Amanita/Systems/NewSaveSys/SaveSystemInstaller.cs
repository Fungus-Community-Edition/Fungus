using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Amanita.VScripting;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Injects the save system's dependencies.
    /// </summary>
    public class SaveSystemInstaller : MonoBehaviour
    {
        // Other modules that want to inject their own dependencies (say, for an RPG) should
        // do so in Start. This installer will handle all the initialization for the SaveSystem Singleton,
        // not just giving it its initial dependencies.
        [SerializeField] protected List<ScriptableObject> mainCodecs = new List<ScriptableObject>() { };
        [SerializeField] protected List<ScriptableObject> mainAppliers = new List<ScriptableObject>() { };
        [SerializeField] protected SaveWriter saveWriter = null;
        [SerializeField] protected SaveReader saveReader = null;
        [Tooltip("The base path for where the saves are stored, to be more precise. \"In WebGL, things will be saved to PlayerPrefs due to the file system limitations web browsers have. In which case, this field won't make a difference.\"")]
        [SerializeField] protected SaveDirectoryType whereSavesAreStored = SaveDirectoryType.InTheBalls;

        // We have this func instead of Awake so that when the time comes to set up any
        // global Flowcharts, the Amanita Manager will be ready. Otherwise, there's a
        // chance that things can get screwy
        public virtual void Init()
        {
            bool installerAlreadyThere = S != null && S != this;
            if (initted || installerAlreadyThere)
            {
                return; // We expect the AmanitaManager to handle destroying this if needed
            }

            S = this;

            var globalVars = AmanitaManager.S.GlobalVariables;
            if (globalVars == null)
            {
                throw new InvalidOperationException("AmanitaManager.GlobalVariables is null. Ensure AmanitaManager.Init() has run before SaveSystemInstaller.Init().");
            }

            SaveWriter = saveWriter;
            SaveReader = saveReader;
            if (whereSavesAreStored == SaveDirectoryType.InTheBalls)
            {
                whereSavesAreStored = SaveDirectoryType.DataPath;
            }

            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.WebGLPlayer)
            {
                whereSavesAreStored = SaveDirectoryType.PersistentDataPath;
            }

            SaveDirectoryType = whereSavesAreStored;

            // We assume these are valid due to what we have OnValidate do
            IList<IMainSaveCodec> validMainCodecs = mainCodecs.Cast<IMainSaveCodec>().ToList();
            IList<ISaveDataApplier> validAppliers = mainAppliers.Cast<ISaveDataApplier>().ToList();

            PrepDependencies();
            void PrepDependencies()
            {
                PrepManager();
                void PrepManager()
                {
                    var versionProvider = new UnityVersionProvider();
                    MetaFactory = new DefaultMetaFactory(versionProvider);
                    MainStateFactory = new DefaultMainStateFactory(validAppliers, validMainCodecs);

                    Registry = new SaveRegistry();
                    Loader = new SaveLoader(validMainCodecs);
                    SaveRepo = new FileSaveRepository(saveReader, saveWriter, whereSavesAreStored);
                    SaveManager = new SaveManager(SaveRepo, Registry, Loader, MetaFactory, MainStateFactory);
                }

                saveDirectoryPaths = new Dictionary<SaveDirectoryType, string>
                {
                    { SaveDirectoryType.DataPath, Application.dataPath },
                    { SaveDirectoryType.PersistentDataPath, Application.persistentDataPath },
                };

                // We assume that the GlobalVariables Flowchart was already initted by this point, as well
                // as AmanitaManager.S being non-null.

                var globalVars = AmanitaManager.S.GlobalVariables;

                StringVariable saveNameVar = globalVars.GetOrAddVariable<string, StringVariable>(SaveNameKey, "Slot");
                StringVariable saveNamePrefixVar = globalVars.GetOrAddVariable<string, StringVariable>(SaveNamePrefixKey, "");
                StringVariable saveNameSuffixVar = globalVars.GetOrAddVariable<string, StringVariable>(SaveNameSuffixKey, "");

            }

            InjectDependencies();
            void InjectDependencies()
            {
#if UNITY_6000_0_OR_NEWER
                
                saveSystem = UnityObject.FindFirstObjectByType<SaveSystem>();
#else
                saveSystem = UnityObject.FindObjectOfType<SaveSystem>();
#endif
                // ^The save sys may not have set up its singleton field yet, hence why we're not accessing
                // it through that. 

                saveSystem.Init();
                saveSystem.SaveDirectoryType = whereSavesAreStored;
                saveSystem.SaveManager = SaveManager;
                // ^We gave the manager its dependencies already, hence why we won't
                // apply them through the sys
                saveSystem.SaveDirectoryPaths = this.saveDirectoryPaths;
                saveSystem.RegisterSaveDataAppliersMulti(validAppliers);

            }
        }

        protected bool initted = false;

        public static SaveSystemInstaller S
        {
            get { return _s; }
            set
            {
                //Debug.Log($"{nameof(value)} S set to {value} at {Environment.StackTrace}");
                _s = value;
            }
        }
        protected static SaveSystemInstaller _s;
        public static SaveWriter SaveWriter { get; private set; }
        public static SaveReader SaveReader { get; private set; }
        public static SaveDirectoryType SaveDirectoryType { get; private set; }
        public static IMetaFactory MetaFactory { get; private set; }
        public static IMainStateFactory MainStateFactory { get; private set; }
        public static SaveRegistry Registry { get; private set; }
        public static SaveLoader Loader { get; private set; }
        public static ISaveRepository SaveRepo { get; private set; }
        public static ISaveManager SaveManager { get; private set; }
        protected IDictionary<SaveDirectoryType, string> saveDirectoryPaths;

        protected SaveSystem saveSystem;

        protected IList<ISaveDataApplier> validAppliers;
        protected Flowchart saveSysFlowchart;

        public static string SaveNameKey { get => AmanitaConstants.SaveNameVarName; }
        public static string SaveNamePrefixKey { get => AmanitaConstants.SaveNamePrefixVarName; }
        public static string SaveNameSuffixKey { get => AmanitaConstants.SaveNameSuffixVarName; }

        protected virtual void OnValidate()
        {
            if (whereSavesAreStored == SaveDirectoryType.Null)
            {
                whereSavesAreStored = SaveDirectoryType.InTheBalls;
            }

            ValidateAppliers();
            void ValidateAppliers()
            {
                // We assume that the nulls are from the user pressing the + button on adding to the lists.
                // Thus, we won't report those.
                IList<ScriptableObject> invalidAppliers = (from elem in mainAppliers
                                                           where elem != null
                                                           where elem is not ISaveDataApplier
                                                           select elem).ToList();

                for (int i = 0; i < invalidAppliers.Count; i++)
                {
                    ScriptableObject elem = invalidAppliers[i];
                    string warningMessage = $"{elem.name} is not a valid applier. It does not implement ISaveDataApplier.";
                    Debug.LogWarning(warningMessage);
                }

            }

            ValidateMainCodecs();
            void ValidateMainCodecs()
            {
                IList<ScriptableObject> invalidCodecs = (from elem in mainCodecs
                                                         where elem is not IMainSaveCodec
                                                         where elem != null
                                                         select elem).ToList();
                

                for (int i = 0; i < invalidCodecs.Count; i++)
                {
                    ScriptableObject elem = invalidCodecs[i];
                    string warningMessage = $"{elem.name} is not a valid main save codec. It does not implement IMainSaveCodec.";
                    Debug.LogWarning(warningMessage);
                }

            }
        }

        protected virtual void OnDestroy()
        {
            if (S == this)
            {
                S = null;
            }
        }

        public static void ResetStaticsForTest()
        {
            SaveWriter = null;
            SaveReader = null;
            SaveDirectoryType = SaveDirectoryType.DataPath;
            MetaFactory = null;
            MainStateFactory = null;
            Registry = null;
            Loader = null;
            SaveRepo = null;
            SaveManager = null;

            // If we reset the statics for AmanitaManger after calling this func, then this func 
            // should work as intended
            S = null;
            
            
        }
    }
}