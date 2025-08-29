using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using UITKLabel = UnityEngine.UIElements.Label;

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
        public void AddVariable_AfterDispose_CurrentBehavior_NRE()
        {
            var v1 = CreateVar<FloatVariable, float>("f1", 1f);
            _view.Dispose();
            Assert.Catch<NullReferenceException>(() => _view.AddVariable(v1),
                "Behavior changed (no NRE). Update test if a guard was added.");
        }
    }
}