using UnityEngine;

namespace Amanita.SaveSys
{
    public class BooleanVarCodec : IVarCodec
    {
        public int Priority => 0;

        public virtual bool NeedsInput => true;

        public virtual bool CanHandle(object toMakeFrom) =>
            CanHandle(toMakeFrom as Variable);
        public virtual bool CanHandle(Variable variable) =>
            variable is BooleanVariable;
        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(BooleanVariable);
        public virtual bool CanHandle(VariableSaveData saveData) =>
            CanHandle(saveData.VarTypeName);

        public virtual VariableSaveData EncodeToSave(Variable variable)
        {
            VariableSaveData result = new()
            {
                VarTypeName = variable.GetType().Name,
                UniqueID = variable.UniqueId,
                Key = variable.Key,
                Value = EncodeToString(variable)
            };
            return result;
        }

        public virtual string EncodeToString(Variable toEncode)
        {
            BooleanVariable booleanVar = toEncode as BooleanVariable;
            if (booleanVar == null)
            {
                Debug.LogError($"Variable type {toEncode.GetType()} is not supported for encoding in {this.GetType().Name}.");
                return string.Empty;
            }
            bool value = booleanVar.Value;
            string encodedValue = value.ToString();
            return encodedValue;
        }

        public virtual void Decode(Variable variable, VariableSaveData saveData)
        {
            Decode(variable, saveData.Value);
        }

        public virtual void Decode(Variable toDecode, string data)
        {
            BooleanVariable booleanVar = toDecode as BooleanVariable;
            if (booleanVar == null)
            {
                Debug.LogError($"Variable type {toDecode.GetType()} is not supported for decoding in {this.GetType().Name}.");
                return;
            }

            bool value = false;
            if (!bool.TryParse(data, out value))
            {
                Debug.LogError($"Failed to decode boolean value from string: {data}");
                return;
            }
            booleanVar.Value = value;
        }


    }
}