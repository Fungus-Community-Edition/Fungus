using System.Collections.Generic;
using UnityEditor;
using Type = System.Type;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class ClipboardObject
    {
        internal SerializedObject serializedObject;
        internal Type type;
        internal string json;
        internal IList<ObjectReferenceSnapshot> objectReferences = new List<ObjectReferenceSnapshot>();

        internal ClipboardObject(UnityObj obj)
        {
            serializedObject = new SerializedObject(obj);
            type = obj.GetType();
            json = EditorJsonUtility.ToJson(obj);
            CacheObjectReferences(serializedObject, objectReferences);
        }

        private static void CacheObjectReferences(SerializedObject source, IList<ObjectReferenceSnapshot> dest)
        {
            if (source == null || dest == null)
            {
                return;
            }

            var prop = source.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.propertyType != SerializedPropertyType.ObjectReference)
                {
                    continue;
                }

                var obj = prop.objectReferenceValue;
                if (obj is Block block)
                {
                    var fc = block.GetFlowchart();
                    dest.Add(new ObjectReferenceSnapshot(
                        prop.propertyPath,
                        block.ItemId,
                        block.BlockName,
                        fc != null ? fc.UniqueId : null));
                }
                else if (obj is Flowchart flowchart)
                {
                    dest.Add(new ObjectReferenceSnapshot(
                        prop.propertyPath,
                        flowchart.UniqueId,
                        flowchart.name));
                }
            }
        }

        internal sealed class ObjectReferenceSnapshot
        {
            public ObjectReferenceSnapshot(string propertyPath, ushort blockId, string blockName, 
                string flowchartId)
            {
                PropertyPath = propertyPath;
                BlockId = blockId;
                BlockName = blockName;
                FlowchartId = flowchartId;
                IsBlock = true;
            }

            public ObjectReferenceSnapshot(string propertyPath, string flowchartId, string flowchartName)
            {
                PropertyPath = propertyPath;
                FlowchartId = flowchartId;
                FlowchartName = flowchartName;
                IsBlock = false;
            }

            public string PropertyPath { get; }
            public bool IsBlock { get; }
            public ushort BlockId { get; }
            public string BlockName { get; }
            public string FlowchartId { get; }
            public string FlowchartName { get; }
        }
    }
}