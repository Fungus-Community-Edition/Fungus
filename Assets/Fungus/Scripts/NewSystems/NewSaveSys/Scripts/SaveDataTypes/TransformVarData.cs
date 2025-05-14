using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CGTUnity.Fungus.SaveSystem
{
    /// <summary>
    /// Contains the positional, rotational, and size-related state of a Transform
    /// referred to by a Flowchart variable.
    /// </summary>
    [System.Serializable]
    public class TransformVarData : SaveData, System.IEquatable<TransformVarData>
    {
        #region Serialized Fields
        [SerializeField] protected string gameObjectName = "";
        [SerializeField] protected Quaternion rotation;
        [SerializeField] protected Vector3 position, localScale, up, right, forward;
        #endregion

        #region Properties for Serialized Fields
        public string GameObjectName
        {
            get { return gameObjectName; }
            set { gameObjectName = value; }
        }

        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 LocalScale
        {
            get { return localScale; }
            set { localScale = value; }
        }

        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        public Vector3 Right
        {
            get { return right; }
            set { right = value; }
        }

        public Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }
        #endregion

        public TransformVarData() {}
        public TransformVarData(Transform transform)
        {
            this.SetFrom(transform);
        }

        public static TransformVarData CreateFrom(Transform transform)
        {
            TransformVarData newData = new TransformVarData(transform);

            return newData;
        }

        public virtual void SetFrom(Transform transform)
        {
            Validate(transform);
            GetStateFrom(transform);
        }

        protected virtual void Validate(Transform transform)
        {
            if (transform == null)
                throw new System.ArgumentNullException("Cannot set TransformData from null Transform.");
        }

        protected virtual void GetStateFrom(Transform transform)
        {
            GameObjectName = transform.gameObject.name;
            Rotation = transform.rotation;
            Position = transform.position;
            LocalScale = transform.localScale;
            Up = transform.up;
            Right = transform.right;
            Forward = transform.forward;
        }

        public override void Clear()
        {
            base.Clear();
            GameObjectName = "";
            Rotation = default(Quaternion);
            Position = LocalScale = Up = Right = Forward = default(Vector3);
        }

        public virtual bool Equals(TransformVarData other)
        {
            return this.GameObjectName.Equals(other.GameObjectName) &&
                this.Rotation.Equals(other.Rotation) &&
                this.Position.Equals(other.Position) &&
                this.LocalScale.Equals(other.LocalScale) &&
                this.Up.Equals(other.Up) &&
                this.Right.Equals(other.Right) &&
                this.Forward.Equals(other.Forward);
        }

        public virtual void ApplyTo(Transform transform)
        {
            transform.position = position;
            transform.localScale = localScale;
            transform.rotation = rotation;
            transform.up = up;
            transform.right = right;
            transform.forward = forward;
        }

    }
}