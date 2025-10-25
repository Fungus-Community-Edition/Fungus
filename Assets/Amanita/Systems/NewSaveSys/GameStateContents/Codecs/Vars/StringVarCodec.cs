using UnityEngine;
using Amanita.VScripting;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding string data types.
    /// </summary>
    [VarCodec(true, typeof(StringVariable), typeof(StringMuscariable))]
    public class StringVarCodec : IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        public virtual bool CanHandle(IVariable variable) =>
            variable is IVariable<string>;
        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(StringVariable) ||
            typeName == nameof(StringMuscariable);

        public virtual bool CanHandle(VariableSaveData saveData) =>
            CanHandle(saveData.VarTypeName);

        public virtual string EncodeToString(IVariable variable) => ((IVariable<string>)variable).Value;

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

        public virtual void ApplyState(IVariable variable, object data)
        {
            if (data is string strData)
            {
                ApplyState(variable, strData);
            }
            else if (data is VariableSaveData saveData)
            {
                ApplyState(variable, saveData);
            }
            else
            {
                Debug.LogError($"Data type {data.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public virtual void ApplyState(IVariable variable, string data)
        {
            if (variable is IVariable<string> strVar)
            {
                strVar.Value = data;
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public virtual void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            if (variable is not IVariable<string> strVar)
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in {this.GetType().Name}.");
                return;
            }

            strVar.Value = saveData.Value;
        }

        public virtual T DecodeTo<T>(string data)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)data;
            }
            else
            {
                Debug.LogError($"Cannot decode string to type {typeof(T).Name}.");
                return default;
            }
        }
    }
}