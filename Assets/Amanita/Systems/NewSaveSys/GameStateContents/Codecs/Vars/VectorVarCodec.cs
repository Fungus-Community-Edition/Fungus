using Amanita.VScripting;
using System.Linq;
using System;
using UnityEngine;
using FullSerializer;
using Amanita.FSExt;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding Vector2 and Vector3 data types.
    /// </summary>
    [Serializable]
    [VarCodec(true, typeof(Vector2Variable), typeof(Vector3Variable), 
        typeof(VectorTwoMuscariable), typeof(VectorThreeMuscariable))]
    public class VectorVarCodec : IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        public virtual bool CanHandle(IVariable variable) =>
            variable is IVariable<Vector2> || variable is IVariable<Vector3>;

        public virtual bool CanHandle(string typeName) =>
            supportedVarTypes.Any(type => type.Name == typeName);

        protected static Type[] supportedVarTypes = new Type[]
        {
            typeof(Vector2Variable),
            typeof(Vector3Variable),
            typeof(VectorTwoMuscariable),
            typeof(VectorThreeMuscariable),
        };

        public virtual bool CanHandle(VariableSaveData saveData)
        {
            return supportedVarTypes.Any(type => type.Name == saveData.VarTypeName);
        }

        public virtual string EncodeToString(IVariable variable)
        {
            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                if (variable is IVariable<Vector2> vecTwoVar)
                {
                    Vector2State vecState = Vector2State.From(vecTwoVar.Value);
                    return serializer.ToJson(vecState);
                }
                else if (variable is IVariable<Vector3> vecThreeVar)
                {
                    Vector3State vecState = Vector3State.From(vecThreeVar.Value);
                    return serializer.ToJson(vecState);
                }
                else
                {
                    Debug.LogError($"Variable type {variable.GetType()} is not supported for encoding in {this.GetType().Name}.");
                    return string.Empty;
                }
            }
        }

        public virtual void ApplyState(IVariable variable, object data)
        {
            if (data is string strData)
            {
                ApplyState(variable, strData);
            }
            else if (data is VariableSaveData saveData)
            {
                ApplyState(variable, saveData);
            }
            else
            {
                Debug.LogError($"Data type {data.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public virtual void ApplyState(IVariable variable, string data)
        {
            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                // We assume that data is a Vector2State or Vector3State serialized as JSON.
                if (variable is IVariable<Vector2> vecTwoVar)
                {
                    Vector2State vecState = serializer.FromJson<Vector2State>(data);
                    vecTwoVar.Value = vecState.ToVector2();
                }
                else if (variable is IVariable<Vector3> vecThreeVar)
                {
                    Vector3State vecState = serializer.FromJson<Vector3State>(data);
                    vecThreeVar.Value = vecState.ToVector3();
                }
                else
                {
                    Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
                }
            }
        }
    
        public virtual void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            bool validVarType = variable is IVariable<Vector2> ||
                variable is IVariable<Vector3>;
            if (!validVarType)
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for decoding in {this.GetType().Name}.");
                return;
            }
            ApplyState(variable, saveData.Value);
        }

        public virtual VariableSaveData EncodeToSave(IVariable variable)
        {
            string data = EncodeToString(variable);
            if (string.IsNullOrEmpty(data))
            {
                Debug.LogError($"Failed to encode variable {variable} in {this.GetType().Name}.");
                return VariableSaveData.Null;
            }

            VariableSaveData result = new()
            {
                VarTypeName = variable.GetType().Name,
                ItemId = variable.ItemId,
                Key = variable.Key,
                Value = data,
            };

            return result;
        }

        public virtual T DecodeTo<T>(string data)
        {
            // Again, we assume that the data is a Vector2State or Vector3State serialized as JSON.
            T result = default;
            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                if (typeof(T) == typeof(Vector2))
                {
                    Vector2State vecState = serializer.FromJson<Vector2State>(data);
                    result = (T)(object)vecState.ToVector2();
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    Vector3State vecState = serializer.FromJson<Vector3State>(data);
                    result = (T)(object)vecState.ToVector3();
                }
            }

            return result;
        }
    }
}