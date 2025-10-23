using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Amanita.VScripting;
using FullSerializer;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Make sure that this class is NOT used outside the main thread. Unity doesn't
    /// like it when you try to mess with Vector or Transform properties from a different thread.
    /// </summary>
    public class TransformVarCodec : fsDirectConverter<Transform>, IVarCodec
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
            IVariable<Transform> transformVar = toEncode as IVariable<Transform>;
            Transform varValue = null;
            if (transformVar != null)
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
            // It's fine if the state is default. We can assume that at the time of saving, the
            // variable wasn't referring to any transform.

            string json = JsonUtility.ToJson(stateToEncode);
            return json;
        }

        public virtual void Decode(IVariable variable, string data)
        {
            if (variable is not IVariable<Transform> transformVar)
            {
                Debug.LogError($"{this.GetType().Name}: Cannot decode variable of type {variable.GetType()}");
                return;
            }

            TransformState state = JsonUtility.FromJson<TransformState>(data);
            state.OnDeserialize();

            Transform toApplyTo = FindTheRightTransformBasedOn(state);
            transformVar.Value = toApplyTo;

            if (toApplyTo == null)
            {
                Debug.LogWarning($"TransformVarEncoder: Could not find transform with name {state.name} and uniqueID {state.uniqueID}. The variable will be set to null.");
            }
            else
            {
                // We need to set the transform's position, rotation and scale to the values we just found.
                toApplyTo.SetPositionAndRotation(state.Position, state.Rotation);
                toApplyTo.localScale = state.LocalScale;
            }

        }

        protected virtual Transform FindTheRightTransformBasedOn(TransformState state)
        {
            // We need to find the transform based on the uniqueID.
            // This is a bit tricky, because we need to search through all the transforms in the scene.
            // We can use a dictionary to speed up the search.
            Transform whatWeFound = null;
            IList<SaveIdentifier> allIdentifiers = GameObject.FindObjectsByType<SaveIdentifier>(FindObjectsSortMode.None).ToList();
            // ^ Considering how having a SaveIdentifier implies having a Transform
            // (while having a Transform does NOT imply having a SaveIdentifier),
            // searching for former _specifically_ should be less of a performance hit
            // than searching for all the latter

            Transform withTheRightIdentifier = (from elem in allIdentifiers
                                                where elem.UniqueID == state.uniqueID
                                                select elem.transform).FirstOrDefault();
            if (withTheRightIdentifier != null)
            {
                whatWeFound = withTheRightIdentifier;
            }
            else
            {
                // Ow! Right in the clock cycles!
                UseNameAsFallback();
                void UseNameAsFallback()
                {
                    IList<Transform> allTransforms;
#if UNITY_6000_0_OR_NEWER
                    allTransforms = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None).ToList();
#else
                    allTransforms = GameObject.FindObjectsOfType<Transform>().ToList();
#endif
                    whatWeFound = (allTransforms.Where(elem => elem.name == state.name)).FirstOrDefault();
                }
            }

            return whatWeFound;
        }

        public virtual void Decode(IVariable variable, VariableSaveData saveData)
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
            Decode(variable, saveData.Value);
        }

        public virtual T DecodeTo<T>(string data)
        {
            if (typeof(T) == typeof(Transform))
            {
                TransformState state = JsonUtility.FromJson<TransformState>(data);
                state.OnDeserialize();
                return (T)(object)FindTheRightTransformBasedOn(state);
            }
            else
            {
                Debug.LogError($"TransformVarEncoder: Cannot decode to type {typeof(T).Name}");
                return default;
            }
        }

        protected override fsResult DoSerialize(Transform model, Dictionary<string, fsData> serialized)
        {
            TransformState tFormState = TransformState.From(model);
            SerializeMember(serialized, null, nameof(TransformState), tFormState);
            return fsResult.Success;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Transform model)
        {
            // Get the TransformState
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

    [System.Serializable]
    public struct TransformState : IEquatable<TransformState>
    {
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Vector3 LocalScale
        {
            get { return localScale; }
            set { localScale = value; }
        }

        public string name;
        public string uniqueID;

        // We can't expect the Vecs and rotation to be serialized properly,
        // so we need to store them as floats.
        public float XPos
        {
            get { return position.x; }
            set { position.x = value; position.x = value; }
        }

        [SerializeField] private Vector3State position;
        [SerializeField] private Vector3State localScale;
        [SerializeField] private QuaternionState rotation;

        //public TransformState()
        //{
        //    position = Vector3State.From(Vector3.zero);
        //    rotation = QuaternionState.From(Quaternion.identity);
        //    localScale = Vector3State.From(Vector3.one);
        //    name = string.Empty;
        //    uniqueID = string.Empty;
        //}

        public static TransformState From(Transform trans)
        {
            TransformState result = default;
            result.uniqueID = string.Empty;
            if (trans != null)
            {
                // Using the properties here so the backing fields get set properly.
                result.Position = trans.position;
                result.Rotation = trans.rotation;
                result.LocalScale = trans.localScale;

                if (!trans.TryGetComponent<SaveIdentifier>(out var identifier))
                {
                    Debug.LogWarning($"The right Transform might not be loaded since {trans.name} does not have a SaveIdentifier attached to it. We'll have to try loading it based on the name we just found.");
                }
                else
                {
                    result.uniqueID = identifier.UniqueID;
                }

                result.name = trans.name;
            }

            return result;
        }

        public readonly bool Equals(TransformState otherState)
        {
            bool samePos = position.Equals(otherState.position);
            bool sameRotation = rotation.Equals(otherState.rotation);
            bool sameScale = localScale.Equals(otherState.localScale);
            bool sameName = name == otherState.name;
            bool sameID = uniqueID == otherState.uniqueID;
            bool result = samePos &&
                   sameRotation &&
                   sameScale &&
                   sameName &&
                   sameID;
            // ^Did it this way for easier debugging

            return result;
        }

        public void OnDeserialize()
        {
        }

        public override string ToString()
        {
            return $"TransformState(Name: {name}, UniqueID: {uniqueID},\n" +
                $"Pos: {Position},\nRot: {Rotation.eulerAngles},\nScale: {LocalScale})";
        }

    }

    [System.Serializable]
    public struct Vector3State : IEquatable<Vector3State>, IEquatable<Vector3>
    {
        public float x, y, z;

        public static Vector3State From(Vector3 vec)
        {
            return new Vector3State { x = vec.x, y = vec.y, z = vec.z };
        }

        public readonly Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static implicit operator Vector3(Vector3State other)
        {
            return other.ToVector3();
        }

        public static implicit operator Vector3State(Vector3 vec)
        {
            return From(vec);
        }

        public static implicit operator Vector2(Vector3State other)
        {
            return new Vector2(other.x, other.y);
        }

        public readonly bool Equals(Vector3State other)
        {
            return x == other.x && 
                y == other.y && 
                z == other.z;
        }

        public readonly bool Equals(Vector3 other)
        {
            return x == other.x && 
                y == other.y && 
                z == other.z;
        }
    }

    [System.Serializable]
    public struct QuaternionState : IEquatable<QuaternionState>, IEquatable<Quaternion>
    {
        public float x, y, z, w;

        public static QuaternionState From(Quaternion quat)
        {
            return new QuaternionState { x = quat.x, y = quat.y, z = quat.z, w = quat.w };
        }

        public readonly Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

        public static implicit operator Quaternion(QuaternionState other)
        {
            return other.ToQuaternion();
        }

        public static implicit operator QuaternionState(Quaternion quat)
        {
            return From(quat);
        }

        public readonly bool Equals(QuaternionState other)
        {
            return x == other.x &&
                y == other.y &&
                z == other.z &&
                w == other.w;
        }

        public readonly bool Equals(Quaternion other)
        {
            return x == other.x &&
                y == other.y &&
                z == other.z &&
                w == other.w;
        }

        public override string ToString()
        {
            return $"Quaternion({x}, {y}, {z}, {w})";
        }
    }
}