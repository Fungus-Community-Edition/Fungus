using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;

namespace Amanita.Tests.EditMode
{
    public class VariableListViewDragHandleTests
    {
        GameObject _host;
        ListView _uiList;
        UITKLabel _count;
        VariableListView _view;
        VariableRowFactory _factory;
        VariableRowPool _rowPool;
        RowVisualHandlerPool _handlerPool;
        IRowVisualHandlerResolver _resolver;

        MethodInfo _miOnCanStartDrag;
        FieldInfo _fiLastPointerFlag;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("VH_Host");

            _uiList = new ListView();
            _count = new UITKLabel();
            _resolver = new RowVisualHandlerResolver();
            _handlerPool = new RowVisualHandlerPool(_resolver, RowVisualHandlerRegistry.VisualHandlerLookup);
            _rowPool = new VariableRowPool();
            _factory = new VariableRowFactory();
            _factory.Init(new VariableRowFactoryInitArgs
            {
                HandlerPool = _handlerPool,
                RowPool = _rowPool
            });

            _view = new VariableListView(_uiList, _count, new ListViewLayoutRefresher());
            _view.SetFactory(_factory);

            // Enable drag-handle requirement with a made-up handle name we will pretend exists
            _view.RequireDragHandle("DragHandle");

            _miOnCanStartDrag = typeof(VariableListView)
                .GetMethod("OnCanStartDrag", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(_miOnCanStartDrag, "Could not reflect OnCanStartDrag");

            _fiLastPointerFlag = typeof(VariableListView)
                .GetField("_lastPointerDownOnHandle", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(_fiLastPointerFlag, "Could not reflect _lastPointerDownOnHandle");
        }

        [TearDown]
        public void TearDown()
        {
            _view?.Dispose();
            if (_host != null) UnityEngine.Object.DestroyImmediate(_host);
        }

        TVar CreateVar<TVar, TValue>(string key, TValue val = default)
            where TVar : Component, IVariable
        {
            var v = _host.AddComponent<TVar>();
            v.Key = key;
            // Optional value set (ignore reflection ambiguity here)
            return v;
        }

        bool InvokeCanStart()
        {
            // CanStartDragArgs is a struct; we create a default instance
            var argsType = _miOnCanStartDrag.GetParameters()[0].ParameterType;
            object argsInstance = Activator.CreateInstance(argsType);
            return (bool)_miOnCanStartDrag.Invoke(_view, new object[] { argsInstance });
        }

        [Test]
        public void DragDisallowed_WithoutHandleFlag()
        {
#if UNITY_EDITOR
            // Ensure pointer flag false
            _view.ForTests_SetLastPointerDownOnHandle(false);
            bool allowed = InvokeCanStart();
            Assert.False(allowed, "Drag should be blocked when handle not 'pressed'.");
#else
            Assert.Pass("Editor-only test");
#endif
        }

        [Test]
        public void DragAllowed_AfterHandleFlagSet()
        {
#if UNITY_EDITOR
            _view.ForTests_SetLastPointerDownOnHandle(true);
            bool allowed = InvokeCanStart();
            Assert.True(allowed, "Drag should be allowed when handle flag set.");
            // Flag resets after success
            bool flagAfter = (bool)_fiLastPointerFlag.GetValue(_view);
            Assert.False(flagAfter, "Flag should auto-reset after granting drag permission.");
#else
            Assert.Pass("Editor-only test");
#endif
        }

        [Test]
        public void SubsequentDragRequiresHandleAgain()
        {
#if UNITY_EDITOR
            _view.ForTests_SetLastPointerDownOnHandle(true);
            Assert.True(InvokeCanStart(), "First drag should pass.");
            // Without re-setting flag, second attempt should fail
            Assert.False(InvokeCanStart(), "Second drag should fail without new handle pointer.");
#else
            Assert.Pass("Editor-only test");
#endif
        }

        [Test]
        public void DragAlwaysAllowed_WhenHandleModeDisabled()
        {
#if UNITY_EDITOR
            // Re-create in non-handle mode
            _view.Dispose();
            _view = new VariableListView(_uiList, _count, new ListViewLayoutRefresher());
            _view.SetFactory(_factory); // do NOT call RequireDragHandle
            _miOnCanStartDrag = typeof(VariableListView)
                .GetMethod("OnCanStartDrag", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.True(InvokeCanStart(), "Drag should be allowed when handle mode disabled.");
#else
            Assert.Pass("Editor-only test");
#endif
        }

        [Test]
        public void ReorderEventStillFires_WhenHandleUsed()
        {
#if UNITY_EDITOR
            var f = CreateVar<FloatVariable, float>("f1", 1f);
            var s = CreateVar<StringVariable, string>("s1", "a");
            var i = CreateVar<IntegerVariable, int>("i1", 5);
            _view.SetVariables(new IVariable[] { f, s, i });

            int eventCount = 0;
            IReadOnlyList<IVariable> lastOrder = null;
            _view.OrderChanged += o => { eventCount++; lastOrder = o; };

            // Simulate permitted drag (manually set flag then reorder mutate)
            _view.ForTests_SetLastPointerDownOnHandle(true);
            Assert.True(InvokeCanStart(), "Precondition: drag permitted by handle flag.");

            // Simulate Unity mutation: move first to end
            var internalListField = typeof(VariableListView)
                .GetField("_variables", BindingFlags.NonPublic | BindingFlags.Instance);
            var internalList = (List<IVariable>)internalListField.GetValue(_view);
            internalList.RemoveAt(0);
            internalList.Add(f);

            // Invoke reorder callback (from=0,to=3 visually)
            var miReorder = typeof(VariableListView)
                .GetMethod("OnItemIndexChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            miReorder.Invoke(_view, new object[] { 0, 3 });

            Assert.AreEqual(1, eventCount, "OrderChanged not fired.");
            CollectionAssert.AreEqual(new IVariable[] { s, i, f }, lastOrder);
#else
            Assert.Pass("Editor-only test");
#endif
        }
    }
}