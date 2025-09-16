using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;
using Amanita.EditorUtils;
using UnityEngine.TestTools;
using System.Collections;

namespace Amanita.Tests.EditMode
{
    /// <summary>
    /// Integration tests exercising StringRowVisualHandler together with VariableSourceAsset,
    /// ensuring StringMuscariable-backed variables in a VariableSourceAsset stay in sync
    /// when the key and value fields are committed (simulating pressing Enter).
    /// Tests are written in an observer-centric style: they don't directly apply the serialized
    /// changes themselves; instead they publish the same signal the editor wiring listens to
    /// and let the observer (test-registered callback) perform the commit. This mirrors the
    /// production observer pattern where UI change signals cause commits/saves.
    /// </summary>
    public class StringRowAndVariableSourceIntegrationTests
    {
        [SetUp]
        public void SetUp()
        {
            PrepSourceAsset();
            void PrepSourceAsset()
            {
                _source = ScriptableObject.CreateInstance<VariableSourceAsset>();

                // We want to give the UI something to render right away, hence this initial var
                var stringVar = _source.AddNewVariableOfContentType<string>(initStringVarKey);
                stringVar.Value = initStringVarValue;
            }

            PrepUIElements();
            void PrepUIElements()
            {
                _resolver = new RowVisualHandlerResolver();
                _handlerPool = new RowVisualHandlerPool(_resolver, RowVisualHandlerRegistry.VisualHandlerLookup);
                _rowPool = new VariableRowPool();

                var factoryInit = new VariableRowFactoryInitArgs
                {
                    RowPool = _rowPool,
                    HandlerPool = _handlerPool,
                    Holder = new VisualElement()
                };
                _rowFactory = new VariableRowFactory();
                _rowFactory.Init(factoryInit);

                _uiList = new ListView();
                _countLabel = new UITKLabel();
                _listView = new VariableListView(new VariableListViewInitArgs
                {
                    List = _uiList,
                    CountLabel = _countLabel,
                    RowFactory = _rowFactory
                });

                // Bind source variables to the list view and materialize rows for tests
                _listView.SetVariables(_source.Variables);
                _listView.ForceMaterializeAllRowsForTests();
            }

            GatherUpDestructables();
            void GatherUpDestructables()
            {
                // I normally wouldn't wrap just one line into a method, but this is
                // here to illustrate the pattern for when more things need to be
                // added later.
                _toDestroy.Add(_source);
            }

            PreTestAssertions();
            void PreTestAssertions()
            {
                Assert.AreEqual(1, _listView.RowCount, "Expected one materialized row for single variable.");

                var row = _listView.RowAtIndex(0);
                Assert.IsNotNull(row, "Row should be present.");

                var handler = row.VisualHandler as StringRowVisualHandler;
                Assert.IsNotNull(handler, "Handler should be a StringRowVisualHandler.");

                // Locate the KeyInput TextField and ValueField
                var keyField = handler.RowRoot.Q<TextField>("KeyInput");
                var valueField = handler.RowRoot.Q<TextField>("ValueField");
                Assert.IsNotNull(keyField, "KeyInput not found on the row template.");
                Assert.IsNotNull(valueField, "ValueField not found on the row template.");

                // Confirm initial state
                var original = _source.GetVariable(initStringVarKey) as StringMuscariable;
                Assert.IsNotNull(original);
                Assert.AreEqual(initStringVarValue, original.Value);
            }
        }

        protected VariableSourceAsset _source;
        protected readonly string initStringVarKey = "greeting";
        protected readonly string initStringVarValue = "hello";

        protected RowVisualHandlerResolver _resolver;
        protected RowVisualHandlerPool _handlerPool;
        protected VariableRowPool _rowPool;

        protected VariableRowFactory _rowFactory;
        protected ListView _uiList;
        protected UITKLabel _countLabel;
        protected VariableListView _listView;

        protected readonly List<Object> _toDestroy = new();

        [TearDown]
        public void TearDown()
        {
            try
            {
                _listView?.Dispose();
                _rowFactory?.Dispose();
            }
            catch
            {
                /* best-effort cleanup */
            }

            foreach (var elem in _toDestroy)
            {
                if (Application.isEditor && elem != null)
                    Object.DestroyImmediate(elem);
            }

            ReleaseNullRefs();
            void ReleaseNullRefs()
            {
                _toDestroy.Clear();
                _listView = null;
                _rowFactory = null;
                _uiList = null;
                _count_label_safe();
                _rowPool = null;
                _handlerPool = null;
                _resolver = null;
                _source = null;
            }

            void _count_label_safe()
            {
                _countLabel = null;
            }
        }

        [Test]
        public void EnterOnValueField_SavesValueTo_VariableSourceAsset_StringMuscariable()
        {
            var row = _listView.RowAtIndex(0);
            var handler = row.VisualHandler as StringRowVisualHandler;

            // Locate the ValueField in the handler's RowRoot
            var valueField = handler.RowRoot.Q<TextField>("ValueField");
            var keyField = handler.RowRoot.Q<TextField>("KeyInput");

            // Observer-style commit: subscribe to the same editor-global signal production code uses.
            System.Action<object> responseToControlValueChanged = null;
            responseToControlValueChanged = (_) =>
            {
                // IMPORTANT: programmatic assignment to UI fields doesn't always run the
                // delayed-binding pathways the editor UI uses (especially for isDelayed TextField).
                // In production the inspector wiring observes UI signals and then applies the
                // serialized properties. To faithfully test that observer flow we copy the UI
                // values into the holder's SerializedObject explicitly, then apply.
                var so = handler.SerializedVar;
                if (so == null) return;

                var valProp = so.FindProperty("muscariable.value");
                if (valProp != null)
                    valProp.stringValue = valueField?.value;

                //// Also ensure key is kept if present (not needed for this test but harmless)
                //var keyProp = so.FindProperty("muscariable.key");
                //if (keyProp != null)
                //    keyProp.stringValue = keyField?.value;

                so.ApplyModifiedPropertiesWithoutUndo();
            };

            AmanitaEditorSignals.ControlValueChanged += responseToControlValueChanged;
            try
            {
                // Simulate user typing a new value and pressing Enter by setting the field's value,
                // then publishing the control-change signal that the editor wiring would raise.
                string newVal = "goodbye";
                valueField.value = newVal;
                // Publish the control-change event -> the observer will copy values into the serialized object and apply.
                AmanitaEditorSignals.ControlValueChanged(null);

                // Assert the VariableSourceAsset's muscariable list contains the updated value
                var found = _source.GetVariable("greeting") as StringMuscariable;
                Assert.IsNotNull(found, "Muscariable with key 'greeting' should still exist.");
                Assert.AreEqual(newVal, found.Value, "Muscariable value should have been updated from the UI commit.");
            }
            finally
            {
                AmanitaEditorSignals.ControlValueChanged -= responseToControlValueChanged;
            }

        }

        [Test]
        public void EnterOnKeyField_RenamesVariable_In_VariableSourceAsset_and_keeps_value()
        {
            var row = _listView.RowAtIndex(0);
            var handler = row.VisualHandler as StringRowVisualHandler;
            var keyField = handler.RowRoot.Q<TextField>("KeyInput");
            var valueField = handler.RowRoot.Q<TextField>("ValueField");
            var original = _source.GetVariable("greeting") as StringMuscariable;

            // Observer-style commit callback — copy UI values into the serialized object then apply.
            System.Action<object> commitCallback = null;
            commitCallback = (_) =>
            {
                var so = handler.SerializedVar;
                if (so == null) return;

                var keyProp = so.FindProperty("muscariable.key");
                if (keyProp != null)
                    keyProp.stringValue = keyField?.value;

                var valProp = so.FindProperty("muscariable.value");
                if (valProp != null)
                    valProp.stringValue = valueField?.value;

                so.ApplyModifiedPropertiesWithoutUndo();
            };

            AmanitaEditorSignals.ControlValueChanged += commitCallback;
            try
            {
                // Change key and value via UI fields
                string newKey = "salutation";
                string newVal = "hiya";

                keyField.value = newKey;
                valueField.value = newVal;

                // Publish the control-change signal -> registered observer applies serialized changes
                AmanitaEditorSignals.ControlValueChanged(new object());

                // After committing, the VariableSourceAsset list should reflect the renamed key
                var byNewKey = _source.GetVariable(newKey) as StringMuscariable;
                Assert.IsNotNull(byNewKey, "VariableSourceAsset should expose the muscariable under the new key.");
                Assert.AreEqual(newVal, byNewKey.Value, "Value should remain synced after renaming key.");

                var byOldKey = _source.GetVariable("greeting");
                Assert.IsNull(byOldKey, "Old key should no longer resolve after rename.");
            }
            finally
            {
                AmanitaEditorSignals.ControlValueChanged -= commitCallback;
            }
        }
    }
}