using System;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class NumericVarCodec : IVarCodec
    {
        public virtual bool CanHandle(Variable variable) =>
            variable is IntegerVariable || variable is FloatVariable;

        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(IntegerVariable) || typeName == nameof(FloatVariable);

        public virtual bool CanHandle(VariableSaveData saveData) =>
            CanHandle(saveData.VarTypeName);

        public virtual string EncodeToString(Variable variable) => variable switch
        {
            IntegerVariable intVar => intVar.Value.ToString(),
            FloatVariable floatVar => floatVar.Value.ToString(roundTripFormat),
            _ => throw new InvalidOperationException($"Variable type {variable.GetType()} is not supported for encoding in NumericVarEncoder.")
        };

        protected static string roundTripFormat = "R";
        // ^ This is to make sure that when we convert a float to a string and then
        // back to a float, we get the exact same value.
        // We want to decode things as accurately as possible, so...

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

        public virtual void Decode(Variable variable, string data)
        {
            if (variable is IntegerVariable intVar)
                intVar.Value = int.Parse(data);
            else if (variable is FloatVariable floatVar)
                floatVar.Value = float.Parse(data);
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in NumericVarEncoder.");
            }
        }

        public virtual void Decode(Variable variable, VariableSaveData saveData)
        {
            bool validVarType = saveData.VarTypeName == nameof(IntegerVariable) ||
                saveData.VarTypeName == nameof(FloatVariable);

            if (saveData.VarTypeName != nameof(IntegerVariable) &&
                saveData.VarTypeName != nameof(FloatVariable))
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in NumericVarEncoder.");
                return;
            }

            if (variable is IntegerVariable intVar || variable is FloatVariable floatVar)
            {
                Decode(variable, saveData.Value);
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in NumericVarEncoder.");
            }
        }

        public virtual T DecodeTo<T>(string data)
        {
            T result = default;
            if (typeof(T) == typeof(int))
            {
                result = (T)(object)int.Parse(data);
            }
            else if (typeof(T) == typeof(float))
            {
                result = (T)(object)float.Parse(data, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                throw new InvalidOperationException($"Type {typeof(T)} is not supported for decoding in NumericVarEncoder.");
            }

            return result;
        }
    }
}