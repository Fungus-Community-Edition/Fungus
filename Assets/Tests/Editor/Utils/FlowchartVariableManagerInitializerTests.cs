using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AtMycelia.Amanita.EditorUtils;
using AtMycelia.Amanita.VScripting;
using UnityObject = UnityEngine.Object;

namespace VScriptingTests.Utils
{
    public sealed class FlowchartVariableManagerInitializerTests
    {
        private sealed class TestFlowchart : Flowchart
        {
            public void SetLegacyVariables(List<Variable> variables)
            {
                _legacyVariables = variables;
            }

            public void SetOldMuscariables(List<Muscariable> muscariables)
            {
                _oldMuscariables = muscariables;
            }

            public IReadOnlyList<Variable> LegacyVariables => _legacyVariables;

            public IReadOnlyList<Muscariable> OldMuscariables => _oldMuscariables;
        }

        private readonly List<UnityObject> _objectsToDestroy = new List<UnityObject>();
        private TestFlowchart _flowchart;

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var flowchartObject = new GameObject("TestFlowchart");
            _objectsToDestroy.Add(flowchartObject);

            _flowchart = flowchartObject.AddComponent<TestFlowchart>();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = _objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (_objectsToDestroy[i] != null)
                {
                    UnityObject.DestroyImmediate(_objectsToDestroy[i]);
                }
            }

            _objectsToDestroy.Clear();
        }

        [Test]
        public void InitializeFlowcharts_MigratesLegacyAndOldMuscariablesAndClearsLists()
        {
            Muscariable oldMuscariable = VariableFactory.CreateByContentType(typeof(string));
            oldMuscariable.Key = "OldMuscariable";
            oldMuscariable.BoxedValue = "LegacyValue";

            StringVariable legacyVariable = _flowchart.gameObject.AddComponent<StringVariable>();
            legacyVariable.Key = "LegacyVariable";
            legacyVariable.BoxedValue = "LegacyVarValue";

            _flowchart.SetOldMuscariables(new List<Muscariable> { oldMuscariable });
            _flowchart.SetLegacyVariables(new List<Variable> { legacyVariable });

            InvokeInitializeFlowcharts();

            Assert.That(_flowchart.OldMuscariables.Count, Is.EqualTo(0), "Old muscariable list was not cleared.");
            Assert.That(_flowchart.LegacyVariables.Count, Is.EqualTo(0), "Legacy variable list was not cleared.");
            Assert.That(_flowchart.Variables.Any(variable => ReferenceEquals(variable, oldMuscariable)), Is.True,
                "Old muscariable was not migrated into the variable manager.");
            Assert.That(_flowchart.Variables.Any(variable => ReferenceEquals(variable, legacyVariable)), Is.True,
                "Legacy variable was not migrated into the variable manager.");
        }

        private static void InvokeInitializeFlowcharts()
        {
            MethodInfo method = typeof(FlowchartVariableManagerInitializer).GetMethod(
                "InitializeFlowcharts",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(method, "Could not find InitializeFlowcharts via reflection.");
            method.Invoke(null, null);
        }
    }
}