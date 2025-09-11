using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Amanita.VScripting;

namespace Amanita.Tests.EditMode
{
    public class VariableDataIntegrationTests : VariableTests
    {
        private SerializedObject _serializedObj;
        private SerializedProperty _varRefProp;
        private UnityObjTestHolder _unityObjDataHolder;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _unityObjDataHolder = ScriptableObject.CreateInstance<UnityObjTestHolder>();
            _unityObjDataHolder.data = new ObjectData { };

            _serializedObj = new SerializedObject(_unityObjDataHolder);
            _serializedObj.Update();

            var dataProp = _serializedObj.FindProperty("data");
            Assert.IsNotNull(dataProp, "Could not find 'data' property on holder.");

            _varRefProp = dataProp.FindPropertyRelative("varRef");
            Assert.IsNotNull(_varRefProp, "Could not find 'varRef' property on data.");

            _toDestroy.Add(_unityObjDataHolder);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [TestCase(typeof(Animator))]
        [TestCase(typeof(Sprite))]
        [TestCase(typeof(GameObject))]
        [TestCase(typeof(Transform))]
        [TestCase(typeof(Material))]
        [TestCase(typeof(Texture))]
        [TestCase(typeof(AudioClip))]
        public void SelectingLegacyUnityObjVariable_WrapsInPointer_AndResolvesValue(Type contentType)
        {
            var legacyVar = VariableFactory.AddLegacyVarTo(flowchart, contentType);
            Assert.IsNotNull(legacyVar, $"VariableFactory did not create a variable for {contentType}");
            _toDestroy.Add(legacyVar);

            // Create a test value and assign to the legacy variable
            var testValue = CreateTestValue(contentType, fcHolder);
            if (testValue != null)
            {
                legacyVar.Value = testValue;
                Debug.Log($"Legacy var instance Id is {legacyVar.GetInstanceID()}, test value instance Id is {testValue.GetInstanceID()}");
            }

            // quick sanity-check: make sure the legacy var still holds the test value immediately
            Assert.IsTrue(legacyVar.Value != null, "Legacy variable lost its value immediately after assignment.");
            if (testValue != null)
                Assert.AreSame(testValue, legacyVar.Value, "Legacy variable value does not match test value right after assignment.");

            // Assign via simulated popup selection
            _varRefProp.AssignVarRef(legacyVar, contentType);
            // Use WithoutUndo to avoid asset/save-like serialization that strips scene refs
            _serializedObj.ApplyModifiedPropertiesWithoutUndo();
            _serializedObj.Update();

            // Assert wrapper type
            var wrapper = _varRefProp.managedReferenceValue;
            Assert.IsNotNull(wrapper, "Managed reference is null after assignment.");
            Assert.AreEqual(typeof(VariablePointer<>).MakeGenericType(contentType), wrapper.GetType());

            // Probe pointer's _component
            var compField = wrapper.GetType().GetField("_component", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var comp = (UnityEngine.Object)compField.GetValue(wrapper);
            Assert.IsTrue(comp, "Pointer _component is destroyed (Unity null).");
            Assert.AreSame((UnityEngine.Object)legacyVar, comp, "Pointer is not wrapping the selected variable component.");

            // Round-trip before reading Value
            // Keep using WithoutUndo to avoid serialization that strips scene refs
            _serializedObj.ApplyModifiedPropertiesWithoutUndo();
            _serializedObj.Update();

            // --- NEW: make sure the variable-data instance _synchronizes_ its runtime fields ---
            var dataProp = _serializedObj.FindProperty("data");
            var boxed = dataProp?.boxedValue as VariableData;
            boxed?.Refresh(); // derived types (ObjectData, AudioClipData, etc) should sync derived/backing fields here
            // update serialized object after refresh in case it modifies anything
            _serializedObj.ApplyModifiedPropertiesWithoutUndo();
            _serializedObj.Update();

            var resolved = _unityObjDataHolder.data.Value as UnityEngine.Object;
            Debug.Log("Get instance ID for resolved: " + (resolved != null ? resolved.GetInstanceID().ToString() : "null"));
            Assert.IsTrue(resolved, "Resolved Unity object is destroyed (Unity null).");
            if (testValue != null)
                Assert.AreSame(testValue, resolved, "Resolved value does not match test value.");
        }

        // --- Helpers ---

        private UnityEngine.Object CreateTestValue(Type contentType, GameObject go)
        {
            UnityEngine.Object result = null;

            if (contentType == typeof(GameObject))
            {
                result = go;
            }
            else if (contentType == typeof(Transform))
            {
                result = go.transform;
            }
            else if (contentType == typeof(Material))
            {
                var shader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
                var mat = new Material(shader);
                mat.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                _toDestroy.Add(mat);
                result = mat;
            }
            else if (contentType == typeof(Texture))
            {
                var tex = new Texture2D(2, 2);
                tex.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                _toDestroy.Add(tex);
                result = tex;
            }
            else if (contentType == typeof(Sprite))
            {
                var tex = new Texture2D(2, 2);
                tex.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                _toDestroy.Add(tex);

                var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
                sprite.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                _toDestroy.Add(sprite);
                result = sprite;
            }
            else if (contentType == typeof(AudioClip))
            {
                var clip = AudioClip.Create("test", 44100, 1, 44100, false);
                //clip.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                clip.hideFlags = HideFlags.DontSaveInBuild;
                _toDestroy.Add(clip);
                result = clip;
            }
            else if (contentType == typeof(Animator))
            {
                var animator = go.AddComponent<Animator>();
                result = animator;
            }

            return result;
        }

        [Serializable]
        private class UnityObjTestHolder : ScriptableObject
        {
            public ObjectData data;
        }
    }
}