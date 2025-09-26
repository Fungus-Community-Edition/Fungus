using UnityEngine;
using Amanita.VScripting;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding string data types.
    /// </summary>
    [System.Serializable]
    public class StringVarCodec : IVarCodec
    {
        public virtual bool CanHandle(IVariable variable) =>
            variable is StringVariable;
        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(StringVariable);

        public virtual bool CanHandle(VariableSaveData saveData) =>
            CanHandle(saveData.VarTypeName);

        public virtual string EncodeToString(IVariable variable) => ((StringVariable)variable).Value;

        public virtual VariableSaveData EncodeToSave(IVariable variable)
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
        public virtual void Decode(IVariable variable, string data)
        {
            if (variable is StringVariable strVar)
            {
                strVar.Value = data;
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public virtual void Decode(IVariable variable, VariableSaveData saveData)
        {
            if (saveData.VarTypeName != nameof(StringVariable))
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in {this.GetType().Name}.");
                return;
            }

            if (variable is StringVariable strVar)
            {
                strVar.Value = saveData.Value;
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
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