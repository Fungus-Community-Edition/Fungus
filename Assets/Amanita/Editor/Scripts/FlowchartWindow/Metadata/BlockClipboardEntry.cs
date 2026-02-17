using Amanita.VScripting.Commands;
using Amanita.VScripting.EventHandlers;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ClipboardObject = Amanita.EditorUtils.ClipboardObject;
using Object = UnityEngine.Object;
using Type = System.Type;

namespace Amanita.VScripting.EditorUtils
{
    public class BlockClipboardEntry
    {
        protected SerializedObject block = null;
        protected IList<ClipboardObject> commands = new List<ClipboardObject>();
        protected ClipboardObject eventHandler = null;

        public BlockClipboardEntry(Block block)
        {
            this.block = new SerializedObject(block);
            BlockID = block.ItemId;

            foreach (var commandEl in block.CommandList)
            {
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

                Debug.Log($"Copying property: {prop.propertyPath} ({prop.propertyType}) on {dest.GetType().Name}");

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
                    // JSON path — handles SerializeReference, polymorphic graphs, etc.
                    var json = EditorJsonUtility.ToJson(commandEl.serializedObject.targetObject);
                    EditorJsonUtility.FromJsonOverwrite(json, newCommand);
                }
                else
                {
                    // Default path — safe for simple, flat, non-polymorphic Commands
                    CopyProperties(commandEl.serializedObject, newCommand);
                }

                newCommand.ItemId = flowchart.NextItemId();
            }

            // Copy event handler
            if (eventHandler != null)
            {
                var newEventHandler = Undo.AddComponent(flowchart.gameObject, eventHandler.type) as EventHandler;
                CopyProperties(eventHandler.serializedObject, newEventHandler);
                newEventHandler.ParentBlock = newBlock;
                newBlock._EventHandler = newEventHandler;
            }

            // Copy block properties, but do not copy references because those were just assigned
            CopyProperties(
                block,
                newBlock,
                SerializedPropertyType.ObjectReference,
                SerializedPropertyType.Generic,
                SerializedPropertyType.ArraySize
            );

            newBlock.BlockName = flowchart.GetUniqueBlockKey(block.FindProperty("blockName").stringValue + " (Copy)");
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            // ^Due to how the Commands' summaries can otherwise be misleading after being pasted. For example, 
            // a Set Variable that says "error: no Variable selected" even though that copy's internal 
            // state is what it should be. Might be an issue of the Command's cached summary not updating
            // until the next inspector update, but this is a simple fix.

            return newBlock;
        }
    }
}
