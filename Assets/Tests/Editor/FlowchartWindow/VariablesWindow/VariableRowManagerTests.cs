using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;
using UITKLabel = UnityEngine.UIElements.Label;

namespace Amanita.Tests.Editor
{
    public class VariableRowManagerTests 
    {
        [SetUp]
        public virtual void SetUp()
        {
            PrepFlowchart();
            PrepUIElements();
            
        }

        protected virtual void PrepUIElements()
        {
            _root = new VisualElement();
            _listContainer = new VisualElement();
            _countLabel = new UITKLabel();
            _addButton = new Button();

            _handlerResolver = new RowVisualHandlerResolver();
            _rowManager = new VariableRowManager(_handlerResolver);

            VariableRowInitArgs args = new VariableRowInitArgs()
            {
                Root = _root,
                CountLabel = _countLabel,
                ListContainer = _listContainer,
                AddButton = _addButton,
                Flowchart = _flowchart,
            };

            _handlerResolver = new RowVisualHandlerResolver();
            _rowManager = new VariableRowManager(_handlerResolver);

            _rowManager.Init(args);
        }

        protected VisualElement _root;
        protected UITKLabel _countLabel;
        protected VisualElement _listContainer;
        protected Button _addButton;
        protected IRowVisualHandlerResolver _handlerResolver;
        protected VariableRowManager _rowManager;

        protected virtual void PrepFlowchart()
        {
            GameObject fcHolder = new GameObject("FC");
            _flowchart = fcHolder.AddComponent<Flowchart>();

            _flowchart.AddNewVariable<float, FloatVariable>("floatVar");
            _flowchart.AddNewVariable<string, StringVariable>("stringVar");
            _flowchart.AddNewVariable<int, IntegerVariable>("intVar");
            _flowchart.AddNewVariable<GameObject, GameObjectVariable>("goVar", fcHolder);
            _flowchart.AddNewVariable<bool, BooleanVariable>("boolVar");
        }

        protected Flowchart _flowchart;

        [TearDown]
        public virtual void TearDown()
        {
            _rowManager.Dispose();
            _handlerResolver = null;
            _rowManager = null;
            UnityObject.DestroyImmediate(_flowchart.gameObject);
        }

        [Test]
        public virtual void VarRemovalReturnsRowsAndHandlersToPool()
        {
            Assert.Ignore();
        }

        [Test]
        public virtual void CountLabel_TextUpdates_AddingVars()
        {
            Assert.Ignore();
        }

        [Test]
        public virtual void CountLabel_TextUpdates_RemovingVars()
        {
            Assert.Ignore();
        }

        [Test]
        public virtual void CountLabel_TextUpdates_MixVarAddsAndRemoves()
        {
            Assert.Ignore();
        }

        [Test]
        public void RowsAddedOnVariableAdditions()
        {
            // initially, no vars → no rows
            _flowchart.ClearVariables();
            Assert.AreEqual(0, _listContainer.childCount);
            Assert.AreEqual("0", _countLabel.text);

            // add one variable
            _flowchart.AddNewVariable<float, FloatVariable>("floatVar1");

            // manager should have heard the event and added exactly one row
            Assert.AreEqual(1, _listContainer.childCount);
            Assert.AreEqual("1", _countLabel.text);

            // add two more
            _flowchart.AddNewVariable<int, IntegerVariable>("intVar1");
            _flowchart.AddNewVariable<string, StringVariable>("stringVar1");

            // now 3 total
            Assert.AreEqual(3, _listContainer.childCount);
            Assert.AreEqual("3", _countLabel.text);
        }
    }

    //What to Test
    //- Pooling mechanics:
    //- Adding variables should allocate new rows until the pool is empty, then reuse.
    //- Removing variables should return rows—and their handlers—to their pools.
    //- Event wiring:
    //- Simulate VariableAdded/VariableRemoved calls and verify the right calls to Init(), Dispose(), and list updates.
    //- Count display:
    //- After a mixture of adds/removes, check that _countLabel.text matches the expected count.


}
