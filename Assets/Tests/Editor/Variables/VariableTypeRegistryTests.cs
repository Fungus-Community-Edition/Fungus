using Amanita.VScripting;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace Amanita.Tests.EditMode
{
    public class VariableTypeRegistryTests
    {
        [SetUp]
        public void SetUp() => VariableTypeRegistry.Clear();

        [Test]
        public void RegisterVariableType_SeparatesLegacyAndMuscariables()
        {
            VariableTypeRegistry.RegisterVariableType(typeof(IntegerVariable), new VariableTypeActions());
            VariableTypeRegistry.RegisterVariableType(typeof(IntMuscariable), new VariableTypeActions());

            Assert.Contains(typeof(IntegerVariable), VariableTypeRegistry.AllLegacyTypes.ToList());
            Assert.Contains(typeof(IntMuscariable), VariableTypeRegistry.AllMuscariableTypes.ToList());
        }

        [Test]
        public void MuscariTypeFor_FallsBackToGeneric()
        {
            var type = VariableTypeRegistry.MuscariTypeFor(typeof(Random));
            Assert.AreEqual(typeof(GenericMuscariable), type);
        }
    }
}