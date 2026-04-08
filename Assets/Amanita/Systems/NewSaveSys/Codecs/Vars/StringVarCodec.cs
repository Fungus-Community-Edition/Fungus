using UnityEngine;
using System.Collections.Generic;
using AtMycelia.Amanita.VScripting;
using Type = System.Type;

namespace AtMycelia.Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding string data types.
    /// </summary>
    [VarCodec(true, typeof(StringVariable), typeof(StringMuscariable))]
    public class StringVarCodec : VarCodec, IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        public int Order => 0;
        protected override IReadOnlyList<Type> SupportedContentTypes => (IReadOnlyList<Type>)_supportedContentTypes;

        private static readonly IList<Type> _supportedContentTypes = new Type[]
        {
            typeof(string),
        };

        public override string EncodeToString(IVariable variable) => ((IVariable<string>)variable).Value;

        public override VariableSaveData EncodeToSave(IVariable variable)
        {
            string val = EncodeToString(variable);
            VariableSaveData result = VariableSaveData.From(variable, val);

            return result;
        }

        public override void ApplyState(IVariable variable, string data)
        {
            if (variable is IVariable<string> strVar)
            {
                strVar.Value = data;
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public override void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            if (variable is not IVariable<string> strVar)
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in {this.GetType().Name}.");
                return;
            }

            strVar.Value = saveData.Value;
        }

        public override T DecodeTo<T>(string data)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)data;
            }
            else
            {
                Debug.LogError($"Cannot decode string to type {typeof(T).Name}.");
                return default;
            }
        }
    }
}