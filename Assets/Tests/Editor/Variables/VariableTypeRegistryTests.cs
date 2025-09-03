using Amanita.VScripting;
using NUnit.Framework;
using System.Linq;
using UnityEngine.TestTools;
using Type = System.Type;

namespace Amanita.Tests.EditMode
{
    public class VariableTypeRegistryTests
    {
        [SetUp]
        public void SetUp()
        {
            VariableTypeRegistry.Clear();
            VariableTypeRegistry.RegisterVariableType(typeof(IntegerVariable), new VariableTypeActions());
            VariableTypeRegistry.RegisterVariableType(typeof(IntMuscariable), new VariableTypeActions());
        }

        [TearDown]
        public virtual void TearDown()
        {
            UnityEngine.Debug.unityLogger.logEnabled = true;
        }

        [Test]
        public void RegisterVariableType_SeparatesLegacyAndMuscariables()
        {
            Type legacyInt = typeof(IntegerVariable), muscariInt = typeof(IntMuscariable);
            Assert.Contains(legacyInt, VariableTypeRegistry.AllLegacyTypes.ToList());
            Assert.Contains(muscariInt, VariableTypeRegistry.AllMuscariableTypes.ToList());
        }

        [Test]
        public void MuscariTypeFor_FallsBackToGeneric()
        {
            var type = VariableTypeRegistry.MuscariTypeFor(typeof(UnityEngine.Random));
            Assert.AreEqual(typeof(GenericMuscariable), type);
        }

        [Test]
        public void RegisterVariableType_DuplicateRegistration_OverwritesWithDiffActions()
        {
            // Arrange
            var firstActions = new VariableTypeActions();
            var secondActions = new VariableTypeActions
            {
                CompareFuncID = "weguihsuig",
                DescFuncID = "43fw5yt",
                SetFuncID = "wetguiyh4tw5iou8",
                DescFunc = null,
            };


            VariableTypeRegistry.RegisterVariableType(typeof(IntMuscariable), firstActions);
            VariableTypeRegistry.RegisterVariableType(typeof(IntMuscariable), secondActions);

            // Act
            VariableTypeRegistry.TryGetTypeActionsFor(typeof(IntMuscariable), out var actions);

            // Assert
            Assert.AreSame(secondActions, actions, "Duplicate registration should overwrite previous mapping");
        }

        [Test]
        public void MuscariTypeFor_UnregisteredContentType_ReturnsGenericMuscariable()
        {
            // Act
            var type = VariableTypeRegistry.MuscariTypeFor(typeof(string));

            // Assert
            Assert.AreEqual(typeof(GenericMuscariable), type, "Should fall back to GenericMuscariable for unknown content type");
        }

        [Test]
        public void LegacyTypeFor_UnregisteredContentType_ReturnsNull()
        {
            // Act
            var type = VariableTypeRegistry.LegacyTypeFor(typeof(string));

            // Assert
            Assert.IsNull(type, "Should return null when no legacy type is registered for content type");
        }

        [Test]
        public void ActionsFor_UnregisteredType_ReturnsNull()
        {
            UnityEngine.Debug.unityLogger.logEnabled = false;
            VariableTypeRegistry.Clear();
            VariableTypeRegistry.TryGetTypeActionsFor(typeof(IntMuscariable), out var actions);

            // Assert
            Assert.IsNull(actions, "Should return null when no actions are registered for type");
        }

        [Test]
        public void ActionsFor_UnregisteredType_LogsError()
        {
            VariableTypeRegistry.Clear();

            Type unregistered = typeof(IntMuscariable);
            string expectedLogMessage = $"Could not get type actions for type {unregistered.Name}.";
            LogAssert.Expect(UnityEngine.LogType.Error, expectedLogMessage);

            VariableTypeRegistry.TryGetTypeActionsFor(unregistered, out var actions);
        }


    }
}