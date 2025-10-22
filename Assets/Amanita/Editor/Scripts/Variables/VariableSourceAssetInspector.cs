using Amanita.EditorUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace Amanita.VScripting.EditorUtils
{
    [CustomEditor(typeof(VariableSourceAsset))]
    public class VariableSourceAssetInspector : Editor
    {
        protected virtual void OnEnable()
        {
            var target = (VariableSourceAsset)this.target;
            target.Refresh(); // Ensure variable ownership is properly asserted
            PrepGUI();
            ToggleSubs(false);
            ToggleSubs(true);
        }

        protected virtual void PrepGUI()
        {
            _manager = new VariableRowManager();
            var visualHandlerLookup = RowVisualHandlerRegistry.VisualHandlerLookup;
            handlerPool ??= new RowVisualHandlerPool(_resolver, visualHandlerLookup);
            rowPool ??= new VariableRowPool();
            uxml = Resources.Load<VisualTreeAsset>(pathToUxml);

            rootElement = new VisualElement();

            inspectorRoot = uxml.CloneTree();
            rootElement.Add(inspectorRoot);
            BuildManager(inspectorRoot);
        }

        protected VariableRowManager _manager;
        protected RowVisualHandlerPool handlerPool;
        protected VariableRowPool rowPool;
        protected VisualTreeAsset uxml;
        protected readonly string pathToUxml = "UIToolkitTemplates/VariableDisplayEditor";
        protected VisualElement rootElement;
        protected TemplateContainer inspectorRoot;

        protected void BuildManager(VisualElement rootElem)
        {
            var varSource = (VariableSourceAsset)target;
            if (varSource == null)
                return;

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
                    VariableSource = varSource,
                    AssetResolver = new DefaultEditorAssetResolver(),
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

        protected VariableRowFactoryInitArgs _factoryInitArgs = new VariableRowFactoryInitArgs();
        protected VariableRowFactory _rowFactory = new VariableRowFactory();

        protected virtual void ToggleSubs(bool on)
        {
            var source = (VariableSourceAsset)target;
            if (on)
            {
                AmanitaEditorSignals.VarRowControlLostFocus += OnVarRowControlLostFocus;
                source.VariableAdded += OnVariableAdded;
                source.VariableRemoved += OnVariableRemoved;
                source.VariablesReordered += UpdateSourceAssetFile;
                source.Refreshed += UpdateSourceAssetFile;
            }
            else
            {
                AmanitaEditorSignals.VarRowControlLostFocus -= OnVarRowControlLostFocus;
                source.VariableAdded -= OnVariableAdded;
                source.VariableRemoved -= OnVariableRemoved;
                source.VariablesReordered -= UpdateSourceAssetFile;
                source.Refreshed -= UpdateSourceAssetFile;
            }
        }

        protected virtual void OnVarRowControlLostFocus(FocusOutEvent evt)
        {
            UpdateSourceAssetFile();
        }

        protected virtual void UpdateSourceAssetFile()
        {
            if (target is VariableSourceAsset source)
            {
                EditorUtility.SetDirty(source);
                AssetDatabase.SaveAssetIfDirty(source);
                Debug.Log($"VariableSourceInspector: Updated source asset file");
            }
        }

        private void OnVariableRemoved(IVariable variable)
        {
            UpdateSourceAssetFile();
        }

        private void OnVariableAdded(IVariable variable)
        {
            UpdateSourceAssetFile();
        }

        protected readonly IRowVisualHandlerResolver _resolver = new RowVisualHandlerResolver();
        
        // This executes twice in a row when the asset is clicked, and then once again when you click some
        // other asset
        public override VisualElement CreateInspectorGUI()
        {
            return rootElement;
        }

        protected virtual void OnDisable()
        {
            _manager?.Dispose();
            _manager = null;
            inspectorRoot = null;
            rootElement = null;
            ToggleSubs(false);
        }

    }
}