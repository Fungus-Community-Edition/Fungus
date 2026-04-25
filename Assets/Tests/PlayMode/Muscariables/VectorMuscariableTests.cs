using AtMycelia.Hyphlow;
using NUnit.Framework;
using System;
using UnityEngine;

namespace VScriptingTests.MuscariableTests.DataOnly
{
    public class VectorMuscariableTests : MuscariableTestsCommon
    {

        [Test]
        public void VecTwo_ValueAssignmentAndEvent()
        {
            var v = new VectorTwoMuscariable { Key = "v2", ItemId = 11 };
            v.Init();

            Vector2 captured = default;
            v.OnValueChanged += val => captured = (Vector2)val.BoxedValue;

            v.Value = V2A;
            Assert.AreEqual(V2A, v.Value);
            Assert.AreEqual(V2A, captured);
        }

        [Test]
        public void VecTwo_ComponentSetters_TriggerEvent()
        {
            var v = new VectorTwoMuscariable { Key = "v2", ItemId = 12 };
            v.Init();

            Vector2 recorded = default;
            v.OnValueChanged += val => recorded = (Vector2)val.BoxedValue;

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
            var a = new VectorTwoMuscariable { Key = "a", ItemId = 13, Value = V2A };
            var b = new VectorTwoMuscariable { Key = "b", ItemId = 14, Value = V2B };

            var sum = a + b;
            Assert.AreEqual(V2A + V2B, sum.Value);

            var diff = a - b;
            Assert.AreEqual(V2A - V2B, diff.Value);
        }

        [Test]
        public void VecTwo_OperatorMultiply_IntFloatAndMuscariable()
        {
            var v = new VectorTwoMuscariable { Key = "v", ItemId = 15, Value = V2A };
            var iv = new IntMuscariable { Key = "iv", ItemId = 16, Value = 2 };
            var fv = new FloatMuscariable { Key = "fv", ItemId = 17, Value = 0.5f };

            Assert.AreEqual(V2A * 3, (v * 3).Value);
            Assert.AreEqual(V2A * -1.0f, (v * -1.0f).Value);
            Assert.AreEqual(V2A * iv.Value, (v * iv).Value);
            Assert.AreEqual(V2A * fv.Value, (v * fv).Value);
        }

        [Test]
        public void VecTwo_EqualityOperators()
        {
            var a = new VectorTwoMuscariable { Key = "a", ItemId = 18, Value = V2A };
            var b = new VectorTwoMuscariable { Key = "b", ItemId = 19, Value = V2A };
            var c = new VectorTwoMuscariable { Key = "c", ItemId = 20, Value = V2B };

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);
        }

        [Test]
        public void VecTwo_CompareToVectorThree()
        {
            var v2 = new VectorTwoMuscariable { Key = "v2", ItemId = 21, Value = new Vector2(1, 2) };
            var v3 = new VectorThreeMuscariable { Key = "v3", ItemId = 22, Value = new Vector3(1, 2, 0) };

            Assert.IsTrue(v2 == v3);
            Assert.IsFalse(v2 != v3);
        }

        [Test]
        public void VecTwo_WrongTypeAssignment_Throws()
        {
            VectorTwoMuscariable vecVar = new VectorTwoMuscariable { Key = "v2", ItemId = 23 };
            vecVar.Init();
            Muscariable baseVar = vecVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = 123);
        }

        [Test]
        public void VectorThree_ComponentSettersAndEvent()
        {
            var v = new VectorThreeMuscariable { Key = "v3", ItemId = 24 };
            v.Init();

            Vector3 captured = default;
            v.OnValueChanged += val => captured = (Vector3)val.BoxedValue;

            v.X = 7.5f;
            Assert.AreEqual(7.5f, v.Value.x, Epsilon);
            Assert.AreEqual(v.Value, captured);

            v.Y = -1.25f;
            Assert.AreEqual(-1.25f, v.Value.y, Epsilon);

            v.Z = 3.0f;
            Assert.AreEqual(3.0f, v.Value.z, Epsilon);
        }

        [Test]
        public void VectorThree_OperatorOverloads()
        {
            var a = new VectorThreeMuscariable { Key = "a", ItemId = 25, Value = V3A };
            var b = new VectorThreeMuscariable { Key = "b", ItemId = 26, Value = V3B };

            Assert.AreEqual(V3A + V3B, (a + b).Value);
            Assert.AreEqual(V3A - V3B, (a - b).Value);

            // Vector3 + Vector2
            var v2 = new VectorTwoMuscariable { Key = "v2", ItemId = 27, Value = V2A };
            Assert.AreEqual(new Vector3(V3A.x + V2A.x, V3A.y + V2A.y, V3A.z),
                            (a + v2).Value);

            // Multiply by scalar
            Assert.AreEqual(V3A * 2, (a * 2).Value);
            Assert.AreEqual(V3A * 0.5f, (a * 0.5f).Value);
        }

        [Test]
        public void VectorThree_EqualityOperators()
        {
            var a = new VectorThreeMuscariable { Key = "a", ItemId = 28, Value = V3A };
            var b = new VectorThreeMuscariable { Key = "b", ItemId = 29, Value = V3A };
            var c = new VectorThreeMuscariable { Key = "c", ItemId = 30, Value = V3B };

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);
        }

        [Test]
        public void VectorThree_Evaluate_UnsupportedOperators()
        {
            var vecVar = new VectorThreeMuscariable { Key = "v3", ItemId = 31, Value = V3A };
            vecVar.Init(vecVar.BoxedValue);

            // Equals/NotEquals should work
            Assert.IsTrue(vecVar.Evaluate(CompareOperator.Equals, V3A));
            Assert.IsFalse(vecVar.Evaluate(CompareOperator.Equals, V3B));

            // LessThan should throw
            Assert.Throws<ArgumentException>(
                () => vecVar.Evaluate(CompareOperator.LessThan, V3B)
            );
        }

        [Test]
        public void VectorThree_WrongTypeAssignment_Throws()
        {
            VectorThreeMuscariable vecVar = new VectorThreeMuscariable { Key = "v3", ItemId = 32 };
            Muscariable baseVar = vecVar;
            vecVar.Init();
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = "not a vector");
        }
    }
}