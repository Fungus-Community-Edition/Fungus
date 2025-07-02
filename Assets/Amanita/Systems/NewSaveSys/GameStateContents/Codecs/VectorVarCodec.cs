using UnityEngine;
using System;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding Vector2 and Vector3 data types.
    /// </summary>
    [System.Serializable]
    public class VectorVarCodec : IVarCodec
    {
        public virtual bool CanHandle(Variable variable) =>
            variable is Vector2Variable || variable is Vector3Variable;

        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(Vector2Variable) || typeName == nameof(Vector3Variable);

        public virtual bool CanHandle(VariableSaveData saveData)
        {            
            return saveData.VarTypeName == nameof(Vector2Variable) ||
                saveData.VarTypeName == nameof(Vector3Variable);
        }

        public virtual string EncodeToString(Variable variable) => variable switch
            {
            Vector2Variable vector2Var => $"{vector2Var.Value.x},{vector2Var.Value.y}",
            Vector3Variable vector3Var => $"{vector3Var.Value.x},{vector3Var.Value.y},{vector3Var.Value.z}",
            _ => throw new InvalidOperationException($"Variable type {variable.GetType()} is not supported for encodng in {this.GetType().Name}")
        };

        public virtual void Decode(Variable variable, string data)
        {
            if (variable is Vector2Variable vecTwoVar)
            {
                string[] parts = data.Split(',');
                float xVal = 0, yVal = 0;

                bool validVecTwoFormat = parts.Length == 2 &&
                    float.TryParse(parts[0], out xVal) &&
                    float.TryParse(parts[1], out yVal);
                if (!validVecTwoFormat)
                    throw new FormatException($"Invalid Vector2 format: {data}");

                vecTwoVar.Value = new Vector2(xVal, yVal);
            }
            else if (variable is Vector3Variable vecThreeVar)
            {
                string[] parts = data.Split(',');
                float xVal = 0, yVal = 0, zVal = 0;

                bool validVecThreeFormat = parts.Length == 3 &&
                    float.TryParse(parts[0], out xVal) &&
                    float.TryParse(parts[1], out yVal) &&
                    float.TryParse(parts[2], out zVal);
                if (!validVecThreeFormat)
                    throw new FormatException($"Invalid Vector3 format: {data}");

                vecThreeVar.Value = new Vector3(xVal, yVal, zVal);
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }
    
        public virtual void Decode(Variable variable, VariableSaveData saveData)
        {
            bool validVarType = saveData.VarTypeName == nameof(Vector2Variable) ||
                saveData.VarTypeName == nameof(Vector3Variable);
            if (!validVarType)
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in {this.GetType().Name}.");
                return;
            }
            Decode(variable, saveData.Value);
        }
        public virtual VariableSaveData EncodeToSave(Variable variable)
        {
            string data = EncodeToString(variable);
            if (string.IsNullOrEmpty(data))
            {
                Debug.LogError($"Failed to encode variable {variable} in {this.GetType().Name}.");
                return VariableSaveData.Null;
            }

            VariableSaveData result = new()
            {
                VarTypeName = variable.GetType().Name,
                ItemID = variable.ItemID,
                Key = variable.Key,
                Value = data,
            };

            return result;
        }

        public virtual T DecodeTo<T>(string data)
        {
            T result = default;
            string[] parts;
            float x = 0, y = 0, z = 0;
            bool isVecTwo = typeof(T) == typeof(Vector2);
            bool isVecThree = typeof(T) == typeof(Vector3);
            bool validTypeArg = isVecThree || isVecTwo;

            if (!validTypeArg)
            {
                string errorMessage = $"Cannot decode to type {typeof(T).Name}. Only Vector2 and Vector3 are supported.";
                throw new InvalidCastException(errorMessage);
            }

            parts = data.Split(',');

            if (typeof(T) == typeof(Vector2))
            {
                if (parts.Length != 2)
                {
                    string errorMessage = $"Invalid Vector2 format: {data}. Expected format: 'x,y' where x and y are floats.";
                    throw new FormatException(errorMessage);
                }
            }
            else if (typeof(T) == typeof(Vector3))
            {
                if (parts.Length != 3)
                {
                    string errorMessage = $"Invalid Vector3 format: {data}. Expected format: 'x,y,z' where x, y, and z are floats.";
                    throw new FormatException(errorMessage);
                }
            }

            x = float.Parse(parts[0]);
            y = float.Parse(parts[1]);

            if (isVecThree)
            {
                z = float.Parse(parts[2]);
                result = (T)(object)new Vector3(x, y, z);
            }
            else if (isVecTwo)
            {
                result = (T)(object)new Vector2(x, y);
            }

            return result;
        }
    }
}