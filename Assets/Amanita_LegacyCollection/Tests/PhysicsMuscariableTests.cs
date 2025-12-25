using System;
using NUnit.Framework;
using UnityEngine;
using Amanita.VScripting;
using UnityEngine.TestTools;

namespace Amanita.MuscariableTests.DataOnly
{
    [TestFixture]
    public class PhysicsMuscariableTests
    {
        [SetUp]
        public void Setup()
        {
            firstGameObjectForThreeD = GameObject.CreatePrimitive(PrimitiveType.Cube);
            secondGameObjectForThreeD = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            firstColliderThreeD = firstGameObjectForThreeD.GetComponent<Collider>();
            secondColliderThreeD = secondGameObjectForThreeD.GetComponent<Collider>();

            firstGameObjectForTwoD = new GameObject("2DA", typeof(BoxCollider2D));
            secondGameObjectForTwoD = new GameObject("2DB", typeof(CircleCollider2D));
            firstColliderTwoD = firstGameObjectForTwoD.GetComponent<Collider2D>();
            secondColliderTwoD = secondGameObjectForTwoD.GetComponent<Collider2D>();
        }

        protected GameObject firstGameObjectForThreeD;
        protected GameObject secondGameObjectForThreeD;
        protected Collider firstColliderThreeD;
        protected Collider secondColliderThreeD;
        protected GameObject firstGameObjectForTwoD;
        protected GameObject secondGameObjectForTwoD;
        protected Collider2D firstColliderTwoD;
        protected Collider2D secondColliderTwoD;

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(firstGameObjectForThreeD);
            UnityEngine.Object.DestroyImmediate(secondGameObjectForThreeD);
            UnityEngine.Object.DestroyImmediate(firstGameObjectForTwoD);
            UnityEngine.Object.DestroyImmediate(secondGameObjectForTwoD);
        }

        [Test]
        public void Collider3D_ValueAssignmentAndEvent()
        {
            var collVar = new ColliderThreeDMuscariable { Key = "col3D", ItemId = 40 };
            collVar.Init();

            Collider captured = null;
            collVar.OnValueChanged += passedToEvent => captured = passedToEvent;

            collVar.Value = firstColliderThreeD;
            Assert.AreEqual(firstColliderThreeD, collVar.Value);
            Assert.AreEqual(firstColliderThreeD, captured);
        }

        [Test]
        public void Collider3D_EqualityOperatorsAndEvaluate()
        {
            var firstCollVar = new ColliderThreeDMuscariable { Key = "a", ItemId = 41, Value = firstColliderThreeD };
            var secondCollVar = new ColliderThreeDMuscariable { Key = "b", ItemId = 42, Value = firstColliderThreeD };
            var thirdCollVar = new ColliderThreeDMuscariable { Key = "c", ItemId = 43, Value = secondColliderThreeD };

            bool firstEqualsSecond = firstCollVar.Evaluate(CompareOperator.Equals, secondCollVar.Value);
            Assert.IsTrue(firstEqualsSecond);
            Assert.IsFalse(firstCollVar != secondCollVar);
            Assert.IsFalse(firstCollVar == thirdCollVar);
            Assert.IsTrue(firstCollVar != thirdCollVar);

            Assert.IsTrue(firstCollVar.Evaluate(CompareOperator.Equals, firstColliderThreeD));
            Assert.IsFalse(firstCollVar.Evaluate(CompareOperator.Equals, secondColliderThreeD));

            // Rather than expecting an exception, let's expect an error log
            string expectedErrorMessage = $"CompareOperator GreaterThan not supported for {firstCollVar.ContentType.Name}";

            LogAssert.Expect(LogType.Error, expectedErrorMessage);
            firstCollVar.Evaluate(CompareOperator.GreaterThan, firstColliderThreeD);
        }

        [Test]
        public void Collider3D_DestroyedObjectBehavesAsNull()
        {
            var collVar = new ColliderThreeDMuscariable { Key = "col3D", ItemId = 44 };
            collVar.Init();

            collVar.Value = firstColliderThreeD;
            UnityEngine.Object.DestroyImmediate(firstColliderThreeD);
            Assert.IsTrue(collVar.Value == null);
        }

        [Test]
        public void Collider3D_WrongTypeAssignment_Throws()
        {
            var collVar = new ColliderThreeDMuscariable { Key = "col3D", ItemId = 45 };
            collVar.Init();
            Muscariable baseVar = collVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = "not a collider");
        }

        [Test]
        public void Collider2D_ValueAssignmentAndEvent()
        {
            var collVar = new ColliderTwoDMuscariable { Key = "col2D", ItemId = 41 };
            collVar.Init();

            Collider2D captured = null;
            collVar.OnValueChanged += c => captured = c;

            collVar.Value = firstColliderTwoD;
            Assert.AreEqual(firstColliderTwoD, collVar.Value);
            Assert.AreEqual(firstColliderTwoD, captured);
        }

        [Test]
        public void Collider2D_EqualityOperatorsAndEvaluate()
        {
            var firstCollVar = new ColliderTwoDMuscariable { Key = "a", ItemId = 41, Value = firstColliderTwoD };
            var secondCollVar = new ColliderTwoDMuscariable { Key = "b", ItemId = 42, Value = firstColliderTwoD };
            var thirdCollVar = new ColliderTwoDMuscariable { Key = "c", ItemId = 43, Value = secondColliderTwoD };

            bool firstEqualsSecond = firstCollVar.Evaluate(CompareOperator.Equals, secondCollVar.Value);
            Assert.IsTrue(firstEqualsSecond);

            bool firstEqualsThird = firstCollVar.Evaluate(CompareOperator.Equals, thirdCollVar.Value);
            Assert.IsFalse(firstEqualsThird);

            Assert.IsTrue(firstCollVar.Evaluate(CompareOperator.Equals, firstColliderTwoD));
            Assert.IsFalse(firstCollVar.Evaluate(CompareOperator.Equals, secondColliderTwoD));
        }

        [Test]
        public void Collider2D_DestroyedObjectBehavesAsNull()
        {
            var collVar = new ColliderTwoDMuscariable { Key = "col2D", ItemId = 44 };
            collVar.Init();

            collVar.Value = firstColliderTwoD;
            UnityEngine.Object.DestroyImmediate(firstColliderTwoD);
            Assert.IsTrue(collVar.Value == null);
        }

        [Test]
        public void Collider2D_WrongTypeAssignment_Throws()
        {
            var collVar = new ColliderTwoDMuscariable { Key = "col2D", ItemId = 45 };
            collVar.Init();
            Muscariable baseVar = collVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = 123);
        }
    }
}