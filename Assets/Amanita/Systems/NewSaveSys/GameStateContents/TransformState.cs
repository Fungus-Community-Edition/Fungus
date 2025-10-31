using System;
using UnityEngine;

namespace Amanita.SaveSys
{
    [Serializable]
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

            const float rotAngleEpsilon = 1e-4f;
            bool sameRotation = Quaternion.Angle(rotation, otherState.rotation) <= rotAngleEpsilon;
            // ^Since the built-in Quaternion.Equals is a bit finicky due to floating point precision issues
            // and how quaternions can represent the same rotation with different values
            // See: https://stackoverflow.com/questions/4655615/why-does-quaternion-equals-not-work-as-expected

            bool sameLocalScale = localScale.Equals(otherState.localScale);
            bool sameName = name == otherState.name;
            bool sameID = uniqueID == otherState.uniqueID;
            bool result = samePos &&
                   sameRotation &&
                   sameLocalScale &&
                   sameName &&
                   sameID;
            // ^Did it this way for easier debugging

            return result;
        }

        public readonly void OnDeserialize()
        {
        }

        public override string ToString()
        {
            return $"TransformState(Name: {name}, UniqueID: {uniqueID},\n" +
                $"Pos: {Position},\nRot: {Rotation.eulerAngles},\nScale: {LocalScale})";
        }

    }

}