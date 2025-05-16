using Fungus;
using UnityEngine;
using System;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding Vector2 and Vector3 data types.
    /// </summary>
    [System.Serializable]
    public class VectorVarEncoder : IVarEncoder
    {
        public virtual bool CanHandle(Variable variable) =>
            variable is Vector2Variable || variable is Vector3Variable;

        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(Vector2Variable) || typeName == nameof(Vector3Variable);

        public virtual string Encode(Variable variable) => variable switch
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
    }
}