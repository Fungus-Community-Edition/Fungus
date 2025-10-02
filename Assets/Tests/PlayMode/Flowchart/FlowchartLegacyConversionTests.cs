using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;
using Amanita.VScripting;

namespace VScriptingTests
{
    public class FlowchartLegacyConversionTests
    {
        public class FLCTestCase
        {
            public Type contentType;
            public Type legacyVarType;
            public Type muscVarType;
            public object sampleValue;
            public FLCTestCase(Type contentType, Type legacyVarType, Type muscVarType, object sampleValue)
            {
                this.contentType = contentType;
                this.legacyVarType = legacyVarType;
                this.muscVarType = muscVarType;
                this.sampleValue = sampleValue;
            }
            public override string ToString()
            {
                return $"{legacyVarType.Name} -> {muscVarType.Name} (ContentType: " +
                    $"{contentType.Name}, SampleValue: {sampleValue})";
            }
        }

        [SetUp]
        public void Setup()
        {
            testGo = new GameObject("FlowchartTest_GO");
            testFc = testGo.AddComponent<Flowchart>();
            toDestroyInTearDown.Add(testGo);
        }

        protected GameObject testGo;
        protected Flowchart testFc;
        protected readonly IList<UnityObj> toDestroyInTearDown = new List<UnityObj>();

        [TearDown]
        public void TearDown()
        {
            foreach (var elem in toDestroyInTearDown)
            {
                if (elem == null) continue;
                UnityObj.DestroyImmediate(elem);
            }
            toDestroyInTearDown.Clear();
        }

        // Test cases: (contentType, legacyVariableType, muscariableType, sampleValue)
        static IEnumerable<FLCTestCase> LegacyVarCases()
        {
            yield return new FLCTestCase(typeof(float), typeof(FloatVariable), typeof(FloatMuscariable), 11.11f);
            yield return new FLCTestCase(typeof(bool), typeof(BooleanVariable), typeof(BoolMuscariable),  true);
            // Extend with more cases as new legacy types are available.
        }

        protected readonly Type fcType = typeof(Flowchart);
        protected readonly BindingFlags fcBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic 
            | BindingFlags.Public;

        // Reflection helper to call the protected conversion method.
        void Invoke_GetAndInitVars(Flowchart fc)
        {
            var mi = fcType.GetMethod("GetAndInitVars", fcBindingFlags);
            if (mi == null) Assert.Fail("Failed to find Flowchart.GetAndInitVars via reflection");
            try
            {
                mi.Invoke(fc, null);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException ?? tie;
            }
        }

        // Reflection helper to call Flowchart.AddNewVariable<TValHeld, TVarType>(string key, TValHeld value, VariableScope scope)
        IVariable CreateLegacyVar(Flowchart fc, Type contentType, Type legacyVarType,
            string key, object sampleValue, VariableScope scope)
        {
            // Find generic AddNewVariable method with 2 generic args and 3 parameters
            var methods = typeof(Flowchart).GetMethods(fcBindingFlags);
            MethodInfo addNewVarMI = methods
                .Where(elem => elem.Name == "AddNewVariable" && elem.IsGenericMethodDefinition && 
                elem.GetGenericArguments().Length == 2)
                .FirstOrDefault();
            if (addNewVarMI == null) Assert.Fail("AddNewVariable<TValHeld, TVarType> not found on Flowchart");

            var generic = addNewVarMI.MakeGenericMethod(contentType, legacyVarType);
            object[] args = new object[] { key, sampleValue, scope };
            var res = generic.Invoke(fc, args);
            // Return as IVariable for general assertions
            return res as IVariable;
        }

        // Reflection helper to call Flowchart.GetMuscariableWithKey<TVarType>(string key)
        object GetConvertedMuscariable(Flowchart fc, Type muscVarType, string key)
        {
            var mi = fcType.GetMethod("GetMuscariableWithKey", fcBindingFlags);
            if (mi == null) Assert.Fail("GetMuscariableWithKey<TVarType> not found on Flowchart");
            var generic = mi.MakeGenericMethod(muscVarType);
            return generic.Invoke(fc, new object[] { key });
        }

        [UnityTest]
        public IEnumerator Conversion_Completes_WithoutExceptions([ValueSource(nameof(LegacyVarCases))]
        FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVar(testFc, testCase.contentType, testCase.legacyVarType,
                key, testCase.sampleValue, VariableScope.Private);

            Assert.IsNotNull(legacy, "Legacy variable creation failed");
            Assert.AreEqual(testCase.contentType, legacy.ContentType, "Legacy ContentType mismatch");

            // Conversion should not throw
            Invoke_GetAndInitVars(testFc);

            // After conversion: legacy list should no longer expose the converted var (VariableCount counts legacy list)
            Assert.AreEqual(1, testFc.MuscariableCount, "Expected a muscariable to be present after conversion");
            Assert.AreEqual(0, testFc.VariableCount, "Expected legacy variable list to be empty after conversion");

            yield break;
        }

        protected virtual string GenerateRandomKey(string prefix)
        {
            return prefix + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        [UnityTest]
        public IEnumerator ConvertedMuscariable_Preserves_Key([ValueSource(nameof(LegacyVarCases))]
        FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVar(testFc, testCase.contentType, testCase.legacyVarType,
                key, testCase.sampleValue, VariableScope.Private);

            string originalKey = legacy.Key;
            Invoke_GetAndInitVars(testFc);

            var converted = GetConvertedMuscariable(testFc, testCase.muscVarType, originalKey) as IVariable;
            Assert.IsNotNull(converted, "Converted muscariable with original key not found");
            Assert.AreEqual(originalKey, converted.Key, "Converted muscariable key does not match original");

            yield break;
        }

        [UnityTest]
        public IEnumerator ConvertedMuscariable_Preserves_Value([ValueSource(nameof(LegacyVarCases))]
        FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVar(testFc, testCase.contentType, testCase.legacyVarType, key,
                testCase.sampleValue, VariableScope.Private);

            Invoke_GetAndInitVars(testFc);

            var converted = GetConvertedMuscariable(testFc, testCase.muscVarType, legacy.Key) as IVariable;
            Assert.IsNotNull(converted, "Converted muscariable not found");

            // Compare boxed values; for floats/bools direct equality is fine
            Assert.AreEqual(legacy.Value, converted.Value, "Converted muscariable value does not match legacy value");

            yield break;
        }

        [UnityTest]
        public IEnumerator ConvertedMuscariable_Preserves_ContentType([ValueSource(nameof(LegacyVarCases))]
        FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVar(testFc, testCase.contentType, testCase.legacyVarType, key,
                testCase.sampleValue, VariableScope.Private);
            var legacyContentType = legacy.ContentType;

            Invoke_GetAndInitVars(testFc);

            var converted = GetConvertedMuscariable(testFc, testCase.muscVarType, legacy.Key) as IVariable;
            Assert.IsNotNull(converted, "Converted muscariable not found");
            Assert.AreEqual(legacyContentType, converted.ContentType, "Converted muscariable ContentType " +
                "does not match legacy variable");

            yield break;
        }

        [UnityTest]
        public IEnumerator ConvertedMuscariable_Preserves_Owner([ValueSource(nameof(LegacyVarCases))]
        FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVar(testFc, testCase.contentType, testCase.legacyVarType, key,
                testCase.sampleValue, VariableScope.Private);

            // Sanity: legacy should be associated with the flowchart (owner may be exposed via Owner)
            Assert.AreEqual(testFc, legacy.Owner ?? testFc, "Legacy variable owner unexpected");

            Invoke_GetAndInitVars(testFc);

            var converted = GetConvertedMuscariable(testFc, testCase.muscVarType, legacy.Key) as Muscariable;
            Assert.IsNotNull(converted, "Converted muscariable not found");
            Assert.AreEqual(testFc, converted.ParentFlowchart ?? converted.Owner,
                "Converted muscariable owner/parent flowchart does not match original");

            yield break;
        }

        [UnityTest]
        public IEnumerator Conversion_Preserves_Scope([ValueSource(nameof(LegacyVarCases))]
        FLCTestCase testCase)
        {
            string privKey = GenerateRandomKey("scopePriv_");
            var legacyPrivate = CreateLegacyVar(testFc, testCase.contentType, testCase.legacyVarType,
                privKey, testCase.sampleValue, VariableScope.Private);

            string pubKey = GenerateRandomKey("scopePub_");
            var legacyPublic = CreateLegacyVar(testFc, testCase.contentType, testCase.legacyVarType,
                pubKey, testCase.sampleValue, VariableScope.Public);

            var expectedPrivate = legacyPrivate.Scope;
            var expectedPublic = legacyPublic.Scope;

            Invoke_GetAndInitVars(testFc);

            var convPrivate = GetConvertedMuscariable(testFc, testCase.muscVarType, privKey) as Muscariable;
            var convPublic = GetConvertedMuscariable(testFc, testCase.muscVarType, pubKey) as Muscariable;

            Assert.IsNotNull(convPrivate, "Converted private muscariable not found");
            Assert.IsNotNull(convPublic, "Converted public muscariable not found");

            Assert.AreEqual(expectedPrivate, convPrivate.Scope, "Private scope was not preserved after conversion");
            Assert.AreEqual(expectedPublic, convPublic.Scope, "Public scope was not preserved after conversion");

            yield break;
        }
    }
}