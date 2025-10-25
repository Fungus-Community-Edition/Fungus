using Amanita.VScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Amanita.FSExt;
using FullSerializer.Internal;
using FullSerializer;

namespace Amanita.SaveSys
{
    public class VariableSourceAssetSaveCodec : SaveCodec<VariableSourceAsset, VariableSourceAssetSaveData>,
        IMainSaveCodec, IMainSaveDataProducer
    {
        [SerializeField] protected ScriptableObject[] varCodecs = new ScriptableObject[0];

        public virtual void Init()
        {
            _cachedVsas = Resources.LoadAll<VariableSourceAsset>("").ToList();
        }

        protected IList<VariableSourceAsset> _cachedVsas;

        public virtual void RegisterVarCodec(IVarCodec codec)
        {
            if (codec == null)
            {
                Debug.LogError("Cannot register a null codec.");
                return;
            }
            if (validCodecs.Contains(codec))
            {
                Debug.LogWarning($"Codec {codec.GetType().Name} is already registered.");
                return;
            }
            validCodecs.Add(codec);
        }

        protected IList<IVarCodec> validCodecs = new List<IVarCodec>();

        protected virtual void OnEnable()
        {
            RefreshValidCodecs();
        }

        protected virtual void RefreshValidCodecs()
        {
            validCodecs.Clear();
            for (int i = 0; i < varCodecs.Length; i++)
            {
                ScriptableObject toCheck = varCodecs[i];
                if (toCheck is not IVarCodec && toCheck != null)
                {
                    string name = toCheck.name;
                    Debug.LogError($"Element at index {i} ({name}) in varCodecs is not an IVarCodec. " +
                        $"Please fix this.");
                }
                else if (toCheck is IVarCodec codecFound)
                {
                    validCodecs.Add(codecFound);
                }
            }
        }

        public override bool CanHandle(string typeName)
        {
            return typeName == typeof(VariableSourceAssetSaveData).Name;
        }

        public override VariableSourceAssetSaveData EncodeToSave(VariableSourceAsset toCreateFrom)
        {
            if (!toCreateFrom.IncludeInSaves)
            {
                Debug.LogWarning($"Flowchart {toCreateFrom.name} is set to not be included in saves. Thus, it shall not be encoded.");
                return null;
            }

            IList<VariableSaveData> savedVars = SaveVars(toCreateFrom);
            VariableSourceAssetSaveData result = new VariableSourceAssetSaveData();
            result.AssetId = toCreateFrom.AssetId;
            result.SavedVars = savedVars;
            return result;
        }

        protected virtual IList<VariableSaveData> SaveVars(VariableSourceAsset toCreateFrom)
        {
            IList<VariableSaveData> result = new List<VariableSaveData>();

            var variables = toCreateFrom.Variables;
            int count = variables.Count;
            if (count == 0 || !toCreateFrom.IncludeInSaves)
            {
                // Do nothing and just return an empty list later in this func
            }
            else
            {
                foreach (IVariable varEl in variables)
                {
                    IVarCodec forThisVar = FindCodecFor(varEl);
                    if (forThisVar == null)
                    {
                        Debug.LogWarning($"No codec found for variable type: {varEl.GetType().Name}");
                        continue;
                    }

                    VariableSaveData varSave = forThisVar.EncodeToSave(varEl);
                    if (varSave == null)
                    {
                        Debug.LogError($"Failed to encode variable: {varEl.Key}");
                        continue;
                    }

                    result.Add(varSave);
                }
            }

            return result;
        }

        protected virtual IVarCodec FindCodecFor(IVariable variable)
        {
            IVarCodec result = validCodecs.Where((elem) => elem.CanHandle(variable)).FirstOrDefault();
            return result;
        }

        public IList<SaveData> FindAndCreateAll(Action<IList<SaveData>> onComplete = null)
        {
            // TODO: Implement an init method for save codecs so that we only need to load
            // certain things once upon startup, rather than every time we encode.
            IList<VariableSourceAsset> toEncode = Resources.LoadAll<VariableSourceAsset>("");
            IList<SaveData> result = new List<SaveData>();

            for (int i = 0; i < toEncode.Count; i++)
            {
                VariableSourceAsset asset = toEncode[i];
                var data = EncodeToSave(asset);
                if (data != null)
                {
                    result.Add(data);
                }
            }

            onComplete?.Invoke(result);
            return result;
        }

        public override VariableSourceAssetSaveData Decode(string rawText)
        {
            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                VariableSourceAssetSaveData result = serializer.FromJson<VariableSourceAssetSaveData>(rawText);
                return result;
            }
        }
    }
}