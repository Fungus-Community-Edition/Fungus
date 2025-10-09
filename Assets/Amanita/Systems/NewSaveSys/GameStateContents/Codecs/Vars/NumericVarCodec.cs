using System;
using UnityEngine;
using Amanita.VScripting;
using System.Linq;

namespace Amanita.SaveSys
{
    public class NumericVarCodec : IVarCodec
    {
        public virtual bool CanHandle(IVariable variable) =>
            supportedVarTypes.Contains(variable.GetType());

        protected static Type[] supportedVarTypes = new Type[]
        {
            typeof(IntegerVariable),
            typeof(FloatVariable),
            typeof(IntMuscariable),
            typeof(FloatMuscariable),
        };

        public virtual bool CanHandle(string typeName) =>
            supportedVarTypes.Any(type => type.Name == typeName);

        public virtual bool CanHandle(VariableSaveData saveData) =>
            CanHandle(saveData.VarTypeName);

        public virtual string EncodeToString(IVariable variable) => variable switch
        {
            IVariable<int> intVar => intVar.Value.ToString(),
            IVariable<float> floatVar => floatVar.Value.ToString(roundTripFormat),
            _ => throw new InvalidOperationException($"Variable type {variable.GetType()} is not supported for encoding in NumericVarCodec.")
        };

        protected static string roundTripFormat = "R";
        // ^ This is to make sure that when we convert a float to a string and then
        // back to a float, we get the exact same value.
        // We want to decode things as accurately as possible, so...

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
            if (variable is IVariable<int> intVar)
                intVar.Value = int.Parse(data);
            else if (variable is IVariable<float> floatVar)
                floatVar.Value = float.Parse(data);
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in NumericVarEncoder.");
            }
        }

        public virtual void Decode(IVariable variable, VariableSaveData saveData)
        {
            bool validVarType = variable is IVariable<int> ||
                variable is IVariable<float>;

            if (!validVarType)
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in NumericVarEncoder.");
                return;
            }

            Decode(variable, saveData.Value);
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