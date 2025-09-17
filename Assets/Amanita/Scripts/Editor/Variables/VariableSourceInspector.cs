using Amanita.EditorUtils;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace Amanita.VScripting.EditorUtils
{
    [CustomEditor(typeof(VariableSourceAsset))]
    public class VariableSourceInspector : Editor
    {
        protected virtual void OnEnable()
        {
            ToggleSubs(true);
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                AmanitaEditorSignals.VarRowControlLostFocus += OnVarRowControlLostFocus;

                VariableSourceAsset source = (VariableSourceAsset)target;
                source.VariableAdded += OnVariableAdded;
                source.VariableRemoved += OnVariableRemoved;
                source.VariablesReordered += UpdateSourceAssetFile;
                source.Refreshed += UpdateSourceAssetFile;
            }
            else
            {
                AmanitaEditorSignals.VarRowControlLostFocus -= OnVarRowControlLostFocus;

                VariableSourceAsset source = (VariableSourceAsset)target;
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

        protected RowVisualHandlerPool handlerPool;
        protected readonly IRowVisualHandlerResolver _resolver = new RowVisualHandlerResolver();
        protected VariableRowPool rowPool;
        protected VisualTreeAsset uxml;
        protected readonly string pathToUxml = "_EditorResources/UIToolkitTemplates/VariableDisplayEditor";

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

                _manager?.Dispose();
                _manager = new VariableRowManager();
                _manager.Init(managerInitArgs);
            }
        }

        protected VariableRowManager _manager;
        protected VariableRowFactoryInitArgs _factoryInitArgs = new VariableRowFactoryInitArgs();
        protected VariableRowFactory _rowFactory = new VariableRowFactory();

        // This executes twice in a row when the asset is clicked, and then once again when you click some
        // other asset
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
            ToggleSubs(false);
        }

    }
}