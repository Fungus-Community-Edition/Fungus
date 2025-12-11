using Amanita.VScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Amanita.FSExt;
using FullSerializer;
using Amanita.Utils;

namespace Amanita.SaveSys
{
    [SaveSysDisplayName("VSA Codec (Amanita Default)")]
    public class VariableSourceAssetSaveCodec : SaveCodec<VariableSourceAsset, VariableSourceAssetSaveData>,
        IMainSaveCodec, IMainSaveDataProducer
    {
        public virtual void PreInstallInit()
        {
            _cachedVsas = Resources.LoadAll<VariableSourceAsset>("").ToList();
        }

        protected IList<VariableSourceAsset> _cachedVsas;

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
            result.UniqueId = toCreateFrom.UniqueId;
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
                    IVarCodec forThisVar = VarCodecRegistry.GetCodec(varEl);
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

        public IList<SaveData> FindAndCreateAll(Action<IList<SaveData>> onComplete = null)
        {
            // TODO: Implement an init method for save codecs so that we only need to load
            // certain things once upon startup, rather than every time we encode.
            IList<VariableSourceAsset> toEncode = _cachedVsas;
            IList<SaveData> result = new List<SaveData>();
            UnityThreadUtil.RunOnMainThread(() =>
            {
                // Need the following on the main thread since the codecs might need to
                // touch values of UnityObjs.
                for (int i = 0; i < toEncode.Count; i++)
                {
                    VariableSourceAsset asset = toEncode[i];
                    var data = EncodeToSave(asset);
                    if (data != null)
                    {
                        result.Add(data);
                    }
                }
            });
            
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