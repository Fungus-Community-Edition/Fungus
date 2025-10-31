using Amanita.SaveSys;
using Amanita.VScripting;
using UnityEngine;

namespace SaveSystemTests
{
    // Simple test VarCodec that handles GenericMuscariable (object) for strings and ints
    // We avoid mocks of variables/sources by using real VariableSourceAsset and Muscariable,
    // but provide this tiny codec so encode/decode can occur in tests.
    public class GenericVarCodec : ScriptableObject, IVarCodec
    {
        public bool CanHandle(IVariable variable)
            => variable is GenericMuscariable;

        public bool CanHandle(string typeName)
            => typeName == nameof(GenericMuscariable) || typeName == typeof(GenericMuscariable).FullName;

        public bool CanHandle(VariableSaveData variable)
            => variable != null && (variable.VarTypeName == nameof(GenericMuscariable) || variable.VarTypeName == typeof(GenericMuscariable).FullName);

        public string EncodeToString(IVariable variable)
        {
            return variable?.BoxedValue != null ? variable.BoxedValue.ToString() : string.Empty;
        }

        public void ApplyState(IVariable variable, string data)
        {
            // Best-effort roundtrip: try int, else string
            if (int.TryParse(data, out var i))
            {
                variable.BoxedValue = i;
            }
            else
            {
                variable.BoxedValue = data;
            }
        }

        public void ApplyState(IVariable variable, VariableSaveData data)
        {
            if (data == null) return;
            ApplyState(variable, data.Value);
        }

        public T DecodeTo<T>(string data)
        {
            object result = default(T);

            // try to coerce to T from string
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(data, out var i))
                    result = i;
            }
            else if (typeof(T) == typeof(string))
            {
                result = data;
            }

            return (T)result;
        }

        public VariableSaveData EncodeToSave(IVariable variable)
        {
            return new VariableSaveData
            {
                VarTypeName = nameof(GenericMuscariable),
                ItemId = variable.ItemId,
                Key = variable.Key,
                Value = EncodeToString(variable)
            };
        }
    }

}