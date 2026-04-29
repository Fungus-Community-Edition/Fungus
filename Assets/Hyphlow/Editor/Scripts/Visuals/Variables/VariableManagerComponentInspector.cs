using AtMycelia.EditorUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomEditor(typeof(VariableManagerComponent))]
    public class VariableManagerComponentInspector : Editor
    {
        protected VariableRowManager _manager;
        protected VariableRowFactory _rowFactory = new VariableRowFactory();
        protected IRowVisualHandlerResolver _resolver = new RowVisualHandlerResolver();
        protected VariableRowFactoryInitArgs _factoryInitArgs = new VariableRowFactoryInitArgs();
        protected RowVisualHandlerPool _handlerPool;
        protected VariableRowPool _rowPool;
        protected VisualTreeAsset _uxml;
        protected VisualElement _rootElement;
        protected TemplateContainer _inspectorRoot;

        protected readonly string _pathToUxml = "UIToolkitTemplates/VariableDisplayEditor";

        protected virtual void OnEnable()
        {
            _manager?.Dispose();
            PrepGUI();
        }

        protected virtual void OnDisable()
        {
            _manager?.Dispose();
            _manager = null;
            _inspectorRoot = null;
            _rootElement = null;
        }

        public override VisualElement CreateInspectorGUI()
        {
            return _rootElement;
        }

        protected virtual void PrepGUI()
        {
            _manager = new VariableRowManager();
            _handlerPool = new RowVisualHandlerPool(_resolver, RowVisualHandlerRegistry.VisualHandlerLookup);
            _rowPool = new VariableRowPool();
            _uxml = Resources.Load<VisualTreeAsset>(_pathToUxml);

            _rootElement = new VisualElement();
            _inspectorRoot = _uxml.CloneTree();
            _inspectorRoot.style.marginTop = 5;
            _rootElement.Add(_inspectorRoot);

            BuildManager(_inspectorRoot);
            AddMigrationButton(_inspectorRoot);
            AddRefreshButton(_inspectorRoot);
        }

        protected void BuildManager(VisualElement rootElem)
        {
            VariableManagerComponent component = (VariableManagerComponent)target;
            if (component == null)
            {
                return;
            }

            _factoryInitArgs.Holder = rootElem;
            _factoryInitArgs.HandlerPool = _handlerPool;
            _factoryInitArgs.RowPool = _rowPool;
            _rowFactory.Init(_factoryInitArgs);

            ListView list = rootElem.Q<ListView>("rowList");
            
            UitkLabel count = rootElem.Q<UitkLabel>("varCountLabel");
            Button addBtn = rootElem.Q<Button>("addVarButton");

            VariableListViewInitArgs listViewArgs = new VariableListViewInitArgs()
            {
                List = list,
                CountLabel = count,
                RowFactory = _rowFactory,
                VariableSource = component,
                AssetResolver = new DefaultEditorAssetResolver(),
            };
            VariableListView view = new VariableListView(listViewArgs);

            VRowManagerInitArgs managerInitArgs = new VRowManagerInitArgs
            {
                Root = rootElem,
                AddButton = addBtn,
                VariableSource = component,
                VariableListView = view,
            };

            _manager.Init(managerInitArgs);
        }

        protected void AddMigrationButton(VisualElement rootElem)
        {
            Button migrateButton = new Button(OnMigrateClicked)
            {
                text = "Migrate from Flowchart",
            };

            migrateButton.style.height = 30;
            migrateButton.style.fontSize = 14;
            migrateButton.style.marginTop = 6;
            rootElem.Add(migrateButton);
        }

        private void OnMigrateClicked()
        {
            VariableManagerComponent component = (VariableManagerComponent)target;
            component.MigrateFromFlowchart(out _);
        }

        protected void AddRefreshButton(VisualElement rootElem)
        {
            Button refreshButton = new Button(OnRefreshClicked)
            {
                text = "Refresh",
                tooltip = "Might need to use this if you're getting errors about null vars on this GameObject."
            };

            refreshButton.style.height = 30;
            refreshButton.style.fontSize = 14;
            refreshButton.style.marginTop = 6;
            rootElem.Add(refreshButton);
        }

        private void OnRefreshClicked()
        {
            VariableManagerComponent component = (VariableManagerComponent)target;
            component.Refresh();
        }
    }
}