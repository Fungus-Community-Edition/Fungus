using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Amanita.VScripting;
using Amanita;

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
            public override void Init()
            {
                InitCalls++;
                base.Init();
            }
        }

        [UnityTest]
        public IEnumerator ClearVariables_EmptiesAllInternalLists()
        {
            AmanitaManager.EnsureExists();
            var go = new GameObject("Flowchart_ClearVariables");
            var fc = go.AddComponent<Flowchart>();
            go.SetActive(true);
            yield return null;

            // Use reflection to access protected lists
            IList legacyList = GetLegacyVariablesList(fc);
            IList muscariList = GetMuscariablesList(fc);
            Assert.NotNull(legacyList, "Could not access legacyVariables list via reflection.");
            Assert.NotNull(muscariList, "Could not access muscariables list via reflection.");

            // Populate muscariable list with a test muscariable
            var testMusca = new TestIntMuscariable { Value = 42, Key = "muscaA" };
            fc.IntegrateMuscariable(testMusca);

            // Attempt to create a legacy variable component (if any legacy type exists)
            MonoBehaviour legacyVar = TryCreateLegacyVariableComponent(go);
            if (legacyVar != null)
            {
                // Assign a key property (if present) to avoid null key collisions
                SetStringPropertyIfExists(legacyVar, "Key", "legacyA");
                legacyList.Add(legacyVar);
            }

            Assert.Greater(muscariList.Count, 0, "Precondition failed: muscariables list not populated.");
            if (legacyVar != null)
            {
                Assert.Greater(legacyList.Count, 0, "Precondition failed: legacyVariables list not populated.");
            }

            fc.ClearVariables();
            yield return null;

            Assert.AreEqual(0, muscariList.Count, "muscariables list should be empty after ClearVariables.");
            Assert.AreEqual(0, legacyList.Count, "legacyVariables list should be empty after ClearVariables.");
            Assert.AreEqual(0, fc.Variables.Count, "Flowchart.Variables should report empty after ClearVariables.");

            UnityEngine.Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator AddedVariables_GetUniqueNonClashingItemIds()
        {
            AmanitaManager.EnsureExists();
            var go = new GameObject("Flowchart_VarIds");
            var fc = go.AddComponent<Flowchart>();
            go.SetActive(true);
            yield return null;

            const int varCount = 6;
            var created = new List<Muscariable>();
            for (int i = 0; i < varCount; i++)
            {
                var v = fc.AddNewMuscariable<int, TestIntMuscariable>($"idVar_{i}", i);
                created.Add(v);
            }

            // Extract ItemIds
            var ids = created.Select(v => v.ItemId).ToList();
            Assert.AreEqual(varCount, ids.Distinct().Count(), "All ItemIds must be unique among newly added variables.");

            // Ensure no ID clashes with re-added variable
            var extra = fc.AddNewMuscariable<int, TestIntMuscariable>("idVar_extra", 999);
            Assert.False(ids.Contains(extra.ItemId), "New variable should not reuse an existing ItemId.");

            UnityEngine.Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Variables_Init_IsCalledAtLeastOnce()
        {
            AmanitaManager.EnsureExists();
            TestIntMuscariable.InitCalls = 0;

            var go = new GameObject("Flowchart_VarInit");
            var fc = go.AddComponent<Flowchart>();
            go.SetActive(true);
            yield return null;

            var v1 = fc.AddNewMuscariable<int, TestIntMuscariable>("initVar1", 10);
            var v2 = fc.AddNewMuscariable<int, TestIntMuscariable>("initVar2", 20);

            yield return null; // Allow any additional lifecycle init passes

            Assert.GreaterOrEqual(TestIntMuscariable.InitCalls, 2,
                "Each added muscariable should have had Init called at least once (total calls >= number created).");

            // Disable & re-enable to trigger potential re-init paths (if any)
            go.SetActive(false);
            yield return null;
            go.SetActive(true);
            yield return null;

            // If Flowchart re-initializes variables on re-enable, calls should increase
            Assert.GreaterOrEqual(TestIntMuscariable.InitCalls, 2,
                "Init call count should remain >= initial variable count after re-enable.");

            UnityEngine.Object.Destroy(go);
        }

        // ------------- Helper Reflection Methods -------------

        private static IList GetLegacyVariablesList(Flowchart fc)
        {
            return typeof(Flowchart)
                .GetField("legacyVariables", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(fc) as IList;
        }

        private static IList GetMuscariablesList(Flowchart fc)
        {
            return typeof(Flowchart)
                .GetField("muscariables", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(fc) as IList;
        }

        private static MonoBehaviour TryCreateLegacyVariableComponent(GameObject host)
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

            return host.AddComponent(variableType) as MonoBehaviour;
        }

        private static bool IsMuscariableType(Type t)
        {
            return t != null && (t == typeof(Muscariable) ||
                                 (t.IsSubclassOf(typeof(Muscariable)) ||
                                  (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Muscariable<>))));
        }

        private static void SetStringPropertyIfExists(object obj, string propName, string value)
        {
            if (obj == null) return;
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(string))
            {
                prop.SetValue(obj, value, null);
            }
            else
            {
                var field = obj.GetType().GetField(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null && field.FieldType == typeof(string))
                {
                    field.SetValue(obj, value);
                }
            }
        }
    }
}