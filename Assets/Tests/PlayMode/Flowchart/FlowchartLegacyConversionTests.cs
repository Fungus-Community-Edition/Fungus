using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;
using Amanita.VScripting;

namespace VScriptingTests
{
    public class FlowchartLegacyConversionTests
    {
        [SetUp]
        public void Setup()
        {
            // Any setup code if needed
            testGo = new GameObject("FlowchartTest_GO");
            testFc = testGo.AddComponent<Flowchart>();
            toDestroyInTearDown.Add(testGo); // No need to add the FC here; destroying the GO will also destroy the FC
        }

        protected GameObject testGo;
        protected Flowchart testFc;
        protected readonly IList<UnityObj> toDestroyInTearDown = new List<UnityObj>();

        [TearDown]
        public void TearDown()
        {
            foreach (var elem in toDestroyInTearDown)
            {
                if (elem == null)
                {
                    continue;
                }

                UnityObj.DestroyImmediate(elem);
            }

            toDestroyInTearDown.Clear();
        }

        [UnityTest]
        public IEnumerator Conversion_Completes_WithoutExceptions()
        {
            // Add a legacy float variable via the Flowchart helper so it is registered in legacyVariables
            var legacy = testFc.AddNewVariable<float, FloatVariable>("testFloat", 1.23f, VariableScope.Private);

            // Ensure preconditions
            Assert.IsNotNull(legacy);
            Assert.AreEqual(typeof(float), legacy.ContentType);

            // Invoke conversion (protected method)
            // Should not throw; any thrown exception will fail the test
            Invoke_GetAndInitVars(testFc);

            // After conversion the flowchart should have one muscariable and zero legacy variables
            Assert.AreEqual(1, testFc.MuscariableCount, "Expected exactly one muscariable after conversion");
            Assert.AreEqual(0, testFc.VariableCount, "Expected legacy variable list to be empty after conversion");

            yield break;
        }

        // Helper: invokes the protected GetAndInitVars() method on Flowchart via reflection.
        // The reason we need this is that we want to make sure that the test vars we add
        // are converted to muscaris, and that only happens when this method is called.
        void Invoke_GetAndInitVars(Flowchart fc)
        {
            var mi = fcType.GetMethod("GetAndInitVars", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null) Assert.Fail("Failed to find Flowchart.GetAndInitVars via reflection");
            try
            {
                mi.Invoke(fc, null);
            }
            catch (TargetInvocationException tie)
            {
                // Unwrap so NUnit shows the real assertion/exception
                throw tie.InnerException ?? tie;
            }
        }

        protected readonly Type fcType = typeof(Flowchart);

        [UnityTest]
        public IEnumerator ConvertedMuscariable_Preserves_Key()
        {
            var legacy = testFc.AddNewVariable<float, FloatVariable>("myUniqueKey", 7.5f, VariableScope.Private);
            string originalKey = legacy.Key;

            Invoke_GetAndInitVars(testFc);

            var converted = testFc.GetMuscariableWithKey<FloatMuscariable>(originalKey);
            Assert.IsNotNull(converted, "Converted muscariable with original key not found");
            Assert.AreEqual(originalKey, converted.Key, "Converted muscariable key does not match original");

            yield break;
        }

        [UnityTest]
        public IEnumerator ConvertedMuscariable_Preserves_Value()
        {
            float startVal = 42.42f;
            var legacy = testFc.AddNewVariable<float, FloatVariable>("valKey", startVal, VariableScope.Private);

            Invoke_GetAndInitVars(testFc);

            var converted = testFc.GetMuscariableWithKey<FloatMuscariable>(legacy.Key);
            Assert.IsNotNull(converted, "Converted muscariable not found");
            // Float comparison exact because we set the same stored value
            Assert.AreEqual(startVal, converted.Value, "Converted muscariable value does not match legacy value");

            yield break;
        }

        [UnityTest]
        public IEnumerator ConvertedMuscariable_Preserves_ContentType()
        {
            var legacy = testFc.AddNewVariable<float, FloatVariable>("ctKey", 3.14f, VariableScope.Private);
            var legacyContentType = legacy.ContentType;

            Invoke_GetAndInitVars(testFc);

            var converted = testFc.GetMuscariableWithKey<FloatMuscariable>(legacy.Key);
            Assert.IsNotNull(converted, "Converted muscariable not found");
            Assert.AreEqual(legacyContentType, converted.ContentType, "Converted muscariable ContentType does not match legacy variable");

            yield break;
        }

        [UnityTest]
        public IEnumerator ConvertedMuscariable_Preserves_Owner()
        {
            var legacy = testFc.AddNewVariable<float, FloatVariable>("ownerKey", 9.9f, VariableScope.Private);

            // The legacy variable was created/registered through the Flowchart, so its owner should be the flowchart.
            // Some legacy Variable implementations expose Owner or GetFlowchart; we assert that the original was associated with fc.
            Assert.AreEqual(testFc, legacy.Owner ?? legacy.GetComponent<Flowchart>() ?? testFc, "Legacy variable not owned by flowchart as expected");

            Invoke_GetAndInitVars(testFc);

            var converted = testFc.GetMuscariableWithKey<FloatMuscariable>(legacy.Key);
            Assert.IsNotNull(converted, "Converted muscariable not found");

            // Converted muscariable sets ParentFlowchart to this flowchart in the conversion code.
            // Check that the converted muscariable is also associated with the same flowchart instance.
            // There is a ParentFlowchart property on Muscariable; compare to fc.
            Assert.AreEqual(testFc, converted.ParentFlowchart, "Converted muscariable ParentFlowchart does not match original owner flowchart");

            yield break;
        }

        // --- Additional coverage requested: multiple legacy types and public/private scope preservation ---

        [UnityTest]
        public IEnumerator Conversion_Preserves_Multiple_Types_And_Values()
        {
            // Add a legacy float and a legacy boolean variable
            var legacyFloat = testFc.AddNewVariable<float, FloatVariable>("multiFloat", 11.11f, VariableScope.Private);
            var legacyBool = testFc.AddNewVariable<bool, BooleanVariable>("multiBool", true, VariableScope.Private);
            string legacyFloatKey = legacyFloat.Key;
            string legacyBoolKey = legacyBool.Key;

            PreConversionSanityChecks();
            void PreConversionSanityChecks()
            {                 
                Assert.IsNotNull(legacyFloat, "Legacy float variable not created");
                Assert.IsNotNull(legacyBool, "Legacy bool variable not created");
                Assert.AreEqual(typeof(float), legacyFloat.ContentType, "Legacy float ContentType incorrect");
                Assert.AreEqual(typeof(bool), legacyBool.ContentType, "Legacy bool ContentType incorrect");
            }

            Invoke_GetAndInitVars(testFc);

            var convFloat = testFc.GetMuscariableWithKey<FloatMuscariable>(legacyFloatKey);
            var convBool = testFc.GetMuscariableWithKey<BoolMuscariable>(legacyBoolKey);

            Assert.IsNotNull(convFloat, "Converted float muscariable not found");
            Assert.IsNotNull(convBool, "Converted bool muscariable not found");

            Assert.AreEqual(legacyFloat.Value, convFloat.Value, "Float value was not preserved during conversion");
            Assert.AreEqual(legacyBool.Value, convBool.Value, "Bool value was not preserved during conversion");

            Assert.AreEqual(legacyFloat.ContentType, convFloat.ContentType, "Float content type mismatch after conversion");
            Assert.AreEqual(legacyBool.ContentType, convBool.ContentType, "Bool content type mismatch after conversion");

            Assert.AreEqual(testFc, convFloat.ParentFlowchart, "Converted float owner incorrect");
            Assert.AreEqual(testFc, convBool.ParentFlowchart, "Converted bool owner incorrect");

            yield break;
        }

        [UnityTest]
        public IEnumerator Conversion_Preserves_Public_And_Private_Scopes()
        {
            // Add two legacy floats with different scopes
            var legacyPrivate = testFc.AddNewVariable<float, FloatVariable>("scopePrivate", 2.0f, VariableScope.Private);
            var legacyPublic = testFc.AddNewVariable<float, FloatVariable>("scopePublic", 3.0f, VariableScope.Public);

            // Remember scopes before conversion
            var expectedPrivateScope = legacyPrivate.Scope;
            var expectedPublicScope = legacyPublic.Scope;

            Invoke_GetAndInitVars(testFc);

            var convPrivate = testFc.GetMuscariableWithKey<FloatMuscariable>(legacyPrivate.Key);
            var convPublic = testFc.GetMuscariableWithKey<FloatMuscariable>(legacyPublic.Key);

            Assert.IsNotNull(convPrivate, "Converted private muscariable not found");
            Assert.IsNotNull(convPublic, "Converted public muscariable not found");

            Assert.AreEqual(expectedPrivateScope, convPrivate.Scope, "Private scope was not preserved after conversion");
            Assert.AreEqual(expectedPublicScope, convPublic.Scope, "Public scope was not preserved after conversion");

            yield break;
        }
    }
}