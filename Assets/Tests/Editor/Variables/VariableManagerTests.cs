using System.Collections.Generic;
using System.Reflection;
using AtMycelia.Hyphlow;
using NUnit.Framework;

namespace VScriptingTests.Variables
{
    public sealed class VariableManagerTests
    {
        [Test]
        public void RemoveVariable_RemovesMuscariableWithNullValue()
        {
            VariableManager manager = new VariableManager();
            manager.Initialize();

            StringMuscariable variable = manager.AddNewMuscari<string, StringMuscariable>("NullVar", null);

            List<Muscariable> muscariables = GetMuscariablesList(manager);
            Assert.AreEqual(1, muscariables.Count);

            manager.RemoveVariable(variable);

            Assert.AreEqual(0, muscariables.Count);
            Assert.AreEqual(0, manager.VariableCount);
        }

        private static List<Muscariable> GetMuscariablesList(VariableManager manager)
        {
            FieldInfo field = typeof(VariableManager).GetField("_muscariables",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(field, "VariableManager._muscariables field not found.");

            return (List<Muscariable>)field.GetValue(manager);
        }
    }
}