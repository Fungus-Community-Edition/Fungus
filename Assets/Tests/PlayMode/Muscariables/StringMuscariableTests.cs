using AtMycelia.Hyphlow;
using NUnit.Framework;
using System;

namespace VScriptingTests.MuscariableTests.DataOnly
{
    public class StringMuscariableTests : MuscariableTestsCommon
    {

        [Test]
        public void StringMuscariable_AssignAndEventFires()
        {
            var strVar = new StringMuscariable();
            strVar.Key = "greeting";
            strVar.ItemId = 1;
            strVar.Init();

            string captured = null;
            strVar.OnValueChanged += v => captured = v.BoxedValue.ToString();

            strVar.Value = SampleS;
            Assert.AreEqual(SampleS, strVar.Value);
            Assert.AreEqual(SampleS, captured);
        }

        [Test]
        public void StringMuscariable_NullAssignmentAllowed()
        {
            var strVar = new StringMuscariable();
            strVar.Key = "maybeNull";
            strVar.ItemId = 2;
            strVar.Init();

            Assert.DoesNotThrow(() => strVar.Value = null);
            Assert.IsNull(strVar.Value);
        }

        [Test]
        public void StringMuscariable_WrongTypeThrows()
        {
            Muscariable baseVar = new StringMuscariable();
            baseVar.Key = "typeTest";
            baseVar.ItemId = 3;
            baseVar.Init();

            var ex = Assert.Throws<ArgumentException>(
                () => baseVar.BoxedValue = 12345
            );

            StringAssert.Contains("Cannot set", ex.Message);
        }

    }
}