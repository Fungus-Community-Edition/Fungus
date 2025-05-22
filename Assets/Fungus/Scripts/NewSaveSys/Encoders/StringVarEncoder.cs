using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding string data types.
    /// </summary>
    [System.Serializable]
    public class StringVarEncoder : IVarEncoder
    {
        public virtual bool CanHandle(Variable variable) =>
            variable is StringVariable;
        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(StringVariable);

        public virtual bool CanHandle(VariableSaveData saveData) =>
            CanHandle(saveData.VarTypeName);

        public virtual string EncodeToString(Variable variable) => ((StringVariable)variable).Value;

        public virtual VariableSaveData Encode(Variable variable)
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
        public virtual void Decode(Variable variable, string data)
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

        public virtual void Decode(Variable variable, VariableSaveData saveData)
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
    }
}