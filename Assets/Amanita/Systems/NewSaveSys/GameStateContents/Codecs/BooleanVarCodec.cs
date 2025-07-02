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
                ItemID = variable.ItemID,
                Key = variable.Key,
                Value = EncodeToString(variable)
            };
            return result;
        }

        public virtual string EncodeToString(Variable toEncode)
        {
            // Try direct cast first
            BooleanVariable booleanVar = toEncode as BooleanVariable;
            if (booleanVar != null)
            {
                return booleanVar.Value.ToString();
            }

            // Fallback: check type name and use reflection
            if (toEncode.GetType().Name == "BooleanVariable")
            {
                var valueProp = toEncode.GetType().GetProperty("Value");
                if (valueProp != null)
                {
                    var value = valueProp.GetValue(toEncode);
                    return value?.ToString() ?? string.Empty;
                }
            }

            Debug.LogError($"Variable type {toEncode.GetType()} is not supported for encoding in {this.GetType().Name}.");
            return string.Empty;
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

        public virtual T DecodeTo<T>(string data)
        {
            if (typeof(T) != typeof(bool))
            {
                throw new System.InvalidCastException($"Cannot decode to type {typeof(T).Name} from boolean data.");
            }

            bool value = false;
            if (!bool.TryParse(data, out value))
            {
                Debug.LogError($"Failed to decode boolean value from string: {data}");
                return default;
            }
            return (T)(object)value;
        }


    }
}