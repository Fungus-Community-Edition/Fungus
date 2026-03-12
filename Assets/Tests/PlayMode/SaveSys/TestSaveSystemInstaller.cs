using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.SaveSys
{
    public sealed class TestSaveSystemInstaller : SaveSystemInstaller
    {
        public SaveStorageSettings StorageSettings { get; set; }
        public ISaveReader SaveReaderOverride { get; set; }
        public ISaveWriter SaveWriterOverride { get; set; }
        public IList<ISaveDataApplier> MainAppliersOverride { get; set; }
        public IList<IMainSaveCodec> MainCodecsOverride { get; set; }

        public override void Init(SaveSystemInstallContext context = null)
        {
            var effectiveContext = context ?? new SaveSystemInstallContext();
            effectiveContext.SettingsOverride ??= BuildTestSettings();
            base.Init(effectiveContext);
        }

        private SaveSystemSettings BuildTestSettings()
        {
            if (StorageSettings == null || SaveReaderOverride == null || SaveWriterOverride == null)
            {
                Debug.LogError("[TestSaveSystemInstaller] StorageSettings, SaveReaderOverride, and SaveWriterOverride are required.");
                return null;
            }

            SaveSystemSettings testSettings = ScriptableObject.CreateInstance<SaveSystemSettings>();
            testSettings.StorageSettings = StorageSettings;
            testSettings.SaveReader = SaveReaderOverride;
            testSettings.SaveWriter = SaveWriterOverride;

            SaveSystemSettings baseSettings = Resources.Load<SaveSystemSettings>(SaveSysConstants.PathToSaveSysSettings);

            if (MainAppliersOverride != null)
            {
                testSettings.MainAppliers = MainAppliersOverride;
            }
            else if (baseSettings != null)
            {
                testSettings.MainAppliers = baseSettings.MainAppliers;
            }

            if (MainCodecsOverride != null)
            {
                testSettings.MainCodecs = MainCodecsOverride;
            }
            else if (baseSettings != null)
            {
                testSettings.MainCodecs = baseSettings.MainCodecs;
            }

            return testSettings;
        }
    }
}