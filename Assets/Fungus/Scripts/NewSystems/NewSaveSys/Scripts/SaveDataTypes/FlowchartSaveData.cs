using Fungus;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class FlowchartSaveData : SaveData
    {
        // Note that no two vars in the same Flowchart can share the same name/key. That's why 
        // we can have these two lists of keys and values.
        // Also, when finding which flowchart this should be applied to, we search
        // by ID first. If not found, then we search by name.
        [SerializeField] protected string flowchartID;
        [SerializeField] protected string flowchartName;
        [SerializeField] protected List<string> varIDs = new();
        [SerializeField] protected List<string> varNames = new(); 
        // ^For when we can't find a var based on the ID
        [SerializeField] protected List<string> values = new();
        [SerializeField] protected List<string> objectIdentifiers = new(); // Stores GameObject references

        public FlowchartSaveData(Flowchart toCreateFrom)
        {
            SaveIdentifier identifier = toCreateFrom.GetComponent<SaveIdentifier>();
            flowchartID = identifier.UniqueID;
            flowchartName = toCreateFrom.name;

            foreach (Variable varEl in toCreateFrom.Variables)
            {
                IVarEncoder forThisVar = EncoderRegistry.GetEncoder(varEl);
                if (forThisVar == null)
                {
                    Debug.LogError($"No serializer found for variable type: {varEl.GetType().Name}");
                    continue;
                }

                varIDs.Add(varEl.UniqueId);
                varNames.Add(varEl.Key);
                string encodedData = forThisVar.Encode(varEl);
                values.Add(encodedData);
            }
        }

        public override SerializedSaveData Serialized()
        {
            string json = JsonUtility.ToJson(this, true);
            string typeName = GetType().Name;
            SerializedSaveData newItem = new(typeName, json);
            return newItem;
        }

        public static void ApplyFlowchartSaveData(Flowchart flowchart, FlowchartSaveData saveData)
        {
            Dictionary<string, object> variables = new();

            for (int i = 0; i < saveData.varIDs.Count; i++)
            {
                string key = saveData.varIDs[i];
                Variable toApplyTo = flowchart.GetVariable(key);

                string value = saveData.values[i];


                if (saveData.objectIdentifiers.Contains(key))
                {
                    var foundObject = GameObject.Find(value.Split('/')[0]); // Find GameObject
                    if (foundObject != null)
                    {
                        if (value.Contains("/"))
                        {
                            var transformPath = value.Split('/')[1..]; // Extract Transform hierarchy
                            var targetTransform = FindTransformByPath(foundObject.transform, transformPath);
                            variables[key] = targetTransform;
                        }
                        else
                        {
                            variables[key] = foundObject;
                        }
                    }
                }
                else
                {
                    variables[key] = value;
                }
            }

        }

        private static Transform FindTransformByPath(Transform root, string[] path)
        {
            foreach (string step in path)
            {
                root = root.Find(step);
                if (root == null) return null;
            }
            return root;
        }
    }

    [System.Serializable]
    public class VarSaveData<T> : SaveData
    {
        [SerializeField] protected string key = string.Empty;
        [SerializeField] protected string uniqueID = string.Empty;
        [SerializeField] protected string valueStr = string.Empty;

        protected T value;
        public virtual string Key => key;
        public virtual string UniqueID => uniqueID;
        public virtual string ValueStr => valueStr;
        public virtual T Value => value;

        public override SerializedSaveData Serialized()
        {
            SerializedSaveData newItem = new SerializedSaveData(GetType().Name, JsonUtility.ToJson(this, true));
            throw new System.NotImplementedException();
        }
    }
}