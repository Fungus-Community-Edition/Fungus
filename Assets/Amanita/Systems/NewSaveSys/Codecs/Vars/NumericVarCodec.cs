using AtMycelia.Amanita.VScripting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Amanita.SaveSys
{
    [VarCodec(true, typeof(IntegerVariable), typeof(FloatVariable), typeof(IntMuscariable), typeof(FloatMuscariable))]
    public class NumericVarCodec : VarCodec, IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        protected override IReadOnlyList<Type> SupportedContentTypes => (IReadOnlyList<Type>)_supportedContentTypes;

        private static readonly IList<Type> _supportedContentTypes = new Type[]
        {
            typeof(int),
            typeof(float)
        };

        public override string EncodeToString(IVariable variable)
        {
            if (!CanHandle(variable))
            {
                throw new InvalidOperationException($"Variable type {variable.GetType()} is not supported for encoding in NumericVarCodec.");
            }
            
            string result = "";
            if (variable is IVariable<int> intVar)
            {
                result = intVar.Value.ToString();
            }
            else if (variable is IVariable<float> floatVar)
            {
                result = floatVar.Value.ToString(roundTripFormat, System.Globalization.CultureInfo.InvariantCulture);
            }

            return result;
        }

        public override VariableSaveData EncodeToSave(IVariable variable)
        {
            string typeName = "";
            if (variable is IVariable<int>)
                typeName = typeof(IVariable<int>).Name;
            else if (variable is IVariable<float>)
                typeName = typeof(IVariable<float>).Name;
            else
                throw new InvalidOperationException($"Variable type {variable.GetType()} is not supported for encoding in NumericVarCodec.");

            VariableSaveData result = base.EncodeToSave(variable);
            result.VarTypeName = typeName;
            return result;
        }

        //public override void ApplyState(IVariable variable, object data)
        //{
        //    if (data is string strData)
        //    {
        //        IVarStateApplier<string> stringApplier = this as IVarStateApplier<string>;
        //        stringApplier.ApplyState(variable, strData);
        //    }
        //    else if (data is VariableSaveData saveData)
        //    {
        //        IVarStateApplier<VariableSaveData> saveDataApplier = this as IVarStateApplier<VariableSaveData>;
        //        saveDataApplier.ApplyState(variable, saveData);
        //    }
        //    else
        //    {
        //        Debug.LogError($"Data type {data.GetType()} is not supported for decoding in NumericVarEncoder.");
        //    }
        //}

        public override void ApplyState(IVariable variable, string data)
        {
            if (variable is IVariable<int> intVar)
                intVar.Value = int.Parse(data);
            else if (variable is IVariable<float> floatVar)
                floatVar.Value = float.Parse(data);
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in NumericVarEncoder.");
            }
        }

        public override void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            bool validVarType = variable is IVariable<int> ||
                variable is IVariable<float>;

            if (!validVarType)
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in NumericVarEncoder.");
                return;
            }

            ApplyState(variable, saveData.Value);
        }

        public override T DecodeTo<T>(string data)
        {
            T result = default;
            Type tType = typeof(T);
            if (tType == typeof(int))
            {
                result = (T)(object)int.Parse(data);
            }
            else if (tType == typeof(float))
            {
                result = (T)(object)float.Parse(data, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                throw new InvalidOperationException($"Type {typeof(T)} is not supported for decoding in NumericVarEncoder.");
            }

            return result;
        }
    }
}