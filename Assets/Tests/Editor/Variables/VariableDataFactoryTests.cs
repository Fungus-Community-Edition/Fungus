using Amanita.VScripting;
using NUnit.Framework;
using UnityEngine;
using System;
using System.Reflection;

namespace Amanita.Tests.EditMode
{
    public class VariableDataFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            VariableDataTypeRegistry.Clear();

            Type fakeDataType = typeof(FakeIntVariableData);
            VariableDataAttribute attr = fakeDataType.GetCustomAttribute<VariableDataAttribute>();
            VariableDataTypeRegistry.Register(fakeDataType, attr);
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
    }

    [VariableData(typeof(int), typeof(IntMuscariable))]
    [System.Serializable]
    public class FakeIntVariableData : VariableData<int, IntMuscariable>
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
                    _intRef.Value = (int)value.Value;
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