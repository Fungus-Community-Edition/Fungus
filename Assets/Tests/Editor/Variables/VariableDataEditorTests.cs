using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Amanita;
using Amanita.VScripting;
using System.Reflection;
using UnityObj = UnityEngine.Object;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

namespace VScriptingTests.VariableOperations
{
    /// <summary>
    /// Editor-side tests that validate VariableData drawer behavior without relying on obsolete helpers.
    /// </summary>
    public class VariableDataEditorTests
    {
        private AmanitaManager _manager;
        private Flowchart _flowchart;

        [SetUp]
        public void SetUp()
        {
            // Ensure manager/registry exists (VariableDataDrawer uses AmanitaManager.S)
            _manager = AmanitaManager.EnsureExists();

            // Create a Flowchart to act as owner for variables referenced by VariableData
            var fcGo = new GameObject("TestFlowchart");
            _flowchart = fcGo.AddComponent<Flowchart>();

            AmanitaState amanitaState;
            EnsureEditorSideFlowchartIsSet();
            void EnsureEditorSideFlowchartIsSet()
            {
                amanitaState = GameObject.FindFirstObjectByType<AmanitaState>();
                if (amanitaState == null)
                {
                    GameObject stateHolder = new GameObject("_AmanitaState");
                    stateHolder.hideFlags = HideFlags.HideInHierarchy;
                    amanitaState = stateHolder.AddComponent<AmanitaState>();
                }
                amanitaState.SelectedFlowchart = _flowchart;
            }

            toDestroyInTearDown.Add(_flowchart.gameObject);
            toDestroyInTearDown.Add(amanitaState.gameObject);
            toDestroyInTearDown.Add(_manager.gameObject);
        }

        private readonly IList<UnityObj> toDestroyInTearDown = new List<UnityObj>();
        private readonly WaitForSecondsRealtime windowViewWait = new WaitForSecondsRealtime(3f);
        // So that when running the tests, we can see the results. Who knows, maybe the tests pass, 
        // but the results suggest they shouldn't have.

        [TearDown]
        public void TearDown()
        {
            _manager.VariableRegistry.Rebuild(); // Clear out any test vars
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
            holder.data.VarRef = null;
            holder.data.Value = 428192;

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
            var itemIdProp = so.FindProperty("data.backingVarRef.itemId");
            yield return windowViewWait;
            Assert.AreEqual(Variable.InvalidID, itemIdProp.intValue);
            wnd.Close();
            
        }

        // Assigning a valid IVariable (owned by a Flowchart) should keep variable selection (not literal).
        [UnityTest]
        public IEnumerator VarRef_FlowchartVariable_DrawsVariableSelection()
        {
            var intVar = _flowchart.gameObject.AddComponent<IntegerVariable>();
            intVar.Key = "Health";
            intVar.Value = 123;
            intVar.ItemId = 5;

            // Use reflection to assign the var to the Flowchart's legacyVariables list
            var legacyVarsField = typeof(Flowchart).GetField("legacyVariables", BindingFlags.NonPublic | BindingFlags.Instance);
            var legacyVars = (List<Variable>)legacyVarsField.GetValue(_flowchart);
            legacyVars.Add(intVar);
            legacyVarsField.SetValue(_flowchart, legacyVars);
            _flowchart.Refresh();
            _manager.VariableRegistry.Rebuild(_flowchart); // To make sure the registry knows about it

            var holder = ScriptableObject.CreateInstance<IntegerDataHolder>();
            toDestroyInTearDown.Add(holder);
            holder.data.VarRef = intVar;

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
            var itemIdProp = so.FindProperty("data.backingVarRef.itemId");
            yield return windowViewWait;
            Assert.AreNotEqual(Variable.InvalidID, itemIdProp.intValue);
            wnd.Close();
        }

        // Non-generic ScriptableObject holder for IntegerData (Unity cannot instantiate generic ScriptableObjects)
        [Serializable]
        public class IntegerDataHolder : ScriptableObject
        {
            public IntegerData data = new IntegerData();
        }
    }
}