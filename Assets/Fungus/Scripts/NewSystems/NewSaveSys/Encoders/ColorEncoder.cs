using Fungus;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class ColorEncoder : IVarEncoder
    {
        public virtual bool CanHandle(Variable variable) =>
            variable is ColorVariable;

        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(ColorVariable);

        public virtual string Encode(Variable toEncode)
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
    }
}