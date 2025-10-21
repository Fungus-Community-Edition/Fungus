using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;
using Amanita.VScripting;
using Amanita;

namespace VScriptingTests
{
    // This suite now validates converting legacy Fungus Variables to Muscariables directly via ToMuscariable,
    // without involving Flowchart (which auto-converts on add/lookup).
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
            AmanitaManager.EnsureExists();
            toDestroyInTearDown.Clear();
        }

        protected static readonly IList<UnityObj> toDestroyInTearDown = new List<UnityObj>();

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

        // In this test suite, we only include cases for variable types that ship with the base package.
        static IEnumerable<FLCTestCase> LegacyVarCases()
        {
            // Numerics
            yield return new FLCTestCase(typeof(float), typeof(FloatVariable),
                typeof(FloatMuscariable), 11.11f);
            yield return new FLCTestCase(typeof(bool), typeof(BooleanVariable),
                typeof(BoolMuscariable), true);
            yield return new FLCTestCase(typeof(int), typeof(IntegerVariable),
                typeof(IntMuscariable), 42);
            yield return new FLCTestCase(typeof(Vector2), typeof(Vector2Variable),
                typeof(VectorTwoMuscariable), new Vector2(1, 2));
            yield return new FLCTestCase(typeof(Vector3), typeof(Vector3Variable),
                typeof(VectorThreeMuscariable), new Vector3(1, 2, 3));

            // Graphics
            yield return new FLCTestCase(typeof(string), typeof(StringVariable),
                typeof(StringMuscariable), "test string");
            yield return new FLCTestCase(typeof(Color), typeof(ColorVariable),
                typeof(ColorMuscariable), Color.cyan);
            yield return new FLCTestCase(typeof(Sprite), typeof(SpriteVariable),
                typeof(SpriteMuscariable), null);
            yield return new FLCTestCase(typeof(Texture), typeof(TextureVariable),
                typeof(TextureMuscariable), null);
            yield return new FLCTestCase(typeof(Material), typeof(MaterialVariable),
                typeof(MaterialMuscariable), null);

            // Unity general
            yield return new FLCTestCase(typeof(GameObject), typeof(GameObjectVariable),
                typeof(GameObjectMuscariable), null);
            yield return new FLCTestCase(typeof(Transform), typeof(TransformVariable),
                typeof(TransformMuscariable), null);
            yield return new FLCTestCase(typeof(UnityObj), typeof(ObjectVariable),
                typeof(UnityObjectMuscariable), null);

            // Audio
            yield return new FLCTestCase(typeof(AudioClip), typeof(AudioClipVariable),
                typeof(AudioClipMuscariable), null);
            yield return new FLCTestCase(typeof(AudioSource), typeof(AudioSourceVariable),
                typeof(AudioSourceMuscariable), null);
        }

        // Helper: create a legacy Variable on a fresh GameObject and assign key/value.
        Variable CreateLegacyVariable(Type legacyVarType, string key, object value)
        {
            var go = new GameObject($"LegacyVar_{legacyVarType.Name}");
            toDestroyInTearDown.Add(go);

            var legacy = go.AddComponent(legacyVarType) as Variable;
            Assert.IsNotNull(legacy, $"Failed to add legacy component of type {legacyVarType.Name}");

            legacy.Key = key;

            // Don't worry about strong typing here, just assign the value.
            legacy.Value = value;

            return legacy;
        }

        protected virtual string GenerateRandomKey(string prefix)
        {
            return prefix + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        [UnityTest]
        public IEnumerator ToMuscariable_Completes_And_Type_Matches([ValueSource(nameof(LegacyVarCases))] FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVariable(testCase.legacyVarType, key, testCase.sampleValue);

            var converted = legacy.ToMuscariable();

            Assert.IsNotNull(converted, "Conversion resulted in null");
            Assert.IsInstanceOf(testCase.muscVarType, converted, $"Expected muscariable type {testCase.muscVarType.Name}");
            yield break;
        }

        [UnityTest]
        public IEnumerator ToMuscariable_Preserves_Key([ValueSource(nameof(LegacyVarCases))] FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVariable(testCase.legacyVarType, key, testCase.sampleValue);

            var converted = legacy.ToMuscariable();

            Assert.IsNotNull(converted, "Conversion resulted in null");
            Assert.AreEqual(key, converted.Key, "Converted muscariable key does not match original");
            yield break;
        }

        [UnityTest]
        public IEnumerator ToMuscariable_Preserves_ContentType([ValueSource(nameof(LegacyVarCases))] FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVariable(testCase.legacyVarType, key, testCase.sampleValue);

            var converted = legacy.ToMuscariable();

            Assert.IsNotNull(converted, "Conversion resulted in null");
            Assert.AreEqual(testCase.contentType, converted.ContentType, "Converted muscariable ContentType mismatch");
            yield break;
        }

        [UnityTest]
        public IEnumerator ToMuscariable_Preserves_Value([ValueSource(nameof(LegacyVarCases))] FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVariable(testCase.legacyVarType, key, testCase.sampleValue);

            var converted = legacy.ToMuscariable();

            Assert.IsNotNull(converted, "Conversion resulted in null");

            dynamic dynLegacy = legacy;
            var legacyVal = (object)dynLegacy.Value;
            var convertedVal = converted.BoxedValue;

            Assert.AreEqual(legacyVal, convertedVal, "Converted muscariable value does not match legacy value");
            yield break;
        }

        [UnityTest]
        public IEnumerator ToMuscariable_Has_No_ParentFlowchart_When_Converted_Standalone([ValueSource(nameof(LegacyVarCases))] FLCTestCase testCase)
        {
            string key = GenerateRandomKey("testVar_");
            var legacy = CreateLegacyVariable(testCase.legacyVarType, key, testCase.sampleValue);

            var converted = legacy.ToMuscariable() as Muscariable;

            Assert.IsNotNull(converted, "Conversion resulted in null");
            Assert.IsNull(converted.ParentFlowchart, "Standalone conversion should not assign a ParentFlowchart");
            yield break;
        }
    }
}