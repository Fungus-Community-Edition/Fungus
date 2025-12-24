using Amanita.VScripting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using Amanita;

namespace VScriptingTests.VariableOperations
{
    public class VariableSourceTests
    {
        private VariableSourceAsset _source;

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(TestAssetPath);
            PrepSourceAsset();
            void PrepSourceAsset()
            {
                // We want to make it an actual asset file so that it handles MuscariableHolders 
                // like it should in production.
                _source = ScriptableObject.CreateInstance<VariableSourceAsset>();
                AssetDatabase.CreateAsset(_source, TestAssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // We want to give the UI something to render right away, hence this initial var
                var stringVar = _source.AddNewVariableOfContentType<string>(initStringVarKey);
                stringVar.Value = initStringVarValue;
            }

            manager = AmanitaManager.EnsureExists();
            fcHolder = new GameObject("Flowchart");
            flowchart = fcHolder.AddComponent<Flowchart>();
            VariableTypeDiscovery.DiscoverAndRegister();

            toDestroy.Add(manager.gameObject);
            toDestroy.Add(flowchart.gameObject);
            toDestroy.Add(_source);
        }

        protected AmanitaManager manager;
        protected GameObject fcHolder;
        protected Flowchart flowchart;
        protected readonly IList<UnityObj> toDestroy = new List<UnityObj>();

        private const string TestAssetPath = "Assets/TestVariableSource.asset";
        protected readonly string initStringVarKey = "greeting";
        protected readonly string initStringVarValue = "hello";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestAssetPath);

            foreach (UnityObj obj in toDestroy)
            {
                if (obj != null && obj != AmanitaManager.S.gameObject)
                {
                    // Why avoid destroying the AmanitaManager singleton here?
                    // Because we need to keep it intact so that this suite's tests work
                    // right when being run as part of a suite, not just individually.
                    UnityObj.DestroyImmediate(obj);
                }
            }

            toDestroy.Clear();
            manager = null;
            fcHolder = null;
            flowchart = null;

        }

        [Test]
        public void AddNewVariableOfContentType_Generic_CreatesMuscariableWithKeyAndValue()
        {
            var floatVar = _source.AddNewVariableOfContentType<float>("myFloat", 3.5f);
            Assert.IsNotNull(floatVar);
            Assert.AreEqual("myFloat", floatVar.Key);
            Assert.AreEqual(3.5f, ((Muscariable<float>)floatVar).Value);
            Assert.AreSame(floatVar, _source.GetVariable("myFloat"));
        }

        [Test]
        public void AddNewVariableOfContentType_TypeOverload_CreatesMuscariableWithKey()
        {
            var intVar = _source.AddNewVariableOfContentType(typeof(int), "myInt");
            Assert.IsNotNull(intVar);
            Assert.AreEqual("myInt", intVar.Key);
            Assert.AreSame(intVar, _source.GetVariable("myInt"));
        }

        [Test]
        public void AddVariable_AddsMuscariableAndPreventsDuplicates()
        {
            var intVar = VariableFactory.Create<int>(7);
            intVar.Key = "dup";
            _source.AddVariable(intVar);
            _source.AddVariable(intVar); // second should no-op
            var list = _source.Variables;
            Assert.AreEqual(1, list.Count(elem => elem.Key == "dup"));
        }

        [Test]
        public void GetVarsByContentType_ReturnsMatchingContentType()
        {
            var floatVar = VariableFactory.Create<float>(1f);
            floatVar.Key = "f";
            var intVar = VariableFactory.Create<int>(2);
            intVar.Key = "i";
            _source.AddVariable(floatVar);
            _source.AddVariable(intVar);

            var floats = _source.GetVarsByContentType<float>();
            Assert.IsTrue(floats.Any(x => x.Key == "f"));
            Assert.IsFalse(floats.Any(x => x.Key == "i"));
        }

        [Test]
        public void GetVarsByType_ReturnsExactTypeMatch()
        {
            var floatVar = VariableFactory.Create(1f);
            floatVar.Key = "f";
            var otherFloat = VariableFactory.Create(2f);
            otherFloat.Key = "f2";
            _source.AddVariable(floatVar);
            _source.AddVariable(otherFloat);
            Debug.Log($"Float var is float muscariable: {floatVar is Muscariable<float>}");
            var results = _source.GetVarsByType(typeof(Muscariable<float>));
            Assert.IsTrue(results.Any(x => x.Key == "f"));
            Assert.IsTrue(results.Any(x => x.Key == "f2"));//
        }

        [TestCaseSource(nameof(NumericContentTypes))]
        public void AddVariable_ConvertsLegacyVariable_ToMuscariable(Type numericContentType)
        {
            var legacy = VariableFactory.AddLegacyVarTo(flowchart, numericContentType);
            legacy.Key = $"some{numericContentType.Name}Lol";
            legacy.Value = 1.23;

            _source.AddVariable(legacy);
            var found = _source.GetVariable(legacy.Key);

            Assert.IsNotNull(found, "Converted variable not added");
            Assert.AreEqual(numericContentType, found.ContentType);

            // Casting the results to floats so we don't need to mess around as much
            // with reflection
            float legacyValue = legacy.GetValueAs<float>();
            float foundValue = found.GetValueAs<float>();
            Assert.AreEqual(legacyValue, foundValue, epsilon);
        }

        protected static readonly float epsilon = 0.001f;

        public static Type[] NumericContentTypes =
        {
            typeof(float), typeof(int)
        };

        [Test]
        public void GetVariable_ReturnsNullWhenNotFound()
        {
            Assert.IsNull(_source.GetVariable("nope"));
        }
    }
}
