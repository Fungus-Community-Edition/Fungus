using UnityEngine;
using Amanita.VScripting;

namespace Amanita.SaveSys
{
    [VarCodec(true, typeof(BooleanVariable), typeof(BoolMuscariable))]
    public class BooleanVarCodec : IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        public int Order => 0;

        public virtual bool NeedsInput => true;

        public virtual bool CanHandle(object toMakeFrom) =>
            CanHandle(toMakeFrom as IVariable);
        public virtual bool CanHandle(IVariable variable) =>
            variable is IVariable<bool>;
        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(BooleanVariable) ||
            typeName == nameof(BoolMuscariable);
        public virtual bool CanHandle(VariableSaveData saveData) =>
            CanHandle(saveData.VarTypeName);

        public virtual VariableSaveData EncodeToSave(IVariable variable)
        {
            VariableSaveData result = new()
            {
                VarTypeName = variable.GetType().Name,
                ItemId = variable.ItemId,
                Key = variable.Key,
                Value = EncodeToString(variable)
            };
            return result;
        }

        public virtual string EncodeToString(IVariable toEncode)
        {
            // Try direct cast first
            IVariable<bool> booleanVar = toEncode as IVariable<bool>;
            if (booleanVar != null)
            {
                return booleanVar.Value.ToString();
            }

            // Fallback: check type name and use reflection
            string varTypeName = toEncode.GetType().Name;
            if (varTypeName == nameof(BooleanVariable) || varTypeName == nameof(BoolMuscariable))
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

        public virtual void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            ApplyState(variable, saveData.Value);
        }

        public virtual void ApplyState(IVariable toDecode, object data)
        {
            if (data is string strData)
            {
                ApplyState(toDecode, strData);
            }
            else
            {
                Debug.LogError($"Data type {data.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public virtual void ApplyState(IVariable toDecode, string data)
        {
            IVariable<bool> booleanVar = toDecode as IVariable<bool>;
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