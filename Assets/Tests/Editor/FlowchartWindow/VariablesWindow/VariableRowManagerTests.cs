using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.PerformanceTesting;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityObject = UnityEngine.Object;

namespace Amanita.Tests.Editor
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
            string pathToUxml = "_EditorResources/UIToolkitTemplates/VariableDisplayEditor";
            _rootTemplate = Resources.Load<VisualTreeAsset>(pathToUxml);
            _root = _rootTemplate.CloneTree();
            _holdsManager = new VisualElement();
            _listContainer = _root.Q<ScrollView>("rowList");
            _countLabel = _root.Q<UITKLabel>("varCountLabel");
            _addButton = _root.Q<Button>("addVarButton");

            VRowManagerInitArgs args = new VRowManagerInitArgs()
            {
                HoldsManager = _holdsManager,
                Root = _root,
                CountLabel = _countLabel,
                ListContainer = _listContainer,
                AddButton = _addButton,
                Flowchart = _flowchart,
            };

            _handlerResolver = new SilentTestResolver();
            _rowManager = new VariableRowManager(_handlerResolver);
            _rowManager.Init(args);
            _handlerPool = _rowManager.HandlerPool;
        }

        protected VisualTreeAsset _rootTemplate;
        protected VisualElement _holdsManager;
        protected VisualElement _root;
        protected UITKLabel _countLabel;
        protected VisualElement _listContainer;
        protected Button _addButton;
        protected IRowVisualHandlerResolver _handlerResolver;
        protected VariableRowManager _rowManager;
        protected RowVisualHandlerPool _handlerPool;

        protected virtual void PrepFlowchart()
        {
            _fcHolder = new GameObject("FC");
            _flowchart = _fcHolder.AddComponent<Flowchart>();

            var floatVar = _fcHolder.AddComponent<FloatVariable>();
            floatVar.Key = "floatVar";

            var stringVar =_fcHolder.AddComponent<StringVariable>();
            stringVar.Key = "stringVar";

            var intVar = _fcHolder.AddComponent<IntegerVariable>();
            intVar.Key = "intVar";

            var goVar = _fcHolder.AddComponent<GameObjectVariable>();
            goVar.Key = "goVar";

            var boolVar = _fcHolder.AddComponent<BooleanVariable>();
            boolVar.Key = "boolVar";

            _initVars = new List<IVariable>()
            {
                floatVar, stringVar, intVar, goVar,
                boolVar
            };

            AddInitVarsToFlowchart();

        }

        protected GameObject _fcHolder;
        protected Flowchart _flowchart;

        protected IList<IVariable> _initVars;

        protected virtual void AddInitVarsToFlowchart()
        {
            foreach (var elem in _initVars)
            {
                _flowchart.AddVariable(elem);
            }
        }

        protected virtual void DoPreTestAssumptions()
        {
            var handlerPool = _rowManager.HandlerPool;
            int expectedHandlersInPool = 0;
            Assume.That(handlerPool.PooledHandlerCount == expectedHandlersInPool,
                "Handler count doesn't start as 0");
        }

        [TearDown]
        public virtual void TearDown()
        {
            _initVars?.Clear();
            _rowManager.Dispose();
            _handlerResolver = null;
            _rowManager = null;
            UnityObject.DestroyImmediate(_fcHolder);
        }

        [TearDown]
        public void DrainLogs()
        {
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public virtual void VarRemovalReturnsRowsAndHandlersToPool()
        {
            
            var toRemove = new IVariable[] 
            {
                // Both of these should be from the init setup
                _flowchart.Variables[0],
                _flowchart.Variables[1]
            };

            foreach (var elem in toRemove)
                _flowchart.RemoveVariable(elem);

            // 3. UI updated
            Assert.AreEqual(_flowchart.VariableCount, _listContainer.childCount);
            string expectedLabelText = string.Format(countLabelFormat, _flowchart.VariableCount);
            Assert.AreEqual(expectedLabelText, _countLabel.text);

            // 4. Exactly two rows & two handlers should be back in their pools
            Assert.AreEqual(2, _rowManager.PooledRowCount);
            Assert.AreEqual(2, _rowManager.PooledHandlerCount);
            
        }

        [Test]
        public virtual void CountLabel_TextUpdates_AddingVars()
        {
            int currentCount = _flowchart.VariableCount;
            string expectedLabelText = string.Format(countLabelFormat, currentCount);
            Assert.AreEqual(expectedLabelText, _countLabel.text);

            IList<IVariable> varsToAdd = new List<IVariable>()
            {
                _fcHolder.AddComponent<StringVariable>(),
                _fcHolder.AddComponent<FloatVariable>(),
                _fcHolder.AddComponent<IntegerVariable>(),
                _fcHolder.AddComponent<GameObjectVariable>(),
            };

            foreach (var elem in varsToAdd)
            {
                _flowchart.AddVariable(elem);
                currentCount++;
                expectedLabelText = string.Format(countLabelFormat, currentCount);
                Assert.AreEqual(expectedLabelText, _countLabel.text);
            }

            
        }

        [Test]
        public virtual void CountLabel_TextUpdates_RemovingVars()
        {
            
            int currentCount = _flowchart.VariableCount;
            string expectedLabelText = string.Format(countLabelFormat, currentCount);
            Assert.AreEqual(expectedLabelText, _countLabel.text);

            while (_flowchart.VariableCount > 0)
            {
                _flowchart.RemoveVariable(0);
                currentCount--;
                expectedLabelText = string.Format(countLabelFormat, currentCount);
                Assert.AreEqual(expectedLabelText, _countLabel.text);
            }

            
        }

        [Test]
        public virtual void CountLabel_TextUpdates_MixVarAddsAndRemoves()
        {
            int currentCount = _flowchart.VariableCount;
            string expectedLabelText = string.Format(countLabelFormat, currentCount);
            Assert.AreEqual(expectedLabelText, _countLabel.text);

            IList<IVariable> varsToAddOrRemove = PrepVarsToAddOrRemove();
            IList<IVariable> PrepVarsToAddOrRemove()
            {
                StringVariable stringVar = _fcHolder.AddComponent<StringVariable>();
                stringVar.Key = "someString123";
                stringVar.Value = "1";

                FloatVariable floatVar = _fcHolder.AddComponent<FloatVariable>();
                floatVar.Key = "someFloat123";
                floatVar.Value = 8192;

                IntegerVariable intVar = _fcHolder.AddComponent<IntegerVariable>();
                intVar.Key = "someInt123";
                intVar.Value = 16384;

                GameObjectVariable goVar = _fcHolder.AddComponent<GameObjectVariable>();
                goVar.Key = "someGo123";
                goVar.Value = _fcHolder;

                IList<IVariable> varsToAddOrRemove = new List<IVariable>()
                {
                    stringVar, floatVar, intVar, goVar
                };

                return varsToAddOrRemove;

            }

            AddTheVarsAndCheckLabel();
            void AddTheVarsAndCheckLabel()
            {
                foreach (IVariable elem in varsToAddOrRemove)
                {
                    _flowchart.AddVariable(elem);
                    currentCount++;
                    expectedLabelText = string.Format(countLabelFormat, currentCount);
                    Assert.AreEqual(expectedLabelText, _countLabel.text);
                }
            }

            RemoveTheVarsAndCheckLabel();
            void RemoveTheVarsAndCheckLabel()
            {
                foreach (IVariable elem in varsToAddOrRemove)
                {
                    _flowchart.RemoveVariable(elem);
                    currentCount--;
                    expectedLabelText = string.Format(countLabelFormat, currentCount);
                    Assert.AreEqual(expectedLabelText, _countLabel.text);
                }
            }

            
        }

        protected static readonly string countLabelFormat = "Count: {0}";
        [Test]
        public void RowsAddedOnVariableAdditions()
        {
            
            // initially, no vars = no rows
            _flowchart.ClearVariables();
            int expectedRowCount = 0;
            Assert.AreEqual(expectedRowCount, _listContainer.childCount);
            string expectedResult = string.Format(countLabelFormat, expectedRowCount);
            Assert.AreEqual(expectedResult, _countLabel.text);

            _flowchart.AddNewVariable<float, FloatVariable>("floatVar1");
            expectedRowCount += 1; // The manager should've responded to the var addition, adding just one row

            Assert.AreEqual(expectedRowCount, _listContainer.childCount);
            expectedResult = string.Format(countLabelFormat, expectedRowCount);
            Assert.AreEqual(expectedResult, _countLabel.text);

            _flowchart.AddNewVariable<int, IntegerVariable>("intVar1");
            _flowchart.AddNewVariable<string, StringVariable>("stringVar1");
            expectedRowCount += 2;

            Assert.AreEqual(expectedRowCount, _listContainer.childCount);
            expectedResult = string.Format(countLabelFormat, expectedRowCount);
            Assert.AreEqual(expectedResult, _countLabel.text);
            
        }

        [Test]
        public virtual void RowsAndHandlersReusedAfterRemoval()
        {
            
            int expectedAmountInPools = 0;
            IList<IVariable> varsToRemove = _flowchart.Variables;
            foreach (var elem in varsToRemove)
            {
                _flowchart.RemoveVariable(elem);
                expectedAmountInPools++;
                Assert.AreEqual(expectedAmountInPools, _rowManager.PooledHandlerCount,
                    "Removed a var yet the handler pool didn't have the right amount of stuff");
                Assert.AreEqual(expectedAmountInPools, _rowManager.PooledRowCount,
                    "Removed a var yet the row pool didn't have the right amount of stuff");
            }

            IList<IVariable> varsToAdd = _initVars;

            expectedAmountInPools = _initVars.Count;
            foreach (var elem in varsToAdd)
            {
                _flowchart.AddVariable(elem);
                expectedAmountInPools--;
                Assert.AreEqual(expectedAmountInPools, _rowManager.PooledHandlerCount,
                    "Added a var yet the handler pool didn't reuse a handler");
                Assert.AreEqual(expectedAmountInPools, _rowManager.PooledRowCount,
                    "Add a var yet the row pool didn't reuse a row");
            }

            Assert.AreEqual(0, _rowManager.PooledRowCount,
                "Pooled row count should be zero when all the vars are added back in");
            Assert.AreEqual(0, _rowManager.PooledHandlerCount,
                "Pooled handler count should be zero when all the vars are added back in");
            
        }


        [Test]
        public void GetHandlerFor_ReusesSameHandlerInstance()
        {
            
            _flowchart.ClearVariables();
            var floatVar = _flowchart.AddNewVariable<float, FloatVariable>("f1");

            _rowManager.Refresh(); // Should prep and "display" the initial row
            var firstRow = _rowManager.GetVisibleRowAt(0);
            var firstHandler = firstRow.VisualHandler;

            _flowchart.RemoveVariable(floatVar);
            _flowchart.AddVariable(floatVar);
            // ^Should keep the row using the old handler

            // Assert: pool returned the exactsame handler instance
            var secondHandler = firstRow.VisualHandler;
            bool poolReturnedExactSameHandlerInstance = ReferenceEquals(firstHandler, secondHandler);
            Assert.IsTrue(poolReturnedExactSameHandlerInstance,
                "The pool apparently made a new handler instance instead of reusing the old one");
            
        }

        [Test]
        public void GetHandlerFor_CreatesNewWhenPoolIsEmpty()
        {
            // Remember: with each test, the flowchart starts with as many variables as the
            // init vars list has. This implies that the handler amount at the start is
            // equal to that same variable count. And that none of those are pooled yet.
            var handlerPool = _rowManager.HandlerPool;
            int expectedPooledHandlerCount = 0;
            Assume.That(handlerPool.PooledHandlerCount == expectedPooledHandlerCount,
                "Handler pool has stuff in it despite how we only just added multiple vars");

            _flowchart.ClearVariables(); // After this, all the handlers should be in the pool
            expectedPooledHandlerCount = _initVars.Count;
            Assume.That(handlerPool.PooledHandlerCount == expectedPooledHandlerCount,
                $"Right after the first var clear, the pool didn't get all the handlers back. " +
                $"How many it has: {handlerPool.PooledHandlerCount}");

            _rowManager.Refresh();
            _flowchart.AddNewVariable<string, StringVariable>("s1");
            expectedPooledHandlerCount -= 1; // Since that string variable should've gotten one of the handlers unpooled
            Assume.That(handlerPool.PooledHandlerCount == expectedPooledHandlerCount,
                $"The pool didn't release one handler after having multiple pooled and just one var added. " +
                $"How many the pool has: {handlerPool.PooledHandlerCount}\n" +
                $"What we expectedc: {expectedPooledHandlerCount}");

            var firstRow = _rowManager.GetVisibleRowAt(0);
            Assume.That(firstRow, Is.Not.Null, "First row wasn't registered properly");
            var firstHandler = firstRow.VisualHandler;
            Assume.That(firstHandler, Is.Not.Null, "First row doesn't have a visual handler");

            _flowchart.RemoveVariable(firstHandler.Variable); // Should get the row (and its handler) into the pool
            // ^ERROR: For some reason, the first handler's variable is null

            expectedPooledHandlerCount += 1;
            int howManyPooled = _rowManager.PooledHandlerCount;
            bool wasReleasedBackIntoPool = howManyPooled == expectedPooledHandlerCount;
            Assert.IsTrue(wasReleasedBackIntoPool,
                $"The row wasn't pooled after the one var from the manager was removed. " +
                $"Handler amount: {howManyPooled} Expected amount: {expectedPooledHandlerCount}");

            // Drain the pool by manually ReleaseHandler
            handlerPool.Clear();

            // With the pool now empty, adding another var should create a new 
            // handler instance
            _flowchart.AddNewVariable<string, StringVariable>("s2");
            var handler2 = firstRow.VisualHandler;

            bool createdBrandNewInstance = !ReferenceEquals(firstHandler, handler2);
            Assert.AreNotSame(createdBrandNewInstance,
                "Did not create brand new instance of a handler after draining the pool");
            
        }

        [Test]
        public void ReleaseHandler_ResetsHandlerState()
        {
            
            _flowchart.ClearVariables();
            int expectedHandlersInPool = _initVars.Count;
            Assume.That(_handlerPool.PooledHandlerCount == expectedHandlersInPool,
                "Pool doesn't get all its handlers back after a full-on row-clear");
            
            IntegerVariable firstIntVar = _flowchart.AddNewVariable<int, IntegerVariable>("i1");
            expectedHandlersInPool--;
            Assume.That(expectedHandlersInPool == _handlerPool.PooledHandlerCount,
                "Handler count should be one less that the full amount when only one row should be 'visible'");

            var firstRow = _rowManager.GetVisibleRowAt(0);
            var handler = firstRow.VisualHandler;

            // Act: remove, then re-add a *different* integer var. We expect the same handler
            // from before to be reused here
            _flowchart.RemoveVariable(handler.Variable);
            var secondIntVar = _fcHolder.AddComponent<IntegerVariable>();
            secondIntVar.Key = "i2";
            _flowchart.AddVariable(secondIntVar);

            var reusedHandler = firstRow.VisualHandler;
            Assert.AreSame(handler, reusedHandler,
                "The handler for the second var is different from that of the first, " +
                "meaning the former was NOT reused as it should've");

            // Assert: handler.Variable is updated to second, not left pointing at i1
            Assert.AreEqual("i2", reusedHandler.Variable.Key, "After reuse, the handler doesn't point to the second var");
            
        }

        [Test]
        public void PoolsAreSeparatePerHandlerType()
        {
            
            _flowchart.ClearVariables();

            _flowchart.AddNewVariable<float, FloatVariable>("f");
            _flowchart.AddNewVariable<string, StringVariable>("s");

            // Remove both
            foreach (var v in _flowchart.Variables.ToArray())
                _flowchart.RemoveVariable(v);

            // Pools should have 1 float‐handler and 1 string‐handler
            // You can reflect into the pool map
            var poolMap = _handlerPool.PoolMap;

            Assert.IsTrue(poolMap.ContainsKey(typeof(FloatRowVisualHandler)),
                "There is no dedicated pool for FloatRowVisualHandlers");
            Assert.IsTrue(poolMap.ContainsKey(typeof(DefaultRowVisualHandler)),
                "There is no dedicated pool for DefaultRowVisualHandlers"); // string uses default

            int floatVisualHandlerCount = poolMap[typeof(FloatRowVisualHandler)].Count;
            int defaultRowVisualHandlerCount = poolMap[typeof(DefaultRowVisualHandler)].Count;

            // We have 5 vars prepped in set up, 4 of which (at this time) should get us a default vis handler.
            // 1 should get us a float vis handler
            int expectedFloatHandlerCount = 1, expectedDefaultHandlerCount = 4;
            Assert.AreEqual(expectedFloatHandlerCount, floatVisualHandlerCount,
                $"Expected {expectedFloatHandlerCount} float visual handler, got {floatVisualHandlerCount}");
            Assert.AreEqual(expectedDefaultHandlerCount, defaultRowVisualHandlerCount,
                $"Expected {expectedDefaultHandlerCount} default visual handler(s), got {defaultRowVisualHandlerCount}");
            
        }

        [Test]
        public void Refresh_ClearsAndRegeneratesRowsCorrectly()
        {
            
            _flowchart.ClearVariables();

            _flowchart.AddNewVariable<bool, BooleanVariable>("b1");
            _flowchart.AddNewVariable<int, IntegerVariable>("i1");
            _rowManager.Refresh();

            Assert.AreEqual(2, _listContainer.childCount);
            string expectedLabelText = string.Format(countLabelFormat, 2);
            Assert.AreEqual(expectedLabelText, _countLabel.text);

            // In setup, we add vars that leads to there being _initVars.Count handlers. After we
            // clear the flowchart and add 2 vars, that means only two rows should
            // be outside the pool. 
            int expectedPooledRowCount = _initVars.Count - 2;
            Assert.AreEqual(expectedPooledRowCount, _rowManager.PooledRowCount, 
                $"We expected only {expectedPooledRowCount} left in the pool after adding " +
                $"those two vars. Instead, we got {_rowManager.PooledRowCount}");

            // Act: clear flowchart and refresh again
            _flowchart.ClearVariables();
            _rowManager.Refresh();

            Assert.AreEqual(0, _listContainer.childCount);
            expectedLabelText = string.Format(countLabelFormat, 0);
            Assert.AreEqual(expectedLabelText, _countLabel.text);
            Assert.AreEqual(_initVars.Count, _rowManager.PooledRowCount);
            
        }

        [Test]
        public void Dispose_ClearsAllAndUnsubscribes()
        {
            _flowchart.ClearVariables();
            _flowchart.AddNewVariable<float, FloatVariable>("x");
            Assert.AreEqual(1, _listContainer.childCount);

            _rowManager.Dispose();

            // UI is cleared
            Assert.AreEqual(0, _listContainer.childCount, 
                $"Expected nothing in the list container, but we got {_listContainer.childCount}");
            Assert.AreEqual(0, _rowManager.PooledRowCount,
                $"Expected no more rows pooled, but we got {_rowManager.PooledRowCount}");
            Assert.AreEqual(0, _rowManager.PooledHandlerCount,
                $"Expected no more handlers pooled, but we got {_rowManager.PooledHandlerCount}");

            // Further adds/removes have no effect
            _flowchart.AddNewVariable<int, IntegerVariable>("y");
            Assert.AreEqual(0, _listContainer.childCount);
            string expectedLabelText = string.Format(countLabelFormat, 0);
            Assert.AreEqual(expectedLabelText, _countLabel.text,
                $"Expected the count label to say '{expectedLabelText}', but instead it says '{_countLabel.text}'");
            
        }

        [Test]
        public void Init_CanBeCalledMultipleTimesSafely()
        {
            
            // Given what we do in SetUp, this test's first init is the session's second init
            VisualElement newRoot, newList;
            UITKLabel newLabel;
            Button newAddButton;
            Flowchart newFC;
            GameObject newFCHolder;

            Flowchart secondFC = ApplyNewInitToManager();
            Flowchart ApplyNewInitToManager()
            {
                newRoot = new VisualElement();
                newList = new VisualElement();
                newLabel = new UITKLabel();
                newAddButton = new Button();
                var fcsInScene = UnityObject.FindObjectsOfType<Flowchart>(); 
                // ^Keeping this obsolete func call; we want compatibility with 2022 LTS
                int fcCount = fcsInScene.Length;
                newFCHolder = new GameObject($"FC_{fcCount.ToString("D2")}");
                newFC = newFCHolder.AddComponent<Flowchart>();

                var newInitArgs = new VRowManagerInitArgs
                {
                    Root = newRoot,
                    ListContainer = newList,
                    CountLabel = newLabel,
                    AddButton = newAddButton,
                    Flowchart = newFC
                };
                _rowManager.Init(newInitArgs);

                Assert.That(newList.childCount == 0, $"List #{fcCount} has children despite the new init");
                Assert.That(_rowManager.VisibleRowCount == 0, $"The manager (after we created List #{fcCount}) has " +
                    $"visible rows despite the new init");

                return newFC;
            }

            VisualElement secondList = newList;

            secondFC.AddNewVariable<bool, BooleanVariable>("var1");
            Assert.AreEqual(1, secondList.childCount, 
                $"Adding a var after the first init doesn't get us the right child count. " +
                $"List child count: {secondList.childCount}");

            Flowchart thirdFC = ApplyNewInitToManager();
            VisualElement thirdList = newList;

            // Adding vars to the first flowchart should not affect the second or third roots
            _flowchart.AddNewVariable<bool, BooleanVariable>("z");
            Assert.AreEqual(1, secondList.childCount, $"Adding a var to the first FC " +
                $"changed the child count of the second list. Second list " +
                $"child count: {secondList.childCount}");

            Assert.AreEqual(0, thirdList.childCount, $"Adding a var to the first FC " +
                $"changed the child count of the third list. Second list " +
                $"child count: {thirdList.childCount}");

            // Adding to the third Flowchart only affects the third root
            thirdFC.AddNewVariable<bool, BooleanVariable>("w");

            Assert.AreEqual(1, secondList.childCount, $"Adding a var to the third FC " +
                $"changed the child count of the second list. Second list " +
                $"child count: {secondList.childCount}");

            Assert.AreEqual(1, thirdList.childCount, $"Adding a var to the third FC " +
                $"changed the child count of the third list. Second list " +
                $"child count: {thirdList.childCount}");
            
        }

        [Test]
        public void ReadyTheTemplates_LogsError_WhenTemplateMissing()
        {
            // Arrange
            LogAssert.ignoreFailingMessages = false;
            FakeHandlerWithBadPath.SuppressTemplateErrorsForTests = false;
            var handler = new FakeHandlerWithBadPath();
            
            // Act
            handler.ReadyTheTemplate();

            string expectedPath = "_EditorResources/UIToolkitTemplates/VarRows/BadPathRow";
            string handlerName = nameof(FakeHandlerWithBadPath);
            string expectedErrorMessage = string.Format(missingTemplateFormat, handlerName, expectedPath);
            // Assert
            LogAssert.Expect(LogType.Error, expectedErrorMessage);
        }

        protected static readonly string missingTemplateFormat = "Template for {0} not found at '{1}'." +
                        "\nPlease update the path in the RowVisualHandlerAttribute of the former.";

        [Test]
        public void AllRowVisualHandlers_HaveAttribute()
        {
            
            var handlerTypes = typeof(RowVisualHandler).Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && typeof(RowVisualHandler).IsAssignableFrom(t));

            foreach (var t in handlerTypes)
            {
                Assert.That(
                    t.GetCustomAttribute<RowVisualHandlerAttribute>(),
                    Is.Not.Null,
                    $"{t.Name} is missing RowVisualHandlerAttribute."
                );
            }

            
        }

        [Test]
        public void Refresh_Idempotent_DoesNotGrowHandlerPool()
        {
            // Arrange
            _flowchart.ClearVariables();
            _flowchart.AddNewVariable<float, FloatVariable>("f");
            _flowchart.AddNewVariable<string, StringVariable>("s");
            _rowManager.Refresh();

            // Force everything into the pool
            _flowchart.ClearVariables();
            _rowManager.Refresh();
            int pooledAfterFirstClear = _rowManager.PooledHandlerCount;

            // Act: repeat without changing the model
            _rowManager.Refresh();
            _rowManager.Refresh();

            // Assert: pool size should NOT grow just by refreshing
            Assert.AreEqual(pooledAfterFirstClear, _rowManager.PooledHandlerCount,
                "Handler pool should not increase when refreshing without changes");
        }





    }

    [RowVisualHandler("Null", typeof(FakeHandlerWithBadPath), "5ryw45y", "_EditorResources/UIToolkitTemplates/VarRows/BadPathRow")]
    public class FakeHandlerWithBadPath : RowVisualHandler<FakeHandlerWithBadPath>
    {
        public override void ReadyTheTemplate()
        {
            if (TemplateReadied) return;

            var typeOfThisHandler = GetType();

            bool whatWeWantIsCached = templateCache.ContainsKey(typeOfThisHandler);
            if (!whatWeWantIsCached)
            {
                var attr = typeOfThisHandler.GetCustomAttribute<RowVisualHandlerAttribute>();
                if (attr == null && !SuppressTemplateErrorsForTests)
                {
                    Debug.LogError($"{typeOfThisHandler.Name} is missing RowVisualHandlerAttribute.");
                    return;
                }

                var template = Resources.Load<VisualTreeAsset>(attr.PathToTemplate);
                if (template == null && !SuppressTemplateErrorsForTests)
                {
                    string errorMessage = string.Format(missingTemplateFormat, typeOfThisHandler.Name, attr.PathToTemplate);
                    Debug.LogError(errorMessage);
                    return;
                }

                templateCache[typeOfThisHandler] = template;
            }

            _template = templateCache[typeOfThisHandler];
        }

        public static bool SuppressTemplateErrorsForTests = true;

    }

    public sealed class SuppressLogsScope : IDisposable
    {
        public SuppressLogsScope()
        {
            prev = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
        }

        readonly bool prev;

        public void Dispose()
        {
            LogAssert.ignoreFailingMessages = prev;
        }

    }

    /// Test-only resolver used to prevent FakeHandlerWithBadPath from
    /// interfering with normal resolution during automated tests.
    /// 
    /// This does NOT affect production RowVisualHandlerResolver — it's purely
    /// a fixture tool. It works by cloning and filtering the provided lookup
    /// so any mapping whose value is FakeHandlerWithBadPath (or a subclass)
    /// is excluded before resolution. This ensures that the bad-path test handler
    /// can exist for targeted tests without breaking unrelated ones.
    public class SilentTestResolver : IRowVisualHandlerResolver
    {
        public virtual Type ResolveHandler(IDictionary<Type, Type> visualHandlerLookup, Type contentType)
        {
            var filteredLookup = visualHandlerLookup
            .Where(kvp => !typeof(FakeHandlerWithBadPath).IsAssignableFrom(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Debug.Log("Filtered lookup:");
            foreach (var kvp in filteredLookup)
                Debug.Log($"Key: {kvp.Key.Name}, Value: {kvp.Value.Name}");


            Type handlerType = null;

            // Exact match
            if (filteredLookup.TryGetValue(contentType, out handlerType))
                return handlerType;

            // Inheritance-based
            var candidates = filteredLookup.Keys
                .Where(baseType => baseType.IsAssignableFrom(contentType))
                .Select(baseType => new
                {
                    BaseType = baseType,
                    Distance = InheritanceDistance(baseType, contentType)
                })
                .OrderBy(x => x.Distance)
                .ToList();

            if (candidates.Any())
                return filteredLookup[candidates.First().BaseType];

            // Fallback
            if (filteredLookup.TryGetValue(typeof(object), out handlerType))
                return handlerType;

            throw new InvalidOperationException(
                $"No handler for {contentType.Name} and no generic fallback found.");

        }

        // Helper: how many steps from baseType → derivedType
        static int InheritanceDistance(Type baseType, Type derivedType)
        {
            int distance = 0;
            for (var t = derivedType; t != null && t != baseType; t = t.BaseType)
                distance++;
            return distance;
        }
    }

}
