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
        [SerializeField] protected List<string> keys = new();
        [SerializeField] protected List<string> values = new();
        [SerializeField] protected List<string> objectIdentifiers = new(); // Stores GameObject references

        public FlowchartSaveData(Flowchart toCreateFrom)
        {
            SaveIdentifier identifier = toCreateFrom.GetComponent<SaveIdentifier>();
            flowchartID = identifier.UniqueID;
            flowchartName = toCreateFrom.name;

            foreach (Variable varEl in toCreateFrom.Variables)
            {
                keys.Add(varEl.Key);

                System.Object value = varEl.GetValue();

                if (value is GameObject obj)
                {
                    values.Add(obj.name); // Store name as identifier
                    objectIdentifiers.Add(obj.name); // Track for later reconstruction
                }
                else if (value is Transform t)
                {
                    values.Add(t.gameObject.name + "/" + GetTransformPath(t)); // Store transform path
                    objectIdentifiers.Add(t.gameObject.name); // Track GameObject separately
                }
                else
                {
                    values.Add(value.ToString()); // Store primitives normally
                }
            }


            IList<Variable> varsToConsiderSaving = toCreateFrom.Variables;
            List<IntegerVariable> intVarsToSave = (from varEl in varsToConsiderSaving
                                                   where varEl is IntegerVariable
                                                   select varEl as IntegerVariable).ToList();
            
            foreach (var varEl in toCreateFrom.Variables)
            {
                string typeName = varEl.GetType().Name;
                string val = varEl.GetValue().ToString();
                SerializedSaveData newItem = new SerializedSaveData(typeName, val);
            }
        }

        protected virtual void SaveNumerics(IList<Variable> varsToConsiderSaving)
        {
            IList<IntegerVariable> intVarsToSave = GetVarsFrom<IntegerVariable>(varsToConsiderSaving);
        }

        protected virtual IList<T> GetVarsFrom<T>(IList<Variable> varsToConsiderSaving) where T: Variable 
        {
            IList<T> result = (from varEl in varsToConsiderSaving
                               where varEl is T
                               select varEl as T).ToList();
            return result;
        }

        [SerializeField] protected List<SerializedSaveData> intVars = new List<SerializedSaveData>();
        [SerializeField] protected List<SerializedSaveData> floatVars = new List<SerializedSaveData>();
        [SerializeField] protected List<SerializedSaveData> doubleVars = new List<SerializedSaveData>();
        [SerializeField] protected List<SerializedSaveData> boolVars = new List<SerializedSaveData>();
        [SerializeField] protected List<SerializedSaveData> stringVars = new List<SerializedSaveData>();
        [SerializeField] protected List<SerializedSaveData> vecTwoVars = new List<SerializedSaveData>();
        [SerializeField] protected List<SerializedSaveData> vecThreeVars = new List<SerializedSaveData>();
        [SerializeField] protected List<SerializedSaveData> colorVars = new List<SerializedSaveData>();

        public FlowchartSaveData(Dictionary<string, object> variables) : base()
        {
            foreach (var kvp in variables)
            {
                keys.Add(kvp.Key);

                if (kvp.Value is GameObject obj)
                {
                    values.Add(obj.name); // Store name as identifier
                    objectIdentifiers.Add(obj.name); // Track for later reconstruction
                }
                else if (kvp.Value is Transform t)
                {
                    values.Add(t.gameObject.name + "/" + GetTransformPath(t)); // Store transform path
                    objectIdentifiers.Add(t.gameObject.name); // Track GameObject separately
                }
                else
                {
                    values.Add(kvp.Value.ToString()); // Store primitives normally
                }
            }
        }

        private static string GetTransformPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        public override SerializedSaveData Serialized()
        {
            string json = JsonUtility.ToJson(this, true);
            string typeName = GetType().Name;
            SerializedSaveData newItem = new SerializedSaveData(typeName, json);
            return newItem;
        }

        public static void ApplyFlowchartSaveData(Flowchart flowchart, FlowchartSaveData saveData)
        {
            Dictionary<string, object> variables = new();

            for (int i = 0; i < saveData.keys.Count; i++)
            {
                string key = saveData.keys[i];
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