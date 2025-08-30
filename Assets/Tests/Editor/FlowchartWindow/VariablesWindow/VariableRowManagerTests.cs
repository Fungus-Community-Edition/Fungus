using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityObject = UnityEngine.Object;

namespace Amanita.Tests.EditMode
{
    public class VariableRowManagerTests 
    {
        [SetUp]
        public virtual void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            FakeHandlerWithBadPath.SuppressTemplateErrorsForTests = true;
            RowVisualHandler.LoggedMissingOnce.Clear();
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
                string pathToUxml = "_EditorResources/UIToolkitTemplates/VariableDisplayEditor";
                _rootTemplate = Resources.Load<VisualTreeAsset>(pathToUxml);
                _root = _rootTemplate.CloneTree();
                _holdsManager = new VisualElement();
                _listContainer = _root.Q<ScrollView>("rowList");
                _countLabel = _root.Q<UITKLabel>("varCountLabel");
                _addButton = _root.Q<Button>("addVarButton");
                _firstListView = new VariableListView(_listContainer, _countLabel, new ScrollViewLayoutRefresher());
                _rowManagerInitArgs = new VRowManagerInitArgs()
                {
                    HoldsManager = _holdsManager,
                    Root = _root,
                    AddButton = _addButton,
                    Flowchart = _firstFc,
                    VariableListView = _firstListView,
                    VariableRowFactory = _rowFactory,
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
        protected ScrollView _listContainer;
        protected UITKLabel _countLabel;
        protected Button _addButton;
        protected VariableListView _firstListView;
        protected VRowManagerInitArgs _rowManagerInitArgs;

        protected VariableRowManager _rowManager;
        
        protected virtual void PrepFlowchart()
        {
            _fcHolder = new GameObject("FC");
            _firstFc = _fcHolder.AddComponent<Flowchart>();

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
        protected Flowchart _firstFc;

        protected IList<IVariable> _initVars;

        protected virtual void AddInitVarsToFlowchart()
        {
            foreach (var elem in _initVars)
            {
                _firstFc.AddVariable(elem);
            }
        }

        protected virtual void DoPreTestAssumptions()
        {
            var handlerPool = _rowFactory.HandlerPool;
            int expectedHandlersInPool = 0;
            Assume.That(handlerPool.PooledHandlerCount == expectedHandlersInPool,
                "Handler count doesn't start as 0");
        }

        [TearDown]
        public virtual void TearDown()
        {

            _handlerResolver = null;
            _handlerPool = null;
            _rowPool = null;

            _rowFactory?.Dispose();
            _rowFactory = null;

            _rootTemplate = null;
            _root = null;
            _holdsManager = null;
            _listContainer = null;
            _countLabel = null;
            _addButton = null;

            _firstListView = null;
            _rowManagerInitArgs = null;

            _initVars?.Clear();

            _rowManager.Dispose();
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

        [Test]
        public virtual void VarRemovalReturnsRowsAndHandlersToPool()
        {
            
            var toRemove = new IVariable[] 
            {
                // Both of these should be from the init setup
                _firstFc.Variables[0],
                _firstFc.Variables[1]
            };

            foreach (var elem in toRemove)
                _firstFc.RemoveVariable(elem);

            // 3. UI updated
            Assert.AreEqual(_firstFc.VariableCount, _listContainer.childCount);
            string expectedLabelText = string.Format(countLabelFormat, _firstFc.VariableCount);
            Assert.AreEqual(expectedLabelText, _countLabel.text);

            // 4. Exactly two rows & two handlers should be back in their pools
            Assert.AreEqual(2, PooledRowCount);
            Assert.AreEqual(2, PooledHandlerCount);
            
        }

        [Test]
        public virtual void CountLabel_TextUpdates_AddingVars()
        {
            int currentCount = _firstFc.VariableCount;
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
                _firstFc.AddVariable(elem);
                currentCount++;
                expectedLabelText = string.Format(countLabelFormat, currentCount);
                Assert.AreEqual(expectedLabelText, _countLabel.text);
            }

            
        }

        [Test]
        public virtual void CountLabel_TextUpdates_RemovingVars()
        {
            
            int currentCount = _firstFc.VariableCount;
            string expectedLabelText = string.Format(countLabelFormat, currentCount);
            Assert.AreEqual(expectedLabelText, _countLabel.text);

            while (_firstFc.VariableCount > 0)
            {
                _firstFc.RemoveVariableAtIndex(0);
                currentCount--;
                expectedLabelText = string.Format(countLabelFormat, currentCount);
                Assert.AreEqual(expectedLabelText, _countLabel.text);
            }

            
        }

        [Test]
        public virtual void CountLabel_TextUpdates_MixVarAddsAndRemoves()
        {
            int currentCount = _firstFc.VariableCount;
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
                    _firstFc.AddVariable(elem);
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
                    _firstFc.RemoveVariable(elem);
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
            // Initially, no vars = no rows
            _firstFc.ClearVariables();
            int expectedRowCount = 0;
            Assert.AreEqual(expectedRowCount, _listContainer.childCount);
            string expectedResult = string.Format(countLabelFormat, expectedRowCount);
            Assert.AreEqual(expectedResult, _countLabel.text);

            _firstFc.AddNewVariable<float, FloatVariable>("floatVar1");
            expectedRowCount += 1; // The manager should've responded to the var addition, adding just one row

            Assert.AreEqual(expectedRowCount, _listContainer.childCount);
            expectedResult = string.Format(countLabelFormat, expectedRowCount);
            Assert.AreEqual(expectedResult, _countLabel.text);

            _firstFc.AddNewVariable<int, IntegerVariable>("intVar1");
            _firstFc.AddNewVariable<string, StringVariable>("stringVar1");
            expectedRowCount += 2;

            Assert.AreEqual(expectedRowCount, _listContainer.childCount);
            expectedResult = string.Format(countLabelFormat, expectedRowCount);
            Assert.AreEqual(expectedResult, _countLabel.text);
            
        }

        [Test]
        public virtual void RowsAndHandlersReusedAfterRemoval()
        {
            
            int expectedAmountInPools = 0;
            IList<IVariable> varsToRemove = new List<IVariable>(_firstFc.Variables);
            foreach (var elem in varsToRemove)
            {
                _firstFc.RemoveVariable(elem);
                expectedAmountInPools++;
                Assert.AreEqual(expectedAmountInPools, PooledHandlerCount,
                    "Removed a var yet the handler pool didn't have the right amount of stuff");
                Assert.AreEqual(expectedAmountInPools, PooledRowCount,
                    "Removed a var yet the row pool didn't have the right amount of stuff");
            }

            IList<IVariable> varsToAdd = _initVars;

            expectedAmountInPools = _initVars.Count;
            foreach (var elem in varsToAdd)
            {
                _firstFc.AddVariable(elem);
                expectedAmountInPools--;
                Assert.AreEqual(expectedAmountInPools, PooledHandlerCount,
                    "Added a var yet the handler pool didn't reuse a handler");
                Assert.AreEqual(expectedAmountInPools, PooledRowCount,
                    "Add a var yet the row pool didn't reuse a row");
            }

            Assert.AreEqual(0, PooledRowCount,
                "Pooled row count should be zero when all the vars are added back in");
            Assert.AreEqual(0, PooledHandlerCount,
                "Pooled handler count should be zero when all the vars are added back in");
            
        }


        [Test]
        public void GetHandlerFor_ReusesSameHandlerInstance()
        {
            _firstFc.ClearVariables();
            var floatVar = _firstFc.AddNewVariable<float, FloatVariable>("f1");

            _rowManager.Refresh(); // Should prep and "display" the initial row
            var firstRow = _rowManager.GetVisibleRowAt(0);
            var firstHandler = firstRow.VisualHandler;

            _firstFc.RemoveVariable(floatVar);
            _firstFc.AddVariable(floatVar);
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
            int expectedPooledHandlerCount = 0;
            Assume.That(_handlerPool.PooledHandlerCount == expectedPooledHandlerCount,
                "Handler pool has stuff in it despite how we only just added multiple vars");

            _firstFc.ClearVariables(); // After this, all the handlers should be in the pool
            expectedPooledHandlerCount = _initVars.Count;
            Assume.That(_handlerPool.PooledHandlerCount == expectedPooledHandlerCount,
                $"Right after the first var clear, the pool didn't get all the handlers back. " +
                $"How many it has: {_handlerPool.PooledHandlerCount}");

            _rowManager.Refresh();
            _firstFc.AddNewVariable<string, StringVariable>("s1");
            expectedPooledHandlerCount -= 1; // Since that string variable should've gotten one of the handlers unpooled
            Assume.That(_handlerPool.PooledHandlerCount == expectedPooledHandlerCount,
                $"The pool didn't release one handler after having multiple pooled and just one var added. " +
                $"How many the pool has: {_handlerPool.PooledHandlerCount}\n" +
                $"What we expectedc: {expectedPooledHandlerCount}");

            var firstRow = _rowManager.GetVisibleRowAt(0);
            Assume.That(firstRow, Is.Not.Null, "First row wasn't registered properly");
            var firstHandler = firstRow.VisualHandler;
            Assume.That(firstHandler, Is.Not.Null, "First row doesn't have a visual handler");

            _firstFc.RemoveVariable(firstHandler.Variable); // Should get the row (and its handler) into the pool
            // ^ERROR: For some reason, the first handler's variable is null

            expectedPooledHandlerCount += 1;
            int howManyPooled = PooledHandlerCount;
            bool wasReleasedBackIntoPool = howManyPooled == expectedPooledHandlerCount;
            Assert.IsTrue(wasReleasedBackIntoPool,
                $"The row wasn't pooled after the one var from the manager was removed. " +
                $"Handler amount: {howManyPooled} Expected amount: {expectedPooledHandlerCount}");

            // Drain the pool by manually ReleaseHandler
            _handlerPool.Clear();

            // With the pool now empty, adding another var should create a new 
            // handler instance
            _firstFc.AddNewVariable<string, StringVariable>("s2");
            var handler2 = firstRow.VisualHandler;

            bool createdBrandNewInstance = !ReferenceEquals(firstHandler, handler2);
            Assert.AreNotSame(createdBrandNewInstance,
                "Did not create brand new instance of a handler after draining the pool");
            
        }

        [Test]
        public void ReleaseHandler_ResetsHandlerState()
        {
            
            _firstFc.ClearVariables();
            int expectedHandlersInPool = _initVars.Count;
            Assume.That(_handlerPool.PooledHandlerCount == expectedHandlersInPool,
                "Pool doesn't get all its handlers back after a full-on row-clear");
            
            IntegerVariable firstIntVar = _firstFc.AddNewVariable<int, IntegerVariable>("i1");
            expectedHandlersInPool--;
            Assume.That(expectedHandlersInPool == _handlerPool.PooledHandlerCount,
                "Handler count should be one less that the full amount when only one row should be 'visible'");

            var firstRow = _rowManager.GetVisibleRowAt(0);
            var handler = firstRow.VisualHandler;

            // Act: remove, then re-add a *different* integer var. We expect the same handler
            // from before to be reused here
            _firstFc.RemoveVariable(handler.Variable);
            var secondIntVar = _fcHolder.AddComponent<IntegerVariable>();
            secondIntVar.Key = "i2";
            _firstFc.AddVariable(secondIntVar);

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
            _firstFc.ClearVariables();

            _firstFc.AddNewVariable<float, FloatVariable>("f");
            _firstFc.AddNewVariable<string, StringVariable>("s");

            // Remove both
            foreach (var v in _firstFc.Variables.ToArray())
                _firstFc.RemoveVariable(v);

            // Pools should have 1 float‐handler and 1 string‐handler
            // You can reflect into the pool map
            var poolMap = _handlerPool.PoolMap;

            var stringRowType = typeof(StringRowVisualHandler);
            Assert.IsTrue(poolMap.ContainsKey(typeof(FloatRowVisualHandler)),
                "There is no dedicated pool for FloatRowVisualHandlers");
            Assert.IsTrue(poolMap.ContainsKey(stringRowType),
                "There is no dedicated pool for StringRowVisualHandlers"); // string uses default

            int floatVisualHandlerCount = poolMap[typeof(FloatRowVisualHandler)].Count;
            int stringRowVisualHandlerCount = poolMap[stringRowType].Count;

            // We have 5 vars prepped in set up, 1 of which (at this time) should get us a string vis handler.
            // 1 should get us a float vis handler
            int expectedFloatHandlerCount = 1, expectedStringHandlerCount = 1;
            Assert.AreEqual(expectedFloatHandlerCount, floatVisualHandlerCount,
                $"Expected {expectedFloatHandlerCount} float visual handler, got {floatVisualHandlerCount}");
            Assert.AreEqual(expectedStringHandlerCount, stringRowVisualHandlerCount,
                $"Expected {expectedStringHandlerCount} default visual handler(s), got {stringRowVisualHandlerCount}");
            
        }

        [Test]
        public void Refresh_ClearsAndRegeneratesRowsCorrectly()
        {
            _firstFc.ClearVariables();

            _firstFc.AddNewVariable<bool, BooleanVariable>("b1");
            _firstFc.AddNewVariable<int, IntegerVariable>("i1");
            _rowManager.Refresh();

            Assert.AreEqual(2, _listContainer.childCount);
            string expectedLabelText = string.Format(countLabelFormat, 2);
            Assert.AreEqual(expectedLabelText, _countLabel.text);

            // In setup, we add vars that leads to there being _initVars.Count handlers. After we
            // clear the flowchart and add 2 vars, that means only two rows should
            // be outside the pool. 
            int expectedPooledRowCount = _initVars.Count - 2;
            Assert.AreEqual(expectedPooledRowCount, PooledRowCount, 
                $"We expected only {expectedPooledRowCount} left in the pool after adding " +
                $"those two vars. Instead, we got {PooledRowCount}");

            // Act: clear flowchart and refresh again
            _firstFc.ClearVariables();
            _rowManager.Refresh();

            Assert.AreEqual(0, _listContainer.childCount);
            expectedLabelText = string.Format(countLabelFormat, 0);
            Assert.AreEqual(expectedLabelText, _countLabel.text);
            Assert.AreEqual(_initVars.Count, PooledRowCount);
            
        }

        protected virtual int PooledRowCount
        {
            get => _rowFactory.PooledRowCount;
        }

        [Test]
        public void Dispose_ClearsAllAndUnsubscribes()
        {
            _firstFc.ClearVariables();
            _firstFc.AddNewVariable<float, FloatVariable>("x");
            Assert.AreEqual(1, _listContainer.childCount);

            _rowManager.Dispose();

            // UI is cleared
            Assert.AreEqual(0, _listContainer.childCount, 
                $"Expected nothing in the list container, but we got {_listContainer.childCount}");

            // We expect that a row getting removed from view means it'll get pooled
            int varCount = _initVars.Count;
            Assert.AreEqual(varCount, PooledRowCount,
                $"Expected the initial amount of rows ({varCount}) pooled, but we got {PooledRowCount}");
            Assert.AreEqual(varCount, PooledHandlerCount,
                $"Expected the initial amount of handlers ({varCount}) pooled, but we got {PooledHandlerCount}");

            // Further adds/removes have no effect
            _firstFc.AddNewVariable<int, IntegerVariable>("y");
            Assert.AreEqual(0, _listContainer.childCount);
            string expectedLabelText = string.Format(countLabelFormat, 0);
            Assert.AreEqual(expectedLabelText, _countLabel.text,
                $"Expected the count label to say '{expectedLabelText}', but instead it says '{_countLabel.text}'");
            
        }

        [Test]
        public void Init_CanBeCalledMultipleTimesSafely()
        {
            // Given what we do in SetUp, this test's first init is the session's second init
            VisualElement newRoot;
            ScrollView newList;
            UITKLabel newLabel;
            Button newAddButton;
            Flowchart newFC;
            GameObject newFCHolder;

            Flowchart secondFC = ApplyNewInitToManager();
            Flowchart ApplyNewInitToManager()
            {
                newRoot = new VisualElement();
                newList = new ScrollView();
                newLabel = new UITKLabel();
                newAddButton = new Button();
                var fcsInScene = UnityObject.FindObjectsOfType<Flowchart>(); 
                // ^Keeping this obsolete func call; we want compatibility with 2022 LTS
                int fcCount = fcsInScene.Length;
                newFCHolder = new GameObject($"FC_{fcCount.ToString("D2")}");
                newList.name = $"ScrollView_{fcCount.ToString("D2")}";
                newFC = newFCHolder.AddComponent<Flowchart>();

                var varListView = new VariableListView(newList, newLabel, new ScrollViewLayoutRefresher());
                var newInitArgs = new VRowManagerInitArgs
                {
                    Root = newRoot,
                    AddButton = newAddButton,
                    Flowchart = newFC,
                    VariableListView = varListView,
                    VariableRowFactory = _rowFactory,
                };
                _rowManager.Init(newInitArgs);

                Assert.That(newList.childCount == 0, $"List #{fcCount} has children despite the new init");
                Assert.That(_rowManager.VisibleRowCount == 0, $"The manager (after we created List #{fcCount}) has " +
                    $"visible rows despite the new init");

                return newFC;
            }

            VisualElement secondList = newList;

            // We already have a test for verifying that adding a new var gets a new
            // row added, and thus we won't need Assert statements to check for
            // that here
            secondFC.AddNewVariable<bool, BooleanVariable>("var1");
            Flowchart thirdFC = ApplyNewInitToManager();
            VisualElement thirdList = newList;

            // Adding vars to the first flowchart should not affect the second or third roots
            _firstFc.AddNewVariable<bool, BooleanVariable>("z");
            Assert.AreEqual(1, secondList.childCount, $"Adding a var to the first FC " +
                $"changed the child count of the second list. Second list " +
                $"child count: {secondList.childCount}. Might want to check " +
                "how the row manager listens for var additions.");

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
            LogAssert.ignoreFailingMessages = false;
            FakeHandlerWithBadPath.SuppressTemplateErrorsForTests = false;

            StringMuscariable testM = (StringMuscariable)MuscariableFactory.Create(typeof(string));

            // Ensure we hit the right handler after suppression is off
            _firstFc.ClearVariables();

            Type handlerType = typeof(FakeHandlerWithBadPath);
            var badVar = MuscariableFactory.Create(handlerType);
            badVar.Key = "badVar";
            badVar.ItemID = 123;

            string expectedPath = "_EditorResources/UIToolkitTemplates/VarRows/BadPathRow";

            string expectedErrorMessage = string.Format(missingTemplateFormat, handlerType.Name, expectedPath);
            LogAssert.Expect(LogType.Error, expectedErrorMessage);

            _firstFc.AddVariable(badVar);
        }

        protected static readonly string missingTemplateFormat =
            "Template for {0} not found at '{1}'.\nPlease update the path in the RowVisualHandlerAttribute of the former.";

        [Muscariable("", typeof(FakeHandlerWithBadPath), "")]
        public class BadHandlerMuscariable : Muscariable<FakeHandlerWithBadPath>
        {

        }

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
            _firstFc.ClearVariables();
            _firstFc.AddNewVariable<float, FloatVariable>("f");
            _firstFc.AddNewVariable<string, StringVariable>("s");
            _rowManager.Refresh();

            // Force everything into the pool
            _firstFc.ClearVariables();
            _rowManager.Refresh();
            int pooledAfterFirstClear = PooledHandlerCount;

            // Act: repeat without changing the model
            _rowManager.Refresh();
            _rowManager.Refresh();

            // Assert: pool size should NOT grow just by refreshing
            Assert.AreEqual(pooledAfterFirstClear, PooledHandlerCount,
                "Handler pool should not increase when refreshing without changes");
        }

        protected virtual int PooledHandlerCount => _rowFactory.PooledHandlerCount;

        [Test]
        public void AddingSameVariableTwice_NoDuplicateRow()
        {
            var v = _fcHolder.AddComponent<BooleanVariable>();
            v.Key = "dupBool";
            _firstFc.AddVariable(v);
            int rowsAfterFirst = _listContainer.childCount;

            // Force a second AddVariable call with same instance (simulate misuse)
            _firstFc.AddVariable(v);

            Assert.AreEqual(rowsAfterFirst, _listContainer.childCount,
                "Duplicate AddVariable call produced an extra row.");
        }

        [Test]
        public void RemoveAlreadyRemoved_Variable_NoCrash_NoPoolChange()
        {
            var v = _firstFc.Variables[0];
            _firstFc.RemoveVariable(v);
            int pooledRowsBefore = PooledRowCount;
            int pooledHandlersBefore = PooledHandlerCount;

            // Second removal attempt (Flowchart.RemoveVariable guards; emulate by calling again)
            _firstFc.RemoveVariable(v);

            Assert.AreEqual(pooledRowsBefore, PooledRowCount,
                "Second removal unexpectedly altered pooled row count.");
            Assert.AreEqual(pooledHandlersBefore, PooledHandlerCount,
                "Second removal unexpectedly altered pooled handler count.");
        }

        [Test]
        public void PreviousRootRowsPersist_AfterReinit()
        {
            // Capture existing list child count
            int originalCount = _listContainer.childCount;

            // Re-init to new Flowchart + new list
            var newHolder = new GameObject("FC_Alt");
            var secondFc = newHolder.AddComponent<Flowchart>();
            var newRoot = new VisualElement();
            var newList = new ScrollView();
            var newLbl = new UITKLabel();
            var newAddBtn = new Button();
            var newView = new VariableListView(newList, newLbl, new ScrollViewLayoutRefresher());

            _rowManager.Init(new VRowManagerInitArgs {
                Root = newRoot,
                AddButton = newAddBtn,
                Flowchart = secondFc,
                VariableListView = newView,
                VariableRowFactory = _rowFactory
            });

            // Old list should still have its children (not forcibly cleared)
            Assert.AreEqual(originalCount, _listContainer.childCount,
                "Original list rows were unexpectedly cleared after re-init.");

            // New list empty initially
            Assert.AreEqual(0, newList.childCount);

            // Add var to new flowchart -> only new list should change
            secondFc.AddNewVariable<bool, BooleanVariable>("second_fc_bool");
            Assert.AreEqual(1, newList.childCount);
            Assert.AreEqual(originalCount, _listContainer.childCount);
        }

        [Test]
        public void Refresh_ReleasesOnlyCurrentRootRows()
        {
            // Prepare second root scenario
            var secondRoot = new VisualElement();
            var secondList = new ScrollView();
            var secondLabel = new UITKLabel();
            var secondAdd = new Button();
            var secondFcHolder = new GameObject("FC_Second");
            var secondFc = secondFcHolder.AddComponent<Flowchart>();
            var secondView = new VariableListView(secondList, secondLabel, new ScrollViewLayoutRefresher());

            // First: add a variable to original FC so we have at least 1 row
            int originalStart = _listContainer.childCount;
            _firstFc.AddNewVariable<bool, BooleanVariable>("first_extra");
            int originalWithExtra = _listContainer.childCount;

            _rowManager.Init(new VRowManagerInitArgs {
                Root = secondRoot,
                AddButton = secondAdd,
                Flowchart = secondFc,
                VariableListView = secondView,
                VariableRowFactory = _rowFactory
            });

            // Add a variable to second flowchart
            secondFc.AddNewVariable<int, IntegerVariable>("second_int");

            // Call Refresh() (operates only on second root now)
            _rowManager.Refresh();

            Assert.AreEqual(originalWithExtra, _listContainer.childCount,
                "Refresh removed rows from previous root unexpectedly.");
            Assert.AreEqual(1, secondList.childCount,
                "Second list should still have its single row after refresh.");
        }

        [Test]
        public void Dispose_IgnoresSubsequentFlowchartEvents()
        {
            _rowManager.Dispose();
            int pooledRowsBefore = PooledRowCount;
            int pooledHandlersBefore = PooledHandlerCount;

            _firstFc.AddNewVariable<bool, BooleanVariable>("postDispose");
            Assert.AreEqual(0, _listContainer.childCount,
                "Rows appeared after dispose—manager still reacting to events.");
            Assert.AreEqual(pooledRowsBefore, PooledRowCount);
            Assert.AreEqual(pooledHandlersBefore, PooledHandlerCount);
        }

        [Test]
        public void ClearVariables_ThenReAdd_ReusesPools()
        {
            // Capture original variable set and its distinct handler content types
            var originalVars = _firstFc.Variables.ToList();
            var originalCount = originalVars.Count;
            Assert.Greater(originalCount, 0, "Precondition failed: need initial variables.");

            // Start: nothing pooled yet
            Assert.AreEqual(0, PooledRowCount, "Unexpected pooled rows at start.");
            Assert.AreEqual(0, PooledHandlerCount, "Unexpected pooled handlers at start.");

            // Clear all -> all rows & handlers should now be pooled
            _firstFc.ClearVariables();
            Assert.AreEqual(originalCount, PooledRowCount, "Rows not pooled after clear.");
            Assert.AreEqual(originalCount, PooledHandlerCount, "Handlers not pooled after clear.");
            Assert.AreEqual(0, _listContainer.childCount, "List not empty after clear.");

            // Re-add ONE variable of each original content type (to force every handler type back out of the pool)
            // We map by ContentType to ensure we exercise each pooled handler type.
            foreach (var contentType in originalVars.Select(v => v.ContentType).Distinct())
            {
                AddReplacementForType(contentType);
            }

            // Expect: all rows and handlers for those types got reused -> pool counts decreased by number of distinct types we re-added
            int distinctTypesReadded = originalVars.Select(v => v.ContentType).Distinct().Count();
            int expectedRowsRemainingInPool = originalCount - distinctTypesReadded;
            int expectedHandlersRemainingInPool = originalCount - distinctTypesReadded;

            Assert.AreEqual(expectedRowsRemainingInPool, PooledRowCount,
                $"Row pool mismatch. Expected {expectedRowsRemainingInPool}, got {PooledRowCount}");
            Assert.AreEqual(expectedHandlersRemainingInPool, PooledHandlerCount,
                $"Handler pool mismatch. Expected {expectedHandlersRemainingInPool}, got {PooledHandlerCount}");

            // Visible rows should equal number of distinct types re-added
            Assert.AreEqual(distinctTypesReadded, _listContainer.childCount,
                "Visible row count does not match number of distinct types re-added.");

            void AddReplacementForType(Type t)
            {
                // Map known variable content types to AddNewVariable overloads
                if (t == typeof(float))
                    _firstFc.AddNewVariable<float, FloatVariable>($"re_float_{GuidFragment()}");
                else if (t == typeof(string))
                    _firstFc.AddNewVariable<string, StringVariable>($"re_string_{GuidFragment()}", "v");
                else if (t == typeof(int))
                    _firstFc.AddNewVariable<int, IntegerVariable>($"re_int_{GuidFragment()}", 1);
                else if (t == typeof(GameObject))
                    _firstFc.AddNewVariable<GameObject, GameObjectVariable>($"re_go_{GuidFragment()}", _fcHolder);
                else if (t == typeof(bool))
                    _firstFc.AddNewVariable<bool, BooleanVariable>($"re_bool_{GuidFragment()}", true);
                else
                {
                    // Fallback: treat as object reference if needed (extend when you add more types)
                    Debug.LogWarning($"Unhandled content type in re-add test: {t.Name}");
                }
            }

            string GuidFragment() => Guid.NewGuid().ToString("N").Substring(0, 4);
        }
    }

    [RowVisualHandler("Null", typeof(FakeHandlerWithBadPath), "5ryw45y", "_EditorResources/UIToolkitTemplates/VarRows/BadPathRow")]
    public class FakeHandlerWithBadPath : RowVisualHandler<FakeHandlerWithBadPath>
    {
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

            //Debug.Log("Filtered lookup:");
            //foreach (var kvp in filteredLookup)
            //    Debug.Log($"Key: {kvp.Key.Name}, Value: {kvp.Value.Name}");

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
