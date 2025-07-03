using Amanita.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Injects the save system's dependencies.
    /// </summary>
    public class SaveSystemInstaller : MonoBehaviour
    {
        [SerializeField] protected List<ScriptableObject> mainCodecs = new List<ScriptableObject>() { };
        [SerializeField] protected List<ScriptableObject> mainAppliers = new List<ScriptableObject>() { };
        [SerializeField] protected SaveWriter saveWriter = null;
        [SerializeField] protected SaveReader saveReader = null;
        [Tooltip("The base path for where the saves are stored, to be more precise. \"In WebGL, things will be saved to PlayerPrefs due to the file system limitations web browsers have. In which case, this field won't make a difference.\"")]
        [SerializeField] protected SaveDirectoryType whereSavesAreStored = SaveDirectoryType.InTheBalls;
        
        // Other modules that want to inject their own dependencies (say, for an RPG) should
        // do so in Start. This installer will handle all the initialization for the SaveSystem Singleton,
        // not just giving it its initial dependencies.
        protected virtual void Awake()
        {
            bool installerAlreadyThere = S != null && S != this;
            if (installerAlreadyThere)
            {
                return; // We expect the AmanitaManager to handle destroying this
            }

            S = this;

            if (whereSavesAreStored == SaveDirectoryType.InTheBalls)
            {
                whereSavesAreStored = SaveDirectoryType.DataPath;
            }

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
                    { SaveDirectoryType.StreamingAssetsPath, Application.streamingAssetsPath }
                };

            }

            InjectDependencies();
            void InjectDependencies()
            {
#if UNITY_6000_0_OR_NEWER
                
                saveSystem = Object.FindFirstObjectByType<SaveSystem>();
#else
                saveSystem = Object.FindObjectOfType<SaveSystem>();
#endif
                // ^The save sys may not have set up its singleton field yet, hence why we're not accessing
                // it through that. 

                saveSystem.Initialize();
                saveSystem.SaveDirectoryType = whereSavesAreStored;
                saveSystem.SaveManager = SaveManager;
                // ^We gave the manager its dependencies already, hence why we won't
                // apply them through the sys
                saveSystem.SaveDirectoryPaths = this.saveDirectoryPaths;

                validAppliers = (from elem in mainAppliers
                                where elem is ISaveDataApplier
                                select elem as ISaveDataApplier).ToList();
                saveSystem.RegisterSaveDataAppliersMulti(validAppliers);

            }
        }

        public static SaveSystemInstaller S { get; private set; }
        public static IMetaFactory MetaFactory { get; private set; }
        public static IMainStateFactory MainStateFactory { get; private set; }
        public static SaveRegistry Registry { get; private set; }
        public static SaveLoader Loader { get; private set; }
        public static ISaveRepository SaveRepo { get; private set; }
        public static ISaveManager SaveManager { get; private set; }
        protected IDictionary<SaveDirectoryType, string> saveDirectoryPaths;

        protected SaveSystem saveSystem;

        protected IList<ISaveDataApplier> validAppliers;

        protected virtual void OnValidate()
        {
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

    }
}