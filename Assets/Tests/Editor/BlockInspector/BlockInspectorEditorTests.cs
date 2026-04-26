#if UNITY_EDITOR
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

// Host window to provide a valid IMGUI Event context for OnInspectorGUI calls.
namespace VScriptingTests.FCWindowOperations
{
    public class BlockInspectorEditorTests
    {
        
        
        protected BlockInspectorTestHostWindow _hostWindow;

        // Dummy command to exercise command editor caching
        protected class DummyCommand : Command
        {
            public override string GetSummary() => "DummyCommand Summary";
        }

        [SetUp]
        public void SetUp()
        {
            Flowchart.ResetStaticsForTest();

            _flowchartHolder = new GameObject("FlowchartHost");
            _flowchart = _flowchartHolder.AddComponent<Flowchart>();

            _firstBlock = _flowchartHolder.AddComponent<Block>();
            _firstBlock.BlockName = "Block_A";

            _secondBlock = _flowchartHolder.AddComponent<Block>();
            _secondBlock.BlockName = "Block_B";

            _blockInspectorAsset = ScriptableObject.CreateInstance<BlockInspector>();
            _blockInspectorAsset._block = _firstBlock;

            _editor = Editor.CreateEditor(_blockInspectorAsset, typeof(BlockInspectorEditor)) as BlockInspectorEditor;

            // Ensure static cache is clean
            GetCachedCommandEditors().Clear();
        }

        protected GameObject _flowchartHolder;
        protected Flowchart _flowchart;
        protected Block _firstBlock;
        protected Block _secondBlock;

        protected BlockInspector _blockInspectorAsset;
        protected BlockInspectorEditor _editor;

        [TearDown]
        public void TearDown()
        {
            if (_hostWindow != null)
            {
                _hostWindow.Close();
                _hostWindow = null;
            }

            if (_editor != null)
                UnityEngine.Object.DestroyImmediate(_editor);

            if (_blockInspectorAsset != null)
                UnityEngine.Object.DestroyImmediate(_blockInspectorAsset);

            if (_flowchartHolder != null)
                UnityEngine.Object.DestroyImmediate(_flowchartHolder);

            GetCachedCommandEditors().Clear();
            Flowchart.ResetStaticsForTest();
            BlockInspectorTestHostWindow.EditorUnderTest = null;
        }

        protected static FieldInfo FI_ActiveBlockEditor =>
            typeof(BlockInspectorEditor).GetField("activeBlockEditor",
                BindingFlags.Instance | BindingFlags.NonPublic);

        protected static FieldInfo FI_ActiveCommandEditor =>
            typeof(BlockInspectorEditor).GetField("activeCommandEditor",
                BindingFlags.Instance | BindingFlags.NonPublic);

        protected static FieldInfo FI_CachedCommandEditors =>
            typeof(BlockInspectorEditor).GetField("cachedCommandEditors",
                BindingFlags.Static | BindingFlags.NonPublic);

        protected BlockEditor GetActiveBlockEditor() =>
            (BlockEditor)FI_ActiveBlockEditor.GetValue(_editor);

        protected CommandEditor GetActiveCommandEditor() =>
            (CommandEditor)FI_ActiveCommandEditor.GetValue(_editor);

        protected List<CommandEditor> GetCachedCommandEditors() =>
            (List<CommandEditor>)FI_CachedCommandEditors.GetValue(null);

        [UnityTest]
        public IEnumerator NoBlockSelected_EarlyExit_NoEditorsCreated()
        {
            _firstBlock.IsSelected = false;
            yield return RunInspectorOnce();

            Assert.IsNull(GetActiveBlockEditor(), "BlockEditor should not be created when block is not selected.");
            Assert.IsNull(GetActiveCommandEditor(), "CommandEditor should not exist.");
            Assert.AreEqual(0, GetCachedCommandEditors().Count);
        }

        protected IEnumerator RunInspectorOnce()
        {
            if (_hostWindow == null)
            {
                _hostWindow = ScriptableObject.CreateInstance<BlockInspectorTestHostWindow>();
                _hostWindow.titleContent = new GUIContent("BlockInspectorTestHostWindow");
                _hostWindow.ShowUtility();
            }
            BlockInspectorTestHostWindow.EditorUnderTest = _editor;

            // Let at least one repaint/layout happen
            yield return null;
        }

        [UnityTest]
        public IEnumerator MultipleBlocksSelected_ActiveBlockEditorNotCreated()
        {
            // Use the selection API to register both
            _flowchart.AddToSelection(_firstBlock);
            _flowchart.AddToSelection(_secondBlock);

            Assert.Greater(_flowchart.SelectedBlockCount, 1,
                "Test precondition: should have more than one selected block.");

            yield return RunInspectorOnce();
            Assert.IsNull(GetActiveBlockEditor(), "BlockEditor should not be created when multiple blocks selected.");
        }

        [UnityTest]
        public IEnumerator SingleBlockSelected_BlockEditorCreated()
        {
            _flowchart.AddToSelection(_firstBlock);

            yield return RunInspectorOnce();

            Assert.IsNotNull(GetActiveBlockEditor(), "BlockEditor should be created for single selected block.");
        }

        [UnityTest]
        public IEnumerator SingleCommandSelected_CommandEditorCachedAndReused()
        {
            _flowchart.AddToSelection(_firstBlock);

            var dummyCmd = _flowchartHolder.AddComponent<DummyCommand>();
            // Ensure it belongs logically to the block
            if (!_firstBlock.CommandList.Contains(dummyCmd))
            {
                _firstBlock.CommandList.Add(dummyCmd);
            }

            // Selection
            _flowchart.ClearSelectedCommands();
            _flowchart.AddSelectedCommand(dummyCmd);

            // First pass
            yield return RunInspectorOnce();

            var firstActiveCmdEditor = GetActiveCommandEditor();
            Assert.IsNotNull(firstActiveCmdEditor, "CommandEditor should be created for selected command.");
            Assert.AreEqual(1, GetCachedCommandEditors().Count, "Exactly one cached CommandEditor expected after first draw.");
            Assert.AreSame(dummyCmd, firstActiveCmdEditor.target, "CommandEditor target mismatch.");

            // Second pass (should reuse cached editor)
            yield return RunInspectorOnce();

            var secondActiveCmdEditor = GetActiveCommandEditor();
            Assert.AreSame(firstActiveCmdEditor, secondActiveCmdEditor, "CommandEditor instance should be reused (cached).");
            Assert.AreEqual(1, GetCachedCommandEditors().Count, "Cache size should remain 1.");
        }
    }
}
#endif