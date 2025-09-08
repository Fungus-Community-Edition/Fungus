using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityEngine.TestTools;
using UitkLabel = UnityEngine.UIElements.Label;

namespace Amanita.Tests.EditMode
{
    /// <summary>
    /// Focused tests for VariableListView independent of VariableRowManager.
    /// </summary>
    public class VariableListViewTests
    {
        GameObject _host;
        ListView _uiList;
        UITKLabel _countLabel;
        VariableListView _view;
        VariableRowFactory _factory;
        VariableRowFactoryInitArgs _factoryArgs;
        VariableRowPool _rowPool;
        RowVisualHandlerPool _handlerPool;
        IRowVisualHandlerResolver _resolver;

        readonly List<IVariable> _createdVars = new();

        FieldInfo _fiVariables;
        MethodInfo _miOnItemIndexChanged;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("VarsHost");

            _uiList = new ListView();
            _countLabel = new UITKLabel();

            _resolver = new RowVisualHandlerResolver();
            _handlerPool = new RowVisualHandlerPool(_resolver, RowVisualHandlerRegistry.VisualHandlerLookup);
            _rowPool = new VariableRowPool();

            _factory = new VariableRowFactory();
            _factoryArgs = new VariableRowFactoryInitArgs
            {
                RowPool = _rowPool,
                HandlerPool = _handlerPool,
                Holder = null
            };
            _factory.Init(_factoryArgs);

            var listViewArgs = new VariableListViewInitArgs()
            {
                List = _uiList,
                CountLabel = _countLabel,
                RowFactory = _factory,
                
            };
            _view = new VariableListView(listViewArgs);

            _fiVariables = typeof(VariableListView)
                .GetField("_variables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(_fiVariables, "_variables field not found");

            _miOnItemIndexChanged = typeof(VariableListView)
                .GetMethod("OnItemIndexChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(_miOnItemIndexChanged, "OnItemIndexChanged method not found");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var v in _createdVars)
                if (v is Component c) UnityEngine.Object.DestroyImmediate(c);

            _createdVars.Clear();
            _view?.Dispose();
            _factory?.Dispose();

            if (_host != null)
                UnityEngine.Object.DestroyImmediate(_host);
        }

        // Helpers -------------------------------------------------------------

        TComp CreateVar<TComp, TValue>(string key, TValue value = default)
            where TComp : Component, IVariable
        {
            var v = _host.AddComponent<TComp>();
            v.Key = key;
            TrySetStrongValue(v, value);
            _createdVars.Add(v);
            return v;
        }

        // Fix for AmbiguousMatchException:
        // IVariable<T> introduces a strongly typed Value property hiding IVariable.Value (object).
        // Reflection GetProperty("Value") was ambiguous. We pick the non-object one if present.
        static void TrySetStrongValue<TValue>(IVariable variable, TValue value)
        {
            if (Equals(value, default(TValue))) return; // skip default to avoid unintended overwrite

            var type = variable.GetType();
            var members = type.GetMember("Value", MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public);
            if (members == null || members.Length == 0) return;

            PropertyInfo chosen = null;

            if (members.Length == 1)
            {
                chosen = (PropertyInfo)members[0];
            }
            else
            {
                // Prefer the most derived, strongly-typed (not object) property
                chosen = members
                    .OfType<PropertyInfo>()
                    .Where(p => p.CanWrite && p.PropertyType != typeof(object))
                    .OrderByDescending(p => p.DeclaringType != typeof(IVariable)) // derived first
                    .FirstOrDefault() ?? members.OfType<PropertyInfo>().First();
            }

            if (chosen != null && chosen.CanWrite)
            {
                try
                {
                    chosen.SetValue(variable, value);
                }
                catch
                {
                    // Silently ignore if type mismatch; tests only need successful assignments
                }
            }
        }

        List<IVariable> InternalVariables => (List<IVariable>)_fiVariables.GetValue(_view);

        void InvokeReorder(int from, int to) =>
            _miOnItemIndexChanged.Invoke(_view, new object[] { from, to });

        // Tests ----------------------------------------------------------------

        [Test]
        public void AddVariable_UpdatesCountLabel_AndNoDuplicates()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var v2 = CreateVar<StringVariable, string>("s1", "a");

            _view.AddVariable(v1);
            Assert.AreEqual("Count: 1", _countLabel.text);
            _view.AddVariable(v2);
            Assert.AreEqual("Count: 2", _countLabel.text);

            _view.AddVariable(v1); // duplicate
            Assert.AreEqual("Count: 2", _countLabel.text);
            Assert.AreEqual(2, InternalVariables.Count);
        }

        [Test]
        public void RemoveVariable_UpdatesCountLabel()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var v2 = CreateVar<StringVariable, string>("s1", "a");
            _view.AddVariable(v1);
            _view.AddVariable(v2);

            _view.RemoveVariable(v1);
            Assert.AreEqual("Count: 1", _countLabel.text);
            Assert.False(InternalVariables.Contains(v1));
            Assert.True(InternalVariables.Contains(v2));
        }

        [Test]
        public void SetVariables_ReplacesCollection()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var v2 = CreateVar<StringVariable, string>("s1", "a");
            _view.SetVariables(new IVariable[] { v1, v2 });

            Assert.AreEqual(2, InternalVariables.Count);
            Assert.AreEqual("Count: 2", _countLabel.text);

            var v3 = CreateVar<IntegerVariable, int>("i1", 5);
            _view.SetVariables(new[] { v3 });

            Assert.AreEqual(1, InternalVariables.Count);
            Assert.AreSame(v3, InternalVariables[0]);
            Assert.AreEqual("Count: 1", _countLabel.text);
        }

        [Test]
        public void Clear_RemovesAll()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var v2 = CreateVar<StringVariable, string>("s1", "a");
            _view.SetVariables(new IVariable[] { v1, v2 });

            _view.Clear();
            Assert.AreEqual(0, InternalVariables.Count);
            Assert.AreEqual("Count: 0", _countLabel.text);
        }

        [Test]
        public void Refresh_DoesNotChangeOrderOrCount()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var v2 = CreateVar<StringVariable, string>("s1", "a");
            _view.SetVariables(new IVariable[] { v1, v2 });

            _view.Refresh();
            Assert.AreEqual(2, InternalVariables.Count);
            CollectionAssert.AreEqual(new IVariable[] { v1, v2 }, InternalVariables);
            Assert.AreEqual("Count: 2", _countLabel.text);
        }

        [Test]
        public void OrderChanged_Fires_OnReorder_Down()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var v2 = CreateVar<StringVariable, string>("s1", "a");
            var v3 = CreateVar<IntegerVariable, int>("i1", 5);
            _view.SetVariables(new IVariable[] { v1, v2, v3 });

            IReadOnlyList<IVariable> lastOrder = null;
            int eventCount = 0;
            _view.OrderChanged += o => { eventCount++; lastOrder = o; };

            // Simulate Unity internal reorder (list already mutated)
            InternalVariables.RemoveAt(0);
            InternalVariables.Add(v1);
            InvokeReorder(0, 3);

            Assert.AreEqual(1, eventCount);
            CollectionAssert.AreEqual(new IVariable[] { v2, v3, v1 }, lastOrder);
            CollectionAssert.AreEqual(lastOrder, InternalVariables);
        }

        [Test]
        public void OrderChanged_Fires_OnReorder_Up()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var v2 = CreateVar<StringVariable, string>("s1", "a");
            var v3 = CreateVar<IntegerVariable, int>("i1", 5);
            _view.SetVariables(new IVariable[] { v1, v2, v3 });

            IReadOnlyList<IVariable> lastOrder = null;
            _view.OrderChanged += o => lastOrder = o;

            InternalVariables.RemoveAt(2);
            InternalVariables.Insert(0, v3);
            InvokeReorder(2, 0);

            CollectionAssert.AreEqual(new IVariable[] { v3, v1, v2 }, lastOrder);
            CollectionAssert.AreEqual(lastOrder, InternalVariables);
        }

        [Test]
        public void OnItemIndexChanged_NoChange_NoEvent()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var v2 = CreateVar<StringVariable, string>("s1", "a");
            _view.SetVariables(new IVariable[] { v1, v2 });

            int eventCount = 0;
            _view.OrderChanged += _ => eventCount++;

            InvokeReorder(1, 1);
            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void RowAtIndex_NullWhenNotMaterialized()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            _view.AddVariable(v1);
            Assert.IsNull(_view.RowAtIndex(0));
        }

        [Test]
        public void Dispose_ClearsInternalState()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            _view.AddVariable(v1);
            Assert.AreEqual(1, InternalVariables.Count);

            _view.Dispose();
            Assert.AreEqual(0, InternalVariables.Count);
            Assert.DoesNotThrow(() => _view.Dispose());
        }

        [Test]
        public void AddVariable_AfterDispose_LogWarning()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            _view.Dispose();
            LogAssert.Expect(LogType.Warning, "Tried to add variable to disposed VariableListView.");
            _view.AddVariable(v1);
        }

        [Test]
        public void AddVariable_NullVar_LogWarning()
        {
            IVariable nullVar = null;
            LogAssert.Expect(LogType.Warning, "Tried to add a null variable to VariableListView.");
            _view.AddVariable(nullVar);
        }

        [Test]
        public void AcquireFlowchartIfLost_ReacquiresViaGlobalObjectId()
        {
            // Arrange: create a Flowchart in the scene
            var go = new GameObject("FlowchartHost");
            var flowchart = go.AddComponent<Flowchart>();

            // Hook it into the view
            _view.SetFlowchart(flowchart);

            // Simulate losing the reference (as if after undo/redo)
            var fiFlowchart = typeof(VariableListView)
                .GetField("_flowchart", BindingFlags.Instance | BindingFlags.NonPublic);
            fiFlowchart.SetValue(_view, null);

            // Act: call AcquireFlowchartIfLost
            var miAcquire = typeof(VariableListView)
                .GetMethod("AcquireFlowchartIfLost", BindingFlags.Instance | BindingFlags.NonPublic);
            bool reacquired = (bool)miAcquire.Invoke(_view, null);

            // Assert: reacquired and matches original
            Assert.IsTrue(reacquired, "Flowchart should be reacquired");
            var reacquiredFlowchart = (Flowchart)fiFlowchart.GetValue(_view);
            Assert.AreSame(flowchart, reacquiredFlowchart);
        }

        [Test]
        public void HandleUndoRedoPerformed_CallsSyncFromFlowchart()
        {
            var go = new GameObject("FlowchartHost");
            try
            {
                var flowchart = go.AddComponent<Flowchart>();

                // Directly set the serialized legacy list
                AssignLegacyVariables(flowchart, new List<Variable>());

                var testView = new TestVariableListView(new VariableListViewInitArgs
                {
                    List = new ListView(),
                    CountLabel = new UitkLabel(),
                    RowFactory = _factory
                });

                testView.SetFlowchart(flowchart);

                // Act
                var miHandleUndoRedo = typeof(VariableListView)
                    .GetMethod("HandleUndoRedoPerformed", BindingFlags.Instance | BindingFlags.NonPublic);
                miHandleUndoRedo.Invoke(testView, null);

                // Assert
                Assert.IsTrue(testView.SyncCalled, "SyncFromFlowchart should be called after undo/redo");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SyncFromFlowchart_PopulatesVariablesFromFlowchart()
        {
            // Arrange
            var go = new GameObject("FlowchartHost");
            var flowchart = go.AddComponent<Flowchart>();

            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var v2 = CreateVar<StringVariable, string>("s1", "a");


            AssignLegacyVariables(flowchart, new List<Variable> { v1, v2 });

            _view.SetFlowchart(flowchart);

            // Act
            var miSync = typeof(VariableListView)
                .GetMethod("SyncFromFlowchart", BindingFlags.Instance | BindingFlags.NonPublic);
            miSync.Invoke(_view, null);

            // Assert
            var internalVars = (List<IVariable>)_fiVariables.GetValue(_view);
            CollectionAssert.AreEqual(new IVariable[] { v1, v2 }, internalVars);
        }

        [Test]
        public void SyncFromFlowchart_SkipsNullOrDestroyedVariables()
        {
            // Arrange
            var go = new GameObject("FlowchartHost");
            var flowchart = go.AddComponent<Flowchart>();

            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            var destroyedVar = CreateVar<StringVariable, string>("s1", "a");
            UnityEngine.Object.DestroyImmediate((UnityEngine.Object)destroyedVar);

            AssignLegacyVariables(flowchart, new List<Variable> { v1, destroyedVar, null });

            _view.SetFlowchart(flowchart);

            // Act
            var miSync = typeof(VariableListView)
                .GetMethod("SyncFromFlowchart", BindingFlags.Instance | BindingFlags.NonPublic);
            miSync.Invoke(_view, null);

            // Assert
            var internalVars = (List<IVariable>)_fiVariables.GetValue(_view);
            CollectionAssert.AreEqual(new[] { v1 }, internalVars);
        }

        static void AssignLegacyVariables(Flowchart flowchart, List<Variable> variables)
        {
            var field = typeof(Flowchart).GetField("_legacyVariables",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                Assert.Fail("Could not find _legacyVariables field on Flowchart");

            field.SetValue(flowchart, variables);
        }

        class TestVariableListView : VariableListView
        {
            public bool SyncCalled;

            public TestVariableListView(VariableListViewInitArgs initArgs) : base(initArgs) { }

            protected override void SyncFromFlowchart()
            {
                SyncCalled = true;
                base.SyncFromFlowchart();
            }
        }
    }

}

    