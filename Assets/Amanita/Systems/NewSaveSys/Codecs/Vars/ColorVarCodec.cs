using AtMycelia.Amanita.VScripting;
using AtMycelia.FSExt;
using AtMycelia.SaveSys;
using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Amanita.SaveSys
{
    [VarCodec(true, typeof(ColorVariable), typeof(ColorMuscariable))]
    public class ColorVarCodec : VarCodec, IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        protected override IReadOnlyList<Type> SupportedContentTypes => (IReadOnlyList<Type>)_supportedContentTypes;

        private static readonly IList<Type> _supportedContentTypes = new Type[]
        {
            typeof(Color)
        };

        public virtual int Priority => 0;
        public virtual bool NeedsInput => true;

        public override string EncodeToString(IVariable toEncode)
        {
            if (toEncode is not IVariable<Color> colorVar)
            {
                Debug.LogError($"Variable type {toEncode.GetType()} is not supported for encoding in ColorEncoder.");
                return string.Empty;
            }
            lock (Serializer)
            {
                ColorState colorState = ColorState.From(colorVar.Value);
                string json = Serializer.ToJson(colorState);
                return json;
            }
        }

        public override void ApplyState(IVariable toDecode, string data)
        {
            if (toDecode is not IVariable<Color> colorVar)
            {
                Debug.LogError($"Variable type {toDecode.GetType()} is not supported for decoding in ColorEncoder.");
                return;
            }

            fsSerializer serializer = SaveSystem.DefaultSerializer;
            lock (serializer)
            {
                ColorState colorState = serializer.FromJson<ColorState>(data);
                colorVar.Value = colorState.ToColor();
            }
        }

        public override void ApplyState(IVariable variable, VariableSaveData saveData)
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

        public override T DecodeTo<T>(string data)
        {
            if (typeof(T) != typeof(Color))
            {
                string errorMessage = $"Cannot decode to type {typeof(T)}. Only Color is supported.";
                throw new InvalidCastException(errorMessage);
            }

            lock (Serializer)
            {
                ColorState colorState = Serializer.FromJson<ColorState>(data);
                return (T)(object)colorState.ToColor();
            }
        }

    }

}