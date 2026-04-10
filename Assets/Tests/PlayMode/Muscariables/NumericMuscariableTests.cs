using AtMycelia.Hyphlow;
using NUnit.Framework;

namespace VScriptingTests.MuscariableTests.DataOnly
{
    public class NumericMuscariableTests : MuscariableTestsCommon
    {

        [Test]
        public void IntMuscariable_ArithmeticAndComparison()
        {
            var intVar = new IntMuscariable { Key = "num", ItemId = 4 };
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
            var fVar = new FloatMuscariable { Key = "float", ItemId = 5 };
            fVar.Init();
            fVar.Value = SampleF; // 2.5f

            var fVarCopy = new FloatMuscariable { Key = "float2", ItemId = 6 };
            fVarCopy.Init();
            fVarCopy.Value = SampleF;

            Assert.IsTrue(fVar == fVarCopy);
            Assert.IsFalse(fVar != fVarCopy);

            var dVar = new DoubleMuscariable { Key = "dbl", ItemId = 7 };
            dVar.Init();
            dVar.Value = SampleD; // 3.5

            var dOther = new DoubleMuscariable { Key = "dbl2", ItemId = 8 };
            dOther.Init();
            dOther.Value = 2.5;

            Assert.IsFalse(dVar == dOther);
            Assert.IsTrue(dVar != dOther);
        }

        [Test]
        public void BoolMuscariable_EqualityOnly()
        {
            var bVar = new BoolMuscariable { Key = "flag", ItemId = 9 };
            bVar.Init();
            bVar.Value = true;

            var bVar2 = new BoolMuscariable { Key = "flag2", ItemId = 10 };
            bVar2.Init();
            bVar2.Value = false;

            Assert.IsTrue(bVar == new BoolMuscariable { Key = "x", ItemId = 11, Value = true });
            Assert.IsFalse(bVar == bVar2);
        }

        [Test]
        public void Init_WithValidKeyAndID_DoesNotThrow()
        {
            var v = new DoubleMuscariable { Key = "ok", ItemId = 99 };
            Assert.DoesNotThrow(() => v.Init());
        }

    }
}