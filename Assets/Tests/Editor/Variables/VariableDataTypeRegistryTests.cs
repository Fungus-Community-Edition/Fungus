using AtMycelia.Hyphlow;
using NUnit.Framework;

namespace VScriptingTests.VariableOperations
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