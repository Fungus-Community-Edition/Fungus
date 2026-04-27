using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;
using Type = System.Type;
using StringComparison = System.StringComparison;
using AtMycelia.EditorUtils;

namespace VScriptingTests.VariableOperations
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
                AssetResolver = new DefaultEditorAssetResolver(),
            };
            _view = new VariableListView(listViewArgs);

            _fiVariables = viewType.GetField("varsToDisplay", bindingFlags);
            Assert.NotNull(_fiVariables, "varsToDisplay field not found");

            _miOnItemIndexChanged = viewType.GetMethod("OnItemReordered", bindingFlags);
            Assert.NotNull(_miOnItemIndexChanged, "OnItemReordered method not found");
        }

        protected static readonly Type viewType = typeof(VariableListView);
        protected static readonly BindingFlags bindingFlags = BindingFlags.Instance | 
            BindingFlags.NonPublic |
            BindingFlags.Public;

        [TearDown]
        public void TearDown()
        {
            foreach (var elem in _createdVars)
                if (elem is Component legacyVarComponent) UnityObj.DestroyImmediate(legacyVarComponent);

            _createdVars.Clear();
            _view?.Dispose();
            _factory?.Dispose();

            if (_host != null)
                UnityObj.DestroyImmediate(_host);
        }

        // Helpers -------------------------------------------------------------

        TComp CreateVar<TComp, TValue>(string key, TValue value = default)
            where TComp : Component, IVariable
        {
            var addedLegacyVar = _host.AddComponent<TComp>();
            addedLegacyVar.Key = key;
            TrySetStrongValue(addedLegacyVar, value);
            _createdVars.Add(addedLegacyVar);
            return addedLegacyVar;
        }

        TComp CreateVarOnHost<TComp, TValue>(GameObject host, string key, TValue value = default)
    where TComp : Component, IVariable
        {
            var addedLegacyVar = host.AddComponent<TComp>();
            addedLegacyVar.Key = key;
            TrySetStrongValue(addedLegacyVar, value);
            _createdVars.Add(addedLegacyVar);
            return addedLegacyVar;
        }

        // Fix for AmbiguousMatchException:
        // IVariable<T> introduces a strongly typed Value property hiding IVariable.Value (object).
        // Reflection GetProperty("Value") was ambiguous. We pick the non-object one if present.
        static void TrySetStrongValue<TValue>(IVariable variable, TValue value)
        {
            if (Equals(value, default(TValue))) return; // skip default to avoid unintended overwrite

            var type = variable.GetType();
            var members = type.GetMember("Value", MemberTypes.Property, bindingFlags);
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
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            var secondVar = CreateVar<StringVariable, string>("s1", "a");

            _view.AddVariable(firstVar);
            Assert.AreEqual("Count: 1", _countLabel.text);
            _view.AddVariable(secondVar);
            Assert.AreEqual("Count: 2", _countLabel.text);

            _view.AddVariable(firstVar); // duplicate
            Assert.AreEqual("Count: 2", _countLabel.text);
            Assert.AreEqual(2, InternalVariables.Count);
        }

        [Test]
        public void RemoveVariable_UpdatesCountLabel()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            var secondVar = CreateVar<StringVariable, string>("s1", "a");
            _view.AddVariable(firstVar);
            _view.AddVariable(secondVar);

            _view.RemoveVariable(firstVar);
            Assert.AreEqual("Count: 1", _countLabel.text);
            Assert.False(InternalVariables.Contains(firstVar));
            Assert.True(InternalVariables.Contains(secondVar));
        }

        [Test]
        public void SetVariables_ReplacesCollection()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            var secondVar = CreateVar<StringVariable, string>("s1", "a");
            _view.SetVariables(new IVariable[] { firstVar, secondVar });

            Assert.AreEqual(2, InternalVariables.Count);
            Assert.AreEqual("Count: 2", _countLabel.text);

            var thirdVar = CreateVar<IntegerVariable, int>("i1", 5);
            _view.SetVariables(new[] { thirdVar });

            Assert.AreEqual(1, InternalVariables.Count);
            Assert.AreSame(thirdVar, InternalVariables[0]);
            Assert.AreEqual("Count: 1", _countLabel.text);
        }

        [Test]
        public void Clear_RemovesAll()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            var secondVar = CreateVar<StringVariable, string>("s1", "a");
            _view.SetVariables(new IVariable[] { firstVar, secondVar });

            _view.Clear();
            Assert.AreEqual(0, InternalVariables.Count);
            Assert.AreEqual("Count: 0", _countLabel.text);
        }

        [Test]
        public void Refresh_DoesNotChangeOrderOrCount()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            var secondVar = CreateVar<StringVariable, string>("s1", "a");
            _view.SetVariables(new IVariable[] { firstVar, secondVar });

            _view.Refresh();
            Assert.AreEqual(2, InternalVariables.Count);
            CollectionAssert.AreEqual(new IVariable[] { firstVar, secondVar }, InternalVariables);
            Assert.AreEqual("Count: 2", _countLabel.text);
        }

        [Test]
        public void OrderChanged_Fires_OnReorder_Down()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            var secondVar = CreateVar<StringVariable, string>("s1", "a");
            var thirdVar = CreateVar<IntegerVariable, int>("i1", 5);
            _view.SetVariables(new IVariable[] { firstVar, secondVar, thirdVar });

            IReadOnlyList<IVariable> lastOrder = null;
            int eventCount = 0;
            _view.OrderChanged += o => { eventCount++; lastOrder = (IReadOnlyList<IVariable>)o; };

            // Simulate Unity internal reorder (list already mutated)
            InternalVariables.RemoveAt(0);
            InternalVariables.Add(firstVar);
            InvokeReorder(0, 3);

            Assert.AreEqual(1, eventCount);
            CollectionAssert.AreEqual(new IVariable[] { secondVar, thirdVar, firstVar }, lastOrder);
            CollectionAssert.AreEqual(lastOrder, InternalVariables);
        }

        [Test]
        public void OrderChanged_Fires_OnReorder_Up()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            var secondVar = CreateVar<StringVariable, string>("s1", "a");
            var thirdVar = CreateVar<IntegerVariable, int>("i1", 5);
            _view.SetVariables(new IVariable[] { firstVar, secondVar, thirdVar });

            IReadOnlyList<IVariable> lastOrder = null;
            _view.OrderChanged += elem => lastOrder = (IReadOnlyList<IVariable>)elem;

            InternalVariables.RemoveAt(2);
            InternalVariables.Insert(0, thirdVar);
            InvokeReorder(2, 0);

            CollectionAssert.AreEqual(new IVariable[] { thirdVar, firstVar, secondVar }, lastOrder);
            CollectionAssert.AreEqual(lastOrder, InternalVariables);
        }

        [Test]
        public void OnItemIndexChanged_NoChange_NoEvent()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            var secondVar = CreateVar<StringVariable, string>("s1", "a");
            _view.SetVariables(new IVariable[] { firstVar, secondVar });

            int eventCount = 0;
            _view.OrderChanged += _ => eventCount++;

            InvokeReorder(1, 1);
            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void RowAtIndex_NullWhenNotMaterialized()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            _view.AddVariable(firstVar);
            Assert.IsNull(_view.RowAtIndex(0));
        }

        // New test reproducing the old phantom-row behavior and verifying the reference-equality fix.
        [Test]
        public void AddMultiple_Variables_WithValueBasedEquals_DoNotCollide_InActiveRowMap()
        {
            // Arrange: create three distinct component variables that intentionally override equality
            var a = _host.AddComponent<ValueEqualsVariable>();
            a.Key = "same-key";
            a.ItemId = 1;
            _createdVars.Add(a);

            var b = _host.AddComponent<ValueEqualsVariable>();
            b.Key = "same-key";
            b.ItemId = 2;
            _createdVars.Add(b);

            var c = _host.AddComponent<ValueEqualsVariable>();
            c.Key = "same-key";
            c.ItemId = 3;
            _createdVars.Add(c);

            // Sanity: distinct references but value-equality says they are equal
            Assert.AreNotSame(a, b);
            Assert.IsTrue(a.Equals(b) && b.Equals(c));

            // Act: add them to the view
            _view.AddVariable(a);
            _view.AddVariable(b);
            _view.AddVariable(c);

            // Ensure the source list contains all three
            Assert.AreEqual(3, InternalVariables.Count);

            // Materialize rows as the ListView virtualization would do
            _view.ForceMaterializeAllRowsForTests();

            // Assert: each variable has its own materialized row (no collisions in _activeRows)
            Assert.AreEqual(3, _view.Rows.Count, "Expected three active rows; value-based Equals should not collapse distinct instances.");

            var row0 = _view.RowAtIndex(0);
            var row1 = _view.RowAtIndex(1);
            var row2 = _view.RowAtIndex(2);

            Assert.IsNotNull(row0);
            Assert.IsNotNull(row1);
            Assert.IsNotNull(row2);

            Assert.AreNotSame(row0, row1);
            Assert.AreNotSame(row1, row2);
            Assert.AreNotSame(row0, row2);

            // Also ensure the visuals were created
            Assert.IsNotNull(row0.RootElement);
            Assert.IsNotNull(row1.RootElement);
            Assert.IsNotNull(row2.RootElement);
        }

        [Test]
        public void Dispose_ClearsInternalState()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            _view.AddVariable(firstVar);
            Assert.AreEqual(1, InternalVariables.Count);

            _view.Dispose();
            Assert.AreEqual(0, InternalVariables.Count);
            Assert.DoesNotThrow(() => _view.Dispose());
        }

        [Test]
        public void AddVariable_AfterDispose_LogWarning()
        {
            var firstVar = CreateVar<FloatVariable, float>("f1", 1f);
            _view.Dispose();
            LogAssert.Expect(LogType.Warning, "Tried to add variable to disposed VariableListView.");
            _view.AddVariable(firstVar);
        }

        [Test]
        public void AddVariable_NullVar_LogWarning()
        {
            IVariable nullVar = null;
            LogAssert.Expect(LogType.Warning, "Tried to add a null variable to VariableListView.");
            _view.AddVariable(nullVar);
        }

        [Test]
        public void AcquireSourceIfLost_ReacquiresByUniqueId()
        {
            // Arrange: create a Flowchart in the scene
            var fcHolder = new GameObject("FlowchartHost");
            var flowchart = fcHolder.AddComponent<Flowchart>();
            flowchart.ForceResetUid();

            // Hook it into the view
            _view.SetSource(flowchart);

            // Simulate losing the reference (as if after undo/redo)
            var fiSource = viewType.GetField("_source", bindingFlags);
            fiSource.SetValue(_view, null);

            // Act: call AcquireSourceIfLost
            var miAcquire = viewType.GetMethod("AcquireSourceIfLost", bindingFlags);
            bool reacquired = (bool)miAcquire.Invoke(_view, null);

            // Assert: reacquired and matches original
            Assert.IsTrue(reacquired, "Source should be reacquired");
            var reacquiredSource = (IVariableSource)fiSource.GetValue(_view);
            Assert.AreSame(flowchart, reacquiredSource);
        }

        [Test]
        public void HandleUndoRedoPerformed_CallsSyncFromSource()
        {
            var fcHost = new GameObject("FlowchartHost");
            try
            {
                var flowchart = fcHost.AddComponent<Flowchart>();
                var varManager = flowchart.GetComponent<VariableManagerComponent>();
                Assert.IsNotNull(varManager, "VariableManagerComponent not found on Flowchart.");

                varManager.AddNewVariableOfContentType<int>("testVar", 10);

                var testView = new TestVariableListView(new VariableListViewInitArgs
                {
                    List = new ListView(),
                    CountLabel = new UITKLabel(),
                    RowFactory = _factory
                });

                testView.SetSource(flowchart);

                // Act
                var miHandleUndoRedo = viewType.GetMethod("HandleUndoRedoPerformed", bindingFlags);
                miHandleUndoRedo.Invoke(testView, null);

                // Assert
                Assert.IsTrue(testView.SyncCalled, "SyncFromFlowchart should be called after undo/redo");
            }
            finally
            {
                UnityObj.DestroyImmediate(fcHost);
            }
        }

        [Test]
        public void SyncFromSource_PopulatesVariablesFromFlowchart()
        {
            // Arrange
            var fcHost = new GameObject("FlowchartHost");
            var flowchart = fcHost.AddComponent<Flowchart>();
            var varManager = flowchart.GetComponent<VariableManagerComponent>();
            Assert.IsNotNull(varManager, "VariableManagerComponent not found on Flowchart.");

            var firstVar = varManager.AddNewVariableOfContentType<float>("f1", 1f);
            var secondVar = varManager.AddNewVariableOfContentType<string>("s1", "a");

            _view.SetSource(flowchart);

            // Act
            var miSync = viewType.GetMethod("SyncFromSource", bindingFlags);
            miSync.Invoke(_view, null);

            // Assert
            var internalVars = (List<IVariable>)_fiVariables.GetValue(_view);
            CollectionAssert.AreEqual(new IVariable[] { firstVar, secondVar }, internalVars);
        }

        [Test]
        public void SyncFromSource_SkipsNullOrDestroyedVariables()
        {
            var destroyedHost = new GameObject("DestroyedVarHost");
            try
            {
                var manager = new VariableManager();
                var firstVar = manager.AddNewVariableOfContentType<float>("f1", 1f);

                var destroyedVar = destroyedHost.AddComponent<FloatVariable>();
                destroyedVar.Key = "dead";
                UnityObj.DestroyImmediate(destroyedVar);

                var source = new StubVariableSource(new IVariable[]
                {
                    firstVar,
                    null,
                    destroyedVar,
                });

                _view.SetSource(source);

                // Act
                var miSync = viewType.GetMethod("SyncFromSource", bindingFlags);
                miSync.Invoke(_view, null);

                // Assert
                var internalVars = (List<IVariable>)_fiVariables.GetValue(_view);
                CollectionAssert.AreEqual(new[] { firstVar }, internalVars);
            }
            finally
            {
                UnityObj.DestroyImmediate(destroyedHost);
            }
        }

        private sealed class StubVariableSource : IVariableSource
        {
            private readonly List<IVariable> _variables;

            public StubVariableSource(IEnumerable<IVariable> variables)
            {
                _variables = variables.ToList();
            }

            public event System.Action<IVariable> VariableAdded = delegate { };
            public event System.Action<IVariable> VariableRemoved = delegate { };

            public IReadOnlyList<IVariable> Variables => _variables;

            public string UniqueId { get; } = System.Guid.NewGuid().ToString();
            public string Name { get; set; } = "StubVariableSource";

            public IVariable AddVariable(IVariable toAdd)
            {
                _variables.Add(toAdd);
                VariableAdded(toAdd);
                return toAdd;
            }

            public void RemoveVariable(IVariable toRemove)
            {
                if (_variables.Remove(toRemove))
                {
                    VariableRemoved(toRemove);
                }
            }

            public IVariable GetVariable(byte itemId)
            {
                return _variables.FirstOrDefault(v => v != null && v.ItemId == itemId);
            }

            public T GetVariableOfType<T>() where T : class, IVariable
            {
                return _variables.OfType<T>().FirstOrDefault();
            }

            public IVariable GetVariable(string name, StringComparison strCompare = StringComparison.Ordinal)
            {
                return _variables.FirstOrDefault(v => v != null && v.Key?.Equals(name, strCompare) == true);
            }

            public T GetVariableOfType<T>(string name, StringComparison strCompare = StringComparison.Ordinal) where T : class, IVariable
            {
                return _variables
                    .OfType<T>()
                    .FirstOrDefault(v => v.Key?.Equals(name, strCompare) == true);
            }

            public IVariable GetVariableOfType(Type type, string name, StringComparison strCompare = StringComparison.Ordinal)
            {
                return _variables.FirstOrDefault(v =>
                    v != null &&
                    type.IsAssignableFrom(v.GetType()) &&
                    v.Key?.Equals(name, strCompare) == true);
            }

            public bool Contains(IVariable var)
            {
                return _variables.Contains(var);
            }
        }

        static void AssignLegacyVariables(Flowchart flowchart, List<Variable> variables)
        {
            var field = typeof(Flowchart).GetField("legacyVariables", bindingFlags);
            if (field == null)
                Assert.Fail("Could not find legacyVariables field on Flowchart");

            field.SetValue(flowchart, variables);
            ResetVariableManagerInit(flowchart);
        }

        static void ResetVariableManagerInit(Flowchart flowchart)
        {
            var managerField = typeof(Flowchart).GetField("variableManager", bindingFlags);
            if (managerField == null)
                Assert.Fail("Could not find variableManager field on Flowchart");

            var manager = managerField.GetValue(flowchart);
            var isInittedField = manager.GetType().GetField("isInitted", bindingFlags);
            if (isInittedField == null)
                Assert.Fail("Could not find isInitted field on VariableManager");

            isInittedField.SetValue(manager, false);
        }

        class TestVariableListView : VariableListView
        {
            public bool SyncCalled;

            public TestVariableListView(VariableListViewInitArgs initArgs) : base(initArgs) { }

            protected override void SyncFromSource()
            {
                SyncCalled = true;
                base.SyncFromSource();
            }
        }

        /// <summary>
        /// Test helper variable type that intentionally implements value-based equality
        /// (e.g. compares solely by Key). This simulates legacy or domain types that
        /// override Equals/GetHashCode and caused the previous phantom-row bug when
        /// the active-row dictionary used value-based equality for keys.
        /// </summary>
        public class ValueEqualsVariable : MonoBehaviour, IVariable
        {
            // Minimal backing storage to satisfy the IVariable contract in tests.
            string _key;
            object _value;
            byte _itemId;

            public virtual int OwnerIdIndex => -1;
            // IHasKey / IVariable.Key
            public string Key
            {
                get => _key;
                set => _key = value;//
            }

            // IHasItemID
            public byte ItemId
            {
                get => _itemId;
                set => _itemId = value;
            }

            // IVariable.Value
            public object Value
            {
                get => _value;
                set => _value = value;
            }

            public object BoxedValue
            {
                get => _value;
                set => _value = value;
            }

            // IVariable.Scope (readonly)
            public VariableScope Scope
            {
                get => VariableScope.Private;
                set { } // no-op
            }

            // IVariable.Owner
            public IVariableSource Owner
            {
                get => null;
                set { } // no-op
            }

            // ContentType - for tests we can return typeof(object)
            public Type ContentType => typeof(object);

            public bool IsRelationalSupported => false;

            // Init is a no-op for test helper
            public void Init() { }

            // Comparison helpers (minimal)
            public bool IsComparisonSupported() => false;
            public bool Evaluate(CompareOperator compareOperator, object value) => false;

            // Apply operator (no-op here)
            public void Apply(SetOperator setOperator, object value) { }

            // Value-based equality: two distinct instances are equal if their Key is equal.
            public override bool Equals(object obj)
            {
                if (obj is IVariable other)
                {
                    return string.Equals(Key, other.Key, System.StringComparison.Ordinal);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (Key != null) ? Key.GetHashCode() : 0;
            }

            public bool IsArithmeticSupported(SetOperator setOperator)
            {
                return false;
            }

            public void Init(object startValue)
            {
                
            }

            public void OnReset()
            {
                
            }
        }
    }

}