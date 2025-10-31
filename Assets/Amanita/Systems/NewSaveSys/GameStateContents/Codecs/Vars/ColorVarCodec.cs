using UnityEngine;
using Amanita.VScripting;
using System;
using FullSerializer;
using Amanita.FSExt;

namespace Amanita.SaveSys
{
    [VarCodec(true, typeof(ColorVariable), typeof(ColorMuscariable))]
    public class ColorVarCodec : IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
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
                ItemId = variable.ItemId,
                Key = variable.Key,
                Value = EncodeToString(variable)
            };
            return result;
        }

        public virtual string EncodeToString(IVariable toEncode)
        {
            if (toEncode is not IVariable<Color> colorVar)
            {
                Debug.LogError($"Variable type {toEncode.GetType()} is not supported for encoding in ColorEncoder.");
                return string.Empty;
            }

            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                ColorState colorState = ColorState.From(colorVar.Value);
                string json = serializer.ToJson(colorState);
                return json;
            }
        }

        public virtual void ApplyState(IVariable toDecode, object data)
        {
            if (data is string strData)
            {
                ApplyState(toDecode, strData);
            }
            else if (data is VariableSaveData saveData)
            {
                ApplyState(toDecode, saveData);
            }
            else
            {
                Debug.LogError($"Data type {data.GetType()} is not supported for decoding in ColorEncoder.");
            }
        }

        public virtual void ApplyState(IVariable toDecode, string data)
        {
            if (toDecode is not IVariable<Color> colorVar)
            {
                Debug.LogError($"Variable type {toDecode.GetType()} is not supported for decoding in ColorEncoder.");
                return;
            }

            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                ColorState colorState = serializer.FromJson<ColorState>(data);
                colorVar.Value = colorState.ToColor();
            }
        }

        public virtual void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            if (saveData.VarTypeName != nameof(ColorVariable) &&
                saveData.VarTypeName != nameof(ColorMuscariable))
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in {this.GetType().Name}.");
                return;
            }

            if (variable is IVariable<Color> colorVar)
            {
                ApplyState(colorVar, saveData.Value);
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public virtual T DecodeTo<T>(string data)
        {
            if (typeof(T) != typeof(Color))
            {
                string errorMessage = $"Cannot decode to type {typeof(T)}. Only Color is supported.";
                throw new System.InvalidCastException(errorMessage);
            }

            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                ColorState colorState = serializer.FromJson<ColorState>(data);
                return (T)(object)colorState.ToColor();
            }
        }

    }

    [Serializable]
    public struct ColorState : IEquatable<ColorState>, IEquatable<Color>, IEquatable<Color32>
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static implicit operator Color(ColorState other)
        {
            return other.ToColor();
        }

        public static implicit operator ColorState(Color col)
        {
            return From(col);
        }

        public static implicit operator Color32(ColorState other)
        {
            return other.ToColor();
        }

        public static ColorState From(Color color)
        {
            return new ColorState(color);
        }

        public ColorState(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public readonly Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        public readonly bool Equals(ColorState other)
        {
            return r == other.r && g == other.g && b == other.b && a == other.a;
        }

        public readonly bool Equals(Color other)
        {
            return r == other.r && g == other.g && b == other.b && a == other.a;
        }

        public readonly bool Equals(Color32 other)
        {
            return Mathf.Approximately(r, other.r / 255f) &&
                   Mathf.Approximately(g, other.g / 255f) &&
                   Mathf.Approximately(b, other.b / 255f) &&
                   Mathf.Approximately(a, other.a / 255f);
        }
    }
}