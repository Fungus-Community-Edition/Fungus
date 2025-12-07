using Amanita.VScripting;
using System.Collections.Generic;
using UnityEngine;
using UnityObj = UnityEngine.Object;

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

        // We have this func instead of Awake so that when the time comes to set up any
        // global Flowcharts, the Amanita Manager will be ready. Otherwise, there's a
        // chance that things can get screwy
        public virtual void Init()
        {
            if (IsFullyInitted)
            {
                return;
            }

            bool otherInstallerAlreadyThere = S != null && S != this;
            if (otherInstallerAlreadyThere)
            {
                return; // We expect the AmanitaManager to handle destroying this if needed
            }

            S = this;

            if (!Application.IsPlaying(this))
            {
                // We don't want to install the save system in edit mode.
                return;
            }

            string pathToSysSettings = "SaveSys/Settings/SaveSystemSettings"; // Relative to the Resources folder
            sysSettings = Resources.Load<SaveSystemSettings>(pathToSysSettings);
            if (sysSettings == null)
            {
                Debug.LogError($"[{nameof(SaveSystemInstaller)}] No SaveSystemSettings found at " +
                    $"Resources/{pathToSysSettings}! Cannot install save system.");
                return;
            }

            storageSettings = sysSettings.StorageSettings;
            CorrectSaveDirTypeAsNeeded();
            void CorrectSaveDirTypeAsNeeded()
            {
                SaveDirectoryType dirType = storageSettings.DirectoryType;
                if (dirType == SaveDirectoryType.InTheBalls)
                {
                    dirType = SaveDirectoryType.DataPath;
                }

                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer ||
                    Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    dirType = SaveDirectoryType.PersistentDataPath;
                }

                storageSettings.DirectoryType = dirType;
            }

            IList<IMainSaveCodec> mainCodecs;
            IList<ISaveDataApplier> appliers;
            InitCodecsAndAppliers();
            void InitCodecsAndAppliers()
            {
                mainCodecs = sysSettings.MainCodecs;
                foreach (var codec in mainCodecs)
                {
                    codec.PreInstallInit();
                }

                appliers = sysSettings.MainAppliers;
                foreach (var applierEl in appliers)
                {
                    applierEl.PreInstallInit();
                }
            }

            PrepDependencies();
            void PrepDependencies()
            {
                PrepManager();
                void PrepManager()
                {
                    var versionProvider = new UnityVersionProvider();
                    MetaFactory = new DefaultMetaFactory(versionProvider);
                    MainStateFactory = new DefaultMainStateFactory(appliers, mainCodecs);

                    Registry = new SaveRegistry();
                    Loader = new SaveLoader(mainCodecs);

                    PrepRepo();
                    void PrepRepo()
                    {
                        SaveStorageSettings defaultSettings = DefaultAmanitaAssets.SaveStorageSettings;
                        var resolver = new DefaultSavePathResolver();
                        resolver.StorageSettings = defaultSettings;
                        SaveRepo = new FileSaveRepository(sysSettings.SaveReader, sysSettings.SaveWriter,
                            sysSettings.StorageSettings.DirectoryType, resolver);
                    }

                    SaveManager = new SaveManager(SaveRepo, Registry, Loader, MetaFactory, MainStateFactory);
                }
            }

            InjectDependencies();
            void InjectDependencies()
            {
                saveSystem = UnityObj.FindFirstObjectByType<SaveSystem>();
                // ^The save sys may not have set up its singleton field yet, hence why we're not accessing
                // it through that. 

                // Injecting dependendies before CoreLockMode activates.
                saveSystem.SaveDirectoryType = sysSettings.StorageSettings.DirectoryType;
                saveSystem.SaveManager = SaveManager;
                // ^We gave the manager its dependencies already, hence why we won't
                // apply them through the sys
                
                saveSystem.RegisterSaveDataAppliersMulti(appliers);
            }

            saveSystem.Init();
            IsFullyInitted = true;
            SaveSysSignals.BaseSaveSysInstallationComplete();
        }

        private SaveSystemSettings sysSettings;
        private SaveStorageSettings storageSettings;
        public virtual bool IsFullyInitted
        {
            get => initted;
            protected set => initted = value;
        }
        protected bool initted = false;

        public static SaveSystemInstaller S
        {
            get { return _s; }
            set
            {
                _s = value;
            }
        }
        protected static SaveSystemInstaller _s;
        public ISaveReader SaveReader
        {
            get
            {
                if (sysSettings == null)
                {
                    return null;
                }

                return sysSettings.SaveReader;
            }

        }
        public static SaveDirectoryType SaveDirectoryType { get; private set; }
        public static IMetaFactory MetaFactory { get; private set; }
        public static IMainStateFactory MainStateFactory { get; private set; }
        public static SaveRegistry Registry { get; private set; }
        public static SaveLoader Loader { get; private set; }
        public static ISaveRepository SaveRepo { get; private set; }
        public static ISaveManager SaveManager { get; private set; }

        protected SaveSystem saveSystem;

        protected Flowchart saveSysFlowchart;

        protected virtual void OnDestroy()
        {
            if (S == this)
            {
                S = null;
            }
        }

        public static void ResetStaticsForTest()
        {
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