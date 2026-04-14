using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class BlockClipboardEntry
    {
        private readonly string blockName;
        private readonly IList<SerializedPropertySnapshot> blockPropertySnapshots = new List<SerializedPropertySnapshot>();
        protected IList<ClipboardObject> commands = new List<ClipboardObject>();
        protected ClipboardObject eventHandler = null;

        public BlockClipboardEntry(Block block)
            : this(block, false)
        {
        }

        public BlockClipboardEntry(Block block, bool isCut)
        {
            blockName = block.BlockName;
            BlockID = block.ItemId;

            CacheProperties(
                new SerializedObject(block),
                blockPropertySnapshots,
                SerializedPropertyType.ObjectReference,
                SerializedPropertyType.Generic,
                SerializedPropertyType.ArraySize);

            foreach (var commandEl in block.CommandList)
            {
                if (commandEl == null)
                {
                    continue;
                }

                PrepareCommandForSnapshot(commandEl, isCut);
                commands.Add(new ClipboardObject(commandEl));
            }
            if (block._EventHandler != null)
            {
                eventHandler = new ClipboardObject(block._EventHandler);
            }
        }

        public virtual int BlockID { get; protected set; }

        protected void CopyProperties(SerializedObject source, Object dest, params SerializedPropertyType[] excludeTypes)
        {
            var destSO = new SerializedObject(dest);
            destSO.Update();

            var prop = source.GetIterator();

            while (prop.NextVisible(true))
            {
                // Skip excluded types
                if (excludeTypes.Contains(prop.propertyType))
                    continue;

                var destProp = destSO.FindProperty(prop.propertyPath);
                if (destProp == null)
                    continue;

                // Managed reference safety
                if (prop.propertyType == SerializedPropertyType.ManagedReference)
                {
                    if (prop.managedReferenceFullTypename != destProp.managedReferenceFullTypename)
                        continue;
                }

                destSO.CopyFromSerializedProperty(prop);
            }

            destSO.ApplyModifiedProperties();
        }

        internal Block PasteBlock(IFlowchartHostCore flowWind, Flowchart flowchart)
        {
            var newBlock = flowWind.CreateBlock(flowchart, Vector2.zero);

            // Copy all command serialized properties
            // Copy references to match duplication behavior
            foreach (var commandEl in commands)
            {
                var newCommand = flowchart.AddCommand(commandEl.type, newBlock);

                if (newCommand.NonStandardPaste)
                {
                    ApplyJson(commandEl, newCommand);
                }
                else if (HasValidTarget(commandEl))
                {
                    // Default path — safe for simple, flat, non-polymorphic Commands
                    CopyProperties(commandEl.serializedObject, newCommand);
                }
                else
                {
                    ApplyJson(commandEl, newCommand);
                }

                newCommand.ItemId = flowchart.NextItemId();
            }

            // Copy event handler
            if (eventHandler != null)
            {
                var newEventHandler = Undo.AddComponent(flowchart.gameObject, eventHandler.type) as EventHandler;
                if (HasValidTarget(eventHandler))
                {
                    CopyProperties(eventHandler.serializedObject, newEventHandler);
                }
                else
                {
                    ApplyJson(eventHandler, newEventHandler);
                }

                newEventHandler.ParentBlock = newBlock;
                newBlock._EventHandler = newEventHandler;
            }

            // Copy block properties, but do not copy references because those were just assigned
            ApplyProperties(blockPropertySnapshots, newBlock);

            newBlock.BlockName = flowchart.GetUniqueBlockKey(blockName + " (Copy)");

            return newBlock;
        }

        internal void RestoreObjectReferences(Block pastedBlock, Flowchart flowchart, IDictionary<ushort, Block> pastedBlockLookup)
        {
            if (pastedBlock == null || flowchart == null)
            {
                return;
            }

            int commandCount = Mathf.Min(commands.Count, pastedBlock.CommandList.Count);
            for (int i = 0; i < commandCount; i++)
            {
                ApplyObjectReferences(commands[i], pastedBlock.CommandList[i], flowchart, pastedBlockLookup);
            }

            if (eventHandler != null && pastedBlock._EventHandler != null)
            {
                ApplyObjectReferences(eventHandler, pastedBlock._EventHandler, flowchart, pastedBlockLookup);
            }
        }

        internal void RefreshPastedObjects(Block pastedBlock)
        {
            if (pastedBlock == null)
            {
                return;
            }

            for (int i = 0; i < pastedBlock.CommandList.Count; i++)
            {
                RefreshCommand(pastedBlock.CommandList[i]);
            }

            RefreshCommand(pastedBlock._EventHandler);
        }

        private static void PrepareCommandForSnapshot(Command command, bool isCut)
        {
            if (command == null)
            {
                return;
            }

            if (isCut && command is IOnPreCutHandler preCutHandler)
            {
                preCutHandler.OnPreCut();
            }

            else if (command is IRefreshable refreshable)
            {
                refreshable.Refresh();
            }
        }

        private static void RefreshCommand(Object commandOrHandler)
        {
            if (commandOrHandler is IRefreshable refreshable)
            {
                refreshable.Refresh();
            }
        }

        private static bool HasValidTarget(ClipboardObject clipboardObject)
        {
            return clipboardObject != null &&
                   clipboardObject.serializedObject != null &&
                   clipboardObject.serializedObject.targetObject != null;
        }

        private static void ApplyJson(ClipboardObject source, Object dest)
        {
            if (source == null || dest == null || string.IsNullOrEmpty(source.json))
            {
                return;
            }

            EditorJsonUtility.FromJsonOverwrite(source.json, dest);
        }

        private static void ApplyObjectReferences(ClipboardObject source, Object dest, Flowchart flowchart, 
            IDictionary<ushort, Block> pastedBlockLookup)
        {
            if (source == null || dest == null || source.objectReferences == null)
            {
                return;
            }

            var destSO = new SerializedObject(dest);
            destSO.Update();

            for (int i = 0; i < source.objectReferences.Count; i++)
            {
                var snapshot = source.objectReferences[i];
                var destProp = destSO.FindProperty(snapshot.PropertyPath);
                bool propDoesNotApply = destProp == null || destProp.propertyType != SerializedPropertyType.ObjectReference;
                if (propDoesNotApply)
                {
                    continue;
                }

                if (snapshot.IsBlock)
                {
                    Block resolved = null;

                    if (pastedBlockLookup != null && pastedBlockLookup.TryGetValue(snapshot.BlockId, out var pasted))
                    {
                        resolved = pasted;
                    }
                    else if (flowchart != null)
                    {
                        resolved = flowchart.FindBlockByItemId(snapshot.BlockId);
                        if (resolved == null)
                        {
                            resolved = flowchart.FindBlock(snapshot.BlockName);
                        }
                        
                    }

                    destProp.objectReferenceValue = resolved;
                }
                else
                {
                    var resolvedFlowchart = ResolveFlowchart(snapshot, flowchart);
                    destProp.objectReferenceValue = resolvedFlowchart;
                }
            }

            destSO.ApplyModifiedProperties();
        }

        private static Flowchart ResolveFlowchart(ClipboardObject.ObjectReferenceSnapshot snapshot, Flowchart current)
        {
            if (snapshot == null)
            {
                return current;
            }

            if (current != null && current.UniqueId == snapshot.FlowchartId)
            {
                return current;
            }

            var flowcharts = Object.FindObjectsByType<Flowchart>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < flowcharts.Length; i++)
            {
                var fc = flowcharts[i];
                if (fc != null && fc.UniqueId == snapshot.FlowchartId)
                {
                    return fc;
                }
            }

            for (int i = 0; i < flowcharts.Length; i++)
            {
                var fc = flowcharts[i];
                if (fc != null && fc.name == snapshot.FlowchartName)
                {
                    return fc;
                }
            }

            return current;
        }

        private static void CacheProperties(SerializedObject source, IList<SerializedPropertySnapshot> dest, 
            params SerializedPropertyType[] excludeTypes)
        {
            if (source == null || dest == null)
            {
                return;
            }

            var prop = source.GetIterator();

            while (prop.NextVisible(true))
            {
                if (excludeTypes.Contains(prop.propertyType))
                    continue;

                if (prop.propertyType == SerializedPropertyType.ManagedReference ||
                    prop.propertyType == SerializedPropertyType.ExposedReference)
                {
                    continue;
                }

                dest.Add(new SerializedPropertySnapshot(prop));
            }
        }

        private static void ApplyProperties(IList<SerializedPropertySnapshot> source, Object dest)
        {
            if (source == null || dest == null)
            {
                return;
            }

            var destSO = new SerializedObject(dest);
            destSO.Update();

            for (int i = 0; i < source.Count; i++)
            {
                var snapshot = source[i];
                var destProp = destSO.FindProperty(snapshot.PropertyPath);
                if (destProp == null || destProp.propertyType != snapshot.PropertyType)
                {
                    continue;
                }

                snapshot.ApplyTo(destProp);
            }

            destSO.ApplyModifiedProperties();
        }

        private sealed class SerializedPropertySnapshot
        {
            public SerializedPropertySnapshot(SerializedProperty prop)
            {
                PropertyPath = prop.propertyPath;
                PropertyType = prop.propertyType;

                switch (PropertyType)
                {
                    case SerializedPropertyType.Integer:
                    case SerializedPropertyType.LayerMask:
                    case SerializedPropertyType.Character:
                        IntValue = prop.intValue;
                        break;

                    case SerializedPropertyType.Boolean:
                        BoolValue = prop.boolValue;
                        break;

                    case SerializedPropertyType.Float:
                        FloatValue = prop.floatValue;
                        break;

                    case SerializedPropertyType.String:
                        StringValue = prop.stringValue;
                        break;

                    case SerializedPropertyType.Color:
                        ColorValue = prop.colorValue;
                        break;

                    case SerializedPropertyType.Enum:
                        EnumValueIndex = prop.enumValueIndex;
                        break;

                    case SerializedPropertyType.Vector2:
                        Vector2Value = prop.vector2Value;
                        break;

                    case SerializedPropertyType.Vector3:
                        Vector3Value = prop.vector3Value;
                        break;

                    case SerializedPropertyType.Vector4:
                        Vector4Value = prop.vector4Value;
                        break;

                    case SerializedPropertyType.Rect:
                        RectValue = prop.rectValue;
                        break;

                    case SerializedPropertyType.Bounds:
                        BoundsValue = prop.boundsValue;
                        break;

                    case SerializedPropertyType.Quaternion:
                        QuaternionValue = prop.quaternionValue;
                        break;

                    case SerializedPropertyType.Vector2Int:
                        Vector2IntValue = prop.vector2IntValue;
                        break;

                    case SerializedPropertyType.Vector3Int:
                        Vector3IntValue = prop.vector3IntValue;
                        break;

                    case SerializedPropertyType.RectInt:
                        RectIntValue = prop.rectIntValue;
                        break;

                    case SerializedPropertyType.BoundsInt:
                        BoundsIntValue = prop.boundsIntValue;
                        break;

                    case SerializedPropertyType.AnimationCurve:
                        AnimationCurveValue = prop.animationCurveValue;
                        break;

                    case SerializedPropertyType.Gradient:
                        GradientValue = prop.gradientValue;
                        break;

                    case SerializedPropertyType.Hash128:
                        Hash128Value = prop.hash128Value;
                        break;
                }
            }

            public string PropertyPath { get; }
            public SerializedPropertyType PropertyType { get; }

            public int IntValue { get; }
            public bool BoolValue { get; }
            public float FloatValue { get; }
            public string StringValue { get; }
            public Color ColorValue { get; }
            public int EnumValueIndex { get; }
            public Vector2 Vector2Value { get; }
            public Vector3 Vector3Value { get; }
            public Vector4 Vector4Value { get; }
            public Rect RectValue { get; }
            public Bounds BoundsValue { get; }
            public Quaternion QuaternionValue { get; }
            public Vector2Int Vector2IntValue { get; }
            public Vector3Int Vector3IntValue { get; }
            public RectInt RectIntValue { get; }
            public BoundsInt BoundsIntValue { get; }
            public AnimationCurve AnimationCurveValue { get; }
            public Gradient GradientValue { get; }
            public Hash128 Hash128Value { get; }

            public void ApplyTo(SerializedProperty dest)
            {
                switch (PropertyType)
                {
                    case SerializedPropertyType.Integer:
                    case SerializedPropertyType.LayerMask:
                    case SerializedPropertyType.Character:
                        dest.intValue = IntValue;
                        break;

                    case SerializedPropertyType.Boolean:
                        dest.boolValue = BoolValue;
                        break;

                    case SerializedPropertyType.Float:
                        dest.floatValue = FloatValue;
                        break;

                    case SerializedPropertyType.String:
                        dest.stringValue = StringValue;
                        break;

                    case SerializedPropertyType.Color:
                        dest.colorValue = ColorValue;
                        break;

                    case SerializedPropertyType.Enum:
                        dest.enumValueIndex = EnumValueIndex;
                        break;

                    case SerializedPropertyType.Vector2:
                        dest.vector2Value = Vector2Value;
                        break;

                    case SerializedPropertyType.Vector3:
                        dest.vector3Value = Vector3Value;
                        break;

                    case SerializedPropertyType.Vector4:
                        dest.vector4Value = Vector4Value;
                        break;

                    case SerializedPropertyType.Rect:
                        dest.rectValue = RectValue;
                        break;

                    case SerializedPropertyType.Bounds:
                        dest.boundsValue = BoundsValue;
                        break;

                    case SerializedPropertyType.Quaternion:
                        dest.quaternionValue = QuaternionValue;
                        break;

                    case SerializedPropertyType.Vector2Int:
                        dest.vector2IntValue = Vector2IntValue;
                        break;

                    case SerializedPropertyType.Vector3Int:
                        dest.vector3IntValue = Vector3IntValue;
                        break;

                    case SerializedPropertyType.RectInt:
                        dest.rectIntValue = RectIntValue;
                        break;

                    case SerializedPropertyType.BoundsInt:
                        dest.boundsIntValue = BoundsIntValue;
                        break;

                    case SerializedPropertyType.AnimationCurve:
                        dest.animationCurveValue = AnimationCurveValue;
                        break;

                    case SerializedPropertyType.Gradient:
                        dest.gradientValue = GradientValue;
                        break;

                    case SerializedPropertyType.Hash128:
                        dest.hash128Value = Hash128Value;
                        break;
                }
            }
        }
    }
}
