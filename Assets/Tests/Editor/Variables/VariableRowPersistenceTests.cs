using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using UnityObject = UnityEngine.Object;
using Amanita;
using UitkLabel = UnityEngine.UIElements.Label;
using Amanita.EditorUtils;
using UnityEngine.TestTools;
using System.Collections;

namespace VScriptingTests.VariableRows
{
    [TestFixture]
    public class VariableRowPersistenceTests
    {
        private Flowchart _flowchart;
        private AmanitaManager _amanitaManager;
        private VariableRowManager _rowManager;
        private VariableListView _variableListView;
        private VariableRowFactory _rowFactory;
        private VariableRowPool _rowPool;
        private RowVisualHandlerPool _handlerPool;
        private List<UnityObject> _objectsToDestroy;

        private IRowVisualTemplateProvider _originalTemplateProvider;
        private IRowVisualElementBuilder _originalElementBuilder;
        private TestTemplateProvider _testTemplateProvider;

        private class TestHostWindow : EditorWindow { }

        private TestHostWindow _uiHost;

        [SetUp]
        public void SetUp()
        {
            Undo.ClearAll();
            Flowchart.ResetStaticsForTest();

            _objectsToDestroy = new List<UnityObject>();

            _originalTemplateProvider = RowVisualTemplateProviderRegistry.Current;
            _originalElementBuilder = RowVisualElementBuilderRegistry.Current;

            _testTemplateProvider = new TestTemplateProvider();
            RowVisualTemplateProviderRegistry.Current = _testTemplateProvider;
            RowVisualElementBuilderRegistry.Current = new TestRowVisualElementBuilder();

            _amanitaManager = AmanitaManager.EnsureExists();
            _objectsToDestroy.Add(_amanitaManager.gameObject);

            var flowchartGO = new GameObject("TestFlowchart");
            _objectsToDestroy.Add(flowchartGO);
            _flowchart = flowchartGO.AddComponent<Flowchart>();
            _flowchart.AlwaysKeepGuid = false;

            _uiHost = ScriptableObject.CreateInstance<TestHostWindow>();
            InitializeVariableUi();
            _uiHost.rootVisualElement.Add(_uiRoot);
            _uiHost.ShowAuxWindow();
            _uiHost.rootVisualElement.schedule.Execute(() => { }).ExecuteLater(0);
            
        }

        private void InitializeVariableUi()
        {
            _rowPool = new VariableRowPool();
            _handlerPool = new RowVisualHandlerPool(new RowVisualHandlerResolver(), BuildHandlerLookup());

            _rowFactory = new VariableRowFactory();
            _rowFactory.Init(new VariableRowFactoryInitArgs
            {
                RowPool = _rowPool,
                HandlerPool = _handlerPool
            });

            var listView = new ListView();
            var countLabel = new UitkLabel();

            _variableListView = new VariableListView(new VariableListViewInitArgs
            {
                List = listView,
                CountLabel = countLabel,
                RowFactory = _rowFactory,
                VariableSource = _flowchart
            });

            _variableListView.SetFlowchart(_flowchart);

            _uiRoot = new VisualElement { name = "VariableRowTestsRoot" };
            _addButton = new Button { name = "TestAddButton" };
            _uiRoot.Add(_addButton);
            _uiRoot.Add(listView);
            _uiRoot.Add(countLabel);

            _rowManager = new VariableRowManager();
            _rowManager.Init(new VRowManagerInitArgs
            {
                Root = _uiRoot,
                AddButton = _addButton,
                VariableSource = _flowchart,
                VariableListView = _variableListView
            });

        }

        private VisualElement _uiRoot;
        private Button _addButton;

        [TearDown]
        public void TearDown()
        {
            _rowManager?.Dispose();
            _variableListView?.Dispose();
            _rowFactory?.Dispose();

            RowVisualTemplateProviderRegistry.Current = _originalTemplateProvider;
            RowVisualElementBuilderRegistry.Current = _originalElementBuilder;
            _testTemplateProvider?.Dispose();

            for (int i = _objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (_objectsToDestroy[i] != null)
                {
                    UnityObject.DestroyImmediate(_objectsToDestroy[i]);
                }
            }

            _objectsToDestroy.Clear();
            Undo.ClearAll();
        }

        [UnityTest]
        public IEnumerator ChangingColorVariableThroughRow_PersistsAndSupportsUndoRedo()
        {
            var variable = _flowchart.AddNewMuscariable<Color, ColorMuscariable>("ColorVar", Color.red);
            var targetValue = new Color(0.1f, 0.4f, 0.9f, 0.5f);

            yield return AssertValueChangePersists(variable, targetValue);
        }

        private IEnumerator AssertValueChangePersists<T>(Muscariable<T> variable, T newValue)
        {
            yield return null;
            Assert.NotNull(variable, "Variable creation failed.");

            VariableRow row = GetRowFor(variable);
            Assert.NotNull(row, "Variable row could not be materialized.");

            T originalValue = variable.Value;

            ApplyValueThroughUi(row, newValue);
            yield return null;
            Assert.AreEqual(newValue, variable.Value, "Value change was not applied.");

            Undo.PerformUndo();
            Assert.AreEqual(originalValue, variable.Value, "Undo did not restore the original value.");

            Undo.PerformRedo();
            Assert.AreEqual(newValue, variable.Value, "Redo did not reapply the edited value.");
        }

        private VariableRow GetRowFor(IVariable variable)
        {
            _variableListView.ForceMaterializeAllRowsForTests();

            int index = FindVariableIndex(variable);
            Assert.GreaterOrEqual(index, 0, "Variable was not found in the list view.");

            return _variableListView.RowAtIndex(index);
        }

        private int FindVariableIndex(IVariable variable)
        {
            var vars = _variableListView.VarsToDisplay;
            for (int i = 0; i < vars.Count; i++)
            {
                if (ReferenceEquals(vars[i], variable))
                {
                    return i;
                }
            }

            return -1;
        }

        

        private static void ApplyValueThroughUi<T>(VariableRow row, T newValue)
        {
            VisualElement valueElement = row.RootElement?.Q("ValueField");
            Assert.NotNull(valueElement, "Variable row is missing a ValueField element.");

            if (valueElement is INotifyValueChanged<float> floatField && newValue is float floatValue)
            {
                floatField.value = floatValue;
                // Use reflection to trigger the SetValueFieldChanged method since some fields may have custom change events
                AmanitaEditorSignals.ValueFieldChanged(row, floatValue);
                return;
            }

            if (valueElement is INotifyValueChanged<Color> colorField && newValue is Color colorValue)
            {
                colorField.value = colorValue;
                return;
            }

            Assert.Fail($"Unsupported value field type '{valueElement.GetType().Name}' for value '{typeof(T).Name}'.");
        }

        private IDictionary<Type, Type> BuildHandlerLookup()
        {
            return new Dictionary<Type, Type>
            {
                { typeof(float), typeof(FloatRowVisualHandler) },
                { typeof(Color), typeof(ColorVariableRow) },
                { typeof(object), typeof(DefaultRowVisualHandler) }
            };
        }

        [UnityTest]
        public IEnumerator ChangingFloatVariableThroughRow_PersistsAndSupportsUndoRedo()
        {
            var variable = _flowchart.AddNewMuscariable<float, FloatMuscariable>("FloatVar", 1f);
            var targetValue = 12.5f;

            yield return AssertValueChangePersists(variable, targetValue);
        }

        private sealed class TestTemplateProvider : IRowVisualTemplateProvider, IDisposable
        {
            private readonly List<VisualTreeAsset> _allocated = new List<VisualTreeAsset>();

            public VisualTreeAsset GetTemplate(Type handlerType)
            {
                var asset = ScriptableObject.CreateInstance<HandlerAwareVisualTreeAsset>();
                asset.HandlerType = handlerType;
                _allocated.Add(asset);
                return asset;
            }

            public void ClearCache()
            {
                Dispose();
            }

            public void Dispose()
            {
                foreach (var asset in _allocated)
                {
                    if (asset != null)
                    {
                        UnityObject.DestroyImmediate(asset);
                    }
                }

                _allocated.Clear();
            }
        }

        private sealed class HandlerAwareVisualTreeAsset : VisualTreeAsset
        {
            public Type HandlerType;
        }

        private sealed class TestRowVisualElementBuilder : IRowVisualElementBuilder
        {
            public RowVisualElements Build(VisualTreeAsset template)
            {
                var handlerAware = template as HandlerAwareVisualTreeAsset;

                var root = new VisualElement();
                var keyField = new TextField { isDelayed = true, multiline = false, name = "KeyInput" };
                var valueHolder = new VisualElement { name = "ValueFieldHolder" };
                var scopeField = new EnumField(VariableScope.Private) { name = "Scope" };
                var removeButton = new Button { name = "RemoveButton" };

                IBindable valueField = CreateValueField(handlerAware?.HandlerType);
                if (valueField is VisualElement valueElement)
                {
                    valueElement.name = "ValueField";
                    valueHolder.Add(valueElement);
                }

                root.Add(keyField);
                root.Add(valueHolder);
                root.Add(scopeField);
                root.Add(removeButton);

                return new RowVisualElements(root, keyField, valueHolder, valueField, scopeField, removeButton);
            }

            private static IBindable CreateValueField(Type handlerType)
            {
                if (handlerType == typeof(ColorVariableRow))
                {
                    return new ColorField();
                }

                if (handlerType == typeof(FloatRowVisualHandler))
                {
                    return new FloatField();
                }

                return new TextField();
            }
        }
    }
}