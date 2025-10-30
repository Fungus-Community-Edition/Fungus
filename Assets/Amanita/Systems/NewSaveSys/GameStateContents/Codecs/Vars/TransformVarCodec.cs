using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Amanita.VScripting;
using FullSerializer;
using Amanita.FSExt;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Make sure that this class is NOT used outside the main thread. Unity doesn't
    /// like it when you try to mess with Vector or Transform properties from a different thread.
    /// </summary>
    [VarCodec(true, typeof(TransformVariable), typeof(TransformMuscariable))]
    public class TransformVarCodec : fsDirectConverter<Transform>, IVarCodec, 
        IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        public virtual bool CanHandle(IVariable variable)
        {
            return variable is IVariable<Transform>;
        }

        public virtual bool CanHandle(string typeName)
        {
            return typeName == nameof(TransformVariable) ||
                typeName == nameof(TransformMuscariable);
        }

        public virtual bool CanHandle(VariableSaveData saveData)
        {
            return CanHandle(saveData.VarTypeName);
        }

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
            Transform varValue = null;
            if (toEncode is IVariable<Transform> transformVar)
            {
                varValue = transformVar.Value;
            }
            else
            {
                bool success = ReflectionFallback();
                bool ReflectionFallback()
                {
                    if (toEncode.GetType().Name == "TransformVariable")
                    {
                        var valueProp = toEncode.GetType().GetProperty("Value");
                        if (valueProp != null)
                        {
                            varValue = valueProp.GetValue(toEncode) as Transform;
                        }
                        else
                        {
                            Debug.LogError($"TransformVarEncoder: Cannot find Value property on {toEncode.GetType()}");
                            return false;
                        }
                    }
                    return true;
                }
                if (!success)
                {
                    Debug.LogError($"TransformVarEncoder: Cannot encode variable of type {toEncode.GetType()}");
                    return string.Empty;
                }
            }

            TransformState stateToEncode = TransformState.From(varValue);

            // Use the shared serializer, not the converter's injected one (which is null outside FS pipeline).
            string json;
            var fs = AmanitaManager.DefaultSerializer;
            lock (fs)
            {
                json = fs.ToJson(stateToEncode, true);
            }
            return json;
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
            if (variable is not IVariable<Transform> transformVar)
            {
                Debug.LogError($"{this.GetType().Name}: Cannot decode variable of type {variable.GetType()}");
                return;
            }

            TransformState state;
            var fs = AmanitaManager.DefaultSerializer;
            lock (fs)
            {
                state = fs.FromJson<TransformState>(data);
            }
            state.OnDeserialize();

            Transform toApplyTo = FindTheRightTransformBasedOn(state);
            transformVar.Value = toApplyTo;

            if (toApplyTo == null)
            {
                Debug.LogWarning($"TransformVarEncoder: Could not find transform with name {state.name} and uniqueID {state.uniqueID}. The variable will be set to null.");
            }
            else
            {
                toApplyTo.SetPositionAndRotation(state.Position, state.Rotation);
                toApplyTo.localScale = state.LocalScale;
            }

        }

        protected virtual Transform FindTheRightTransformBasedOn(TransformState state)
        {
            Transform whatWeFound = null;
            IList<SaveIdentifier> allIdentifiers = GameObject.FindObjectsByType<SaveIdentifier>(FindObjectsSortMode.None).ToList();

            Transform withTheRightIdentifier = (from elem in allIdentifiers
                                                where elem.UniqueID == state.uniqueID
                                                select elem.transform).FirstOrDefault();
            if (withTheRightIdentifier != null)
            {
                whatWeFound = withTheRightIdentifier;
            }
            else
            {
                IList<Transform> allTransforms;
                allTransforms = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None).ToList();
                whatWeFound = (allTransforms.Where(elem => elem.name == state.name)).FirstOrDefault();
            }

            return whatWeFound;
        }

        public virtual void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            IVariable<Transform> transformVar = variable as IVariable<Transform>;
            if (transformVar == null)
            {
                Debug.LogError($"{this.GetType().Name}: Cannot decode variable of type {variable.GetType()}");
                return;
            }

            if (saveData.VarTypeName != variable.GetType().Name)
            {
                Debug.LogError($"TransformVarEncoder: Cannot decode variable of type {variable.GetType()} with data of type {saveData.VarTypeName}");
                return;
            }
            ApplyState(variable, saveData.Value);
        }

        public virtual T DecodeTo<T>(string data)
        {
            if (typeof(T) == typeof(Transform))
            {
                TransformState state;
                var fs = Amanita.AmanitaManager.DefaultSerializer;
                lock (fs)
                {
                    state = fs.FromJson<TransformState>(data);
                }
                state.OnDeserialize();
                return (T)(object)FindTheRightTransformBasedOn(state);
            }
            else
            {
                Debug.LogError($"TransformVarEncoder: Cannot decode to type {typeof(T).Name}");
                return default;
            }
        }

        // Below: these run inside the FS pipeline; using SerializeMember/DeserializeMember is correct.
        protected override fsResult DoSerialize(Transform model, Dictionary<string, fsData> serialized)
        {
            TransformState tFormState = TransformState.From(model);
            SerializeMember(serialized, null, nameof(TransformState), tFormState);
            return fsResult.Success;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Transform model)
        {
            DeserializeMember(data, null, nameof(TransformState), out TransformState tFormState);
            if (!string.IsNullOrEmpty(tFormState.uniqueID))
            {
                model.name = tFormState.name;
            }
            model.SetPositionAndRotation(tFormState.Position, tFormState.Rotation);
            model.localScale = tFormState.LocalScale;
            return fsResult.Success;
        }
    }

    
}