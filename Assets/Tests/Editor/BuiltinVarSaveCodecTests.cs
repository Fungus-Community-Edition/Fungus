using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amanita.VScripting;
using Collections;
using NUnit.Framework;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using Amanita.SaveSys;

namespace SaveSys.Tests
{
    // Editor tests (uses ScriptableObject.CreateInstance and GameObject)
    public class BuiltinVarSaveCodecTests
    {
        // Build the round-trip cases for all concrete Muscariable types handled by BuiltinVarSaveCodec.
        // We use the same type names that appear in BuiltinVarSaveCodec.subCodecs.
        // For Transform we will create a GameObject at runtime and assert by name.
        public static IEnumerable RoundTripCases
        {
            get
            {
                
                yield return new TestCaseData(typeof(IntMuscariable), 42)
                    .SetName("IntMuscariable");

                yield return new TestCaseData(typeof(FloatMuscariable), 3.1415f)
                    .SetName("FloatMuscariable");

                yield return new TestCaseData(typeof(BoolMuscariable), true)
                    .SetName("BoolMuscariable");

                yield return new TestCaseData(typeof(VectorTwoMuscariable), new Vector2(1.5f, -2.25f))
                    .SetName("VectorTwoMuscariable");

                yield return new TestCaseData(typeof(VectorThreeMuscariable), new Vector3(-1f, 2f, 3f))
                    .SetName("VectorThreeMuscariable");

                yield return new TestCaseData(typeof(StringMuscariable), "hello world")
                    .SetName("StringMuscariable");

                yield return new TestCaseData(typeof(ColorMuscariable), Color.red)
                    .SetName("ColorMuscariable");

                yield return new TestCaseData(typeof(TransformMuscariable), null)
                    .SetName("TransformMuscariable");
            }
        }

        [SetUp]
        public virtual void SetUp()
        {
            VariableTypeDiscovery.DiscoverAndRegister();
            codec = ScriptableObject.CreateInstance<BuiltinVarSaveCodec>();
            toDestroyInTearDown.Add(codec);
        }

        protected BuiltinVarSaveCodec codec;
        protected IVariable originalVar, decodedVar;
        protected readonly IList<UnityObj> toDestroyInTearDown = new List<UnityObj>();
        protected GameObject tmpGo;

        [TearDown]
        public void TearDown()
        {
            // Ensure any leftover test GameObjects are cleaned up between tests.
            // (Tests that create a temporary GameObject destroy it explicitly, but this is a safety-net.)

            IList<GameObject> allGameObjects;

#if UNITY_6000_0_OR_NEWER
            allGameObjects = UnityObj.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
            allGameObjects = UnityObj.FindObjectsOfType<GameObject>();
#endif

            IList<UnityObj> testGameObjects = allGameObjects.Where(go => go.name.StartsWith("test-tmp-"))
                .Cast<UnityObj>()
                .ToList();

            toDestroyInTearDown.AddRange(testGameObjects);

            foreach (var obj in toDestroyInTearDown)
            {
                if (obj != null)
                {
                    UnityObj.DestroyImmediate(obj);
                }
            }

            toDestroyInTearDown.Clear();
            originalVar = null;
            decodedVar = null;
            tmpGo = null;
        }

        [Test, TestCaseSource(nameof(RoundTripCases))]
        public void EncodeDecode_String_RoundTrip(Type variableType, object sampleValue)
        {
            originalVar = VariableFactory.CreateByVarType(variableType, null);
            originalVar.Key = "test";
            originalVar.ItemID = 123;
            SpecialHandlingForTransform(ref sampleValue);
            originalVar.Value = sampleValue;

            // Encode to string
            string encoded = codec.EncodeToString(originalVar);
            Assert.IsNotNull(encoded, "EncodeToString returned null for " + variableType.Name);
            Assert.IsNotEmpty(encoded, "EncodeToString returned empty string for " + variableType.Name);

            // Decode onto a fresh variable instance
            decodedVar = VariableFactory.CreateByVarType(variableType, null);
            codec.Decode(decodedVar, encoded);

            // Validate value equality with special cases for floating types and Transform
            AssertValuesEquivalent(variableType, originalVar.Value, decodedVar.Value);
        }

        protected virtual void SpecialHandlingForTransform(ref object sampleValue)
        {
            if (originalVar is not TransformMuscariable) return;
            tmpGo = new GameObject($"test-tmp-{Guid.NewGuid().ToString("N")}");
            tmpGo.AddComponent<SaveIdentifier>(); // So we can restore the state of GameObject and Transform variables
            sampleValue = tmpGo.transform;
            toDestroyInTearDown.Add(tmpGo);
        }

        [Test, TestCaseSource(nameof(RoundTripCases))]
        public void EncodeDecode_SaveData_RoundTrip(Type variableType, object sampleValue)
        {
            originalVar = VariableFactory.CreateByVarType(variableType, null);
            originalVar.Key = "test";
            originalVar.ItemID = 123;
            SpecialHandlingForTransform(ref sampleValue);
            originalVar.Value = sampleValue;

            // Encode to VariableSaveData
            VariableSaveData saveData = codec.EncodeToSave(originalVar);
            Assert.IsNotNull(saveData, "EncodeToSave returned null for " + variableType.Name);

            // Create new var and decode from VariableSaveData
            decodedVar = VariableFactory.CreateByVarType(variableType, null);
            codec.Decode(decodedVar, saveData);

            // Validate
            AssertValuesEquivalent(variableType, originalVar.Value, decodedVar.Value);
        }

        [Test]
        public void CanHandle_ReturnsTrue_For_All_Mapped_Types_AndFalse_For_Others()
        {
            // Access the protected static 'subCodecs' using reflection to obtain expected handled types.
            Type codecType = typeof(BuiltinVarSaveCodec);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            var field = codecType.GetField("subCodecs", flags);
            Assert.IsNotNull(field, "Could not reflect subCodecs field on BuiltinVarSaveCodec.");

            var subCodecs = field.GetValue(null) as IDictionary;
            Assert.IsNotNull(subCodecs, "subCodecs is null or not a dictionary.");

            // Validate that for each mapped type, CanHandle returns true (using an instance)
            IList<Type> mappedTypes = subCodecs.Keys.Cast<Type>().ToList();
            foreach (DictionaryEntry entry in subCodecs)
            {
                var varType = entry.Key as Type;
                Assert.IsNotNull(varType, "subCodecs key is not a Type.");

                VariableInfoAttribute varInfo = varType.GetCustomAttribute<VariableInfoAttribute>();
                Type contentType = varInfo.ContentType;

                IVariable instance = VariableFactory.Create(contentType, null);
                Assert.IsNotNull(instance, 
                    $"Failed to create instance of {varType.Name} for CanHandle test.");

                Assert.IsTrue(codec.CanHandle(instance), 
                    $"CanHandle returned false for mapped type {varType.Name}.");

                // Also test CanHandle(string) by type name
                Assert.IsTrue(codec.CanHandle(varType.Name), 
                    $"CanHandle(string) returned false for mapped type name {varType.Name}.");

                // Also test CanHandle(VariableSaveData).
                // VariableSaveData's type-name is an internal/protected field; don't rely on public setter here.
                var varSaveData = new VariableSaveData();

                // Try to set the internal/protected backing field 'varTypeName' via reflection
                // so we don't rely on a public API
                varSaveData.VarTypeName = varType.Name;

                Assert.IsTrue(codec.CanHandle(varSaveData), 
                    $"CanHandle(VariableSaveData) returned false for mapped type name {varType.Name}.");
            }

            // For an unrelated variable type CanHandle should return false.
            var dummy = ScriptableObject.CreateInstance<DummyVariable>();
            Assert.IsFalse(codec.CanHandle(dummy), 
                "CanHandle returned true for an unmapped (dummy) variable instance.");
            Assert.IsFalse(codec.CanHandle("SomeNonExistentTypeName_12345"), 
                "CanHandle(string) returned true for a non-existent type name.");

            var notMappedData = new VariableSaveData();
            // Make sure notMappedData does not identify as a mapped type
            var notMappedField = typeof(VariableSaveData).GetField("varTypeName", BindingFlags.Instance | BindingFlags.NonPublic);
            if (notMappedField != null) notMappedField.SetValue(notMappedData, "SomeNonExistentTypeName_12345");
            else
            {
                var prop = typeof(VariableSaveData).GetProperty("VarTypeName", BindingFlags.Public | BindingFlags.Instance)
                           ?? typeof(VariableSaveData).GetProperty("TypeName", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite) prop.SetValue(notMappedData, "SomeNonExistentTypeName_12345");
            }

            Assert.IsFalse(codec.CanHandle(notMappedData), "CanHandle(VariableSaveData) returned true for a non-existent type name.");
        }

        [Test]
        public void NullHandling_Methods_Throw_Or_Handle_Null_Gracefully()
        {
            // The current implementation will generally throw (NullReferenceException) when given null IVariable.
            // Assert that an exception is thrown, but accept either NullReferenceException or ArgumentNullException.
            void AssertNullThrows(Action a)
            {
                var ex = Assert.Throws<Exception>(() => a());
                Assert.IsTrue(ex is NullReferenceException || ex is ArgumentNullException, $"Expected NullReferenceException or ArgumentNullException, got {ex.GetType()}");
            }

            AssertNullThrows(() => codec.CanHandle((IVariable)null));
            AssertNullThrows(() => codec.EncodeToSave(null));
            AssertNullThrows(() => codec.EncodeToString(null));

            // For Decode variants: we expect an exception when supplying a null target variable.
            AssertNullThrows(() => codec.Decode((IVariable)null, "dummy"));
            // Passing null VariableSaveData to Decode should also throw (or be handled). We accept an exception here.
            var dummyVar = ScriptableObject.CreateInstance<DummyVariable>();
            AssertNullThrows(() => codec.Decode(dummyVar, (VariableSaveData)null));
        }

        #region Helpers

        // Helper to compare values; handles floats with tolerance and special case for Transform variables
        private static void AssertValuesEquivalent(Type variableType, object expected, object actual)
        {
            if (variableType == typeof(FloatMuscariable))
            {
                Assert.That(actual, Is.TypeOf<float>());
                Assert.AreEqual((float)expected, (float)actual, 1e-5f);
                return;
            }

            if (variableType == typeof(VectorTwoMuscariable))
            {
                Assert.That(actual, Is.TypeOf<Vector2>());
                Assert.AreEqual((Vector2)expected, (Vector2)actual);
                return;
            }

            if (variableType == typeof(VectorThreeMuscariable))
            {
                Assert.That(actual, Is.TypeOf<Vector3>());
                Assert.AreEqual((Vector3)expected, (Vector3)actual);
                return;
            }

            if (variableType == typeof(ColorMuscariable))
            {
                Assert.That(actual, Is.TypeOf<Color>());
                Assert.AreEqual((Color)expected, (Color)actual);
                return;
            }

            if (variableType == typeof(TransformMuscariable))
            {
                // Transform may be resolved by reference or looked up by name by codec.
                // Accept either same instance or same name on the referenced GameObject.
                Assert.IsTrue(actual is Transform || actual == null, $"Decoded Transform must be Transform or null, got {actual?.GetType().Name}");
                if (expected == null && actual == null) return;
                var expectedT = expected as Transform;
                var actualT = actual as Transform;
                Assert.IsNotNull(expectedT, "Expected transform was null in TransformMuscariable check.");
                Assert.IsNotNull(actualT, "Decoded transform was null in TransformMuscariable check.");
                Assert.AreEqual(expectedT.gameObject.name, actualT.gameObject.name, "Transform GameObject name mismatch after decode.");
                return;
            }

            // Default structural equality (works for ints, bools, strings and most Unity structs)
            Assert.AreEqual(expected, actual, $"Value mismatch for variable type {variableType.Name}");
        }

        // Minimal dummy variable that is NOT present in BuiltinVarSaveCodec.subCodecs. Used to verify CanHandle=false.
        private class DummyVariable : ScriptableObject, IVariable
        {
            // Implement minimal members expected by test/runtime; avoid referencing unavailable types (Execution.*).
            public string Key { get; set; }
            public object Value { get; set; }
            public VariableScope Scope => default;
            public Type ContentType => null;
            public IVariableSource Owner => null;

            public int ItemID { get; set; } = 5;

            public void Init() { }
            public bool IsComparisonSupported() => false;
            public bool Evaluate(CompareOperator compareOperator, object value) => throw new NotImplementedException();
            public void Apply(SetOperator setOperator, object value) => Value = value;
        }

        #endregion
    }
}