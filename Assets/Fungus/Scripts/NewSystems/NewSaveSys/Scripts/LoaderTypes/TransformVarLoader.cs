using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CGTUnity.Fungus.SaveSystem
{
    public class TransformVarLoader : SaveLoader<TransformVarData>
    {
        public override bool Load(TransformVarData saveData)
        {
            // Find the game object the data is for, and apply its state to the transform
            GameObject gameObject = GameObject.Find(saveData.GameObjectName);

            if (gameObject == null)
            {
                LetUserKnowTransformNotFoundFor(saveData);
                return false;
            }

            saveData.ApplyTo(gameObject.transform);

            return true;
        }

        protected virtual void LetUserKnowTransformNotFoundFor(TransformVarData data)
        {
            Debug.LogWarning("Transform not found for GameObject " + data.GameObjectName);
        }

        protected virtual bool CanLoadData(TransformVarData data, out string errorMessage)
        {
            errorMessage = null;
            string objNotFound = "Failed to find Flowchart object specified in save data";

            // Find the Game Object in the scene
            GameObject gameObject = FindGameObjectFor(data);

            if (gameObject == null)
                LetUserKnowTransformNotFoundFor(data);

            Transform transform = null;
            gameObject = GameObject.Find(data.GameObjectName);
            
            // If possible, get the Flowchart component from it
            if (gameObject != null)
                transform = gameObject.GetComponent<Transform>();
            
            if (transform == null) // Need the flowchart component to load into
                errorMessage = objNotFound;
            
            return transform != null;
        }

        protected virtual GameObject FindGameObjectFor(TransformVarData data)
        {
            return GameObject.Find(data.GameObjectName);
            
        }

        

    }
}