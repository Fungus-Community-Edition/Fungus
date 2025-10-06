using System.Collections.Generic;
using UnityEngine;
using Amanita.VScripting;

namespace Amanita.SaveSys
{
    public class ColorVarCodec : IVarCodec
    {
        public virtual System.Object ToMakeFrom { get; set; } = null;
        public virtual int Priority => 0;
        public virtual bool NeedsInput => true;
        public bool CanHandle(object toMakeFrom)
        {
            return CanHandle(toMakeFrom as IVariable);
        }
        public virtual bool CanHandle(IVariable variable) =>
            variable is IVariable<Color>;

        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(ColorVariable) ||
            typeName == nameof(ColorMuscariable);

        public virtual bool CanHandle(VariableSaveData saveData) =>
            CanHandle(saveData.VarTypeName);

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

        public virtual string EncodeToString(IVariable toEncode)
        {
            IVariable<Color> colorVar = toEncode as IVariable<Color>;
            if (colorVar == null)
            {
                Debug.LogError($"Variable type {toEncode.GetType()} is not supported for encoding in ColorEncoder.");
                return string.Empty;
            }

            Color color = colorVar.Value;
            string encodedColor = $"{color.r},{color.g},{color.b},{color.a}";
            return encodedColor;
        }

        public virtual void Decode(IVariable toDecode, string data)
        {
            IVariable<Color> colorVar = toDecode as IVariable<Color>;
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

        public virtual void Decode(IVariable variable, VariableSaveData saveData)
        {
            if (saveData.VarTypeName != nameof(ColorVariable) &&
                saveData.VarTypeName != nameof(ColorMuscariable))
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in {this.GetType().Name}.");
                return;
            }

            if (variable is IVariable<Color> colorVar)
            {
                Decode(colorVar, saveData.Value);
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public virtual SaveDataUnit EncodeToUnit()
        {
            VariableSaveData saveData = EncodeToSave(ToMakeFrom as Variable);
            if (saveData == null)
            {
                Debug.LogError($"Failed to encode {ToMakeFrom} as VariableSaveData in {this.GetType().Name}.");
                return null;
            }

            SaveDataUnit unit = saveData.Serialized();
            return unit;
        }

        public IList<SaveDataUnit> EncodeMultiSaves(IList<object> multipleToMakeFrom)
        {
            IList<SaveDataUnit> result = new List<SaveDataUnit>();
            foreach (Variable varElem in multipleToMakeFrom)
            {
                if (CanHandle(varElem))
                {
                    SaveDataUnit unit = EncodeToSave(varElem).Serialized();
                    result.Add(unit);
                }
                else
                {
                    Debug.LogWarning($"Variable type {varElem.GetType()} is not supported for encoding in {this.GetType().Name}.");
                }
            }

            return result;
        }

        public virtual T DecodeTo<T>(string data)
        {
            if (typeof(T) != typeof(Color))
            {
                string errorMessage = $"Cannot decode to type {typeof(T)}. Only Color is supported.";
                throw new System.InvalidCastException(errorMessage);
            }

            string[] colorComponents = data.Split(',');
            if (colorComponents.Length != 4)
            {
                string errorMessage = $"Invalid color data format: {data}. Expected format: 'r,g,b,a' where r, g, b, a are floats.";
                throw new System.FormatException(errorMessage);
            }
            float r = float.Parse(colorComponents[0]);
            float g = float.Parse(colorComponents[1]);
            float b = float.Parse(colorComponents[2]);
            float a = float.Parse(colorComponents[3]);
            T result = (T)(object)new Color(r, g, b, a);
            return result;
        }

    }
}