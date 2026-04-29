using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using AtMycelia.Hyphlow;
using UnityObj = UnityEngine.Object;

namespace VScriptingTests.VariableOperations
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

            VariableTypeDiscovery.DiscoverAndRegister();

            _unityObjDataHolder = ScriptableObject.CreateInstance<UnityObjTestHolder>();
            _unityObjDataHolder.data = new ObjectData();

            _serializedObj = new SerializedObject(_unityObjDataHolder);
            _serializedObj.Update();

            var dataProp = _serializedObj.FindProperty("data");
            if (dataProp == null)
            {
                dataProp = _serializedObj.FindProperty("_data");
            }
            Assert.IsNotNull(dataProp, "Could not find '_data' property on holder.");

            _varRefProp = dataProp.FindPropertyRelative("_backingVarRef");
            Assert.IsNotNull(_varRefProp, "Could not find '_backingVarRef' property on data.");

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
        public void SelectingMuscariUnityObjVariable_AndResolvesValue(Type contentType)
        {
            UnityObjectMuscariable muscariVar = flowchart.AddNewMuscariable<UnityObj, UnityObjectMuscariable>("data");
            Assert.IsNotNull(muscariVar, $"VariableFactory did not create a variable for {contentType}");

            var testValue = CreateTestValue(contentType, fcHolder);
            if (testValue != null)
            {
                muscariVar.Value = testValue;
            }

            Assert.IsTrue(muscariVar.Value != null, "Muscari lost its value immediately after assignment.");
            if (testValue != null)
                Assert.AreSame(testValue, muscariVar.Value,
                    "Muscari value does not match test value right after assignment.");

            // Assign without VariableDataPropertyExtensions
            VariableReference varRef = _varRefProp.boxedValue as VariableReference;
            varRef.Variable = muscariVar;
            _varRefProp.boxedValue = varRef;
            _serializedObj.ApplyModifiedPropertiesWithoutUndo();
            _serializedObj.Update();

            var dataProp = _serializedObj.FindProperty("_data");
            if (dataProp == null)
            {
                dataProp = _serializedObj.FindProperty("data");
            }

            var boxed = dataProp?.boxedValue as VariableData;
            boxed?.Refresh();
            _serializedObj.ApplyModifiedPropertiesWithoutUndo();
            _serializedObj.Update();

            var resolved = _unityObjDataHolder.data.Value;
            Assert.IsTrue(resolved, "Resolved Unity object is destroyed (Unity null).");
            if (testValue != null)
                Assert.AreSame(testValue, resolved, "Resolved value does not match test value.");
        }

        private UnityObj CreateTestValue(Type contentType, GameObject go)
        {
            UnityObj result = null;

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
                var mat = new Material(shader) { hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild };
                _toDestroy.Add(mat);
                result = mat;
            }
            else if (contentType == typeof(Texture))
            {
                var tex = new Texture2D(2, 2) { hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild };
                _toDestroy.Add(tex);
                result = tex;
            }
            else if (contentType == typeof(Sprite))
            {
                var tex = new Texture2D(2, 2) { hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild };
                _toDestroy.Add(tex);
                var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
                sprite.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                _toDestroy.Add(sprite);
                result = sprite;
            }
            else if (contentType == typeof(AudioClip))
            {
                var clip = AudioClip.Create("test", 44100, 1, 44100, false);
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