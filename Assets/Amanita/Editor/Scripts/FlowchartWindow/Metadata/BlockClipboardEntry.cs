using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;
using ClipboardObject = Amanita.VScripting.EditorUtils.FlowchartWindow.ClipboardObject;
using Amanita.VScripting.EventHandlers;

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
            foreach (var command in block.CommandList)
            {
                commands.Add(new ClipboardObject(command));
            }
            if (block._EventHandler != null)
            {
                eventHandler = new ClipboardObject(block._EventHandler);
            }
        }

        public virtual int BlockID { get; protected set; }
        protected void CopyProperties(SerializedObject source, Object dest, params SerializedPropertyType[] excludeTypes)
        {
            var newSerializedObject = new SerializedObject(dest);
            var prop = source.GetIterator();
            while (prop.NextVisible(true))
            {
                // Exclude problematic valObj fields
                if (prop.propertyPath.EndsWith("valObj"))
                    continue;

                if (!excludeTypes.Contains(prop.propertyType))
                {
                    newSerializedObject.CopyFromSerializedProperty(prop);
                }
            }

            newSerializedObject.ApplyModifiedProperties();
        }

        internal Block PasteBlock(IFlowchartHost flowWind, Flowchart flowchart)
        {
            var newBlock = flowWind.CreateBlock(flowchart, Vector2.zero);

            // Copy all command serialized properties
            // Copy references to match duplication behavior
            foreach (var command in commands)
            {
                var newCommand = Undo.AddComponent(flowchart.gameObject, command.type) as Command;
                CopyProperties(command.serializedObject, newCommand);
                newCommand.ItemId = flowchart.NextItemId();
                newBlock.CommandList.Add(newCommand);
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

            return newBlock;
        }
    }
}
