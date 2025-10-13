#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;

namespace VScriptingTests.VariableOperations
{
    public class VariableReferenceDrawerTests
    {
        [SetUp]
        public void SetUp()
        {
            Flowchart.ResetStaticsForTest();

            PrepFlowcharts();
            void PrepFlowcharts()
            {
                _firstFcHolder = new GameObject("Flowchart_A");
                _secondFcHolder = new GameObject("Flowchart_B");
                _firstFc = _firstFcHolder.AddComponent<Flowchart>();
                _secondFc = _secondFcHolder.AddComponent<Flowchart>();

                // Add a BooleanVariable to Flowchart A
                _boolVarA = _firstFcHolder.AddComponent<BooleanVariable>();
                _firstFc.AddVariable(_boolVarA);
            }

            PrepRefHolder();
            void PrepRefHolder()
            {
                // ScriptableObject holder
                _holder = ScriptableObject.CreateInstance<VarRefSO>();
                _holderSO = new SerializedObject(_holder);
                _varRefProp = _holderSO.FindProperty("reference");
                Assert.NotNull(_varRefProp, "Unable to find VariableReference property.");
                _innerVariableProp = _varRefProp.FindPropertyRelative("variable");
                Assert.NotNull(_innerVariableProp, "Unable to find inner 'variable' property on VariableReference.");
            }

            PrepDrawerAndHostWindow();
            void PrepDrawerAndHostWindow()
            {
                // Drawer + host window
                _drawer = new VariableReferenceDrawer();
                _host = ScriptableObject.CreateInstance<VariableReferenceDrawerHostWindow>();
                _host.titleContent = new GUIContent("VarRef Drawer Host");
                _host.Drawer = _drawer;
                _host.SO = _holderSO;
                _host.VarRefProp = _varRefProp;
                _host.InnerVariableProp = _innerVariableProp;
                _host.ShowUtility();
            }

            RegisterWhatToDestroy();
            void RegisterWhatToDestroy()
            {
                _toDestroy.Add(_firstFcHolder);
                _toDestroy.Add(_secondFcHolder);
                _toDestroy.Add(_boolVarA);
                _toDestroy.Add(_holder);
            }
        }

        protected GameObject _firstFcHolder;
        protected GameObject _secondFcHolder;
        protected Flowchart _firstFc;
        protected Flowchart _secondFc;
        protected BooleanVariable _boolVarA;

        protected VarRefSO _holder;
        protected SerializedObject _holderSO;
        protected SerializedProperty _varRefProp;
        protected SerializedProperty _innerVariableProp;

        protected VariableReferenceDrawer _drawer;
        protected VariableReferenceDrawerHostWindow _host;
        
        protected readonly List<Object> _toDestroy = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
                _host.Close();

            foreach (var goingToTheScrapHeap in _toDestroy)
            {
                if (goingToTheScrapHeap != null)
                    Object.DestroyImmediate(goingToTheScrapHeap);
            }

            _toDestroy.Clear();
            Flowchart.ResetStaticsForTest();
        }

        [UnityTest]
        public IEnumerator AutoDetectsFlowchartFromAssignedVariable()
        {
            // Simulate user assigning the variable directly (pre-existing serialized value)
            _innerVariableProp.objectReferenceValue = _boolVarA;
            _holderSO.ApplyModifiedProperties();

            Assert.IsNull(_drawer.lastFlowchart, "Precondition failed: drawer.lastFlowchart should start null.");

            yield return DrawOneFrame();

            Assert.AreSame(_firstFc, _drawer.lastFlowchart,
                "Drawer did not auto-detect the Flowchart from the assigned variable.");        }

        protected IEnumerator DrawOneFrame()
        {
            _host.Repaint();
            yield return null;
        }

        [UnityTest]
        public IEnumerator KeepsVariableWhenFlowchartMatches()
        {
            // Pre-assign variable and flowchart manually (as if previously selected)
            _innerVariableProp.objectReferenceValue = _boolVarA;
            _holderSO.ApplyModifiedProperties();
            _drawer.lastFlowchart = _firstFc;

            yield return DrawOneFrame();

            Assert.AreSame(_boolVarA, _innerVariableProp.objectReferenceValue as BooleanVariable,
                "Variable unexpectedly changed or cleared when drawing with matching Flowchart.");
        }

        [UnityTest]
        public IEnumerator ClearsPrivateVariableWhenSwitchingFlowchart()
        {
            // Assign variable from Flowchart A
            _innerVariableProp.objectReferenceValue = _boolVarA;
            _holderSO.ApplyModifiedProperties();

            // Force drawer to think current selection context is Flowchart B
            _drawer.lastFlowchart = _secondFc;

            yield return DrawOneFrame();

            // Because variable is private & belongs to A, selecting under B should clear it
            Assert.IsNull(_innerVariableProp.objectReferenceValue,
                "Private variable from another Flowchart should have been cleared.");
        }

        private void AssertNoDrawerNRE()
        {
            Assert.IsNotNull(_drawer, "Drawer unexpectedly null.");
            // If we had a null ref inside OnGUI before auto-detection ran, lastFlowchart would still be null after assigning a variable.
            // (Auto-detect only runs when lastFlowchart == null and variable != null)
        }

        [UnityTest]
        public IEnumerator PublicVariableFromOtherFlowchart_IsRetained()
        {
            // Create a public variable on the second Flowchart
            var publicVarB = _secondFcHolder.AddComponent<BooleanVariable>();
            publicVarB.Scope = VariableScope.Public;
            publicVarB.Key = "RemotePublicBool";
            _secondFc.AddVariable(publicVarB);

            // 1) Assign the remote public variable; leave drawer.lastFlowchart = null so auto-detect runs cleanly
            _innerVariableProp.objectReferenceValue = publicVarB;
            _holderSO.ApplyModifiedProperties();

            // First frame: allow auto-detection (sets lastFlowchart to _secondFc)
            yield return DrawOneFrame();
            AssertNoDrawerNRE();
            Assert.AreSame(_secondFc, _drawer.lastFlowchart,
                "Auto-detect failed for public remote variable (expected lastFlowchart == second FC).");
            Assert.AreSame(publicVarB, _innerVariableProp.objectReferenceValue as BooleanVariable,
                "Variable unexpectedly changed during auto-detect phase.");

            // 2) Simulate user switching the Flowchart field to the FIRST flowchart
            _drawer.lastFlowchart = _firstFc;
            yield return DrawOneFrame();

            // Because it's Public, it should NOT be cleared when drawer is bound to a different Flowchart
            Assert.AreSame(publicVarB, _innerVariableProp.objectReferenceValue as BooleanVariable,
                "Public variable from another Flowchart should be retained, but was cleared after context switch.");

            Object.DestroyImmediate(publicVarB);
        }

        [UnityTest]
        public IEnumerator MultipleVariableReferenceDrawers_IsolatedState()
        {
            // Create variables
            var privateVarA = _boolVarA; // already private on first FC
            var publicVarB = _secondFc.AddNewVariable<bool, BooleanVariable>("SecondPublic", false, VariableScope.Public);

            // First holder already exists (_holder / _holderSO / _innerVariableProp)
            _innerVariableProp.objectReferenceValue = privateVarA;
            _holderSO.ApplyModifiedProperties();
            _drawer.lastFlowchart = _firstFc;

            // Create second holder + drawer
            VarRefSO secondHolder;
            SerializedObject secondSO;
            SerializedProperty secondVarRefProp, secondInnerVarProp;
            PrepSecondHolder();
            void PrepSecondHolder()
            {
                secondHolder = ScriptableObject.CreateInstance<VarRefSO>();
                secondSO = new SerializedObject(secondHolder);
                secondVarRefProp = secondSO.FindProperty("reference");
                secondInnerVarProp = secondVarRefProp.FindPropertyRelative("variable");
                secondInnerVarProp.objectReferenceValue = publicVarB;
                secondSO.ApplyModifiedProperties();
            }

            VariableReferenceDrawer secondDrawer;
            VariableReferenceDrawerHostWindow secondHostWindow;
            PrepSecondDrawerAndHostWindow();
            void PrepSecondDrawerAndHostWindow()
            {
                secondDrawer = new VariableReferenceDrawer();
                secondHostWindow = ScriptableObject.CreateInstance<VariableReferenceDrawerHostWindow>();
                secondHostWindow.titleContent = new GUIContent("VarRef Drawer Host 2");
                secondHostWindow.Drawer = secondDrawer;
                secondHostWindow.SO = secondSO;
                secondHostWindow.VarRefProp = secondVarRefProp;
                secondHostWindow.InnerVariableProp = secondInnerVarProp;
                secondDrawer.lastFlowchart = _secondFc;
                secondHostWindow.ShowUtility();
            }

            yield return DrawInitialFrameForBoth();
            IEnumerator DrawInitialFrameForBoth()
            {
                // Draw initial frame for both
                yield return DrawOneFrame();
                secondHostWindow.Repaint();
                yield return null;
            }

            DoPreChangeSanityChecks();
            void DoPreChangeSanityChecks()
            {
                Assert.AreSame(privateVarA, _innerVariableProp.objectReferenceValue as BooleanVariable,
                    "Primary drawer variable mismatch before flowchart swap.");
                Assert.AreSame(publicVarB, secondInnerVarProp.objectReferenceValue as BooleanVariable,
                    "Secondary drawer variable mismatch before flowchart swap.");
            }

            // Change ONLY the first drawer's flowchart to the second FC (should clear private var A)
            _drawer.lastFlowchart = _secondFc;
            yield return DrawOneFrame();

            // First drawer's private variable should clear
            Assert.IsNull(_innerVariableProp.objectReferenceValue,
                "First drawer should have cleared private variable after switching Flowchart context.");

            // Second drawer must remain unaffected
            secondHostWindow.Repaint();
            yield return null;
            Assert.AreSame(publicVarB, secondInnerVarProp.objectReferenceValue as BooleanVariable,
                "Second drawer variable unexpectedly changed after first drawer context switch.");

            secondHostWindow.Close();
            // Clean up additional objects
            Object.DestroyImmediate(publicVarB);
            Object.DestroyImmediate(secondHostWindow);
            Object.DestroyImmediate(secondHolder);
        }

        [Test]
        public void ReportsExpectedPropertyHeight()
        {
            float expected = EditorGUIUtility.singleLineHeight * 2f;
            float heightWeGot = _drawer.GetPropertyHeight(_varRefProp, new GUIContent("Test"));
            Assert.That(heightWeGot, Is.EqualTo(expected).Within(0.01f),
                "Property height mismatch (should be 2 lines).");
        }
    }
}
#endif