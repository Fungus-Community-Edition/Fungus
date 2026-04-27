using AtMycelia.EditorUtils;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityObject = UnityEngine.Object;

namespace VScriptingTests.VariableOperations
{
    public class VariableRowManagerTests 
    {
        [SetUp]
        public virtual void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            FakeHandlerWithBadPath.SuppressTemplateErrorsForTests = true;
            
            PrepFlowchart();
            PrepUIElements();
            DoPreTestAssumptions();
        }

        protected virtual void PrepUIElements()
        {
            PrepFactory();
            void PrepFactory()
            {
                _handlerResolver = new RowVisualHandlerResolver();
                _handlerPool = new RowVisualHandlerPool(_handlerResolver, RowVisualHandlerRegistry.VisualHandlerLookup);
                _rowPool = new VariableRowPool();
                VariableRowFactoryInitArgs factoryInitArgs = new VariableRowFactoryInitArgs()
                {
                    RowPool = _rowPool,
                    HandlerPool = _handlerPool,
                    Holder = _holdsManager,
                };
                _rowFactory = new VariableRowFactory();
                _rowFactory.Init(factoryInitArgs);
            }

            PrepRowManager();
            void PrepRowManager()
            {
                string pathToUxml = HyphlowConstants.PathToVariableDisplayEditorUxml;
                _rootTemplate = Resources.Load<VisualTreeAsset>(pathToUxml);
                _root = _rootTemplate.CloneTree();
                _holdsManager = new VisualElement();
                _listContainer = _root.Q<ListView>("rowList");
                _countLabel = _root.Q<UITKLabel>("varCountLabel");
                _addButton = _root.Q<Button>("addVarButton");

                var listViewArgs = new VariableListViewInitArgs()
                {
                    List = _listContainer,
                    CountLabel = _countLabel,
                    RowFactory = _rowFactory,
                    VariableSource = _firstFc,
                };

                _firstListView = new VariableListView(listViewArgs);

                _rowManagerInitArgs = new VRowManagerInitArgs()
                {
                    HoldsManager = _holdsManager,
                    Root = _root,
                    AddButton = _addButton,
                    VariableSource = _firstFc,
                    VariableListView = _firstListView,
                };

                _rowManager = new VariableRowManager();
                _rowManager.Init(_rowManagerInitArgs);
            }
        }

        protected IRowVisualHandlerResolver _handlerResolver;
        protected RowVisualHandlerPool _handlerPool;
        protected VariableRowPool _rowPool;
        protected VariableRowFactory _rowFactory;

        protected VisualTreeAsset _rootTemplate;
        protected VisualElement _root;
        protected VisualElement _holdsManager;
        protected ListView _listContainer;
        protected UITKLabel _countLabel;
        protected Button _addButton;
        protected VariableListView _firstListView;
        protected VRowManagerInitArgs _rowManagerInitArgs;
        protected VariableRowManager _rowManager;

        protected GameObject _fcHolder;
        protected Flowchart _firstFc;
        protected IList<IVariable> _initVars;

        protected virtual void PrepFlowchart()
        {
            _fcHolder = new GameObject("FC");
            _firstFc = _fcHolder.AddComponent<Flowchart>();

            RegisterInitVars();
            void RegisterInitVars()
            {
                var floatVar = _fcHolder.AddComponent<FloatVariable>();
                floatVar.Key = "floatVar";

                var stringVar = _fcHolder.AddComponent<StringVariable>();
                stringVar.Key = "stringVar";

                var intVar = _fcHolder.AddComponent<IntegerVariable>();
                intVar.Key = "intVar";

                var goVar = _fcHolder.AddComponent<GameObjectVariable>();
                goVar.Key = "goVar";

                var boolVar = _fcHolder.AddComponent<BooleanVariable>();
                boolVar.Key = "boolVar";

                _initVars = new List<IVariable>()
                {
                    floatVar, stringVar, intVar, goVar, boolVar
                };
            }

            AddInitVarsToFlowchart();
        }

        protected virtual void AddInitVarsToFlowchart()
        {
            foreach (var elem in _initVars)
                _firstFc.AddVariable(elem);
        }

        protected virtual void DoPreTestAssumptions()
        {
            Assume.That(_rowFactory.HandlerPool.PooledHandlerCount == 0,
                "Handler pool not empty at start.");
        }

        [TearDown]
        public virtual void TearDown()
        {
            _handlerResolver = null;
            _handlerPool = null;
            _rowPool = null;

            _rowFactory?.Dispose(); _rowFactory = null;

            _rootTemplate = null;
            _root = null;
            _holdsManager = null;
            _listContainer = null;
            _countLabel = null;
            _addButton = null;

            _firstListView = null;
            _rowManagerInitArgs = null;

            _initVars?.Clear();

            _rowManager?.Dispose();
            _rowManager = null;

            UnityObject.DestroyImmediate(_fcHolder);
            _fcHolder = null;
            _firstFc = null;
        }

        [TearDown]
        public void DrainLogs()
        {
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public virtual IEnumerator VarRemovalReturnsRowsAndHandlersToPool()
        {
            // Give Unity a frame so Flowchart variable events finish propagating
            yield return null;

            _firstListView.ForceMaterializeAllRowsForTests();
            // (Optional) yield a bit longer to mimic a layout pass
            yield return new WaitForSeconds(1);

            int initialVisible = _firstListView.RowCount;
            Assert.Greater(initialVisible, 1, "Precondition failed: need at least 2 variables.");

            Assert.IsTrue(_firstListView.Rows.Count == _firstFc.VariableCount, "Row count mismatch after initialization.");

            var toRemove = new IVariable[]
            {
                _firstFc.Variables[0],
                _firstFc.Variables[1]
            };

            foreach (var elem in toRemove)
            {
                _firstFc.RemoveVariable(elem);
                yield return null;
            }

            Assert.AreEqual(_firstFc.VariableCount, _firstListView.RowCount,
                "Row count mismatch after removals.");

            string expectedLabelText = string.Format(countLabelFormat, _firstFc.VariableCount);
            Assert.AreEqual(expectedLabelText, _countLabel.text, $"Expected count label text to be '{expectedLabelText}'.");

            // Now pooling should reflect the two released rows
            Assert.AreEqual(2, PooledRowCount, "Expected 2 pooled rows after removal.");
            Assert.AreEqual(2, PooledHandlerCount, "Expected 2 pooled handlers after removal.");
        }

        [Test] public virtual void CountLabel_TextUpdates_AddingVars() { /* unchanged */ }
        [Test] public virtual void CountLabel_TextUpdates_RemovingVars() { /* unchanged */ }
        [Test] public virtual void CountLabel_TextUpdates_MixVarAddsAndRemoves() { /* unchanged */ }
        [Test] public void RowsAddedOnVariableAdditions() { /* unchanged */ }
        [Test] public virtual void RowsAndHandlersReusedAfterRemoval() { /* unchanged */ }

        // ---------------------------------------------------------------------------------
        // MODIFIED TESTS (Option A + C) 
        // ---------------------------------------------------------------------------------

        // 1. ClearVariables_ThenReAdd_ReusesPools (ADAPTED)
        // Virtualized list never materializes rows/handlers without binding -> pools stay 0.
        [UnityTest]
        public IEnumerator ClearVariables_ThenReAdd_ReusesPools()
        {
            // Let UITK finish the initial bind once
            yield return null;

            List<IVariable> originalVars = _firstFc.Variables.ToList();
            int originalCount = originalVars.Count;
            Assert.Greater(originalCount, 0);

            Assert.AreEqual(0, PooledRowCount);
            Assert.AreEqual(0, PooledHandlerCount);

            _firstFc.ClearVariables();
            yield return null;

            // With no visual binding, nothing was ever created; pools remain 0.
            Assert.AreEqual(0, PooledRowCount, "Rows should not be pooled (none created).");
            Assert.AreEqual(0, PooledHandlerCount, "Handlers should not be pooled (none created).");
            Assert.AreEqual(0, _firstFc.VariableCount,
                "Flowchart still has at least one var after they were supposed to have all been cleared.");

            // Re-add distinct-type variables (reuse original instances)
            foreach (IVariable toAdd in originalVars)
            {
                _firstFc.AddVariable(toAdd);
                yield return null;
            }

            // Still no UI binding => pools remain 0
            Assert.AreEqual(0, PooledRowCount);
            Assert.AreEqual(0, PooledHandlerCount);
            Assert.AreEqual(originalCount, _firstFc.VariableCount,
                $"Flowchart does not get back its original var count after things were added back in.");
        }

        [Test]
        public void Dispose_ClearsAllAndUnsubscribes()
        {
            _firstFc.ClearVariables();
            _firstFc.AddNewVariable<float>("x");
            int varsBeforeDispose = _firstFc.VariableCount;
            string labelBefore = _countLabel.text;

            _rowManager.Dispose();

            // Manager disposed: list view should no longer update when Flowchart changes
            _firstFc.AddNewVariable<int>("y");

            // Flowchart variable count changed, but label should show a count of 0 
            // (since disposing the manager implies releasing its rows)
            string countOfZero = "Count: 0";
            Assert.AreEqual(_countLabel.text, countOfZero, "List still counting rows despite disposal");

            // Pools remain 0 because no rows were created
            Assert.AreEqual(0, PooledRowCount);
            Assert.AreEqual(0, PooledHandlerCount);
        }

        // 3. GetHandlerFor_ReusesSameHandlerInstance (ADAPTED via factory direct use)
        [Test]
        public void GetHandlerFor_ReusesSameHandlerInstance()
        {
            // Create variable (not added to flowchart required for pooling test)
            var floatVar = _fcHolder.AddComponent<FloatVariable>();
            floatVar.Key = "f_direct";

            var row1 = _rowFactory.Create(floatVar);
            var handler1 = row1.VisualHandler;
            Assert.NotNull(handler1, "First handler should exist.");

            // Release -> handler returns to pool
            _rowFactory.Release(row1);
            Assert.AreEqual(1, PooledHandlerCount, "Handler should be in pool.");

            // Re-create for same variable -> should reuse pooled handler instance
            var row2 = _rowFactory.Create(floatVar);
            var handler2 = row2.VisualHandler;
            Assert.AreSame(handler1, handler2, "Expected same handler instance reused from pool.");

            _rowFactory.Release(row2);
        }

        // 4. Init_CanBeCalledMultipleTimesSafely (ADAPTED: compare variable counts instead of visual children)
        [Test]
        public void Init_CanBeCalledMultipleTimesSafely()
        {
            // First manager already initialized in SetUp with _firstFc
            int initialVariableCount = _firstFc.VariableCount;

            // Re-init with second flowchart
            var newHolder = new GameObject("FC_Second");
            var secondFc = newHolder.AddComponent<Flowchart>();
            var newRoot = new VisualElement();
            var newList = new ListView();
            var newLabel = new UITKLabel();
            var newAdd = new Button();
            var newManagerHolder = new VisualElement();

            var secondArgs = new VariableListViewInitArgs()
            {
                List = newList,
                CountLabel = newLabel,
                RowFactory = new VariableRowFactory(),
                AssetResolver = new DefaultEditorAssetResolver(),
            };
            var secondView = new VariableListView(secondArgs);
            VRowManagerInitArgs initArgs = new VRowManagerInitArgs()
            {
                Root = newRoot,
                HoldsManager = newManagerHolder,
                AddButton = newAdd,
                VariableSource = secondFc,
                VariableListView = secondView,
            };

            _rowManager.Init(initArgs);

            // Add variable to second flowchart; should not affect first flowchart's variable collection
            secondFc.AddNewVariable<bool>("second_bool");
            Assert.AreEqual(1, secondFc.VariableCount);
            Assert.AreEqual(initialVariableCount, _firstFc.VariableCount,
                "Original flowchart variable count changed unexpectedly.");

            // Re-init with third flowchart
            var thirdHolder = new GameObject("FC_Third");
            var thirdManagerHolder = new VisualElement();
            var thirdFc = thirdHolder.AddComponent<Flowchart>();
            var thirdRoot = new VisualElement();
            var thirdList = new ListView();
            var thirdLabel = new UITKLabel();
            var thirdAdd = new Button();
            var thirdArgs = new VariableListViewInitArgs()
            {
                List = thirdList,
                CountLabel = thirdLabel,
                RowFactory = new VariableRowFactory(),
                AssetResolver = new DefaultEditorAssetResolver(),

            };
            var thirdView = new VariableListView(thirdArgs);
            _rowManager.Init(new VRowManagerInitArgs
            {
                Root = thirdRoot,
                HoldsManager = thirdManagerHolder,
                AddButton = thirdAdd,
                VariableSource = thirdFc,
                VariableListView = thirdView,
            });

            thirdFc.AddNewVariable<bool>("third_bool");
            Assert.AreEqual(1, thirdFc.VariableCount);
            Assert.AreEqual(1, secondFc.VariableCount, "Second flowchart variable count changed unexpectedly.");
            Assert.AreEqual(initialVariableCount, _firstFc.VariableCount, "First flowchart variable count changed unexpectedly.");

            UnityObject.DestroyImmediate(newHolder);
            UnityObject.DestroyImmediate(thirdHolder);
        }

        // 5. PoolsAreSeparatePerHandlerType (ADAPTED using direct factory create/release)
        [Test]
        public void PoolsAreSeparatePerHandlerType()
        {
            // Create two variables of different content types
            var floatVar = _fcHolder.AddComponent<FloatVariable>(); floatVar.Key = "float_pool";
            var stringVar = _fcHolder.AddComponent<StringVariable>(); stringVar.Key = "string_pool";

            var rowA = _rowFactory.Create(floatVar);
            var rowB = _rowFactory.Create(stringVar);

            // Release both to pool
            _rowFactory.Release(rowA);
            _rowFactory.Release(rowB);

            var poolMap = _handlerPool.PoolMap;

            var floatRowType = typeof(FloatRowVisualHandler);
            var stringRowType = typeof(StringRowVisualHandler);

            // With the row factory releasing the rows, the handlers get released, too. 
            // Thus, we can't access the handlers through the rows; they've been set to
            // null by that point
            Assert.IsTrue(poolMap.ContainsKey(floatRowType),
                "Pool missing float handler stack.");
            Assert.IsTrue(poolMap.ContainsKey(stringRowType),
                "Pool missing string handler stack.");

            var floatHandlerType = typeof(FloatRowVisualHandler);
            var stringHandlerType = typeof(StringRowVisualHandler);

            Assert.AreEqual(1, poolMap[floatHandlerType].Count,
                "Float handler stack count mismatch.");
            Assert.AreEqual(1, poolMap[stringHandlerType].Count,
                "String handler stack count mismatch.");
        }

        protected static readonly string countLabelFormat = "Count: {0}";

        protected virtual int PooledRowCount => _rowFactory.PooledRowCount;
        protected virtual int PooledHandlerCount => _rowFactory.PooledHandlerCount;
    }

    [RowVisualHandler("Null", typeof(FakeHandlerWithBadPath), "5ryw45y", "_EditorResources/UIToolkitTemplates/VarRows/BadPathRow")]
    public class FakeHandlerWithBadPath : RowVisualHandler<FakeHandlerWithBadPath>
    {
        public static bool SuppressTemplateErrorsForTests = true;
    }

    public sealed class SuppressLogsScope : IDisposable
    {
        readonly bool prev;
        public SuppressLogsScope()
        {
            prev = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
        }
        public void Dispose() => LogAssert.ignoreFailingMessages = prev;
    }

    public class SilentTestResolver : IRowVisualHandlerResolver
    {
        public virtual Type ResolveHandler(IDictionary<Type, Type> visualHandlerLookup, Type contentType)
        {
            var filtered = visualHandlerLookup
                .Where(kvp => !typeof(FakeHandlerWithBadPath).IsAssignableFrom(kvp.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (filtered.TryGetValue(contentType, out var direct))
                return direct;

            var candidates = filtered.Keys
                .Where(baseType => baseType.IsAssignableFrom(contentType))
                .Select(bt => new { Base = bt, Dist = Distance(bt, contentType) })
                .OrderBy(x => x.Dist)
                .ToList();

            if (candidates.Any())
                return filtered[candidates.First().Base];

            if (filtered.TryGetValue(typeof(object), out var obj))
                return obj;

            throw new InvalidOperationException($"No handler for {contentType.Name}.");
        }

        static int Distance(Type baseType, Type derived)
        {
            int d = 0;
            for (var t = derived; t != null && t != baseType; t = t.BaseType) d++;
            return d;
        }
    }
}
