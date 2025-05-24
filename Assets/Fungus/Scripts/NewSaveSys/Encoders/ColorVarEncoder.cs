using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class ColorVarEncoder : IVarEncoder, ISaveEncoder<Variable, VariableSaveData>
    {
        public virtual int Priority => 0;
        public bool CanHandle(object toMakeFrom)
        {
            return CanHandle(toMakeFrom as Variable);
        }
        public virtual bool CanHandle(Variable variable) =>
            variable is ColorVariable;

        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(ColorVariable);

        public virtual bool CanHandle(VariableSaveData saveData) =>
            CanHandle(saveData.VarTypeName);

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

        public virtual string EncodeToString(Variable toEncode)
        {
            ColorVariable colorVar = toEncode as ColorVariable;
            if (colorVar == null)
            {
                Debug.LogError($"Variable type {toEncode.GetType()} is not supported for encoding in ColorEncoder.");
                return string.Empty;
            }

            Color color = colorVar.Value;
            string encodedColor = $"{color.r},{color.g},{color.b},{color.a}";
            return encodedColor;
        }

        public virtual void Decode(Variable toDecode, string data)
        {
            ColorVariable colorVar = toDecode as ColorVariable;
            if (colorVar == null)
            {
                Debug.LogError($"Variable type {toDecode.GetType()} is not supported for decoding in ColorEncoder.");
                return;
            }

            string[] colorComponents = data.Split(',');
            float r = 0, g = 0, b = 0, a = 0;

            bool isFormatValid = float.TryParse(colorComponents[0], out r) &&
                float.TryParse(colorComponents[1], out g) &&
                float.TryParse(colorComponents[2], out b) &&
                float.TryParse(colorComponents[3], out a);
            if (!isFormatValid)
            {
                Debug.LogError($"Invalid color data format: {data}");
                return;
            }
            colorVar.Value = new Color(r, g, b, a);
        }

        public virtual void Decode(Variable variable, VariableSaveData saveData)
        {
            if (saveData.VarTypeName != nameof(ColorVariable))
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in {this.GetType().Name}.");
                return;
            }

            if (variable is ColorVariable colorVar)
            {
                Decode(colorVar, saveData.Value);
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public virtual SaveDataUnit Encode(object toMakeFrom = null)
        {
            VariableSaveData saveData = Encode(toMakeFrom as Variable);
            if (saveData == null)
            {
                Debug.LogError($"Failed to encode {toMakeFrom} as VariableSaveData in {this.GetType().Name}.");
                return null;
            }

            SaveDataUnit unit = saveData.Serialized();
            return unit;
        }

        public IList<SaveDataUnit> EncodeMulti(IList<object> toMakeFrom)
        {
            IList<SaveDataUnit> result = new List<SaveDataUnit>();
            foreach (Variable varElem in toMakeFrom)
            {
                if (CanHandle(varElem))
                {
                    SaveDataUnit unit = Encode(varElem).Serialized();
                    result.Add(unit);
                }
                else
                {
                    Debug.LogWarning($"Variable type {varElem.GetType()} is not supported for encoding in {this.GetType().Name}.");
                }
            }

            return result;
        }

        
    }
}