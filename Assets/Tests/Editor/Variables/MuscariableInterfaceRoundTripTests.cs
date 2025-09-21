using NUnit.Framework;
using UnityEngine;
using Amanita.VScripting;

namespace VariableOperations
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
            ivar.Value = expected;

            // Read via strongly-typed property
            Assert.AreEqual(expected, musc.Value, "Strongly-typed Value should match after setting via IVariable");

            // Set via strongly-typed property
            var newVal = new Vector2(1, 2);
            musc.Value = newVal;

            // Read via interface
            Assert.AreEqual(newVal, (Vector2)ivar.Value, "IVariable.Value should match after setting via strongly-typed property");
        }

        [Test]
        public void ReferenceType_RoundTrips_Via_IVariable_Interface()
        {
            var musc = new StringMuscariable();
            var expected = "Hello";

            // Set via interface
            IVariable ivar = musc;
            ivar.Value = expected;

            // Read via strongly-typed property
            Assert.AreEqual(expected, musc.Value);

            // Set via strongly-typed property
            var newVal = "World";
            musc.Value = newVal;

            // Read via interface
            Assert.AreEqual(newVal, ivar.Value);
        }

        [Test]
        public void ValueType_CanBeCleared_Via_EitherPath()
        {
            var musc = new VectorTwoMuscariable { Value = new Vector2(5, 5) };
            IVariable ivar = musc;

            // Clear via interface
            ivar.Value = default(Vector2);
            Assert.AreEqual(default(Vector2), musc.Value, "Strongly-typed Value should be default after clearing via IVariable");

            // Set again
            musc.Value = new Vector2(9, 9);

            // Clear via strongly-typed property
            musc.Value = default;
            Assert.AreEqual(default(Vector2), (Vector2)ivar.Value, "IVariable.Value should be default after clearing via strongly-typed property");
        }

        [Test]
        public void ReferenceType_CanBeCleared_Via_EitherPath()
        {
            var musc = new StringMuscariable { Value = "NotNull" };
            IVariable ivar = musc;

            // Clear via interface
            ivar.Value = null;
            Assert.IsNull(musc.Value, "Strongly-typed Value should be null after clearing via IVariable");

            // Set again
            musc.Value = "StillHere";

            // Clear via strongly-typed property
            musc.Value = null;
            Assert.IsNull(ivar.Value, "IVariable.Value should be null after clearing via strongly-typed property");
        }
    }
}