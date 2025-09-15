using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Amanita.VScripting;
using System.Collections.Generic; // adjust namespace to match your project
using UnityObj = UnityEngine.Object;

namespace Amanita.Tests.EditMode
{
    public class VariableDataEditorTests
    {
        private SerializedObject _serializedObject;
        private SerializedProperty _varRefProp;
        private IntegerData _testData;

        [SetUp]
        public void SetUp()
        {
            // Create a dummy ScriptableObject to hold our VariableData for serialization
            var holder = ScriptableObject.CreateInstance<TestVariableDataHolder>();
            _testData = holder.data;
            _serializedObject = new SerializedObject(holder);
            _varRefProp = _serializedObject.FindProperty("data.varRef");
            manager = AmanitaManager.EnsureExists();
            toDestroyInTearDown.Add(manager.gameObject);
        }

        protected AmanitaManager manager;

        protected IList<UnityObj> toDestroyInTearDown = new List<UnityObj>();

        [TearDown]
        public virtual void TearDown()
        {
            if (_serializedObject != null)
            {
                UnityObj.DestroyImmediate(_serializedObject.targetObject);
                _serializedObject = null;
                _varRefProp = null;
                _testData = null;
            }

            foreach (var obj in toDestroyInTearDown)
            {
                if (obj != null)
                {
                    UnityObj.DestroyImmediate(obj);
                }
            }

            manager = null;
            toDestroyInTearDown.Clear();
        }

        [Test]
        public void AssignVarRef_WithNull_ClearsReference()
        {
            _varRefProp.AssignVarRef(null, typeof(int));
            _serializedObject.ApplyModifiedProperties();

            Assert.IsTrue(_varRefProp.managedReferenceValue == null);
        }

        [Test]
        public void AssignVarRef_WithPureData_AssignsDirectly()
        {
            var pureVar = new IntMuscariable();
            pureVar.Value = 42;
            _varRefProp.AssignVarRef(pureVar, pureVar.ContentType);
            _serializedObject.ApplyModifiedProperties();

            Assert.AreSame(pureVar, _varRefProp.managedReferenceValue);
            
        }

        [Test]
        public void AssignVarRef_WithUnityObject_WrapsInPointer()
        {
            var monoVar = new GameObject("Var").AddComponent<AudioClipVariable>();
            toDestroyInTearDown.Add(monoVar.gameObject);
            _varRefProp.AssignVarRef(monoVar, monoVar.ContentType);
            _serializedObject.ApplyModifiedProperties();

            var wrapper = _varRefProp.managedReferenceValue;
            Assert.IsInstanceOf(typeof(VariablePointer<AudioClip>), wrapper);

            var pointer = (VariablePointer<AudioClip>)wrapper;
            Assert.AreSame(monoVar, pointer.Component);
        }

        [Test]
        public void VariablePointer_ForwardsValueGetSet()
        {
            var monoVar = new GameObject("Var").AddComponent<IntegerVariable>();
            toDestroyInTearDown.Add(monoVar.gameObject);
            monoVar.Value = 5;

            var pointer = new VariablePointer<int>(monoVar);
            Assert.AreEqual(5, pointer.Value);

            pointer.Value = 99;
            Assert.AreEqual(99, monoVar.Value);
        }

        [Test]
        public void VariablePointer_ForwardsKeyAndScope()
        {
            var monoVar = new GameObject("Var").AddComponent<IntegerVariable>();
            toDestroyInTearDown.Add(monoVar.gameObject);
            monoVar.Key = "TestKey";
            monoVar.Scope = VariableScope.Public;

            var pointer = new VariablePointer<int>(monoVar);
            Assert.AreEqual("TestKey", pointer.Key);
            Assert.AreEqual(VariableScope.Public, pointer.Scope);
        }

        // --- Test helpers ---
        // We already have concrete implementations of some things, and thus we won't need stubs
        [Serializable]
        private class TestVariableDataHolder : ScriptableObject
        {
            public IntegerData data = new IntegerData();
        }


    }
}