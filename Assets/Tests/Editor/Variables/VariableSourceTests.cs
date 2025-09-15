using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Amanita.VScripting;
using UnityObj = UnityEngine.Object;
using System.Collections.Generic;

namespace Amanita.Tests.EditMode
{
    public class VariableSourceTests
    {
        private VariableSource src;

        [SetUp]
        public void SetUp()
        {
            manager = AmanitaManager.EnsureExists();
            fcHolder = new GameObject("Flowchart");
            flowchart = fcHolder.AddComponent<Flowchart>();
            src = ScriptableObject.CreateInstance<VariableSource>();
            VariableTypeDiscovery.DiscoverAndRegister();

            toDestroy.Add(manager.gameObject);
            toDestroy.Add(flowchart.gameObject);
            toDestroy.Add(src);
        }

        protected AmanitaManager manager;
        protected GameObject fcHolder;
        protected Flowchart flowchart;
        protected readonly IList<UnityObj> toDestroy = new List<UnityObj>();

        [TearDown]
        public void TearDown()
        {
            foreach (UnityObj obj in toDestroy)
            {
                if (obj != null)
                {
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
            var v = src.AddNewVariableOfContentType<float>("myFloat", 3.5f);
            Assert.IsNotNull(v);
            Assert.AreEqual("myFloat", v.Key);
            Assert.AreEqual(3.5f, ((Muscariable<float>)v).Value);
            Assert.AreSame(v, src.GetVariable("myFloat"));
        }

        [Test]
        public void AddNewVariableOfContentType_TypeOverload_CreatesMuscariableWithKey()
        {
            var v = src.AddNewVariableOfContentType(typeof(int), "myInt");
            Assert.IsNotNull(v);
            Assert.AreEqual("myInt", v.Key);
            Assert.AreSame(v, src.GetVariable("myInt"));
        }

        [Test]
        public void AddVariable_AddsMuscariableAndPreventsDuplicates()
        {
            var m = VariableFactory.Create<int>(7);
            m.Key = "dup";
            src.AddVariable(m);
            src.AddVariable(m); // second should no-op
            var list = src.Variables;
            Assert.AreEqual(1, list.Count(v => v.Key == "dup"));
        }

        [Test]
        public void GetVarsByContentType_ReturnsMatchingContentType()
        {
            var f = VariableFactory.Create<float>(1f);
            f.Key = "f";
            var i = VariableFactory.Create<int>(2);
            i.Key = "i";
            src.AddVariable(f);
            src.AddVariable(i);

            var floats = src.GetVarsByContentType<float>();
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
            src.AddVariable(floatVar);
            src.AddVariable(otherFloat);
            Debug.Log($"Float var is float muscariable: {floatVar is Muscariable<float>}");
            var results = src.GetVarsByType(typeof(Muscariable<float>));
            Assert.IsTrue(results.Any(x => x.Key == "f"));
            Assert.IsTrue(results.Any(x => x.Key == "f2"));//
        }

        [TestCaseSource(nameof(NumericContentTypes))]
        public void AddVariable_ConvertsLegacyVariable_ToMuscariable(Type numericContentType)
        {
            var legacy = VariableFactory.AddLegacyVarTo(flowchart, numericContentType);
            legacy.Key = $"some{numericContentType.Name}Lol";
            legacy.Value = 1.23;

            src.AddVariable(legacy);
            var found = src.GetVariable(legacy.Key);

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
            Assert.IsNull(src.GetVariable("nope"));
        }
    }
}
