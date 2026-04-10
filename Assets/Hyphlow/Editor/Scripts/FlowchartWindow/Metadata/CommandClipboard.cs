using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Clipboard for copying and pasting Flowchart commands. Stores copies of the selected 
    /// commands in a temporary GameObject.
    /// </summary>
    public class CommandClipboard
    {
        public virtual bool HasCommands()
        {
            return CommandCopyBuffer.GetInstance().HasCommands();
        }

        public virtual void CopySelectedCommands(Flowchart flowchart)
        {
            if (flowchart == null || flowchart.SelectedBlock == null)
            {
                return;
            }

            CommandCopyBuffer commandCopyBuffer = CommandCopyBuffer.GetInstance();
            commandCopyBuffer.Clear();

            foreach (Command command in flowchart.SelectedBlock.CommandList)
            {
                if (flowchart.SelectedCommands.Contains(command))
                {
                    System.Type type = command.GetType();
                    Command newCommand = Undo.AddComponent(commandCopyBuffer.gameObject, type) as Command;
                    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        FieldInfo field = fields[i];
                        bool copy = field.IsPublic;

                        object[] attributes = field.GetCustomAttributes(typeof(SerializeField), true);
                        if (attributes.Length > 0)
                        {
                            copy = true;
                        }

                        if (copy)
                        {
                            field.SetValue(newCommand, field.GetValue(command));
                        }
                    }
                }
            }
        }

        public virtual void CutSelectedCommands(Flowchart flowchart)
        {
            CopySelectedCommands(flowchart);
            DeleteSelectedCommands(flowchart);
        }

        public virtual void DeleteSelectedCommands(Flowchart flowchart)
        {
            if (flowchart == null || flowchart.SelectedBlock == null)
            {
                return;
            }

            Block block = flowchart.SelectedBlock;
            int lastSelectedIndex = 0;
            for (int i = block.CommandList.Count - 1; i >= 0; --i)
            {
                Command command = block.CommandList[i];
                foreach (Command selectedCommand in flowchart.SelectedCommands)
                {
                    if (command == selectedCommand)
                    {
                        command.OnCommandRemoved(block);
                        Undo.DestroyObjectImmediate(command);

                        Undo.RecordObject(block, "Delete");
                        block.CommandList.RemoveAt(i);

                        lastSelectedIndex = i;
                        break;
                    }
                }
            }

            Undo.RecordObject(flowchart, "Delete");
            flowchart.ClearSelectedCommands();

            if (lastSelectedIndex < block.CommandList.Count)
            {
                Command nextCommand = block.CommandList[lastSelectedIndex];
                flowchart.AddSelectedCommand(nextCommand);
            }
        }
    }
}