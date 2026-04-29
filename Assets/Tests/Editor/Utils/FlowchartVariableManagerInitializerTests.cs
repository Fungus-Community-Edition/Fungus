using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using AtMycelia.Hyphlow.EditorUtils;
using AtMycelia.Hyphlow;
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
        public void InitializeFlowcharts_DoesNotRemoveVariablesFromManager()
        {
            var varManager = _flowchart.GetComponent<VariableManagerComponent>();
            Assert.IsNotNull(varManager, "VariableManagerComponent not found on Flowchart.");

            var managedVar = varManager.AddNewVariableOfContentType<string>("ManagedVar", "Value");
            Assert.IsNotNull(managedVar, "Failed to add variable to VariableManagerComponent.");

            int beforeCount = varManager.Variables.Count;

            InvokeInitializeFlowcharts();

            Assert.AreEqual(beforeCount, varManager.Variables.Count, 
                "VariableManagerComponent variables should not be removed.");
            Assert.That(varManager.Variables.Any(variable => ReferenceEquals(variable, managedVar)), Is.True,
                "Managed variable should still be present after initialization.");
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