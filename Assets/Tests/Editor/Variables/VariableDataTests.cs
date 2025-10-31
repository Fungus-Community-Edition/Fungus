using NUnit.Framework;
using Amanita.VScripting;
using System;
using UnityEngine;

namespace VScriptingTests.VariableOperations
{
    public class VariableDataTests : VariableTests
    {
        [TestCaseSource(nameof(VariableDatas))]
        public virtual void StartsWithNoVariableReference(VariableData variableData)
        {
            Assert.IsNull(variableData.VarRef);
        }

        [TestCaseSource(nameof(VariableDatas))]
        public virtual void StartsWithDefaultValue(VariableData variableData)
        {
            Assert.AreEqual(default, variableData.BoxedValue);
        }

        public static VariableData[] VariableDatas = new VariableData[]
        {
            new FloatData(),
            new ObjectData(),
            new AudioClipData(),
            new IntegerData(),
            new StringData(),
            new BooleanData(),
            new ColorData(),
            new Vector2Data(),
            new Vector3Data(),
            new ObjectData(),
            new AudioSourceData()
        };

        [Test]
        public void LiteralAssignment_Persists()
        {
            var fd = new FloatData();
            fd.Value = 1.23f;
            Assert.AreEqual(1.23f, fd.Value);
            Assert.AreEqual("1.23", fd.GetDescription());
        }

        [Test]
        public void VarRef_Propagation_BothDirections()
        {
            var fd = new FloatData();
            var varObj = VariableFactory.Create<float>(2f);

            // Wire up variable as reference
            fd.VarRef = varObj;

            // var -> data
            Assert.AreEqual(2f, fd.Value);

            // mutate variable, verify data sees change
            varObj.Value = 5f;
            Assert.AreEqual(5f, fd.Value);

            // mutate data, verify variable updated
            fd.Value = 7f;
            Assert.AreEqual(7f, varObj.Value);
        }

        // Parameterized cases for the following tests:
        // (variableDataType, initialLiteral, variableValue, backLiteral, mismatchContentType)
        public static object[] VarDataCases = new object[]
        {
            new object[] { typeof(FloatData),    3f,                     10f,                    4f,                     typeof(int) },
            new object[] { typeof(IntegerData),  3,                      10,                     4,                      typeof(float) },
            new object[] { typeof(StringData),   "foo",                  "bar",                  "baz",                  typeof(int) },
            new object[] { typeof(BooleanData),  true,                   false,                  true,                   typeof(int) },
            new object[] { typeof(ColorData),    Color.red,              Color.green,            Color.blue,             typeof(float) },
            new object[] { typeof(Vector3Data),  new Vector3(1,2,3),     new Vector3(4,5,6),     new Vector3(7,8,9),     typeof(int) }
        };

        [TestCaseSource(nameof(VarDataCases))]
        public void SwitchingBetweenLiteralAndVarRef_Works(Type dataType, object initialLiteral,
            object varValue, object backLiteral, Type mismatchContentType)
        {
            var data = Activator.CreateInstance(dataType) as VariableData;
            Assert.IsNotNull(data, $"Could not create instance of {dataType.Name}");

            // start with literal
            data.BoxedValue = initialLiteral;
            Assert.AreEqual(initialLiteral, data.BoxedValue);

            // create a Muscariable of the appropriate content type and assign as VarRef
            var musc = VariableFactory.CreateByContentType(data.ContentType, null);
            musc.BoxedValue = varValue;
            data.VarRef = musc;

            Assert.AreEqual(varValue, data.BoxedValue);

            // switch back to literal
            data.VarRef = null;
            data.BoxedValue = backLiteral;
            Assert.AreEqual(backLiteral, data.BoxedValue);
        }

        [TestCaseSource(nameof(VarDataCases))]
        public void SetContentsTo_And_GetCopy_CreateIndependentCopies(Type dataType, object initialLiteral, object varValue, object backLiteral, Type mismatchContentType)
        {
            var original = Activator.CreateInstance(dataType) as VariableData;
            Assert.IsNotNull(original);
            original.BoxedValue = initialLiteral;

            var copyObj = original.GetCopy();
            Assert.IsNotNull(copyObj);
            var copy = copyObj as VariableData;
            Assert.IsNotNull(copy);

            Assert.AreEqual(original.BoxedValue, copy.BoxedValue);

            // Ensure changing copy does not change original
            object newVal = varValue ?? backLiteral ?? initialLiteral;
            copy.BoxedValue = newVal;
            Assert.AreEqual(initialLiteral, original.BoxedValue);
            Assert.AreEqual(newVal, copy.BoxedValue);

            // Test SetContentsTo
            var target = Activator.CreateInstance(dataType) as VariableData;
            Assert.IsNotNull(target);
            target.SetContentsTo(original);
            Assert.AreEqual(original.BoxedValue, target.BoxedValue);
        }

        [TestCaseSource(nameof(VarDataCases))]
        public void VarRef_TypeMismatch_ThrowsInvalidCastException(Type dataType, object initialLiteral, object varValue, object backLiteral, Type mismatchContentType)
        {
            var data = Activator.CreateInstance(dataType) as VariableData;
            Assert.IsNotNull(data);

            // create a muscariable with a different content type
            var mismatchMusc = VariableFactory.CreateByContentType(mismatchContentType, null);
            // set a default value (not important)
            mismatchMusc.BoxedValue = mismatchContentType.IsValueType ? Activator.CreateInstance(mismatchContentType) : null;

            Assert.Throws<InvalidCastException>(() => data.VarRef = mismatchMusc);
        }
    }
}