using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.SaveSys
{
    /// <summary>
    /// Injects the save system's dependencies and handles the initialization of the SaveSystem singleton. 
    /// This is separate from the SaveSystemBootstrapper.
    /// </summary>
    public class SaveSystemInstaller
    {
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

            if (!Application.isPlaying)
            {
                // We don't want to install the save system in edit mode.
                return;
            }

            #region Load settings and correct them as needed
            string pathToSysSettings = "SaveSys/Settings/SaveSystemSettings"; // Relative to the Resources folder
            sysSettings = Resources.Load<SaveSystemSettings>(pathToSysSettings);
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
                        SaveStorageSettings defaultSettings = DefaultSaveSysAssets.SaveStorageSettings;
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