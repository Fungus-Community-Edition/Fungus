using Amanita.VScripting;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;

namespace VScriptingTests.MuscariableTests.DataOnly
{
    [TestFixture]
    public class UnityGeneralMuscariableTests
    {
        [SetUp]
        public void SetUp()
        {
            // Create two GameObjects with distinct Transforms
            firstGameObject = new GameObject("A");
            secondGameObject = new GameObject("B");
            firstTransform = firstGameObject.transform;
            secondTransform = secondGameObject.transform;
        }

        protected GameObject firstGameObject;
        protected GameObject secondGameObject;
        protected Transform firstTransform;
        protected Transform secondTransform;

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(firstGameObject);
            UnityEngine.Object.DestroyImmediate(secondGameObject);
        }

        [Test]
        public void GameObjectMuscariable_InitRequiresKeyAndID()
        {
            var goVar = new GameObjectMuscariable();
            var ex = Assert.Throws<Exception>(() => goVar.Init());
            StringAssert.Contains("needs a valid key", ex.Message);
            StringAssert.Contains("needs a valid ID", ex.Message);

            goVar.Key = "go";
            goVar.ItemID = 301;
            Assert.DoesNotThrow(() => goVar.Init());
        }

        [Test]
        public void GameObjectMuscariable_ValueAssignmentAndEvent()
        {
            var goVar = new GameObjectMuscariable { Key = "go", ItemID = 302 };
            goVar.Init();

            GameObject captured = null;
            goVar.OnValueChanged += g => captured = g;

            goVar.Value = firstGameObject;
            Assert.AreEqual(firstGameObject, goVar.Value);
            Assert.AreEqual(firstGameObject, captured);
        }

        [Test]
        public void GameObjectMuscariable_GONameGetterAndSetter()
        {
            var goVar = new GameObjectMuscariable { Key = "go", ItemID = 303, Value = firstGameObject };
            goVar.Init();

            // Getter
            Assert.AreEqual("A", goVar.GOName);

            // Setter
            goVar.GOName = "Renamed";
            Assert.AreEqual("Renamed", firstGameObject.name);
            Assert.AreEqual("Renamed", goVar.GOName);
        }

        [Test]
        public void GameObjectMuscariable_IsDestroyedAndNameError()
        {
            var goVar = new GameObjectMuscariable { Key = "go", ItemID = 304, Value = firstGameObject };
            goVar.Init();

            // Destroy the underlying GameObject
            UnityEngine.Object.DestroyImmediate(firstGameObject);
            Assert.IsTrue(goVar.IsDestroyed);
            Assert.AreEqual(string.Empty, goVar.GOName);

            // Attempting to set name logs an error but does not throw
            LogAssert.Expect(LogType.Error,
                "Cannot change the name of a GameObject through a GameObjectVariable that has no GO assigned.");
            goVar.GOName = "NoOp";
        }

        [Test]
        public void GameObjectMuscariable_EqualityAndEvaluate()
        {
            var firstGoVar = new GameObjectMuscariable { Key = "a", ItemID = 305, Value = firstGameObject };
            var secondGoVar = new GameObjectMuscariable { Key = "b", ItemID = 306, Value = firstGameObject };
            var thirdGoVar = new GameObjectMuscariable { Key = "c", ItemID = 307, Value = secondGameObject };

            Assert.IsTrue(firstGoVar == secondGoVar);
            Assert.IsFalse(firstGoVar != secondGoVar);
            Assert.IsFalse(firstGoVar == thirdGoVar);
            Assert.IsTrue(firstGoVar != thirdGoVar);

            // Evaluate only supports Equals/NotEquals
            Assert.IsTrue(firstGoVar.Evaluate(CompareOperator.Equals, firstGameObject));
            Assert.IsFalse(firstGoVar.Evaluate(CompareOperator.Equals, secondGameObject));
            Assert.Throws<ArgumentException>(
                () => firstGoVar.Evaluate(CompareOperator.LessThan, firstGameObject)
            );
        }

        [Test]
        public void GameObjectMuscariable_WrongTypeAssignment_Throws()
        {
            var goVar = new GameObjectMuscariable { Key = "go", ItemID = 308 };
            goVar.Init();
            Muscariable baseVar = goVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = 123);
        }

        [Test]
        public void TransformMuscariable_ValueAssignmentAndEvent()
        {
            var transVar = new TransformMuscariable { Key = "tf", ItemID = 309 };
            transVar.Init();

            Transform captured = null;
            transVar.OnValueChanged += t => captured = t;

            transVar.Value = firstTransform;
            Assert.AreEqual(firstTransform, transVar.Value);
            Assert.AreEqual(firstTransform, captured);
        }

        [Test]
        public void TransformMuscariable_EqualityAndEvaluate()
        {
            var firstTransVar = new TransformMuscariable { Key = "a", ItemID = 310, Value = firstTransform };
            var secondTransVar = new TransformMuscariable { Key = "b", ItemID = 311, Value = firstTransform };
            var thirdTransVar = new TransformMuscariable { Key = "c", ItemID = 312, Value = secondTransform };

            Assert.IsTrue(firstTransVar == secondTransVar);
            Assert.IsFalse(firstTransVar != secondTransVar);
            Assert.IsFalse(firstTransVar == thirdTransVar);
            Assert.IsTrue(firstTransVar != thirdTransVar);

            Assert.IsTrue(firstTransVar.Evaluate(CompareOperator.Equals, firstTransform));
            Assert.IsFalse(firstTransVar.Evaluate(CompareOperator.Equals, secondTransform));
            Assert.Throws<ArgumentException>(
                () => firstTransVar.Evaluate(CompareOperator.LessThan, firstTransform)
            );
        }

        [Test]
        public void TransformMuscariable_WrongTypeAssignment_Throws()
        {
            var transVar = new TransformMuscariable { Key = "tf", ItemID = 313 };
            transVar.Init();
            Muscariable baseVar = transVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = "not a transform");
        }

        [Test]
        public void UnityObjectMuscariable_ValueAssignmentAndEvent()
        {
            var unityObjVar = new UnityObjectMuscariable { Key = "uo", ItemID = 314 };
            unityObjVar.Init();

            UnityEngine.Object captured = null;
            unityObjVar.OnValueChanged += o => captured = o;

            unityObjVar.Value = firstGameObject;  // GameObject is a UnityObject
            Assert.AreEqual(firstGameObject, unityObjVar.Value);
            Assert.AreEqual(firstGameObject, captured);
        }

        [Test]
        public void UnityObjectMuscariable_NullAndDestroyedBehavior()
        {
            var unityObjVar = new UnityObjectMuscariable { Key = "uo", ItemID = 315 };
            unityObjVar.Init();

            unityObjVar.Value = null;
            Assert.IsNull(unityObjVar.Value);

            unityObjVar.Value = secondGameObject;
            UnityEngine.Object.DestroyImmediate(secondGameObject);
            Assert.IsTrue(unityObjVar.Value == null);
        }

        [Test]
        public void UnityObjectMuscariable_EqualityAndEvaluate()
        {
            var firstUnityObjVar = new UnityObjectMuscariable { Key = "a", ItemID = 316, Value = firstGameObject };
            var secondUnityObjVar = new UnityObjectMuscariable { Key = "b", ItemID = 317, Value = firstGameObject };
            var thirdUnityObjVar = new UnityObjectMuscariable { Key = "c", ItemID = 318, Value = secondGameObject };

            Assert.IsTrue(firstUnityObjVar == secondUnityObjVar);
            Assert.IsFalse(firstUnityObjVar != secondUnityObjVar);
            Assert.IsFalse(firstUnityObjVar == thirdUnityObjVar);
            Assert.IsTrue(firstUnityObjVar != thirdUnityObjVar);

            Assert.IsTrue(firstUnityObjVar.Evaluate(CompareOperator.Equals, firstGameObject));
            Assert.IsFalse(firstUnityObjVar.Evaluate(CompareOperator.Equals, secondGameObject));
            Assert.Throws<ArgumentException>(
                () => firstUnityObjVar.Evaluate(CompareOperator.GreaterThan, firstGameObject)
            );
        }

        [Test]
        public void UnityObjectMuscariable_WrongTypeAssignment_Throws()
        {
            var unityObjVar = new UnityObjectMuscariable { Key = "uo", ItemID = 319 };
            unityObjVar.Init();
            Muscariable baseVar = unityObjVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = 42);
        }
    }
}