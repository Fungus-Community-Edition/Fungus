using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AtMycelia.Hyphlow;
using UnityObj = UnityEngine.Object;
using UnityEngine.TestTools;

namespace VScriptingTests.VariableOperations
{
    /// <summary>
    /// Editor-side tests that validate VariableData drawer behavior without relying on obsolete helpers.
    /// </summary>
    public class VariableDataEditorTests
    {
        private Flowchart _flowchart;

        [SetUp]
        public void SetUp()
        {
            VariableRegistryService.EnsureDefault();

            // Create a Flowchart to act as owner for variables referenced by VariableData
            var fcGo = new GameObject("TestFlowchart");
            _flowchart = fcGo.AddComponent<Flowchart>();

            Selection.activeGameObject = _flowchart.gameObject;

            toDestroyInTearDown.Add(_flowchart.gameObject);
        }

        private readonly IList<UnityObj> toDestroyInTearDown = new List<UnityObj>();
        private readonly WaitForSecondsRealtime windowViewWait = new WaitForSecondsRealtime(3f);
        // So that when running the tests, we can see the results. Who knows, maybe the tests pass, 
        // but the results suggest they shouldn't have.

        [TearDown]
        public void TearDown()
        {
            VariableRegistryService.RebuildAll(); // Clear out any test vars
            foreach (var obj in toDestroyInTearDown)
            {
                if (obj != null)
                {
                    UnityObj.DestroyImmediate(obj);
                }
            }
        }

        // Replace direct drawer usage with window-based rendering
        [UnityTest]
        public IEnumerator VarRef_Null_DrawsLiteral()
        {
            var holder = ScriptableObject.CreateInstance<IntegerDataHolder>();
            toDestroyInTearDown.Add(holder);
            holder._data.VarRef = null;
            holder._data.Value = 428192;

            // Open window bound to holder.data; Unity will invoke VariableDataDrawer
            var wnd = VariableDataTestWindow.Show(holder, "data");
            toDestroyInTearDown.Add(wnd);

            // Force at least one repaint cycle
            wnd.Repaint();
            yield return null;
            // Allow IMGUI to run once
            EditorApplication.QueuePlayerLoopUpdate();
            yield return null;
            // Optionally wait a frame using EditorUtility
            EditorUtility.SetDirty(holder);
            yield return null;

            var so = new SerializedObject(holder);
            so.Update();
            var itemIdProp = so.FindProperty("_data._backingVarRef._itemId");
            yield return windowViewWait;
            Assert.AreEqual(Variable.InvalidID, itemIdProp.intValue);
            wnd.Close();

        }

        // Assigning a valid IVariable (owned by a Flowchart) should keep variable selection (not literal).
        [UnityTest]
        public IEnumerator VarRef_FlowchartVariable_DrawsVariableSelection()
        {
            var varManagerComponent = _flowchart.GetComponent<VariableManagerComponent>();
            Assert.IsNotNull(varManagerComponent, "VariableManagerComponent not found on Flowchart.");

            var intVar = varManagerComponent.AddNewVariableOfContentType<int>("Health", 123, VariableScope.Private);
            Assert.IsNotNull(intVar, "Failed to create muscariable for test.");
            Assert.Greater(intVar.ItemId, 0, "Muscariable ItemId should be assigned.");

            _flowchart.Refresh();
            VariableRegistryService.RebuildAll(_flowchart); // To make sure the registry knows about it

            var holder = ScriptableObject.CreateInstance<IntegerDataHolder>();
            toDestroyInTearDown.Add(holder);
            holder._data.VarRef = intVar;

            var wnd = VariableDataTestWindow.Show(holder, "data");
            toDestroyInTearDown.Add(wnd);

            wnd.Repaint();
            yield return null;
            EditorApplication.QueuePlayerLoopUpdate();
            yield return null;
            EditorUtility.SetDirty(holder);
            yield return null;

            var so = new SerializedObject(holder);
            so.Update();
            var itemIdProp = so.FindProperty("_data._backingVarRef._itemId");
            yield return windowViewWait;
            Assert.AreNotEqual(Variable.InvalidID, itemIdProp.intValue);
            wnd.Close();
        }

        // Non-generic ScriptableObject holder for IntegerData (Unity cannot instantiate generic ScriptableObjects)
        [Serializable]
        public class IntegerDataHolder : ScriptableObject
        {
            public IntegerData _data = new IntegerData();
        }
    }
}