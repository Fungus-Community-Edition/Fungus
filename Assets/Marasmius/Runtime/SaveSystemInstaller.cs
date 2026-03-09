using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.SaveSys
{
    /// <summary>
    /// Injects the save system's dependencies and handles the initialization of the SaveSystem singleton. 
    /// This is separate from the SaveSystemBootstrapper.
    /// </summary>
    public class SaveSystemInstaller : ISaveSystemInstaller
    {
        public virtual void Init(SaveSystemInstallContext context = null)
        {
            if (IsFullyInitted)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                // We don't want to install the save system in edit mode.
                return;
            }

            #region Load settings and correct them as needed
            string pathToSysSettings = SaveSysConstants.PathToSaveSysSettings;
            sysSettings = context?.SettingsOverride;
            if (sysSettings == null)
            {
                sysSettings = Resources.Load<SaveSystemSettings>(pathToSysSettings);
                Debug.Log("Using default save sys settings");
                // ^Default asset in the project
            }
            if (sysSettings == null)
            {
                Debug.LogError($"[{nameof(SaveSystemInstaller)}] No SaveSystemSettings found at " +
                    $"Resources/{pathToSysSettings}! Cannot install save system.");
                return;
            }
            #endregion

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
                        SaveStorageSettings storage = sysSettings.StorageSettings;
                        var resolver = new DefaultSavePathResolver
                        {
                            StorageSettings = storage
                        };
                        SaveRepo = new FileSaveRepository(sysSettings.SaveReader, sysSettings.SaveWriter,
                            sysSettings.StorageSettings.DirectoryType, resolver);
                    }

                    SaveManager = new SaveManager(SaveRepo, Registry, Loader, MetaFactory, MainStateFactory);
                }
            }

            InjectDependencies();
            void InjectDependencies()
            {
                // This should happen before CoreLockMode activates.
                SaveSystem.SaveDirectoryType = storageSettings.DirectoryType;
                SaveSystem.SaveManager = SaveManager;
                // ^We gave the manager its dependencies already, hence why we won't
                // apply them through the sys

                SaveSystem.RegisterSaveDataAppliersMulti(appliers);
            }

            SaveSystem.Init();
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

        public IMetaFactory MetaFactory { get; private set; }
        public IMainStateFactory MainStateFactory { get; private set; }
        public SaveRegistry Registry { get; private set; }
        public SaveLoader Loader { get; private set; }
        public ISaveRepository SaveRepo { get; private set; }
        public ISaveManager SaveManager { get; private set; }
    }
}