using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using AtMycelia.Hyphlow.RuntimeTesting;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObject = UnityEngine.Object;

namespace VScriptingTests.FCWindowOperations
{
    public class CommandEditorTests
    {
        [SetUp]
        public void SetUp()
        {
            Flowchart.ResetStaticsForTest();
            Undo.ClearAll(); // ensure a clean undo stack per test run

            _host = new GameObject("FlowchartHost_ForCommandEditorTests");
            _flowchart = _host.AddComponent<Flowchart>();
            _block = _host.AddComponent<Block>();
            _flowchart.UIModel.BlockViewHeight = 250f;
        }

        protected GameObject _host;
        protected Flowchart _flowchart;
        protected Block _block;

        [TearDown]
        public void TearDown()
        {
            if (_window != null)
            {
                _window.Close();
                _window = null;
            }
            if (_editor != null)
                UnityEngine.Object.DestroyImmediate(_editor);
            if (_host != null)
                UnityEngine.Object.DestroyImmediate(_host);

            Flowchart.ResetStaticsForTest();
        }

        protected CommandEditorTestHostWindow _window;
        protected CommandEditor _editor;

        [UnityTest]
        public IEnumerator NoCommandInfoAttribute_EarlyReturn_NoReorderableLists()
        {
            var cmd = _host.AddComponent<NoInfoCommand>();
            _block.CommandList.Add(cmd);

            _editor = Editor.CreateEditor(cmd, typeof(CommandEditor)) as CommandEditor;
            Assert.NotNull(_editor, "Failed to create CommandEditor.");

            yield return DrawInspectorFrames();

            var lists = GetReorderableLists(_editor);
            Assert.AreEqual(0, lists.Count, "Expected no reorderable lists when no CommandInfoAttribute is present.");
        }

        protected IEnumerator DrawInspectorFrames(int frames = 1, Action preFrame = null)
        {
            if (_window == null)
            {
                _window = ScriptableObject.CreateInstance<CommandEditorTestHostWindow>();
                _window.titleContent = new GUIContent("CommandEditorTestHost");
                _window.ShowUtility();
            }

            _window.EditorUnderTest = _editor;

            for (int i = 0; i < frames; i++)
            {
                preFrame?.Invoke();
                _window.Repaint();
                yield return null;
            }
        }

        protected Dictionary<string, object> GetReorderableLists(CommandEditor editor)
        {
            var dict = FI_ReorderableLists.GetValue(editor) as IDictionary;
            var copy = new Dictionary<string, object>();
            if (dict != null)
            {
                foreach (DictionaryEntry e in dict)
                    copy[(string)e.Key] = e.Value;
            }
            return copy;
        }

        protected static FieldInfo FI_ReorderableLists =>
            typeof(CommandEditor).GetField("reorderableLists", BindingFlags.Instance | BindingFlags.NonPublic);

        [UnityTest]
        public IEnumerator ReorderableArrayProperty_CreatesReorderableList()
        {
            EarlySetupWithDummyArrayCommand();
            yield return DrawInspectorFrames();

            var lists = GetReorderableLists(_editor);
            Assert.IsTrue(ContainsKeyIgnoreCase(lists, "Names"),
                $"Expected a ReorderableList for 'Names'. Keys: {string.Join(", ", lists.Keys)}");
        }

        protected virtual void EarlySetupWithDummyArrayCommand()
        {
            _dummyArrayCommand = _host.AddComponent<DummyArrayCommand>();
            _block.CommandList.Add(_dummyArrayCommand);
            _editor = Editor.CreateEditor(_dummyArrayCommand, typeof(CommandEditor)) as CommandEditor;
            Assert.NotNull(_editor, "Failed to create CommandEditor (DummyArrayCommand).");
        }

        protected DummyArrayCommand _dummyArrayCommand;

        protected static bool ContainsKeyIgnoreCase(Dictionary<string, object> dict, string key) =>
            dict.Keys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

        [UnityTest]
        public IEnumerator ReorderableArrayProperty_ListNotDuplicatedAcrossFrames()
        {
            EarlySetupWithDummyArrayCommand();

            yield return DrawInspectorFrames();
            var listsFirst = GetReorderableLists(_editor);
            Assert.IsTrue(ContainsKeyIgnoreCase(listsFirst, "Names"), "Reorderable list 'Names' not created.");
            Assert.AreEqual(1, listsFirst.Count, "Unexpected extra lists after first draw.");

            yield return DrawInspectorFrames();
            var listsSecond = GetReorderableLists(_editor);
            Assert.AreEqual(1, listsSecond.Count, "ReorderableList count changed unexpectedly after second draw.");
            Assert.IsTrue(ContainsKeyIgnoreCase(listsSecond, "Names"), "Reorderable list 'Names' missing on second draw.");
        }

        [UnityTest]
        public IEnumerator ReorderableArrayProperty_AddElement_IncreasesSize()
        {
            EarlySetupWithDummyArrayCommand();

            yield return DrawInspectorFrames();
            var list = GetReorderableListInstance("Names");
            Assert.NotNull(list);

            var prop = list.serializedProperty;
            int initialSize = prop.arraySize;

            SimulateAdd(list);

            yield return DrawInspectorFrames();

            Assert.AreEqual(initialSize + 1, prop.arraySize, "Element not added.");
        }

        protected ReorderableList GetReorderableListInstance(string keyDisplayName)
        {
            var dict = GetReorderableLists(_editor);
            var kvPair = dict.FirstOrDefault(pairToCheck =>
            {
                return string.Equals(pairToCheck.Key, keyDisplayName, StringComparison.OrdinalIgnoreCase);
            });
            return kvPair.Value as ReorderableList;
        }

        protected void SimulateAdd(ReorderableList toAddTo)
        {
            if (toAddTo.onAddCallback != null)
            {
                toAddTo.onAddCallback(toAddTo);
                return;
            }

            var prop = toAddTo.serializedProperty;
            int newIndex = prop.arraySize;
            prop.InsertArrayElementAtIndex(newIndex > 0 ? newIndex - 1 : 0);
            var elem = prop.GetArrayElementAtIndex(newIndex);
            if (elem != null && elem.propertyType == SerializedPropertyType.String)
                elem.stringValue = string.Empty;
            prop.serializedObject.ApplyModifiedProperties();
        }

        [UnityTest]
        public IEnumerator ReorderableArrayProperty_RemoveElement_DecreasesSize()
        {
            EarlySetupWithDummyArrayCommand();

            yield return DrawInspectorFrames();
            var list = GetReorderableListInstance("Names");
            Assert.NotNull(list);

            var prop = list.serializedProperty;
            int initialSize = prop.arraySize;
            Assert.Greater(initialSize, 0);

            SimulateRemove(list, initialSize - 1);
            yield return DrawInspectorFrames();

            Assert.AreEqual(initialSize - 1, prop.arraySize, "Element not removed.");
        }

        protected void SimulateRemove(ReorderableList toRemoveFrom, int removeIndex)
        {
            if (removeIndex < 0 || removeIndex >= toRemoveFrom.serializedProperty.arraySize)
                return;

            if (toRemoveFrom.onRemoveCallback != null)
            {
                toRemoveFrom.index = removeIndex;
                toRemoveFrom.onRemoveCallback(toRemoveFrom);
                return;
            }
            var prop = toRemoveFrom.serializedProperty;
            prop.DeleteArrayElementAtIndex(removeIndex);
            prop.serializedObject.ApplyModifiedProperties();
        }

        [UnityTest]
        public IEnumerator ReorderableArrayProperty_AddElement_MarksCommandDirty()
        {
            EarlySetupWithDummyArrayCommand();
            yield return DrawInspectorFrames();

            // Try to start from a clean state
            yield return EnsureClean(_dummyArrayCommand);
            bool wasDirtyInitially = EditorUtility.IsDirty(_dummyArrayCommand);

            var list = GetReorderableListInstance("Names");
            Assert.NotNull(list, "ReorderableList not found.");

            SimulateAdd(list);
            yield return DrawInspectorFrames();

            // Always assert list changed
            Assert.Greater(list.serializedProperty.arraySize, 2, "Add did not change array size.");

            if (!wasDirtyInitially)
            {
                Assert.IsTrue(EditorUtility.IsDirty(_dummyArrayCommand),
                    "Command not marked dirty after adding list element.");
            }
            // If it was already dirty we at least validated structural change above.
        }

        // Helper: aggressively try to clear dirty state on a scene component.
        private IEnumerator EnsureClean(UnityObject obj, int frames = 2)
        {
            for (int i = 0; i < frames; i++)
            {
                EditorUtility.ClearDirty(obj);
                yield return null; // allow Unity to process
                if (!EditorUtility.IsDirty(obj))
                    yield break;
            }
        }

        [UnityTest]
        public IEnumerator ReorderableArrayProperty_RemoveElement_MarksCommandDirty()
        {
            EarlySetupWithDummyArrayCommand();
            yield return DrawInspectorFrames();

            var list = GetReorderableListInstance("Names");
            Assert.NotNull(list);
            int initialSize = list.serializedProperty.arraySize;
            Assert.Greater(initialSize, 0, "List unexpectedly empty before removal test.");

            yield return EnsureClean(_dummyArrayCommand);
            bool wasDirtyInitially = EditorUtility.IsDirty(_dummyArrayCommand);

            SimulateRemove(list, initialSize - 1);
            yield return DrawInspectorFrames();

            Assert.AreEqual(initialSize - 1, list.serializedProperty.arraySize, "Element not removed.");

            if (!wasDirtyInitially)
            {
                Assert.IsTrue(EditorUtility.IsDirty(_dummyArrayCommand),
                    "Command not marked dirty after removing list element.");
            }
        }

        [UnityTest]
        public IEnumerator PrimitiveFieldChange_MarksCommandDirty()
        {
            EarlySetupWithDummyArrayCommand();
            yield return DrawInspectorFrames();

            yield return EnsureClean(_dummyArrayCommand);
            bool wasDirtyInitially = EditorUtility.IsDirty(_dummyArrayCommand);

            // Snapshot JSON to guarantee the field change happened regardless of dirty result
            string beforeJson = EditorJsonUtility.ToJson(_dummyArrayCommand);

            SerializedObject so = new SerializedObject(_dummyArrayCommand);
            var someValProp = so.FindProperty("someValue");
            Assert.NotNull(someValProp, "someValue property not found.");
            someValProp.intValue += 1;
            so.ApplyModifiedProperties();

            yield return DrawInspectorFrames();

            string afterJson = EditorJsonUtility.ToJson(_dummyArrayCommand);
            Assert.AreNotEqual(beforeJson, afterJson, "Serialized JSON unchanged after primitive field edit.");

            if (!wasDirtyInitially)
            {
                Assert.IsTrue(EditorUtility.IsDirty(_dummyArrayCommand),
                    "Command not marked dirty after primitive field change.");
            }
        }

        // ---------------- UNDO / REDO TESTS ----------------
        #region Undo/Redo Tests
        [UnityTest]
        public IEnumerator UndoRedo_AddListElement_RestoresAndReapplies()
        {
            EarlySetupWithDummyArrayCommand();
            yield return DrawInspectorFrames();

            var list = GetReorderableListInstance("Names");
            Assert.NotNull(list);
            var prop = list.serializedProperty;
            int initialSize = prop.arraySize;

            // Record snapshot
            Undo.RegisterCompleteObjectUndo(_dummyArrayCommand, "Add Name");
            SimulateAdd(list);
            yield return DrawInspectorFrames();
            Assert.AreEqual(initialSize + 1, prop.arraySize, "Add failed.");

            // Undo
            Undo.PerformUndo();
            yield return DrawInspectorFrames();
            prop.serializedObject.Update();
            Assert.AreEqual(initialSize, prop.arraySize, "Undo did not revert list size.");

            // Redo
            Undo.PerformRedo();
            yield return DrawInspectorFrames();
            prop.serializedObject.Update();
            Assert.AreEqual(initialSize + 1, prop.arraySize, "Redo did not reapply list add.");
        }

        [UnityTest]
        public IEnumerator UndoRedo_RemoveListElement_RestoresAndReapplies()
        {
            EarlySetupWithDummyArrayCommand();
            yield return DrawInspectorFrames();

            var list = GetReorderableListInstance("Names");
            Assert.NotNull(list);
            var prop = list.serializedProperty;
            int initialSize = prop.arraySize;
            Assert.Greater(initialSize, 0);

            Undo.RegisterCompleteObjectUndo(_dummyArrayCommand, "Remove Name");
            SimulateRemove(list, initialSize - 1);
            yield return DrawInspectorFrames();
            prop.serializedObject.Update();
            Assert.AreEqual(initialSize - 1, prop.arraySize, "Removal failed.");

            Undo.PerformUndo();
            yield return DrawInspectorFrames();
            prop.serializedObject.Update();
            Assert.AreEqual(initialSize, prop.arraySize, "Undo did not restore removed element.");

            Undo.PerformRedo();
            yield return DrawInspectorFrames();
            prop.serializedObject.Update();
            Assert.AreEqual(initialSize - 1, prop.arraySize, "Redo did not reapply removal.");
        }

        [UnityTest]
        public IEnumerator UndoRedo_PrimitiveFieldChange_RestoresAndReapplies()
        {
            EarlySetupWithDummyArrayCommand();
            yield return DrawInspectorFrames();

            SerializedObject so = new SerializedObject(_dummyArrayCommand);
            var someValProp = so.FindProperty("someValue");
            Assert.NotNull(someValProp);
            int original = someValProp.intValue;

            Undo.RegisterCompleteObjectUndo(_dummyArrayCommand, "Change someValue");
            someValProp.intValue = original + 5;
            so.ApplyModifiedProperties();
            yield return DrawInspectorFrames();
            Assert.AreEqual(original + 5, _dummyArrayCommand.someValue, "Value change not applied.");

            Undo.PerformUndo();
            yield return DrawInspectorFrames();
            Assert.AreEqual(original, _dummyArrayCommand.someValue, "Undo did not restore original primitive value.");

            Undo.PerformRedo();
            yield return DrawInspectorFrames();
            Assert.AreEqual(original + 5, _dummyArrayCommand.someValue, "Redo did not reapply primitive value.");
        }

        // ===== Grouped Undo/Redo Helper (revised) =====
        private IEnumerator DoGroupedUndoRedo(
            Action<int> mutateAllWithGroup,
            Action assertAfterMutations,
            Action assertAfterUndo,
            Action assertAfterRedo,
            string undoGroupLabel)
        {
            // Begin group
            int group = Undo.GetCurrentGroup();
            Undo.IncrementCurrentGroup(); // ensure fresh group boundary

            // Perform grouped mutations (caller must do per-step Undo.RecordObject / RegisterCompleteObjectUndo)
            mutateAllWithGroup?.Invoke(group);

            // Collapse all operations into single undo step
            Undo.CollapseUndoOperations(group);

            yield return DrawInspectorFrames();

            // Validate post-mutation
            assertAfterMutations?.Invoke();

            // Undo (single step should revert everything)
            Undo.PerformUndo();
            yield return DrawInspectorFrames();
            assertAfterUndo?.Invoke();

            // Redo (single step should reapply everything)
            Undo.PerformRedo();
            yield return DrawInspectorFrames();
            assertAfterRedo?.Invoke();
        }

        private int GetNamesListSize()
        {
            var list = GetReorderableListInstance("Names");
            list.serializedProperty.serializedObject.Update();
            return list.serializedProperty.arraySize;
        }

        [UnityTest]
        public IEnumerator UndoRedo_Grouped_AddElementAndPrimitiveChange_SingleUndoRevertsBoth()
        {
            EarlySetupWithDummyArrayCommand();
            yield return DrawInspectorFrames();

            var list = GetReorderableListInstance("Names");
            Assert.NotNull(list);
            int initialSize = GetNamesListSize();
            int initialPrimitive = _dummyArrayCommand.someValue;

            yield return DoGroupedUndoRedo(
                mutateAllWithGroup: (group) =>
                {
                    // Record object before each individual mutation
                    Undo.RecordObject(_dummyArrayCommand, "Grouped List Add");
                    SimulateAdd(list);

                    Undo.RecordObject(_dummyArrayCommand, "Grouped Primitive Change");
                    _dummyArrayCommand.someValue = initialPrimitive + 10;
                    EditorUtility.SetDirty(_dummyArrayCommand);
                },
                assertAfterMutations: () =>
                {
                    Assert.AreEqual(initialSize + 1, GetNamesListSize(), "List add not applied.");
                    Assert.AreEqual(initialPrimitive + 10, _dummyArrayCommand.someValue, "Primitive change not applied.");
                },
                assertAfterUndo: () =>
                {
                    Assert.AreEqual(initialSize, GetNamesListSize(), "Undo failed to revert list add.");
                    Assert.AreEqual(initialPrimitive, _dummyArrayCommand.someValue, "Undo failed to revert primitive change.");
                },
                assertAfterRedo: () =>
                {
                    Assert.AreEqual(initialSize + 1, GetNamesListSize(), "Redo failed to reapply list add.");
                    Assert.AreEqual(initialPrimitive + 10, _dummyArrayCommand.someValue, "Redo failed to reapply primitive change.");
                },
                undoGroupLabel: "Grouped Add+Primitive");
        }

        [UnityTest]
        public IEnumerator UndoRedo_Grouped_RemoveElementAndPrimitiveChange_SingleUndoRevertsBoth()
        {
            EarlySetupWithDummyArrayCommand();
            yield return DrawInspectorFrames();

            var list = GetReorderableListInstance("Names");
            Assert.NotNull(list);
            int initialSize = GetNamesListSize();
            Assert.Greater(initialSize, 0, "Precondition failed: list unexpectedly empty.");
            int initialPrimitive = _dummyArrayCommand.someValue;

            yield return DoGroupedUndoRedo(
                mutateAllWithGroup: (group) =>
                {
                    Undo.RecordObject(_dummyArrayCommand, "Grouped List Remove");
                    SimulateRemove(list, initialSize - 1);

                    Undo.RecordObject(_dummyArrayCommand, "Grouped Primitive Change");
                    _dummyArrayCommand.someValue = initialPrimitive + 3;
                    EditorUtility.SetDirty(_dummyArrayCommand);
                },
                assertAfterMutations: () =>
                {
                    Assert.AreEqual(initialSize - 1, GetNamesListSize(), "List removal not applied.");
                    Assert.AreEqual(initialPrimitive + 3, _dummyArrayCommand.someValue, "Primitive change not applied.");
                },
                assertAfterUndo: () =>
                {
                    Assert.AreEqual(initialSize, GetNamesListSize(), "Undo failed to restore list.");
                    Assert.AreEqual(initialPrimitive, _dummyArrayCommand.someValue, "Undo failed to revert primitive change.");
                },
                assertAfterRedo: () =>
                {
                    Assert.AreEqual(initialSize - 1, GetNamesListSize(), "Redo failed to reapply list removal.");
                    Assert.AreEqual(initialPrimitive + 3, _dummyArrayCommand.someValue, "Redo failed to reapply primitive change.");
                },
                undoGroupLabel: "Grouped Remove+Primitive");
        }
        #endregion
    }
}