using NUnit.Framework;
using System;
using UnityEngine;
using Amanita.VScripting;
using UnityEngine.TestTools;

namespace Amanita.Tests.EditMode
{
    public class VariableFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            GetTheRightTypesRegistered();
            static void GetTheRightTypesRegistered()
            {
                VariableTypeRegistry.Clear();
                VariableDataTypeRegistry.Clear();

                VariableTypeRegistry.RegisterVariableType(typeof(IntMuscariable), new VariableTypeActions());
                VariableTypeRegistry.RegisterVariableType(typeof(IntegerVariable), new VariableTypeActions());
                VariableTypeRegistry.RegisterVariableType(typeof(HookedIntMuscariable), new VariableTypeActions());
                VariableTypeRegistry.RegisterVariableType(typeof(FakeIntLegacyVar), new VariableTypeActions());
            }
            
        }

        [TearDown]
        public virtual void TearDown()
        {
            Debug.unityLogger.logEnabled = true;
        }

        [Test]
        public void Create_Muscariable_From_ContentType_CopiesFields()
        {
            // Arrange
            var contentType = typeof(int);
            
            var source = new IntMuscariable
            {
                Key = "TestKey",
                Scope = VariableScope.Global,
                ItemID = 42,
                Value = 99
            };

            // Act
            var created = VariableFactory.Create(contentType, source) as IntMuscariable;

            // Assert
            Assert.NotNull(created);
            Assert.AreEqual("TestKey", created.Key);
            Assert.AreEqual(VariableScope.Global, created.Scope);
            Assert.AreEqual(42, created.ItemID);
            Assert.AreEqual(99, created.Value);
        }

        [Test]
        public void AddLegacyVarTo_AddsComponent_AndRegisters()
        {
            // Arrange
            var go = new GameObject("FlowchartHolder");
            var flowchart = go.AddComponent<Flowchart>();
            
            // Act
            var added = VariableFactory.AddLegacyVarTo(flowchart, typeof(int));

            // Assert
            Assert.NotNull(added);
            Assert.IsTrue(flowchart.HasVariable(added));
        }

        [Test]
        public void CreateMuscari_SourceHasMismatchedContentType_ReturnsNull()
        {
            // Arrange
            Debug.unityLogger.logEnabled = false;
            
            var source = new HookedIntMuscariable { Value = 123 };

            // Act
            Type wrongContentType = typeof(int); // Since the int is the source while the string is the intended output
            Type rightContentType = typeof(string);

            var result = VariableFactory.Create(rightContentType, source);

            // Assert
            Assert.IsNull(result, "Factory should not create when source content type mismatches that of the intended result");
        }

        [Test]
        public void CreateMuscari_SourceHasMismatchedContentType_LogsWarning()
        {
            // Arrange
            var source = new HookedIntMuscariable { Value = 123 };

            // Act
            Type wrongContentType = typeof(int); // Since the int is the source while the string is the intended output
            Type rightContentType = typeof(string);

            string expectedLogMessage = $"Cannot copy over the values of a variable of ContentType " +
                    $"{wrongContentType.Name} when creating a Muscariable of ContentType {rightContentType.Name}. "
                    + "Returning null.";
            LogAssert.Expect(LogType.Warning, expectedLogMessage);
            var result = VariableFactory.Create(rightContentType, source);

        }

        [Test]
        public void AddLegacyVarTo_WhenFlowchartAddFails_LogsError()
        {
            // Arrange
            var fcHolder = new GameObject("FlowchartHolder");
            var flowchart = fcHolder.AddComponent<FakeFlowchartThatFailsAdd>();
            Type desiredLegacyType = typeof(IntegerVariable);

            // Act
            string errorMessage = $"Failed to add legacy variable component of type " +
                    $"{desiredLegacyType.Name} to Flowchart {flowchart.name}. Returning null.";
            LogAssert.Expect(LogType.Warning, errorMessage);
            var result = VariableFactory.AddLegacyVarTo(flowchart, typeof(int));
            
        }

        [Test]
        public void AddLegacyVarTo_WhenFlowchartAddFails_ReturnsNull()
        {
            Debug.unityLogger.logEnabled = false;
            // Arrange
            var fcHolder = new GameObject("FlowchartHolder");
            var flowchart = fcHolder.AddComponent<FakeFlowchartThatFailsAdd>();

            // Act
            var result = VariableFactory.AddLegacyVarTo(flowchart, typeof(int));

            // Assert
            Assert.IsNull(result, "Should return null when Flowchart refuses to add variable");
        }

        // --- Fakes for testing ---
        [VariableInfo("", "", typeof(int))]
        public class FakeIntLegacyVar : IntegerVariable { }

        public class FakeFlowchartThatFailsAdd : Flowchart
        {
            public override void AddVariable(IVariable v)
            {
                throw new System.Exception("Simulated failure");
            }
        }

    }

}