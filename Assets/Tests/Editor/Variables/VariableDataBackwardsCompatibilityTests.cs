using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using AtMycelia.Hyphlow;

namespace VScriptingTests.VariableRows
{
    public sealed class VariableDataBackwardsCompatibilityTests
    {
        [Test]
        public void IntegerData_DoesNotClobberLiteral_WhenLegacyIsDefault()
        {
            var data = new IntegerData();
            data.LiteralValue = 42;
            data.integerVal = default;

            InvokeBackwardsCompatibility(data);

            Assert.AreEqual(42, data.LiteralValue);
        }

        [Test]
        public void IntegerData_MigratesLegacyLiteral_WhenLegacyIsNonDefault()
        {
            var data = new IntegerData();
            data.LiteralValue = 1;
            data.integerVal = 7;

            InvokeBackwardsCompatibility(data);

            Assert.AreEqual(7, data.LiteralValue);
            Assert.AreEqual(default(int), data.integerVal);
        }

        [Test]
        public void GameObjectData_DoesNotClobberLiteral_WhenLegacyIsDestroyed()
        {
            var current = new GameObject("Current");
            var legacy = new GameObject("Legacy");
            var data = new GameObjectData(current);
            data.gameObjectVal = legacy;

            Object.DestroyImmediate(legacy);

            InvokeBackwardsCompatibility(data);

            Assert.AreEqual(current, data.LiteralValue);

            Object.DestroyImmediate(current);
        }

        [Test]
        public void StringData_DoesNotClobberLiteral_WhenLegacyIsEmpty()
        {
            var data = new StringData("Current");
            data.stringVal = string.Empty;

            InvokeBackwardsCompatibility(data);

            Assert.AreEqual("Current", data.LiteralValue);
        }

        [Test]
        public void StringData_MigratesLegacyLiteral_WhenLegacyIsNonEmpty()
        {
            var data = new StringData("Current");
            data.stringVal = "Legacy";

            InvokeBackwardsCompatibility(data);

            Assert.AreEqual("Legacy", data.LiteralValue);
            Assert.That(string.IsNullOrEmpty(data.stringVal), Is.True);
        }

        private static void InvokeBackwardsCompatibility(object target)
        {
            MethodInfo method = target.GetType().GetMethod(
                "DoBackwardsCompatibility",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(method, $"Could not find DoBackwardsCompatibility on {target.GetType().Name}.");
            method.Invoke(target, null);
        }
    }
}