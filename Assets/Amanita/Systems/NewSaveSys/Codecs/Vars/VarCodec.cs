using AtMycelia.Amanita.VScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using Type = System.Type;
using UnityEngine;

namespace AtMycelia.Amanita.SaveSys
{
    public abstract class VarCodec : IVarCodec
    {
        public virtual bool CanHandle(IVariable variable)
        {
            Type contentType = variable.ContentType;
            return SupportedContentTypes.Contains(contentType);
        }

        protected abstract IReadOnlyList<Type> SupportedContentTypes { get; }

        public virtual bool CanHandle(string contentTypeName)
        {
            bool result = false;

            for (int i = 0; i < SupportedContentTypes.Count; i++)
            {
                var supportedType = SupportedContentTypes[i];
                if (supportedType.Name.Equals(contentTypeName, _caseInsensitive) || 
                    supportedType.FullName.Equals(contentTypeName, _caseInsensitive))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private static readonly StringComparison _caseInsensitive = StringComparison.OrdinalIgnoreCase;

        public virtual bool CanHandle(VariableSaveData variable)
        {
            string contentTypeName = variable.ContentTypeName;

            for (int i = 0; i < SupportedContentTypes.Count; i++)
            {
                var supportedType = SupportedContentTypes[i];
                if (supportedType.Name.Equals(contentTypeName, _caseInsensitive) || 
                    supportedType.FullName.Equals(contentTypeName, _caseInsensitive))
                {
                    return true;
                }
            } 
            
            return false;
        }

        public abstract void ApplyState(IVariable variable, string data);

        public abstract void ApplyState(IVariable variable, VariableSaveData data);

        protected static string roundTripFormat = "R";
        // ^ For more accurate decoding when needed

        public abstract T DecodeTo<T>(string data);

        public virtual VariableSaveData EncodeToSave(IVariable variable)
        {
            VariableSaveData result = VariableSaveData.Null;
            string data = EncodeToString(variable);
            if (!string.IsNullOrEmpty(data))
            {
                result = VariableSaveData.From(variable, data);
            }
            return result;
        }

        public abstract string EncodeToString(IVariable variable);

        public virtual void ApplyState(IVariable toApplyTo, object data)
        {
            if (data is string strData)
            {
                ApplyState(toApplyTo, strData);
            }
            else if (data is VariableSaveData saveData)
            {
                ApplyState(toApplyTo, saveData);
            }
            else
            {
                Debug.LogError($"Data type {data.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }
    }
}