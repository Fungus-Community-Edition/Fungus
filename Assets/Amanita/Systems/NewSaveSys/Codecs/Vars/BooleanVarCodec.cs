using AtMycelia.Amanita.VScripting;
using System.Collections.Generic;
using UnityEngine;
using Type = System.Type;

namespace AtMycelia.Amanita.SaveSys
{
    [VarCodec(true, typeof(BooleanVariable), typeof(BoolMuscariable))]
    public class BooleanVarCodec : VarCodec, IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        public int Order => 0;
        protected override IReadOnlyList<Type> SupportedContentTypes => (IReadOnlyList<Type>)_supportedContentTypes;

        private static readonly IList<Type> _supportedContentTypes = new Type[]
        {
            typeof(bool),
        };
        public virtual bool NeedsInput => true;

        public override string EncodeToString(IVariable toEncode)
        {
            if (!CanHandle(toEncode))
            {
                Debug.LogError($"Variable type {toEncode.GetType()} is not supported for encoding in {this.GetType().Name}.");
                return string.Empty;
            }
            // Try direct cast first
            IVariable<bool> booleanVar = toEncode as IVariable<bool>;
            return booleanVar.Value.ToString();
            
            //// Fallback: check type name and use reflection
            //string varTypeName = toEncode.GetType().Name;
            //if (varTypeName == nameof(BooleanVariable) || varTypeName == nameof(BoolMuscariable))
            //{
            //    var valueProp = toEncode.GetType().GetProperty("Value");
            //    if (valueProp != null)
            //    {
            //        var value = valueProp.GetValue(toEncode);
            //        return value?.ToString() ?? string.Empty;
            //    }
            //}
        }

        public override void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            ApplyState(variable, saveData.Value);
        }

        public override void ApplyState(IVariable toDecode, string data)
        {
            if (!CanHandle(toDecode))
            {
                Debug.LogError($"Variable type {toDecode.GetType()} is not supported for decoding in {this.GetType().Name}.");
                return;
            }
            IVariable<bool> booleanVar = toDecode as IVariable<bool>;
            booleanVar.Value = DecodeTo<bool>(data);
        }

        public override T DecodeTo<T>(string data)
        {
            if (typeof(T) != typeof(bool))
            {
                throw new System.InvalidCastException($"Cannot decode to type {typeof(T).Name} from boolean data.");
            }

            bool value = false;
            if (!bool.TryParse(data, out value))
            {
                Debug.LogError($"Failed to decode boolean value from string: {data}");
                return default;
            }
            return (T)(object)value;
        }

    }
}