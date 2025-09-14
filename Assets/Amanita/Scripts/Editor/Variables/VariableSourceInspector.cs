using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace Amanita.VScripting.EditorUtils
{
    [CustomEditor(typeof(VariableSource))]
    public class VariableSourceInspector : Editor
    {
        protected virtual void OnEnable()
        {
            
        }

        protected RowVisualHandlerPool handlerPool;
        protected readonly IRowVisualHandlerResolver _resolver = new RowVisualHandlerResolver();
        protected VariableRowPool rowPool;
        protected VisualTreeAsset uxml;
        protected readonly string pathToUxml = "_EditorResources/UIToolkitTemplates/VariableDisplayEditor";

        protected void BuildManager(VisualElement rootElem)
        {
            var varSource = (VariableSource)target;
            if (varSource == null)
                return;

            _manager?.Dispose();
            _manager = new VariableRowManager();

            var holder = rootElem;

            PrepFactory();
            void PrepFactory()
            {
                _factoryInitArgs.Holder = holder;
                _factoryInitArgs.HandlerPool = handlerPool;
                _factoryInitArgs.RowPool = rowPool;
                _rowFactory.Init(_factoryInitArgs);
            }

            VariableListView view;
            Button addBtn;
            PrepVarListView();
            void PrepVarListView()
            {
                var list = rootElem.Q<ListView>("rowList");
                var count = rootElem.Q<UitkLabel>("varCountLabel");
                addBtn = rootElem.Q<Button>("addVarButton");

                var listViewArgs = new VariableListViewInitArgs()
                {
                    List = list,
                    CountLabel = count,
                    RowFactory = _rowFactory,
                };
                view = new VariableListView(listViewArgs);
            }

            InitManager();
            void InitManager()
            {
                VRowManagerInitArgs managerInitArgs = new VRowManagerInitArgs
                {
                    Root = rootElem,
                    AddButton = addBtn,
                    VariableSource = varSource,
                    VariableListView = view,
                };
                _manager.Init(managerInitArgs);
            }
        }

        protected VariableRowManager _manager;
        protected VariableRowFactoryInitArgs _factoryInitArgs = new VariableRowFactoryInitArgs();
        protected VariableRowFactory _rowFactory = new VariableRowFactory();

        public override VisualElement CreateInspectorGUI()
        {
            var visualHandlerLookup = RowVisualHandlerRegistry.VisualHandlerLookup;
            handlerPool ??= new RowVisualHandlerPool(_resolver, visualHandlerLookup);
            rowPool ??= new VariableRowPool();
            uxml = Resources.Load<VisualTreeAsset>(pathToUxml);

            rootElement = new VisualElement();
            
            inspectorRoot = uxml.CloneTree();
            rootElement.Add(inspectorRoot);
            BuildManager(inspectorRoot);
            return rootElement;
        }

        protected VisualElement rootElement;
        protected TemplateContainer inspectorRoot;

        protected virtual void OnDisable()
        {
            _manager?.Dispose();
            inspectorRoot = null;
            rootElement = null;
        }

    }
}