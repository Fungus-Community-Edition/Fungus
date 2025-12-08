using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.SaveSys.EditorUtils
{
    /// <summary>
    /// Discovers and caches valid SaveSys types (readers, writers, appliers).
    /// Provides lookup dictionaries for dropdowns and applier instances.
    /// </summary>
    public sealed class SaveSysSettingsTypeCache
    {
        private readonly IList<Type> _validReaderTypes = new List<Type>();
        private readonly IList<Type> _validWriterTypes = new List<Type>();
        private readonly IList<Type> _validMainApplierTypes = new List<Type>();
        private readonly Dictionary<string, ISaveDataApplier> _validMainApplierChoices =
            new Dictionary<string, ISaveDataApplier>();
        private readonly IList<Type> _validCodecTypes = new List<Type>();
        private readonly Dictionary<string, IMainSaveCodec> _validMainCodecChoices =
            new Dictionary<string, IMainSaveCodec>();

        private static readonly Type ScriptableObjType = typeof(ScriptableObject);
        private static readonly Type ReaderInterface = typeof(ISaveReader);
        private static readonly Type WriterInterface = typeof(ISaveWriter);
        private static readonly Type ApplierInterface = typeof(ISaveDataApplier);

        public IReadOnlyList<Type> ReaderTypes => (IReadOnlyList<Type>)_validReaderTypes;
        public IReadOnlyList<Type> WriterTypes => (IReadOnlyList<Type>)_validWriterTypes;
        public IReadOnlyList<Type> MainApplierTypes => (IReadOnlyList<Type>)_validMainApplierTypes;
        public IReadOnlyDictionary<string, ISaveDataApplier> MainApplierChoices => _validMainApplierChoices;
        public IReadOnlyList<Type> MainCodecTypes => (IReadOnlyList<Type>)_validCodecTypes;
        public IReadOnlyDictionary<string, IMainSaveCodec> MainCodecChoices => _validMainCodecChoices;

        public void Refresh()
        {
            SaveReaderTypeRegistry.DiscoverAndRegister();
            SaveWriterTypeRegistry.DiscoverAndRegister();
            SaveDataApplierTypeRegistry.DiscoverAndRegister();
            SaveDataCodecTypeRegistry.DiscoverAndRegister();

            ClearCaches();
            PopulateReaderTypes();
            PopulateWriterTypes();
            PopulateMainApplierTypesAndChoices();
            PopulateMainCodecTypesAndChoices();
        }

        private void ClearCaches()
        {
            _validReaderTypes.Clear();
            _validWriterTypes.Clear();
            _validMainApplierTypes.Clear();
            _validMainApplierChoices.Clear();
        }

        private void PopulateReaderTypes()
        {
            var readers = SaveReaderTypeRegistry.Types
                .Where(readerType => !readerType.Name.Contains("Test") &&
                            ScriptableObjType.IsAssignableFrom(readerType) &&
                            ReaderInterface.IsAssignableFrom(readerType))
                .ToList();
            _validReaderTypes.AddRange(readers);
        }

        private void PopulateWriterTypes()
        {
            var writers = SaveWriterTypeRegistry.Types
                .Where(writerType => !writerType.Name.Contains("Test") &&
                            ScriptableObjType.IsAssignableFrom(writerType) &&
                            WriterInterface.IsAssignableFrom(writerType))
                .ToList();
            _validWriterTypes.AddRange(writers);
        }

        private void PopulateMainApplierTypesAndChoices()
        {
            List<Type> appliers = SaveDataApplierTypeRegistry.Types
                .Where(IsValidApplierType)
                .ToList();

            _validMainApplierTypes.AddRange(appliers);

            foreach (var applierType in _validMainApplierTypes)
            {
                string baseDisplayName = GetDisplayName(applierType);
                string assetName = $"Generated_{baseDisplayName}";
                assetName = assetName.Replace(" ", "_");

                var applierInstance = SOUtils.GetOrCreateScriptableObject(applierType,
                    whereAppliersShouldGo,
                    assetName);

                // We already know that these instances inherit the right interface, so...
                _validMainApplierChoices[baseDisplayName] = (ISaveDataApplier)applierInstance;
            }
        }

        private void PopulateMainCodecTypesAndChoices()
        {
            List<Type> codecs = SaveDataCodecTypeRegistry.Types
                .Where(codecType => !codecType.Name.Contains("Test") &&
                            ScriptableObjType.IsAssignableFrom(codecType) &&
                            CodecInterface.IsAssignableFrom(codecType))
                .ToList();
            _validCodecTypes.AddRange(codecs);

            foreach (var codecType in _validCodecTypes)
            {
                string baseDisplayName = GetDisplayName(codecType);
                string assetName = $"Generated_{baseDisplayName}";
                assetName = assetName.Replace(" ", "_");
                var codecInstance = SOUtils.GetOrCreateScriptableObject(codecType,
                    whereCodecsShouldGo,
                    assetName);
                // We already know that these instances inherit the right interface, so...
                _validMainCodecChoices[baseDisplayName] = (IMainSaveCodec)codecInstance;
            }
        }

        private static readonly Type CodecInterface = typeof(IMainSaveCodec);

        private static bool IsValidApplierType(Type type)
        {
            return !type.Name.Contains("Test") &&
                    ScriptableObjType.IsAssignableFrom(type) &&
                    ApplierInterface.IsAssignableFrom(type);
        }

        private static readonly string whereAppliersShouldGo = "SaveSys/SaveAppliers";
        private static readonly string whereCodecsShouldGo = "SaveSys/SaveCodecs";

        private static string GetDisplayName(Type type)
        {
            return SaveSysTypeUtils.GetDisplayName(type);
        }
    }
}