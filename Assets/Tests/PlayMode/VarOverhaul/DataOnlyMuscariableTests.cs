using NUnit.Framework;
using System;
using UnityEngine;
using Amanita.VScripting;

namespace Amanita.MuscariableTests
{
    public class DataOnlyMuscariableTests : MuscariableTestsCommon
    {
        [Test]
        public void StringMuscariable_AssignAndEventFires()
        {
            var strVar = new StringMuscariable();
            strVar.Key = "greeting";
            strVar.ItemID = 1;
            strVar.Init();

            string captured = null;
            strVar.OnValueChanged += v => captured = v;

            strVar.Value = SampleS;
            Assert.AreEqual(SampleS, strVar.Value);
            Assert.AreEqual(SampleS, captured);
        }

        [Test]
        public void StringMuscariable_NullAssignmentAllowed()
        {
            var strVar = new StringMuscariable();
            strVar.Key = "maybeNull";
            strVar.ItemID = 2;
            strVar.Init();

            Assert.DoesNotThrow(() => strVar.Value = null);
            Assert.IsNull(strVar.Value);
        }

        [Test]
        public void StringMuscariable_WrongTypeThrows()
        {
            Muscariable baseVar = new StringMuscariable();
            baseVar.Key = "typeTest";
            baseVar.ItemID = 3;
            baseVar.Init();

            var ex = Assert.Throws<ArgumentException>(
                () => baseVar.Value = 12345
            );
            StringAssert.Contains("cannot hold", ex.Message);
        }

        [Test]
        public void IntMuscariable_ArithmeticAndComparison()
        {
            var intVar = new IntMuscariable { Key = "num", ItemID = 4 };
            intVar.Init();
            intVar.Value = SampleInt; // 10

            // Addition
            intVar.Apply(SetOperator.Add, 5);
            Assert.AreEqual(15, intVar.Value);

            // Subtraction
            intVar.Apply(SetOperator.Subtract, 3);
            Assert.AreEqual(12, intVar.Value);

            // Multiply & Divide
            intVar.Apply(SetOperator.Multiply, 2);
            Assert.AreEqual(24, intVar.Value);
            intVar.Apply(SetOperator.Divide, 4);
            Assert.AreEqual(6, intVar.Value);

            // Compare operators
            Assert.IsTrue(intVar.Evaluate(CompareOperator.GreaterThan, 5));
            Assert.IsFalse(intVar.Evaluate(CompareOperator.LessThan, 5));
            Assert.IsTrue(intVar.Evaluate(CompareOperator.Equals, 6));
        }

        [Test]
        public void FloatAndDoubleMuscariable_EqualityOperators()
        {
            var fVar = new FloatMuscariable { Key = "float", ItemID = 5 };
            fVar.Init();
            fVar.Value = SampleF; // 2.5f

            var fVarCopy = new FloatMuscariable { Key = "float2", ItemID = 6 };
            fVarCopy.Init();
            fVarCopy.Value = SampleF;

            Assert.IsTrue(fVar == fVarCopy);
            Assert.IsFalse(fVar != fVarCopy);

            var dVar = new DoubleMuscariable { Key = "dbl", ItemID = 7 };
            dVar.Init();
            dVar.Value = SampleD; // 3.5

            var dOther = new DoubleMuscariable { Key = "dbl2", ItemID = 8 };
            dOther.Init();
            dOther.Value = 2.5;

            Assert.IsFalse(dVar == dOther);
            Assert.IsTrue(dVar != dOther);
        }

        [Test]
        public void BoolMuscariable_EqualityOnly()
        {
            var bVar = new BoolMuscariable { Key = "flag", ItemID = 9 };
            bVar.Init();
            bVar.Value = true;

            var bVar2 = new BoolMuscariable { Key = "flag2", ItemID = 10 };
            bVar2.Init();
            bVar2.Value = false;

            Assert.IsTrue(bVar == new BoolMuscariable { Key = "x", ItemID = 11, Value = true });
            Assert.IsFalse(bVar == bVar2);
        }

        [Test]
        public void Init_WithNoKeyOrID_ThrowsException()
        {
            var v = new IntMuscariable();
            var ex = Assert.Throws<Exception>(() => v.Init());
            StringAssert.Contains("needs a valid key", ex.Message);
            StringAssert.Contains("needs a valid ID", ex.Message);
        }

        [Test]
        public void Init_WithValidKeyAndID_DoesNotThrow()
        {
            var v = new DoubleMuscariable { Key = "ok", ItemID = 99 };
            Assert.DoesNotThrow(() => v.Init());
        }

        [Test]
        public void VecTwo_Init_RequiresKeyAndID()
        {
            var v = new VectorTwoMuscariable();
            var ex = Assert.Throws<Exception>(() => v.Init());
            StringAssert.Contains("needs a valid key", ex.Message);
            StringAssert.Contains("needs a valid ID", ex.Message);

            v.Key = "v2"; v.ItemID = 10;
            Assert.DoesNotThrow(() => v.Init());
        }

        [Test]
        public void VecTwo_ValueAssignmentAndEvent()
        {
            var v = new VectorTwoMuscariable { Key = "v2", ItemID = 11 };
            v.Init();

            Vector2 captured = default;
            v.OnValueChanged += val => captured = val;

            v.Value = V2A;
            Assert.AreEqual(V2A, v.Value);
            Assert.AreEqual(V2A, captured);
        }

        [Test]
        public void VecTwo_ComponentSetters_TriggerEvent()
        {
            var v = new VectorTwoMuscariable { Key = "v2", ItemID = 12 };
            v.Init();

            Vector2 recorded = default;
            v.OnValueChanged += val => recorded = val;

            v.X = 5.0f;
            Assert.AreEqual(5.0f, v.Value.x, Epsilon);
            Assert.AreEqual(v.Value, recorded);

            v.Y = -3.0f;
            Assert.AreEqual(-3.0f, v.Value.y, Epsilon);
            Assert.AreEqual(v.Value, recorded);
        }

        [Test]
        public void VecTwo_OperatorAddSubtract()
        {
            var a = new VectorTwoMuscariable { Key = "a", ItemID = 13, Value = V2A };
            var b = new VectorTwoMuscariable { Key = "b", ItemID = 14, Value = V2B };

            var sum = a + b;
            Assert.AreEqual(V2A + V2B, sum.Value);

            var diff = a - b;
            Assert.AreEqual(V2A - V2B, diff.Value);
        }

        [Test]
        public void VecTwo_OperatorMultiply_IntFloatAndMuscariable()
        {
            var v = new VectorTwoMuscariable { Key = "v", ItemID = 15, Value = V2A };
            var iv = new IntMuscariable { Key = "iv", ItemID = 16, Value = 2 };
            var fv = new FloatMuscariable { Key = "fv", ItemID = 17, Value = 0.5f };

            Assert.AreEqual(V2A * 3, (v * 3).Value);
            Assert.AreEqual(V2A * -1.0f, (v * -1.0f).Value);
            Assert.AreEqual(V2A * iv.Value, (v * iv).Value);
            Assert.AreEqual(V2A * fv.Value, (v * fv).Value);
        }

        [Test]
        public void VecTwo_EqualityOperators()
        {
            var a = new VectorTwoMuscariable { Key = "a", ItemID = 18, Value = V2A };
            var b = new VectorTwoMuscariable { Key = "b", ItemID = 19, Value = V2A };
            var c = new VectorTwoMuscariable { Key = "c", ItemID = 20, Value = V2B };

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);
        }

        [Test]
        public void VecTwo_CompareToVectorThree()
        {
            var v2 = new VectorTwoMuscariable { Key = "v2", ItemID = 21, Value = new Vector2(1, 2) };
            var v3 = new VectorThreeMuscariable { Key = "v3", ItemID = 22, Value = new Vector3(1, 2, 0) };

            Assert.IsTrue(v2 == v3);
            Assert.IsFalse(v2 != v3);
        }

        [Test]
        public void VecTwo_WrongTypeAssignment_Throws()
        {
            VectorTwoMuscariable vecVar = new VectorTwoMuscariable { Key = "v2", ItemID = 23 };
            vecVar.Init();
            Muscariable baseVar = vecVar;
            Assert.Throws<ArgumentException>(() => baseVar.Value = 123);
        }

        [Test]
        public void V3_ComponentSettersAndEvent()
        {
            var v = new VectorThreeMuscariable { Key = "v3", ItemID = 24 };
            v.Init();

            Vector3 captured = default;
            v.OnValueChanged += val => captured = val;

            v.X = 7.5f;
            Assert.AreEqual(7.5f, v.Value.x, Epsilon);
            Assert.AreEqual(v.Value, captured);

            v.Y = -1.25f;
            Assert.AreEqual(-1.25f, v.Value.y, Epsilon);

            v.Z = 3.0f;
            Assert.AreEqual(3.0f, v.Value.z, Epsilon);
        }

        [Test]
        public void V3_OperatorOverloads()
        {
            var a = new VectorThreeMuscariable { Key = "a", ItemID = 25, Value = V3A };
            var b = new VectorThreeMuscariable { Key = "b", ItemID = 26, Value = V3B };

            Assert.AreEqual(V3A + V3B, (a + b).Value);
            Assert.AreEqual(V3A - V3B, (a - b).Value);

            // Vector3 + Vector2
            var v2 = new VectorTwoMuscariable { Key = "v2", ItemID = 27, Value = V2A };
            Assert.AreEqual(new Vector3(V3A.x + V2A.x, V3A.y + V2A.y, V3A.z),
                            (a + v2).Value);

            // Multiply by scalar
            Assert.AreEqual(V3A * 2, (a * 2).Value);
            Assert.AreEqual(V3A * 0.5f, (a * 0.5f).Value);
        }

        [Test]
        public void V3_EqualityOperators()
        {
            var a = new VectorThreeMuscariable { Key = "a", ItemID = 28, Value = V3A };
            var b = new VectorThreeMuscariable { Key = "b", ItemID = 29, Value = V3A };
            var c = new VectorThreeMuscariable { Key = "c", ItemID = 30, Value = V3B };

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);
        }

        [Test]
        public void V3_Evaluate_UnsupportedOperators()
        {
            var v = new VectorThreeMuscariable { Key = "v3", ItemID = 31, Value = V3A };
            v.Init();

            // Equals/NotEquals should work
            Assert.IsTrue(v.Evaluate(CompareOperator.Equals, V3A));
            Assert.IsFalse(v.Evaluate(CompareOperator.Equals, V3B));

            // LessThan should throw
            Assert.Throws<ArgumentException>(
                () => v.Evaluate(CompareOperator.LessThan, V3B)
            );
        }

        [Test]
        public void V3_WrongTypeAssignment_Throws()
        {
            VectorThreeMuscariable vecVar = new VectorThreeMuscariable { Key = "v3", ItemID = 32 };
            Muscariable baseVar = vecVar;
            vecVar.Init();
            Assert.Throws<ArgumentException>(() => baseVar.Value = "not a vector");
        }


    }
}