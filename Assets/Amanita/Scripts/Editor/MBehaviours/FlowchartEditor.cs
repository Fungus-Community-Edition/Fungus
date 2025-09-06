using Amanita.EditorUtils;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace Amanita.VScripting.EditorUtils
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

        protected UitkVariableListAdaptor _uitkVarListAdaptor;

        public static bool FlowchartDataStale { get; set; }

        protected virtual void OnEnable()
        {
            if (EraseOrphanedInstance()) // Check for an orphaned editor instance
                return;

            FetchSerializedProperties();

            addTexture = AmanitaEditorResources.AddSmall;

            _manager?.Dispose();
            _uitkVarListAdaptor?.Dispose();
            //uitkVarListAdaptor = new UitkVariableListAdaptor(variablesProp, target as Flowchart);
        }

        protected void BuildManager(VisualElement rootElem)
        {
            var flowchart = (Flowchart)target;
            if (flowchart == null)
                return;

            _manager?.Dispose();
            _manager = new VariableRowManager();

            var visualHandlerLookup = RowVisualHandlerRegistry.VisualHandlerLookup;
            var handlerPool = new RowVisualHandlerPool(_resolver, visualHandlerLookup);
            var rowPool = new VariableRowPool();
            var holder = rootElem;

            _factoryInitArgs.Holder = holder;
            _factoryInitArgs.HandlerPool = handlerPool;
            _factoryInitArgs.RowPool = rowPool;
            _rowFactory.Init(_factoryInitArgs);

            var list = rootElem.Q<ListView>("rowList");
            var count = rootElem.Q<UitkLabel>("varCountLabel");
            var addBtn = rootElem.Q<Button>("addVarButton");

            var listViewArgs = new VariableListViewInitArgs()
            {
                List = list,
                CountLabel = count,
                RowFactory = _rowFactory,
            };
            var view = new VariableListView(listViewArgs);

            _manager.Init(new VRowManagerInitArgs
            {
                Root = rootElem,
                AddButton = addBtn,
                Flowchart = flowchart,
                VariableListView = view,
            });
        }

        protected VariableRowManager _manager;
        protected VariableRowFactory _rowFactory = new VariableRowFactory();
        protected IRowVisualHandlerResolver _resolver = new RowVisualHandlerResolver();
        protected VariableRowFactoryInitArgs _factoryInitArgs = new VariableRowFactoryInitArgs();

        protected virtual void OnDisable()
        {
            _uitkVarListAdaptor?.Dispose();
            _manager?.Dispose();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var _rootElement = new VisualElement();
            
            VisualElement OldWay()
            {
                Flowchart fcTarget = (Flowchart)target;

                FetchSerializedProperties();

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

                VisualElement varsUi = _uitkVarListAdaptor.CreateVariablesUI();
                var imguiArea = new IMGUIContainer(() =>
                {
                    serializedObject.Update();
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
                    varsFoldout.RegisterValueChangedCallback(evt =>
                    {
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
                    _rootElement.Add(elem);
                }

                _rootElement.Bind(serializedObject);

                return _rootElement;
            }
        
            VisualElement NewWay()
            {
                // Clone the uxml
                //string pathToUxml = "_EditorResources/UIToolkitTemplates/VariableDisplayEditor";
                string pathToUxml = "_EditorResources/UIToolkitTemplates/FlowchartInspector";
                var uxml = Resources.Load<VisualTreeAsset>(pathToUxml);

                var vdeRoot = uxml.CloneTree();
                _rootElement.Add(vdeRoot);

                var managerRoot = vdeRoot.Q("VariableDisplayEditor");
                BuildManager(managerRoot);
                return _rootElement;
            }

            return NewWay();
        }

        protected virtual void FetchSerializedProperties()
        {
            descriptionProp = serializedObject.FindProperty("description");
            colorCommandsProp = serializedObject.FindProperty("colorCommands");
            hideComponentsProp = serializedObject.FindProperty("hideComponents");
            stepPauseProp = serializedObject.FindProperty("_stepPause");
            saveSelectionProp = serializedObject.FindProperty("saveSelection");
            localizationIdProp = serializedObject.FindProperty("localizationId");
            variablesProp = serializedObject.FindProperty("_legacyVariables");
            showLineNumbersProp = serializedObject.FindProperty("showLineNumbers");
            hideCommandsProp = serializedObject.FindProperty("hideCommands");
            luaEnvironmentProp = serializedObject.FindProperty("_luaEnvironment");
            luaBindingNameProp = serializedObject.FindProperty("_luaBindingName");

            includeInSaveProp = serializedObject.FindProperty("_includeInSaves");
            saveBlocksProp = serializedObject.FindProperty("_saveBlocks");
            saveVariablesProp = serializedObject.FindProperty("_saveVariables");
            loadPriorityProp = serializedObject.FindProperty("_loadPriority");
        }

        /// <summary>
        /// When modifying custom editor code you can occasionally end up with orphaned editor instances.
        /// When this happens, you'll get a null exception error every time the scene serializes / deserialized.
        /// Once this situation occurs, the only way to fix it is to restart the Unity editor.
        /// As a workaround, this function detects if this editor is an orphan and deletes it. 
        /// </summary>
        protected virtual bool EraseOrphanedInstance()
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
