using NUnit.Framework;
using UnityEngine;
using AtMycelia.Hyphlow;

namespace VScriptingTests.VariableOperations
{
    public class MuscariableInterfaceRoundTripTests
    {
        [Test]
        public void ValueType_RoundTrips_Via_IVariable_Interface()
        {
            var musc = new VectorTwoMuscariable();
            var expected = new Vector2(3.14f, 2.72f);

            // Set via interface
            IVariable ivar = musc;
            ivar.BoxedValue = expected;

            // Read via strongly-typed property
            Assert.AreEqual(expected, musc.BoxedValue, "Strongly-typed Value should match after setting via IVariable");

            // Set via strongly-typed property
            var newVal = new Vector2(1, 2);
            musc.BoxedValue = newVal;

            // Read via interface
            Assert.AreEqual(newVal, (Vector2)ivar.BoxedValue, "IVariable.BoxedValue should match after setting via strongly-typed property");
        }

        [Test]
        public void ReferenceType_RoundTrips_Via_IVariable_Interface()
        {
            var musc = new StringMuscariable();
            var expected = "Hello";

            // Set via interface
            IVariable ivar = musc;
            ivar.BoxedValue = expected;

            // Read via strongly-typed property
            Assert.AreEqual(expected, musc.BoxedValue);

            // Set via strongly-typed property
            var newVal = "World";
            musc.BoxedValue = newVal;

            // Read via interface
            Assert.AreEqual(newVal, ivar.BoxedValue);
        }

        [Test]
        public void ValueType_CanBeCleared_ViaEitherPath()
        {
            var musc = new VectorTwoMuscariable { Value = new Vector2(5, 5) };
            IVariable ivar = musc;

            // Clear via interface
            ivar.BoxedValue = default(Vector2);
            Assert.AreEqual(default(Vector2), musc.BoxedValue, "Strongly-typed Value should be default after clearing via IVariable");

            // Set again
            musc.BoxedValue = new Vector2(9, 9);

            // Clear via strongly-typed property
            musc.Value = default;
            Assert.AreEqual(default(Vector2), (Vector2)ivar.BoxedValue, "IVariable.BoxedValue should be default after clearing via strongly-typed property");
        }

        [Test]
        public void ReferenceType_CanBeCleared_ViaEitherPath()
        {
            var musc = new StringMuscariable { Value = "NotNull" };
            IVariable ivar = musc;

            // Clear via interface
            ivar.BoxedValue = null;
            Assert.IsNull(musc.BoxedValue, "Strongly-typed Value should be null after clearing via IVariable");

            // Set again
            musc.BoxedValue = "StillHere";

            // Clear via strongly-typed property
            musc.BoxedValue = null;
            Assert.IsNull(ivar.BoxedValue, "IVariable.BoxedValue should be null after clearing via strongly-typed property");
        }
    }
}