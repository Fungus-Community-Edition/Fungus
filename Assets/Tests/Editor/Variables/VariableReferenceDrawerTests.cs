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

                // Add a BoolMuscariable to Flowchart A (private by default)
                _boolVarA = _firstFc.AddNewMuscariable<bool, BoolMuscariable>("LocalBoolA", default, VariableScope.Private);
            }

            PrepRefHolder();
            void PrepRefHolder()
            {
                // ScriptableObject holder
                _varRefHolder = ScriptableObject.CreateInstance<VarRefSO>();
                _holderSO = new SerializedObject(_varRefHolder);
                _varRefProp = _holderSO.FindProperty("reference");
                Assert.NotNull(_varRefProp, "Unable to find VariableReference property.");

                // We now use the managed IVariable slot named 'variable'
                _innerVariableProp = _varRefProp.FindPropertyRelative("variable");
                Assert.NotNull(_innerVariableProp, "Unable to find managed 'variable' property on VariableReference.");
                Assert.AreEqual(SerializedPropertyType.ManagedReference, _innerVariableProp.propertyType,
                    "VariableReference.variable should be a ManagedReference IVariable.");
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
                _toDestroy.Add(_varRefHolder);
            }
        }

        protected GameObject _firstFcHolder;
        protected GameObject _secondFcHolder;
        protected Flowchart _firstFc;
        protected Flowchart _secondFc;
        protected BoolMuscariable _boolVarA;

        protected VarRefSO _varRefHolder;
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
            {
                _host.Close();
            }

            foreach (var goingToTheScrapHeap in _toDestroy)
            {
                if (goingToTheScrapHeap != null)
                {
                    Object.DestroyImmediate(goingToTheScrapHeap);
                }
            }

            _toDestroy.Clear();
            Flowchart.ResetStaticsForTest();
        }

        [UnityTest]
        public IEnumerator AutoDetectsFlowchartFromAssignedVariable()
        {
            // Simulate user assigning the variable directly (pre-existing serialized value)
            _innerVariableProp.managedReferenceValue = _boolVarA;
            _holderSO.ApplyModifiedProperties();

            Assert.IsNull(_drawer.lastFlowchart, "Precondition failed: drawer.lastFlowchart should start null.");

            yield return DrawOneFrame();

            Assert.AreSame(_firstFc, _drawer.lastFlowchart,
                "Drawer did not auto-detect the Flowchart from the assigned variable.");
        }

        protected IEnumerator DrawOneFrame()
        {
            _host.Repaint();
            yield return null;
        }

        [UnityTest]
        public IEnumerator KeepsVariableWhenFlowchartMatches()
        {
            // Pre-assign variable and flowchart manually (as if previously selected)
            _innerVariableProp.managedReferenceValue = _boolVarA;
            _holderSO.ApplyModifiedProperties();
            _drawer.lastFlowchart = _firstFc;

            yield return DrawOneFrame();

            var selected = _innerVariableProp.managedReferenceValue as IVariable;
            Assert.IsTrue(VarsSemanticallyEqual(_boolVarA, selected),
                "Variable unexpectedly changed or cleared when drawing with matching Flowchart.");
        }

        [UnityTest]
        public IEnumerator ClearsPrivateVariableWhenSwitchingFlowchart()
        {
            // Assign variable from Flowchart A
            _innerVariableProp.managedReferenceValue = _boolVarA;
            _holderSO.ApplyModifiedProperties();

            // Force drawer to think current selection context is Flowchart B
            _drawer.lastFlowchart = _secondFc;

            yield return DrawOneFrame();

            // Refresh the cached properties before reading
            _holderSO.Update();

            // Because variable is private & belongs to A, selecting under B should clear it (managed path)
            var refreshedInner = _varRefProp.FindPropertyRelative("variable");
            Assert.IsNull(refreshedInner.managedReferenceValue,
                "Private variable from another Flowchart should have been cleared.");
        }

        private void AssertNoDrawerNRE()
        {
            Assert.IsNotNull(_drawer, "Drawer unexpectedly null.");
        }

        [UnityTest]
        public IEnumerator PublicVariableFromOtherFlowchart_IsRetained()
        {
            // Create a public muscariable on the second Flowchart
            var publicVarB = _secondFc.AddNewMuscariable<bool, BoolMuscariable>("RemotePublicBool", 
                default, VariableScope.Public);

            // 1) Assign the remote public variable; leave drawer.lastFlowchart = null so auto-detect runs cleanly
            _innerVariableProp.managedReferenceValue = publicVarB;
            _holderSO.ApplyModifiedProperties();

            // First frame: allow auto-detection (sets lastFlowchart to _secondFc)
            yield return DrawOneFrame();
            AssertNoDrawerNRE();
            Assert.AreSame(_secondFc, _drawer.lastFlowchart,
                "Auto-detect failed for public remote variable (expected lastFlowchart == second FC).");

            var selectedAfterDetect = _innerVariableProp.managedReferenceValue as IVariable;
            Assert.IsTrue(VarsSemanticallyEqual(publicVarB, selectedAfterDetect),
                "Variable unexpectedly changed during auto-detect phase.");

            // 2) Simulate user switching the Flowchart field to the FIRST flowchart
            _drawer.lastFlowchart = _firstFc;
            yield return DrawOneFrame();

            // Because it's Public, it should NOT be cleared when drawer is bound to a different Flowchart
            Assert.AreSame(publicVarB, _innerVariableProp.managedReferenceValue as BoolMuscariable,
                "Public variable from another Flowchart should be retained, but was cleared after context switch.");
        }

        [Ignore("Might need to reimplement VariableReferences entirely later")]
        [UnityTest]
        public IEnumerator MultipleVariableReferenceDrawers_IsolatedState()
        {
            // Create variables
            BoolMuscariable privateVarA = _boolVarA; // already private on first FC
            BoolMuscariable publicVarB = _secondFc.AddNewMuscariable<bool, BoolMuscariable>("SecondPublic", 
                false, VariableScope.Public);

            // First holder already exists (_holder / _holderSO / _innerVariableProp)
            _innerVariableProp.managedReferenceValue = privateVarA;
            _holderSO.ApplyModifiedProperties();
            _drawer.lastFlowchart = _firstFc;

            // Create second holder + drawer
            VarRefSO secondHolder;
            SerializedObject secondSO;
            SerializedProperty secondVarRefProp, secondInnerVarProp;
            {
                secondHolder = ScriptableObject.CreateInstance<VarRefSO>();
                secondSO = new SerializedObject(secondHolder);
                secondVarRefProp = secondSO.FindProperty("reference");
                secondInnerVarProp = secondVarRefProp.FindPropertyRelative("variable");
                Assert.NotNull(secondInnerVarProp);
                Assert.AreEqual(SerializedPropertyType.ManagedReference, secondInnerVarProp.propertyType);
                secondInnerVarProp.managedReferenceValue = publicVarB;
                secondSO.ApplyModifiedProperties();
            }

            VariableReferenceDrawer secondDrawer;
            VariableReferenceDrawerHostWindow secondHostWindow;
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

            // Draw initial frame for both
            yield return DrawOneFrame();
            secondHostWindow.Repaint();
            yield return null;

            // Ensure we read fresh values
            _holderSO.Update();
            secondSO.Update();

            // Pre-change sanity checks (semantic equality due to SerializeReference cloning)
            {
                var firstSelected = _varRefProp.FindPropertyRelative("variable").managedReferenceValue as IVariable;
                Assert.IsTrue(VarsSemanticallyEqual(privateVarA, firstSelected),
                    "Primary drawer variable mismatch before flowchart swap.");

                var secondSelected = secondVarRefProp.FindPropertyRelative("variable").managedReferenceValue as IVariable;
                Assert.IsTrue(VarsSemanticallyEqual(publicVarB, secondSelected),
                    "Secondary drawer variable mismatch before flowchart swap.");
            }

            // Change ONLY the first drawer's flowchart to the second FC (should clear private var A)
            _drawer.lastFlowchart = _secondFc;
            yield return DrawOneFrame();
            yield return null;

            // Refresh before reading the cleared value
            _holderSO.Update();
            yield return null;

            // First drawer's private variable should clear
            var firstAfterSwitch = _varRefProp.FindPropertyRelative("variable");
            Assert.IsNull(firstAfterSwitch.managedReferenceValue,
                "First drawer should have cleared private variable after switching Flowchart context.");

            // Second drawer must remain unaffected
            secondHostWindow.Repaint();
            yield return null;
            yield return null;

            secondSO.Update();
            yield return null;

            var secondStillSelected = secondVarRefProp.FindPropertyRelative("variable").managedReferenceValue as IVariable;
            Assert.IsTrue(VarsSemanticallyEqual(publicVarB, secondStillSelected),
                "Second drawer variable unexpectedly changed after first drawer context switch.");

            // Clean up additional objects
            secondHostWindow.Close();
            _toDestroy.Add(secondHolder);
        }

        [Test]
        public void ReportsExpectedPropertyHeight()
        {
            float expected = EditorGUIUtility.singleLineHeight * 2f;
            float heightWeGot = _drawer.GetPropertyHeight(_varRefProp, new GUIContent("Test"));
            Assert.That(heightWeGot, Is.EqualTo(expected).Within(0.01f),
                "Property height mismatch (should be 2 lines).");
        }

        // Helper: semantic equality for IVariable references (handles SerializeReference cloning)
        private static bool VarsSemanticallyEqual(IVariable a, IVariable b)
        {
            if (a == null || b == null) return false;

            try
            {
                if ((a.ItemId != 0 || b.ItemId != 0) && a.ItemId == b.ItemId) return true;
            }
            catch { /* ignore */ }

            try
            {
                bool bothHaveOwners = a.Owner != null && b.Owner != null;
                bool sameOwner = bothHaveOwners && ReferenceEquals(a.Owner, b.Owner);
                bool sameKey = !string.IsNullOrEmpty(a.Key) && a.Key == b.Key;
                if (sameOwner && sameKey) return true;
            }
            catch { /* ignore */ }

            return ReferenceEquals(a, b);
        }
    }
}
#endif