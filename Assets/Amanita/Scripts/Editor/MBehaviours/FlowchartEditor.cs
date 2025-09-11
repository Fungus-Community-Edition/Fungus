using Amanita.EditorUtils;
using UnityEditor;
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

        public static bool FlowchartDataStale { get; set; }

        protected virtual void OnEnable()
        {
            if (EraseOrphanedInstance()) // Check for an orphaned editor instance
                return;

            FetchSerializedProperties();

            addTexture = AmanitaEditorResources.AddSmall;

            _manager?.Dispose();
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
            _manager?.Dispose();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var _rootElement = new VisualElement();
            
            VisualElement NewWay()
            {
                // Clone the uxml
                //string pathToUxml = "_EditorResources/UIToolkitTemplates/VariableDisplayEditor";
                string pathToUxml = "_EditorResources/UIToolkitTemplates/FlowchartInspector";
                var uxml = Resources.Load<VisualTreeAsset>(pathToUxml);
                var inspectorRoot = uxml.CloneTree();
                Button flowchartWindowButton = inspectorRoot.Q<Button>("OpenFlowchartWindow");
                flowchartWindowButton.RegisterCallback<ClickEvent>(OpenFlowchartWindow);
                _rootElement.Add(inspectorRoot);

                var managerRoot = inspectorRoot.Q("VariableDisplayEditor");
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
    
        protected virtual void OpenFlowchartWindow(ClickEvent clickEvent)
        {
            EditorWindow.GetWindow(typeof(FlowchartWindow), false, "Flowchart");
        }
    }
}
