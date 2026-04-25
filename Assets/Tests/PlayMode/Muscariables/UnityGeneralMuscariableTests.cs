using AtMycelia.Hyphlow;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;

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
        public void GameObjectMuscariable_ValueAssignmentAndEvent()
        {
            var goVar = new GameObjectMuscariable { Key = "go", ItemId = 32 };
            goVar.Init();

            GameObject captured = null;
            goVar.OnValueChanged += g => captured = g.BoxedValue as GameObject;

            goVar.Value = firstGameObject;
            Assert.AreEqual(firstGameObject, goVar.Value);
            Assert.AreEqual(firstGameObject, captured);
        }

        [Test]
        public void GameObjectMuscariable_GONameGetterAndSetter()
        {
            var goVar = new GameObjectMuscariable { Key = "go", ItemId = 33, Value = firstGameObject };
            goVar.Init(goVar.Value);

            // Getter
            Assert.AreEqual(firstGameObject.name, goVar.GOName);

            // Setter
            goVar.GOName = "Renamed";
            Assert.AreEqual("Renamed", firstGameObject.name);
            Assert.AreEqual("Renamed", goVar.GOName);
        }

        [Test]
        public void GameObjectMuscariable_IsDestroyedAndNameError()
        {
            var goVar = new GameObjectMuscariable { Key = "go", ItemId = 48, Value = firstGameObject };
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
            var firstGoVar = new GameObjectMuscariable { Key = "a", ItemId = 35, Value = firstGameObject };
            var secondGoVar = new GameObjectMuscariable { Key = "b", ItemId = 36, Value = firstGameObject };
            var thirdGoVar = new GameObjectMuscariable { Key = "c", ItemId = 37, Value = secondGameObject };

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
            var goVar = new GameObjectMuscariable { Key = "go", ItemId = 38 };
            goVar.Init();
            Muscariable baseVar = goVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = 123);
        }

        [Test]
        public void TransformMuscariable_ValueAssignmentAndEvent()
        {
            var transVar = new TransformMuscariable { Key = "tf", ItemId = 39 };
            transVar.Init();

            Transform captured = null;
            transVar.OnValueChanged += t => captured = t.BoxedValue as Transform;

            transVar.Value = firstTransform;
            Assert.AreEqual(firstTransform, transVar.Value);
            Assert.AreEqual(firstTransform, captured);
        }

        [Test]
        public void TransformMuscariable_EqualityAndEvaluate()
        {
            var firstTransVar = new TransformMuscariable { Key = "a", ItemId = 30, Value = firstTransform };
            var secondTransVar = new TransformMuscariable { Key = "b", ItemId = 31, Value = firstTransform };
            var thirdTransVar = new TransformMuscariable { Key = "c", ItemId = 32, Value = secondTransform };

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
            var transVar = new TransformMuscariable { Key = "tf", ItemId = 33 };
            transVar.Init();
            Muscariable baseVar = transVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = "not a transform");
        }

        [Test]
        public void UnityObjectMuscariable_ValueAssignmentAndEvent()
        {
            var unityObjVar = new UnityObjectMuscariable { Key = "uo", ItemId = 34 };
            unityObjVar.Init();

            UnityEngine.Object captured = null;
            unityObjVar.OnValueChanged += o => captured = o.BoxedValue as UnityObj;

            unityObjVar.Value = firstGameObject;  // GameObject is a UnityObject
            Assert.AreEqual(firstGameObject, unityObjVar.Value);
            Assert.AreEqual(firstGameObject, captured);
        }

        [Test]
        public void UnityObjectMuscariable_NullAndDestroyedBehavior()
        {
            var unityObjVar = new UnityObjectMuscariable { Key = "uo", ItemId = 35 };
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
            var firstUnityObjVar = new UnityObjectMuscariable { Key = "a", ItemId = 36, Value = firstGameObject };
            var secondUnityObjVar = new UnityObjectMuscariable { Key = "b", ItemId = 37, Value = firstGameObject };
            var thirdUnityObjVar = new UnityObjectMuscariable { Key = "c", ItemId = 38, Value = secondGameObject };

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
            var unityObjVar = new UnityObjectMuscariable { Key = "uo", ItemId = 31 };
            unityObjVar.Init();
            Muscariable baseVar = unityObjVar;
            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = 42);
        }
    }
}