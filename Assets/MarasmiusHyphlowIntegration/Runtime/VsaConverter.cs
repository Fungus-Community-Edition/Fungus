using AtMycelia.Hyphlow;
using FullSerializer;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Amanita.SaveSys
{
    public class VSAConverter : fsDirectConverter<VariableSourceAsset>
    {
        protected override fsResult DoSerialize(VariableSourceAsset model, Dictionary<string, fsData> serialized)
        {
            VariableSourceAssetSaveData saveData = new VariableSourceAssetSaveData();
            saveData.UniqueId = model.UniqueId;
            saveData.SavedVars = (IList<VariableSaveData>)model.Variables;
            SerializeMember(serialized, null, "saveData", saveData);
            return fsResult.Success;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref VariableSourceAsset model)
        {
            // We assume that the data contains a VariableSourceAssetSaveData under "saveData".
            fsData saveDataData;
            if (data.TryGetValue("saveData", out saveDataData))
            {
                fsResult result;
                VariableSourceAssetSaveData saveData = null;
                result = DeserializeMember(data, null, "saveData", out saveData);
                if (result.Failed)
                {
                    return result;
                }
                // Now, we can reconstruct the VariableSourceAsset from the save data.
                model = ScriptableObject.CreateInstance<VariableSourceAsset>();
                model.IncludeInSaves = true;
                model.Refresh();

                model.UniqueId = saveData.UniqueId;
                foreach (var varSave in saveData.SavedVars)
                {
                    Muscariable var = VariableFactory.CreateByVarTypeName(varSave.VarTypeName, null);
                    model.AddVariable(var);
                }
                return fsResult.Success;
            }
            else
            {
                return fsResult.Fail("No 'saveData' found in data for VariableSourceAsset deserialization.");
            }
        }
    }

}