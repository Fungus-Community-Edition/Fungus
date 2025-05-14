using Fungus;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class FlowchartSaveData : SaveData
    {
        [SerializeField] protected List<string> keys = new();
        [SerializeField] protected List<string> values = new();
        [SerializeField] protected List<string> objectIdentifiers = new(); // Stores GameObject references

        public FlowchartSaveData(Flowchart toCreateFrom)
        {
            
        }

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

        public override SaveDataItem ToSaveDataItem()
        {
            throw new System.NotImplementedException();
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
}