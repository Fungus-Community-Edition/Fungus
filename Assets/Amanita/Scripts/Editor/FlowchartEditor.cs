// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.EditorUtils
{
    [CustomEditor (typeof(Flowchart))]
    public class FlowchartEditor : Editor 
    {
        protected SerializedProperty descriptionProp;
        protected SerializedProperty colorCommandsProp;
        protected SerializedProperty hideComponentsProp;
        protected SerializedProperty stepPauseProp;
        protected SerializedProperty saveSelectionProp;
        protected SerializedProperty localizationIdProp;
        protected SerializedProperty variablesProp;
        protected SerializedProperty showLineNumbersProp;
        protected SerializedProperty hideCommandsProp;
        protected SerializedProperty luaEnvironmentProp;
        protected SerializedProperty luaBindingNameProp;

        protected SerializedProperty includeInSaveProp;
        protected SerializedProperty saveBlocksProp;
        protected SerializedProperty saveVariablesProp;
        protected SerializedProperty loadPriorityProp;


        protected Texture2D addTexture;

        protected VariableListAdaptor variableListAdaptor;

        protected UitkVariableListAdaptor uitkVarListAdaptor;

        public static bool FlowchartDataStale { get; set; }

        protected virtual void OnEnable()
        {
            if (NullTargetCheck()) // Check for an orphaned editor instance
                return;

            FetchSerializedProperties();

            addTexture = AmanitaEditorResources.AddSmall;

            uitkVarListAdaptor = new UitkVariableListAdaptor(variablesProp, target as Flowchart);
            variableListAdaptor = new VariableListAdaptor(variablesProp, target as Flowchart);
        }

        public override VisualElement CreateInspectorGUI()
        {
            // Root container for all UI Toolkit controls
            var root = new VisualElement();
            Flowchart fcTarget = (Flowchart)target;

            // 1. Find the serialized 'description' property
            FetchSerializedProperties();
            //var descProp = serializedObject.FindProperty("description");

            
            PropertyField descField = new PropertyField(descriptionProp),
                colorCommandsField = new PropertyField(colorCommandsProp),
                 hideComponentsField = new PropertyField(hideComponentsProp),
                stepPauseField = new PropertyField(stepPauseProp),
                saveSelectionField = new PropertyField(saveSelectionProp),
                localizationIDField = new PropertyField(localizationIdProp),
                showLineNumbersField = new PropertyField(showLineNumbersProp),
                luaEnvironmentField = new PropertyField(luaEnvironmentProp),
                luaBindingNameField = new PropertyField(luaBindingNameProp),
                hideCommandsField = new PropertyField(hideCommandsProp),

                includeInSaveField = new PropertyField(includeInSaveProp),
                saveBlocksField = new PropertyField(saveBlocksProp),
                saveVariablesField = new PropertyField(saveVariablesProp),
                loadPriorityField = new PropertyField(loadPriorityProp);

            Foldout editorOnlyFoldout = new Foldout(), 
                hidingFoldout = new Foldout(),
                saveSysInvolvementFoldout = new Foldout(),
                luaFoldout = new Foldout(),
                varsFoldout = new Foldout();

            Button openFlowchartWindowButton = new Button()
            {
                text = "Open Flowchart Window"
            };

            openFlowchartWindowButton.RegisterCallback<ClickEvent>((ClickEvent evt) =>
            {
                EditorWindow.GetWindow(typeof(FlowchartWindow), false, "Flowchart");
            });

            VisualElement varsUi = uitkVarListAdaptor.CreateVariablesUI();
            var imguiArea = new IMGUIContainer(() =>
            {
                serializedObject.Update();

                // Draw your existing list. 
                // The width calc below mimics your old skirt offset.
                int w = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth)
                        - UitkVariableListAdaptor.ReorderListSkirts;

                serializedObject.ApplyModifiedProperties();
            });

            PrepFields();
            void PrepFields()
            {
                SetLabels();
                void SetLabels()
                {
                    descField.label = "Description";
                    colorCommandsField.label = "Color Commands";
                    hideComponentsField.label = "Hide Components";
                    stepPauseField.label = "Step Pause";
                    saveSelectionField.label = "Save Selection";
                    localizationIDField.label = "Localization ID";
                    luaEnvironmentField.label = "Lua Environment";
                    luaBindingNameField.label = "Lua Binding Name";
                    hideCommandsField.label = ""; // To prevent double labeling

                    includeInSaveField.label = "Include in Saves";
                    saveBlocksField.label = "Save Blocks";
                    saveVariablesField.label = "Save Variables";
                    loadPriorityField.label = "Load Priority";
                }

                editorOnlyFoldout.text = "Editor Only";
                editorOnlyFoldout.tooltip = "For most of the stuff that only matters in the editor.";
                editorOnlyFoldout.Add(colorCommandsField);
                editorOnlyFoldout.Add(hideComponentsField);
                editorOnlyFoldout.Add(stepPauseField);
                editorOnlyFoldout.Add(saveSelectionField);
                editorOnlyFoldout.Add(hideCommandsField);

                luaFoldout.text = "Lua Support";
                luaFoldout.tooltip = "For stuff that involves using Flowcharts with Lua";
                luaFoldout.Add(luaEnvironmentField);
                luaFoldout.Add(luaBindingNameField);

                descField.style.marginBottom = 4;
                openFlowchartWindowButton.style.marginTop = 10;

                saveSysInvolvementFoldout.text = "Save Sys Involvement";
                saveSysInvolvementFoldout.Add(includeInSaveField);
                saveSysInvolvementFoldout.Add(saveBlocksField);
                saveSysInvolvementFoldout.Add(saveVariablesField);
                saveSysInvolvementFoldout.Add(loadPriorityField);

                varsFoldout.text = "Variables";
                varsFoldout.value = fcTarget.VariablesExpanded;
                varsFoldout.RegisterValueChangedCallback(evt => {
                    fcTarget.VariablesExpanded = evt.newValue;
                });

                


            }

            IList<VisualElement> fields = new List<VisualElement>()
            {
                descField, localizationIDField, editorOnlyFoldout, luaFoldout,
                saveSysInvolvementFoldout, openFlowchartWindowButton, varsUi,
                imguiArea,
            };

            foreach (var elem in fields)
            {
                root.Add(elem);
            }

            root.Bind(serializedObject);

            return root;
        }


        protected virtual void FetchSerializedProperties()
        {
            descriptionProp = serializedObject.FindProperty("description");
            colorCommandsProp = serializedObject.FindProperty("colorCommands");
            hideComponentsProp = serializedObject.FindProperty("hideComponents");
            stepPauseProp = serializedObject.FindProperty("stepPause");
            saveSelectionProp = serializedObject.FindProperty("saveSelection");
            localizationIdProp = serializedObject.FindProperty("localizationId");
            variablesProp = serializedObject.FindProperty("variables");
            showLineNumbersProp = serializedObject.FindProperty("showLineNumbers");
            hideCommandsProp = serializedObject.FindProperty("hideCommands");
            luaEnvironmentProp = serializedObject.FindProperty("luaEnvironment");
            luaBindingNameProp = serializedObject.FindProperty("luaBindingName");


            includeInSaveProp = serializedObject.FindProperty("includeInSaves");
            saveBlocksProp = serializedObject.FindProperty("saveBlocks");
            saveVariablesProp = serializedObject.FindProperty("saveVariables");
            loadPriorityProp = serializedObject.FindProperty("loadPriority");
        }

        //public override void OnInspectorGUI()
        //{
        //    serializedObject.Update();

        //    var flowchart = target as Flowchart;

        //    flowchart.UpdateHideFlags();

        //    EditorGUI.BeginChangeCheck();

        //    EditorGUILayout.PropertyField(descriptionProp);
        //    EditorGUILayout.PropertyField(colorCommandsProp);
        //    EditorGUILayout.PropertyField(hideComponentsProp);
        //    EditorGUILayout.PropertyField(stepPauseProp);
        //    EditorGUILayout.PropertyField(saveSelectionProp);
        //    EditorGUILayout.PropertyField(localizationIdProp);
        //    EditorGUILayout.PropertyField(showLineNumbersProp);
        //    EditorGUILayout.PropertyField(luaEnvironmentProp);
        //    EditorGUILayout.PropertyField(luaBindingNameProp);

        //    // Show list of commands to hide in Add Command menu
        //    //ReorderableListGUI.Title(new GUIContent(hideCommandsProp.displayName, hideCommandsProp.tooltip));
        //    //ReorderableListGUI.ListField(hideCommandsProp);
        //    EditorGUILayout.PropertyField(hideCommandsProp, new GUIContent(hideCommandsProp.displayName,
        //        hideCommandsProp.tooltip), true);

        //    EditorGUILayout.PropertyField(includeInSaveProp, new GUIContent(includeInSaveProp.displayName,
        //        includeInSaveProp.tooltip), true);
        //    EditorGUILayout.PropertyField(saveBlocksProp, new GUIContent(saveBlocksProp.displayName,
        //        saveBlocksProp.tooltip), true);
        //    EditorGUILayout.PropertyField(saveVariablesProp, new GUIContent(saveVariablesProp.displayName,
        //        saveVariablesProp.tooltip), true);
        //    EditorGUILayout.PropertyField(loadPriorityProp, new GUIContent(loadPriorityProp.displayName,
        //        loadPriorityProp.tooltip), true);


        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        FlowchartDataStale = true;
        //    }


        //    GUILayout.BeginHorizontal();
        //    GUILayout.FlexibleSpace();
        //    if (GUILayout.Button(new GUIContent("Open Flowchart Window", "Opens the Flowchart Window")))
        //    {
        //        EditorWindow.GetWindow(typeof(FlowchartWindow), false, "Flowchart");
        //    }


        //    GUILayout.FlexibleSpace();
        //    GUILayout.EndHorizontal();

        //    serializedObject.ApplyModifiedProperties();

        //    //Show the variables in the flowchart inspector
        //    GUILayout.Space(20);

        //    DrawVariablesGUI(false, Mathf.FloorToInt(EditorGUIUtility.currentViewWidth) - VariableListAdaptor.ReorderListSkirts);

        //}

        public virtual void DrawVariablesGUI(bool showVariableToggleButton, int w)
        {
            var targFc = target as Flowchart;

            if (targFc == null)
            {
                return;
            }

            serializedObject.Update();


            if (targFc.Variables.Count == 0)
            {
                targFc.VariablesExpanded = true;
                //showVariableToggleButton = true;
            }

            if (showVariableToggleButton && !targFc.VariablesExpanded)
            {
                if (GUILayout.Button ("Variables (" + targFc.Variables.Count + ")", GUILayout.Height(24)))
                {
                    targFc.VariablesExpanded = true;
                }

                // Draw disclosure triangle
                Rect lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x += 5;
                lastRect.y += 5;
                EditorGUI.Foldout(lastRect, false, "");
            }
            else
            {
                // Remove any null variables from the list
                // Can sometimes happen when upgrading to a new version of Fungus (if .meta GUID changes for a variable class)
                for (int i = targFc.Variables.Count - 1; i >= 0; i--)
                {
                    if (targFc.Variables[i] == null)
                    {
                        targFc.Variables.RemoveAt(i);
                    }
                }

                variableListAdaptor.DrawVarList(w);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public static List<System.Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }
        
        public static List<System.Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                    .Where(t =>
                           t != derivedType &&
                           derivedType.IsAssignableFrom(t)
                           ).ToList();
            
        }

        /// <summary>
        /// When modifying custom editor code you can occasionally end up with orphaned editor instances.
        /// When this happens, you'll get a null exception error every time the scene serializes / deserialized.
        /// Once this situation occurs, the only way to fix it is to restart the Unity editor.
        /// As a workaround, this function detects if this editor is an orphan and deletes it. 
        /// </summary>
        protected virtual bool NullTargetCheck()
        {
            try
            {
                // The serializedObject accessor creates a new SerializedObject if needed.
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