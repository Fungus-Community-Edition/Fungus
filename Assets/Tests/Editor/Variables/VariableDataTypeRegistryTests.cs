using Amanita.VScripting;
using NUnit.Framework;
using System.Reflection;

namespace Amanita.Tests.EditMode
{
    public class VariableDataTypeRegistryTests
    {
        [SetUp]
        public void SetUp() => VariableDataTypeRegistry.Clear();

        [Test]
        public void Register_MapsVariableTypeToDataType()
        {
            var fakeDataType = typeof(FakeIntVariableData);

            VariableDataTypeRegistry.Register(fakeDataType);

            var linked = VariableDataTypeRegistry.GetDataTypeLinkedToVarType(typeof(IntMuscariable));
            Assert.AreEqual(typeof(FakeIntVariableData), linked);
        }
    }
}