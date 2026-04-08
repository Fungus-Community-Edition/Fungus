using AtMycelia.Hyphlow;
using NUnit.Framework;
using UnityEngine;
using System;
using UnityEngine.TestTools;

namespace VScriptingTests.VariableOperations
{
    public class VariableDataFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            VariableDataTypeRegistry.Clear();
            Type fakeDataType = typeof(FakeIntVariableData);
            VariableDataTypeRegistry.Register(fakeDataType);
        }

        [TearDown]
        public virtual void TearDown()
        {
            Debug.unityLogger.logEnabled = true;
        }

        [Test]
        public void CreateForVar_ReturnsCorrectDataType()
        {
            // Act
            IVariableData data = VariableDataFactory.CreateForVar(typeof(IntMuscariable));

            // Assert
            Assert.IsInstanceOf<FakeIntVariableData>(data);
        }

        [Test]
        public void CreateForVar_UnknownType_ReturnsNull()
        {
            VariableDataTypeRegistry.Clear();
            var data = VariableDataFactory.CreateForVar(typeof(IntMuscariable));
            Assert.IsNull(data);
        }

        [Test]
        public void CreateForVar_WithUnknownVarType_ReturnsNull()
        {
            Debug.unityLogger.logEnabled = false;

            // Act
            var result = VariableDataFactory.CreateForVar(typeof(UnityEngine.Random));

            // Assert
            Assert.IsNull(result, "Factory should return null when no mapping exists for var type");
        }

        [Test]
        public void CreateForVar_WithNullVarType_ReturnsNull()
        {
            Debug.unityLogger.logEnabled = false;

            // Act
            var result = VariableDataFactory.CreateForVar(null);

            // Assert
            Assert.IsNull(result, "Factory should return null when var type is null");
        }

        [Test]
        public void CreateForVar_WithNullVarType_LogsWarning()
        {
            string expectedLogMessage = "Cannot create variable data for a null var type. Returning null.";
            LogAssert.Expect(LogType.Warning, expectedLogMessage);
            VariableDataFactory.CreateForVar(null);
        }

    }

    [VariableData(typeof(int), typeof(IntMuscariable))]
    [System.Serializable]
    public class FakeIntVariableData : VariableData<int>
    {
        [VariableProperty("<Value>", typeof(IntMuscariable))]
        [SerializeField] protected IntMuscariable _intRef = new IntMuscariable();

        public FakeIntVariableData() : base(default) { }

        public override IVariable VarRef
        {
            get { return _intRef; }
            set
            {
                if (value == null) { _intRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    _intRef.BoxedValue = (int)value.BoxedValue;
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new System.InvalidCastException(errorMessage);
                }

            }
        }

    }
}