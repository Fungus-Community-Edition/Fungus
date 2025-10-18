using System;
using NUnit.Framework;
using UnityEngine;
using Amanita.VScripting;

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
            var collVar = new ColliderMuscariableThreeD { Key = "col3D", ItemID = 400 };
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
            var firstCollVar = new ColliderMuscariableThreeD { Key = "a", ItemID = 401, Value = firstColliderThreeD };
            var secondCollVar = new ColliderMuscariableThreeD { Key = "b", ItemID = 402, Value = firstColliderThreeD };
            var thirdCollVar = new ColliderMuscariableThreeD { Key = "c", ItemID = 403, Value = secondColliderThreeD };

            Assert.IsTrue(firstCollVar == secondCollVar);
            Assert.IsFalse(firstCollVar != secondCollVar);
            Assert.IsFalse(firstCollVar == thirdCollVar);
            Assert.IsTrue(firstCollVar != thirdCollVar);

            Assert.IsTrue(firstCollVar.Evaluate(CompareOperator.Equals, firstColliderThreeD));
            Assert.IsFalse(firstCollVar.Evaluate(CompareOperator.Equals, secondColliderThreeD));

            Assert.Throws<ArgumentException>(
                () => firstCollVar.Evaluate(CompareOperator.GreaterThan, firstColliderThreeD)
            );
        }

        [Test]
        public void Collider3D_DestroyedObjectBehavesAsNull()
        {
            var collVar = new ColliderMuscariableThreeD { Key = "col3D", ItemID = 404 };
            collVar.Init();

            collVar.Value = firstColliderThreeD;
            UnityEngine.Object.DestroyImmediate(firstColliderThreeD);
            Assert.IsTrue(collVar.Value == null);
        }

        [Test]
        public void Collider3D_WrongTypeAssignment_Throws()
        {
            var collVar = new ColliderMuscariableThreeD { Key = "col3D", ItemID = 405 };
            collVar.Init();
            Muscariable baseVar = collVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = "not a collider");
        }

        [Test]
        public void Collider2D_ValueAssignmentAndEvent()
        {
            var collVar = new ColliderMuscariableTwoD { Key = "col2D", ItemID = 410 };
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
            var firstCollVar = new ColliderMuscariableTwoD { Key = "a", ItemID = 411, Value = firstColliderTwoD };
            var secondCollVar = new ColliderMuscariableTwoD { Key = "b", ItemID = 412, Value = firstColliderTwoD };
            var thirdCollVar = new ColliderMuscariableTwoD { Key = "c", ItemID = 413, Value = secondColliderTwoD };

            Assert.IsTrue(firstCollVar == secondCollVar);
            Assert.IsFalse(firstCollVar != secondCollVar);
            Assert.IsFalse(firstCollVar == thirdCollVar);
            Assert.IsTrue(firstCollVar != thirdCollVar);

            Assert.IsTrue(firstCollVar.Evaluate(CompareOperator.Equals, firstColliderTwoD));
            Assert.IsFalse(firstCollVar.Evaluate(CompareOperator.Equals, secondColliderTwoD));
        }

        [Test]
        public void Collider2D_DestroyedObjectBehavesAsNull()
        {
            var collVar = new ColliderMuscariableTwoD { Key = "col2D", ItemID = 414 };
            collVar.Init();

            collVar.Value = firstColliderTwoD;
            UnityEngine.Object.DestroyImmediate(firstColliderTwoD);
            Assert.IsTrue(collVar.Value == null);
        }

        [Test]
        public void Collider2D_WrongTypeAssignment_Throws()
        {
            var collVar = new ColliderMuscariableTwoD { Key = "col2D", ItemID = 415 };
            collVar.Init();
            Muscariable baseVar = collVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = 123);
        }
    }
}