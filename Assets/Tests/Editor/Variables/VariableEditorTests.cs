using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using UnityObject = UnityEngine.Object;
using Amanita;

namespace VScriptingTests.VariableOperations
{
    public class VariableEditorTests
    {
        [SetUp]
        public void SetUp()
        {
            Flowchart.ResetStaticsForTest();
            ammieManager = AmanitaManager.EnsureExists();

            _firstFcHolder = new GameObject("Flowchart_A");
            _secondFcHolder = new GameObject("Flowchart_B");
            _firstFc = _firstFcHolder.AddComponent<Flowchart>();
            _secondFc = _secondFcHolder.AddComponent<Flowchart>();

            _toDestroy = new List<UnityObject>();
            _toDestroy.Add(_firstFcHolder);
            _toDestroy.Add(_secondFcHolder);
            _toDestroy.Add(ammieManager.gameObject);
        }

        protected AmanitaManager ammieManager;
        protected GameObject _firstFcHolder;
        protected GameObject _secondFcHolder;
        protected Flowchart _firstFc;
        protected Flowchart _secondFc;
        protected List<UnityObject> _toDestroy;

        [TearDown]
        public void TearDown()
        {
            foreach (var needsToGo in _toDestroy.Where(elem => elem != null))
            {
                UnityObject.DestroyImmediate(needsToGo);
            }

            _toDestroy.Clear();
            Flowchart.ResetStaticsForTest();
        }

        [Test]
        public void GetVariableInfo_ReturnsAttribute()
        {
            // IntegerVariable should have a VariableInfoAttribute (from Fungus)
            var attr = VariableEditor.GetVariableInfo(typeof(IntMuscariable));
            Assert.NotNull(attr, "VariableInfoAttribute not found for IntMuscariable.");
            Assert.IsFalse(string.IsNullOrEmpty(attr.OptionDisplayName), "VariableType string should not be empty.");
        }

        [Test]
        public void VariableField_SelectsLocalVariable()
        {
            var boolVar = _firstFc.AddNewMuscariable<bool, BoolMuscariable>("BoolVar", default, VariableScope.Private);
            var floatVar = _firstFc.AddNewMuscariable<float, FloatMuscariable>("FloatVar", default, VariableScope.Private);

            var (serialObj, holdsVar, _) = MakeHolder();

            // Force selection of index 2 (index 0 = default, index 1 = BoolVar, index 2 = FloatVar)
            var (selected, options) = InvokeVariableFieldWithCapture(_firstFc, holdsVar, forcedIndex: 2);

            Assert.AreEqual(floatVar, selected, "Expected FloatVar to be selected.");
            CollectionAssert.Contains(options, "BoolVar");
            CollectionAssert.Contains(options, "FloatVar");
        }

        protected (SerializedObject serialObj, SerializedProperty prop, VariableRefHolder holder) MakeHolder()
        {
            var holder = ScriptableObject.CreateInstance<VariableRefHolder>();
            _toDestroy.Add(holder);
            var serialObj = new SerializedObject(holder);
            var prop = serialObj.FindProperty("varField");
            Assert.NotNull(prop, "Failed to find varField property on holder.");
            return (serialObj, prop, holder);
        }

        /// <summary>
        /// Invokes VariableField capturing the produced options (via drawer delegate) and forcing selection index.
        /// Returns (selectedVariable, optionsArrayPassedToDrawer).
        /// </summary>
        protected (IVariable selected, string[] options) InvokeVariableFieldWithCapture(
            Flowchart owningFlowchart,
            SerializedProperty prop,
            int forcedIndex,
            Func<IVariable, bool> filter = null,
            string defaultText = "<None>")
        {
            string[] capturedOptions = null;
            int drawer(string label, int selectedIndex, string[] options)
            {
                capturedOptions = options;
                return forcedIndex;
            }

            VariableEditor.VariableField(prop,
                                         new GUIContent("Test Var"),
                                         owningFlowchart,
                                         defaultText,
                                         filter,
                                         drawer);

            prop.serializedObject.ApplyModifiedProperties();

            IVariable selected =
                prop.propertyType == SerializedPropertyType.ManagedReference
                    ? prop.managedReferenceValue as IVariable
                    : prop.objectReferenceValue as IVariable;

            return (selected, capturedOptions);
        }

        [Test]
        public void VariableField_Filter_OnlyBooleanVars()
        {
            var boolVar = _firstFc.AddNewMuscariable<bool, BoolMuscariable>("BoolVar", default, VariableScope.Private);
            var floatVar = _firstFc.AddNewMuscariable<float, FloatMuscariable>("FloatVar", default, VariableScope.Private);

            var (serialObj, holdsVar, _) = MakeHolder();

            Type boolType = typeof(bool);
            bool AcceptOnlyBools(IVariable varToCheck) => varToCheck.ContentType.Equals(boolType);
            var (selected, options) = InvokeVariableFieldWithCapture(_firstFc, holdsVar, forcedIndex: 1,
                filter: AcceptOnlyBools);

            Assert.AreEqual(boolVar, selected, "Expected BoolVar to be chosen after filtering.");
            Assert.AreEqual(2, options.Length, "Expected only default + BoolVar options.");
            Assert.IsTrue(options[1] == "BoolVar");
        }

        [Test]
        public void VariableField_IncludesPublicFromOtherFlowchart_ExcludesPrivate()
        {
            // Local
            var localVar = _firstFc.AddNewMuscariable<bool, BoolMuscariable>("LocalBool", default, VariableScope.Private);

            // Remote public + private
            var publicVar = _secondFc.AddNewMuscariable<bool, BoolMuscariable>("RemotePublic", default, VariableScope.Public);
            _secondFc.AddNewMuscariable<int, IntMuscariable>("RemotePrivate", default, VariableScope.Private);

            var (serialObj, holdsProp, _) = MakeHolder();

            // Force index 2: options expected order: 0 default null, 1 LocalBool, 2 Flowchart_B/RemotePublic
            // Note that after the default null is local vars followed by other flowchart vars, 
            // and then finally whatever globals AmanitaManager might have.
            var (selected, options) = InvokeVariableFieldWithCapture(_firstFc, holdsProp, forcedIndex: 2);

            string expectedRemoteLabel = $"{_secondFc.name}/RemotePublic";
            CollectionAssert.Contains(options, expectedRemoteLabel, "Public remote variable not included.");
            Assert.IsFalse(options.Any(optionEl => optionEl.Contains("RemotePrivate")),
                "Private remote variable should not appear.");
            Assert.AreEqual(publicVar, selected, $"Expected remote public variable {publicVar.Key} to be " +
                $"selected. Instead selected {selected.Key}");
        }

        [Test]
        public void VariableField_DefaultOptionPresentAsFirstEntry()
        {
            var boolVar = _firstFc.AddNewMuscariable<bool, BoolMuscariable>("BoolVar", default, VariableScope.Private);
            var (serialObj, propWithVar, _) = MakeHolder();

            string defaultOption = "<Select>";
            var (selected, options) = InvokeVariableFieldWithCapture(_firstFc, propWithVar,
                forcedIndex: 0, defaultText: defaultOption);
            Assert.AreEqual(defaultOption, options[0], "Custom default text not in first slot.");
            Assert.IsNull(selected, "Selecting default entry should yield null reference.");
        }
    }
}