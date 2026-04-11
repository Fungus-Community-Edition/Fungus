using AtMycelia.Hyphlow;
using AtMycelia.FSExt;
using AtMycelia.SaveSys;
using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding Vector2 and Vector3 data types.
    /// </summary>
    [Serializable]
    [VarCodec(true, typeof(Vector2Variable), typeof(Vector3Variable), 
        typeof(VectorTwoMuscariable), typeof(VectorThreeMuscariable))]
    public class VectorVarCodec : VarCodec, IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        protected override IReadOnlyList<Type> SupportedContentTypes => (IReadOnlyList<Type>)_supportedContentTypes;

        private static readonly IList<Type> _supportedContentTypes = new Type[]
        {
            typeof(Vector2),
            typeof(Vector3),
        };


        public override string EncodeToString(IVariable variable)
        {
            fsSerializer serializer = SaveSystem.DefaultSerializer;
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


        public override void ApplyState(IVariable variable, string data)
        {
            lock (Serializer)
            {
                // We assume that data is a Vector2State or Vector3State serialized as JSON.
                if (variable is IVariable<Vector2> vecTwoVar)
                {
                    Vector2State vecState = Serializer.FromJson<Vector2State>(data);
                    vecTwoVar.Value = vecState.ToVector2();
                }
                else if (variable is IVariable<Vector3> vecThreeVar)
                {
                    Vector3State vecState = Serializer.FromJson<Vector3State>(data);
                    vecThreeVar.Value = vecState.ToVector3();
                }
                else
                {
                    Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
                }
            }
        }
    
        public override void ApplyState(IVariable variable, VariableSaveData saveData)
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

        public override T DecodeTo<T>(string data)
        {
            // Again, we assume that the data is a Vector2State or Vector3State serialized as JSON.
            T result = default;
            lock (Serializer)
            {
                if (typeof(T) == typeof(Vector2))
                {
                    Vector2State vecState = Serializer.FromJson<Vector2State>(data);
                    result = (T)(object)vecState.ToVector2();
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    Vector3State vecState = Serializer.FromJson<Vector3State>(data);
                    result = (T)(object)vecState.ToVector3();
                }
            }

            return result;
        }
    }
}