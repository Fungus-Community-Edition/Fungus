using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Reflection;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomEditor (typeof(Command), true)]
    public class CommandEditor : Editor 
    {
        // Note that unlike PropertyDrawers, Editor subclasses each work with
        // their own instance of the inspected property. Thus, it's fine
        // to have instance variables here.
        #region statics
        public static bool SelectedCommandDataStale { get; set; }

        public static CommandInfoAttribute GetCommandInfo(System.Type commandType)
        {
            CommandInfoAttribute returnVal = null;

            object[] attributes = commandType.GetCustomAttributes(typeof(CommandInfoAttribute), false);
            for (int i = 0; i < attributes.Length; i++)
            {
                object obj = attributes[i];
                CommandInfoAttribute commandInfoAttr = obj as CommandInfoAttribute;
                if (commandInfoAttr != null)
                {
                    // We want the CommandInfo with the highest priority
                    if (returnVal == null)
                        returnVal = commandInfoAttr;
                    else if (returnVal.Priority < commandInfoAttr.Priority)
                        returnVal = commandInfoAttr;
                }
            }
            
            return returnVal;
        }

        #endregion statics

        private Dictionary<string, ReorderableList> reorderableLists;

        public virtual void OnEnable()
        {
            if (NullTargetCheck()) // Check for an orphaned editor instance
                return;

            reorderableLists = new Dictionary<string, ReorderableList>();

            var targetCommand = target as Command;
            if (targetCommand == null)
            {
                return;
            }
            Flowchart fc = targetCommand.GetFlowchart();
            if (fc != null)
            {
                VariableRegistryService.RebuildAll(fc);
            }
        }

        public virtual void DrawCommandInspectorGUI()
        {
            Command targetCommand = target as Command;
            if (targetCommand == null)
            {
                return;
            }

            var flowchart = targetCommand.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }

            var commandType = targetCommand.GetType();

            CommandInfoAttribute commandInfoAttr = CommandEditor.GetCommandInfo(commandType);
            if (commandInfoAttr == null)
            {
                return;
            }
            
            var obsAttr = commandType.GetCustomAttribute<System.ObsoleteAttribute>();
            if(obsAttr != null)
            {
                EditorGUILayout.HelpBox(obsAttr.Message, MessageType.Warning, true);
            }

            GUILayout.BeginVertical(GUI.skin.box);

            if (targetCommand.enabled)
            {
                if (flowchart.ColorCommands)
                {
                    GUI.backgroundColor = targetCommand.GetButtonColor();
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }
            }
            else
            {
                GUI.backgroundColor = Color.grey;
            }
            GUILayout.BeginHorizontal(GUI.skin.button);

            string commandName = commandInfoAttr.CommandName;
            GUILayout.Label(commandName, GUILayout.MinWidth(80), GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();

            GUILayout.Label(new GUIContent("(" + targetCommand.ItemId + ")"));

            GUILayout.Space(10);

            GUI.backgroundColor = Color.white;
            bool enabled = targetCommand.enabled;
            enabled = GUILayout.Toggle(enabled, new GUIContent());

            if (targetCommand.enabled != enabled)
            {
                Undo.RecordObject(targetCommand, "Set Enabled");
                targetCommand.enabled = enabled;
            }

            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            DrawCommandGUI();
            if(EditorGUI.EndChangeCheck())
            {
                SelectedCommandDataStale = true;
            }

            EditorGUILayout.Separator();

            if (targetCommand.ErrorMessage.Length > 0)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = new Color(1,0,0);
                EditorGUILayout.LabelField(new GUIContent("Error: " + targetCommand.ErrorMessage), style);
            }

            GUILayout.EndVertical();

            // Display help text
            CommandInfoAttribute infoAttr = CommandEditor.GetCommandInfo(targetCommand.GetType());
            if (infoAttr != null && !HyphlowEditorPreferences.suppressHelpBoxes)
            {
                EditorGUILayout.HelpBox(infoAttr.HelpText, MessageType.Info, true);
            }
        }

        public virtual void DrawCommandGUI()
        {
            Command targetCommand = target as Command;
            
            // Code below was copied from here
            // http://answers.unity3d.com/questions/550829/how-to-add-a-script-field-in-custom-inspector.html

            // Users should not be able to change the MonoScript for the command using the usual Script field.
            // Doing so could cause block.commandList to contain null entries.
            // To avoid this we manually display all properties, except for m_Script.
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            int index = 0;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.name == "m_Script")
                {
                    continue;
                }

                if (!targetCommand.IsPropertyVisible(iterator.name))
                {
                    continue;
                }

                if (iterator.isArray &&
                    targetCommand.IsReorderableArray(iterator.name))
                {
                    ReorderableList reordList = null;
                    reorderableLists.TryGetValue(iterator.displayName, out reordList);
                    if(reordList == null)
                    {
                        var locSerProp = iterator.Copy();
                        //create and insert
                        reordList = new ReorderableList(serializedObject, locSerProp, true, false, true, true)
                        {
                            drawHeaderCallback = (Rect rect) =>
                            {
                                EditorGUI.LabelField(rect, locSerProp.displayName);
                            },
                            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
                                EditorGUI.PropertyField(rect, locSerProp.GetArrayElementAtIndex(index));
                            },
                            elementHeightCallback = (int index) =>
                            {
                                return EditorGUI.GetPropertyHeight(locSerProp.GetArrayElementAtIndex(index), null, true);// + EditorGUIUtility.singleLineHeight;
                            }
                    };

                        reorderableLists.Add(iterator.displayName, reordList);
                    }

                    reordList.DoLayoutList();
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
                    //Debug.Log($"Drew property {index} named {iterator.name}" );
                }
                index++;
            }

            serializedObject.ApplyModifiedProperties();
        }

        
        public static void ObjectField<T>(SerializedProperty property, GUIContent label, GUIContent nullLabel, List<T> objectList) where T : Object 
        {
            if (property == null)
            {
                return;
            }

            List<GUIContent> objectNames = new List<GUIContent>();

            T selectedObject = property.objectReferenceValue as T;

            int selectedIndex = -1; // Invalid index

            // First option in list is <None>
            objectNames.Add(nullLabel);
            if (selectedObject == null)
            {
                selectedIndex = 0;
            }

            for (int i = 0; i < objectList.Count; ++i)
            {
                if (objectList[i] == null) continue;
                objectNames.Add(new GUIContent(objectList[i].name));

                if (selectedObject == objectList[i])
                {
                    selectedIndex = i + 1;
                }
            }

            T result;
            
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, objectNames.ToArray());

            if (selectedIndex == -1)
            {
                // Currently selected object is not in list, but nothing else was selected so no change.
                return;
            }
            else if (selectedIndex == 0)
            {
                result = null; // Null option
            }
            else
            {
                result = objectList[selectedIndex - 1];
            }

            property.objectReferenceValue = result;
        }

        // When modifying custom editor code you can occasionally end up with orphaned editor instances.
        // When this happens, you'll get a null exception error every time the scene serializes or
        // deserializes. Once that happens, the only way to fix it is to restart the Unity editor.
        // 
        // As a workaround, this function detects if this command editor is an orphan and deletes it. 
        // To use it, just call this function at the top of the OnEnable() method in your custom editor.
        protected virtual bool NullTargetCheck()
        {
            try
            {
                // The serializedObject accessor create a new SerializedObject if needed.
                // However, this will fail with a null exception if the target object no longer exists.
                #pragma warning disable 0219
                SerializedObject so = serializedObject;
            }
            catch (System.NullReferenceException)
            {
                DestroyImmediate(this);
                return true;
            }

            return false;
        }
    }
}
