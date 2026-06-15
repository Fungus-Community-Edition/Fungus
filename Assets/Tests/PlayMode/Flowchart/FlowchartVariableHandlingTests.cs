using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AtMycelia.Hyphlow;
using AtMycelia.Amanita;
using UnityObj = UnityEngine.Object;

namespace VScriptingTests.FlowchartLifecycle
{
    /// <summary>
    /// PlayMode tests focusing on Flowchart variable handling.
    /// Covers:
    /// - ClearVariables empties legacy & muscariable internal lists
    /// - ItemId uniqueness / non-clashing for newly added variables
    /// - Variable.Init gets called at least once during lifecycle
    /// </summary>
    public class FlowchartVariableHandlingTests
    {
        // Test muscariable subclass to observe Init() calls
        private class TestIntMuscariable : Muscariable<int>
        {
            public static int InitCalls;

            public override void Init(object startValue = default)
            {
                InitCalls++;
                base.Init(startValue);
            }

            public override void Init(int startVal)
            {
                InitCalls++;
                base.Init(startVal);
            }
        }

        [SetUp]
        public virtual void DoSetUp()
        {
            fChartHolder = new GameObject("Flowchart_VariableHandlingTestHolder");
            fChart = fChartHolder.AddComponent<Flowchart>();
            fChart.AlwaysKeepGuid = false;
            toDestroyInTearDown.Add(fChartHolder);
            toDestroyInTearDown.Add(AmanitaManager.S.gameObject);
        }

        private GameObject fChartHolder;
        private Flowchart fChart;
        private readonly IList<UnityObj> toDestroyInTearDown = new List<UnityObj>();

        [TearDown]
        public virtual void DoTearDown()
        {
            fChart.OnTearDown();
            AmanitaManager.S.OnTearDown();

            foreach (var obj in toDestroyInTearDown)
            {
                if (obj != null)
                {
                    UnityObj.Destroy(obj);
                }
            }
            toDestroyInTearDown.Clear();
            fChartHolder = null;
            fChart = null;
        }

        [UnityTest]
        public IEnumerator ClearVariables_EmptiesAllInternalLists()
        {
            fChartHolder.SetActive(true);
            yield return null;

            var varManagerComponent = fChart.GetComponent<VariableManagerComponent>();
            Assert.IsNotNull(varManagerComponent, "VariableManagerComponent not found on Flowchart.");

            VariableManager varManager = GetVariableManager(varManagerComponent);
            Assert.IsNotNull(varManager, "Could not access VariableManager via reflection.");

            // Use reflection to access VariableManager's internal lists
            IList legacyList = GetLegacyVariablesList(varManager);
            IList muscariList = GetMuscariablesList(varManager);
            Assert.NotNull(legacyList, "Could not access VariableManager legacy list via reflection.");
            Assert.NotNull(muscariList, "Could not access VariableManager muscariables list via reflection.");

            // Populate muscariable list with a test muscariable
            var testMusca = varManagerComponent.AddVariable(new TestIntMuscariable
            {
                Value = 42,
                Key = "muscaA",
            });

            Assert.IsNotNull(testMusca, "Failed to add test muscariable.");

            // Attempt to create a legacy variable component (if any legacy type exists)
            Variable legacyVar = TryCreateLegacyVariableComponent(fChartHolder);
            if (legacyVar != null)
            {
                // Assign a key property (if present) to avoid null key collisions
                SetStringPropertyIfExists(legacyVar, "Key", "legacyA");
                varManagerComponent.AddVariable(legacyVar);
            }

            Assert.Greater(muscariList.Count, 0, "Precondition failed: muscariables list not populated.");
            if (legacyVar != null)
            {
                Assert.Greater(legacyList.Count, 0, "Precondition failed: legacyVariables list not populated.");
            }

            fChart.ClearVariables();
            yield return null;

            Assert.AreEqual(0, muscariList.Count, "muscariables list should be empty after ClearVariables.");
            Assert.AreEqual(0, legacyList.Count, "legacyVariables list should be empty after ClearVariables.");
            Assert.AreEqual(0, varManagerComponent.Variables.Count, "VariableManagerComponent.Variables should report empty after ClearVariables.");

        }

        [UnityTest]
        public IEnumerator AddedVariables_GetUniqueNonClashingItemIds()
        {
            yield return null;

            var varManager = fChart.GetComponent<VariableManagerComponent>();
            Assert.IsNotNull(varManager, "VariableManagerComponent not found on Flowchart.");

            const int varCount = 6;
            var created = new List<Muscariable>();
            for (int i = 0; i < varCount; i++)
            {
                var varElem = varManager.AddVariable(new TestIntMuscariable
                {
                    Key = $"idVar_{i}",
                    Value = i,
                });

                Assert.IsNotNull(varElem, $"Failed to add muscariable at index {i}.");
                created.Add(varElem);
            }

            // Extract ItemIds
            var ids = created.Select(varElem => varElem.ItemId).ToList();
            Assert.AreEqual(varCount, ids.Distinct().Count(), "All ItemIds must be unique among newly added variables.");

            // Ensure no ID clashes with re-added variable
            var extra = varManager.AddVariable(new TestIntMuscariable
            {
                Key = "idVar_extra",
                Value = 999,
            });

            Assert.IsNotNull(extra, "Failed to add extra muscariable.");
            Assert.False(ids.Contains(extra.ItemId), "New variable should not reuse an existing ItemId.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Variables_Init_IsCalledAtLeastOnce()
        {
            TestIntMuscariable.InitCalls = 0;

            yield return null;

            var varManager = fChart.GetComponent<VariableManagerComponent>();
            Assert.IsNotNull(varManager, "VariableManagerComponent not found on Flowchart.");

            var firstVar = varManager.AddVariable(new TestIntMuscariable
            {
                Key = "initVar1",
                Value = 10,
            });

            var secondVar = varManager.AddVariable(new TestIntMuscariable
            {
                Key = "initVar2",
                Value = 20,
            });

            Assert.IsNotNull(firstVar, "Failed to add initVar1 muscariable.");
            Assert.IsNotNull(secondVar, "Failed to add initVar2 muscariable.");

            // Trigger VariableManager OnEnable to run Init on registered variables
            fChartHolder.SetActive(false);
            yield return null;
            fChartHolder.SetActive(true);
            yield return null;

            Assert.GreaterOrEqual(TestIntMuscariable.InitCalls, 2,
                "Each added muscariable should have had Init called at least once (total calls >= number created).");

            // Disable & re-enable to trigger potential re-init paths (if any)
            fChartHolder.SetActive(false);
            yield return null;
            fChartHolder.SetActive(true);
            yield return null;

            // If Flowchart re-initializes variables on re-enable, calls should increase
            Assert.GreaterOrEqual(TestIntMuscariable.InitCalls, 2,
                "Init call count should remain >= initial variable count after re-enable.");

        }

        // ------------- Helper Reflection Methods -------------

        private static VariableManager GetVariableManager(VariableManagerComponent component)
        {
            return varManagerComponentType.GetField("_variableManager", bindingFlags)?.GetValue(component) as VariableManager;
        }

        private static IList GetLegacyVariablesList(VariableManager varManager)
        {
            return varManagerType.GetField("_legacyVariables", bindingFlags)?.GetValue(varManager) as IList;
        }

        private static IList GetMuscariablesList(VariableManager varManager)
        {
            return varManagerType.GetField("_muscariables", bindingFlags)?.GetValue(varManager) as IList;
        }

        private static readonly Type varManagerComponentType = typeof(VariableManagerComponent);
        private static readonly Type varManagerType = typeof(VariableManager);

        private static void SetStringPropertyIfExists(object obj, string propName, string value)
        {
            if (obj == null) return;
            Type objType = obj.GetType();
            var prop = objType.GetProperty(propName, bindingFlags);
            if (prop != null && prop.CanWrite && prop.PropertyType == stringType)
            {
                prop.SetValue(obj, value, null);
            }
            else
            {
                var field = objType.GetField(propName, bindingFlags);
                if (field != null && field.FieldType == stringType)
                {
                    field.SetValue(obj, value);
                }
            }
        }

        private static readonly BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Type stringType = typeof(string);

        private static Variable TryCreateLegacyVariableComponent(GameObject host)
        {
            // Find any type that looks like a legacy variable (implements IVariable, derives MonoBehaviour, not Muscariable)
            var variableType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    Type[] types;
                    try { types = a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
                    return types;
                })
                .FirstOrDefault(t =>
                    t != null &&
                    typeof(MonoBehaviour).IsAssignableFrom(t) &&
                    typeof(IVariable).IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    !IsMuscariableType(t));

            if (variableType == null)
            {
                // No legacy variable type available; test will proceed without legacy coverage
                return null;
            }

            return host.AddComponent(variableType) as Variable;
        }

        private static bool IsMuscariableType(Type typeToCheck)
        {
            return typeToCheck != null && muscariableType.IsAssignableFrom(typeToCheck);
        }

        private static readonly Type muscariableType = typeof(Muscariable);

    }
}