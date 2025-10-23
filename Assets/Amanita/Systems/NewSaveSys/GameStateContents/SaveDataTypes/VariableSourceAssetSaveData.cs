using UnityEngine;
using System.Collections.Generic;
using FullSerializer;

namespace Amanita.SaveSys
{
    public class VariableSourceAssetSaveData : SaveData
    {
        [SerializeField] protected string assetId = string.Empty;
        [SerializeField] protected List<VariableSaveData> savedVars = new List<VariableSaveData>();

        public virtual string AssetId
        {
            get => assetId;
            set => assetId = value;
        }

        public virtual IList<VariableSaveData> SavedVars
        {
            get => savedVars;
            set
            {
                savedVars.Clear();
                savedVars.AddRange(value);
            }
        }

        public override SaveDataUnit Serialized()
        {
            fsSerializer serializer = SaveSystem.DefaultSerializer;
            string json = serializer.ToJson(this, true);
            string typeName = GetType().Name;
            SaveDataUnit newItem = new(typeName, json);
            return newItem;
        }

        public virtual T GetVarValue<T>(string varName)
        {
            T result = default;
            VariableSaveData foundVar = savedVars.Find(v => v.VarName == varName);

            if (foundVar != null)
            {
                IVarCodec codec = CodecRegistry.GetCodec(foundVar.VarTypeName);
                if (codec != null)
                {
                    result = codec.DecodeTo<T>(foundVar.Value);
                }
                else
                {
                    Debug.LogError($"Codec for variable type '{foundVar.VarTypeName}' not found.");
                }
            }

            return result;
        }
    }
}