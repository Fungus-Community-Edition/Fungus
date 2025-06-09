using UnityEngine;
using AmanitaVar = Amanita.Variable;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Amanita.SaveSys
{
    public class TransformVarCodec : IVarCodec
    {
        public virtual bool CanHandle(AmanitaVar variable)
        {
            return variable is TransformVariable;
        }
        public virtual bool CanHandle(string typeName)
        {
            return typeName == nameof(TransformVariable);
        }

        public virtual bool CanHandle(VariableSaveData saveData)
        {
            return CanHandle(saveData.VarTypeName);
        }

        public virtual VariableSaveData EncodeToSave(AmanitaVar variable)
        {
            VariableSaveData result = new()
            {
                VarTypeName = variable.GetType().Name,
                UniqueID = variable.UniqueId,
                Key = variable.Key,
                Value = EncodeToString(variable)
            };
            return result;
        }

        public virtual string EncodeToString(AmanitaVar variable)
        {
            TransformVariable transformVar = variable as TransformVariable;
            if (transformVar == null)
            {
                Debug.LogError($"TransformVarEncoder: Cannot encode variable of type {variable.GetType()}");
                return string.Empty;
            }

            TransformState stateToEncode = TransformState.From(transformVar.Value);
            // It's fine if the state is default. We can assume that at the time of saving, the
            // variable wasn't referring to any transform.

            string json = JsonUtility.ToJson(stateToEncode);
            return json;
        }

        public virtual void Decode(AmanitaVar variable, string data)
        {
            TransformVariable transformVar = variable as TransformVariable;
            if (transformVar == null)
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
                IList<Transform> allTransforms = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None).ToList();
                whatWeFound = (allTransforms.Where(elem => elem.name == state.name)).FirstOrDefault();
            }

            return whatWeFound;
        }

        public virtual void Decode(AmanitaVar variable, VariableSaveData saveData)
        {
            TransformVariable transformVar = variable as TransformVariable;
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
    }

    [System.Serializable]
    public struct TransformState : IEquatable<TransformState>
    {
        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                xPos = position.x;
                yPos = position.y;
                zPos = position.z;
            }
        }

        public Quaternion Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                xRot = rotation.x;
                yRot = rotation.y;
                zRot = rotation.z;
                wRot = rotation.w;
            }
        }

        public Vector3 LocalScale
        {
            get { return localScale; }
            set
            {
                localScale = value;
                xScale = localScale.x;
                yScale = localScale.y;
                zScale = localScale.z;
            }
        }
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 localScale;
        public string name;
        public string uniqueID;

        // We can't expect the Vecs and rotation to be serialized properly,
        // so we need to store them as floats.
        public float XPos
        {
            get { return xPos; }
            set { xPos = value; position.x = value; }
        }

        [SerializeField]
        private float xPos, yPos, zPos;
        [SerializeField]
        private float xRot, yRot, zRot, wRot;
        [SerializeField]
        private float xScale, yScale, zScale;

        public static TransformState From(Transform trans)
        {
            TransformState result = default(TransformState);
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
            return position == otherState.position &&
                   rotation == otherState.rotation &&
                   localScale == otherState.localScale &&
                   name == otherState.name &&
                   uniqueID == otherState.uniqueID;
        }

        public void OnDeserialize()
        {
            position.x = xPos;
            position.y = yPos;
            position.z = zPos;

            rotation.x = xRot;
            rotation.y = yRot;
            rotation.z = zRot;
            rotation.w = wRot;

            localScale.x = xScale;
            localScale.y = yScale;
            localScale.z = zScale;
        }

    }
}