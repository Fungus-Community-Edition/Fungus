using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using UnityObject = UnityEngine.Object;
using Amanita;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.Audio;
using Lorekeeper;
using System.Linq;

namespace VScriptingTests.VariableRows
{
    [TestFixture]
    public class VariableRowPersistenceTests
    {
        private Flowchart _flowchart;
        private AmanitaManager _amanitaManager;
        private VariableRowManager _rowManager;
        private VariableListView _variableListView;
        private VariableRowFactory _rowFactory;
        private VariableRowPool _rowPool;
        private RowVisualHandlerPool _handlerPool;
        private List<UnityObject> _objectsToDestroy;

        private IRowVisualTemplateProvider _originalTemplateProvider;
        private IRowVisualElementBuilder _originalElementBuilder;

        private class TestHostWindow : EditorWindow { }

        private TestHostWindow _uiHost;
        private VisualElement _uiRoot;
        private Button _addButton;

        [SetUp]
        public void SetUp()
        {
            Undo.ClearAll();
            Flowchart.ResetStaticsForTest();

            _objectsToDestroy = new List<UnityObject>();

            _originalTemplateProvider = RowVisualTemplateProviderRegistry.Current;
            _originalElementBuilder = RowVisualElementBuilderRegistry.Current;

            //RowVisualElementBuilderRegistry.Current = new TestRowVisualElementBuilder();

            _amanitaManager = AmanitaManager.EnsureExists();
            _objectsToDestroy.Add(_amanitaManager.gameObject);

            var flowchartGO = new GameObject("TestFlowchart");
            _objectsToDestroy.Add(flowchartGO);
            _flowchart = flowchartGO.AddComponent<Flowchart>();
            _flowchart.AlwaysKeepGuid = false;

            _uiHost = ScriptableObject.CreateInstance<TestHostWindow>();
            InitializeVariableUi();
            _uiHost.rootVisualElement.Add(_uiRoot);
            _uiHost.ShowAuxWindow();
            _uiHost.rootVisualElement.schedule.Execute(() => { }).ExecuteLater(0);
        }

        private void InitializeVariableUi()
        {
            _rowPool = new VariableRowPool();
            _handlerPool = new RowVisualHandlerPool(new RowVisualHandlerResolver(), BuildHandlerLookup());

            _rowFactory = new VariableRowFactory();
            _rowFactory.Init(new VariableRowFactoryInitArgs
            {
                RowPool = _rowPool,
                HandlerPool = _handlerPool
            });

            var listView = new ListView();
            var countLabel = new UitkLabel();

            _variableListView = new VariableListView(new VariableListViewInitArgs
            {
                List = listView,
                CountLabel = countLabel,
                RowFactory = _rowFactory,
                VariableSource = _flowchart
            });

            _variableListView.SetFlowchart(_flowchart);

            _uiRoot = new VisualElement { name = "VariableRowTestsRoot" };
            _addButton = new Button { name = "TestAddButton" };
            _uiRoot.Add(_addButton);
            _uiRoot.Add(listView);
            _uiRoot.Add(countLabel);

            _rowManager = new VariableRowManager();
            _rowManager.Init(new VRowManagerInitArgs
            {
                Root = _uiRoot,
                AddButton = _addButton,
                VariableSource = _flowchart,
                VariableListView = _variableListView
            });
        }

        [TearDown]
        public void TearDown()
        {
            _rowManager?.Dispose();
            _variableListView?.Dispose();
            _rowFactory?.Dispose();

            RowVisualTemplateProviderRegistry.Current = _originalTemplateProvider;
            RowVisualElementBuilderRegistry.Current = _originalElementBuilder;

            for (int i = _objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (_objectsToDestroy[i] != null)
                {
                    UnityObject.DestroyImmediate(_objectsToDestroy[i]);
                }
            }

            _objectsToDestroy.Clear();
            Undo.ClearAll();
            _uiHost.Close();
        }

        [UnityTest]
        public IEnumerator VariableRowChange_PersistsAndSupportsUndoRedo(
            [ValueSource(nameof(VariableRowCases))] VariableRowTestCase testCase)
        {
            var variable = testCase.CreateVariable(_flowchart);
            yield return AssertValueChangePersists(variable, testCase.TargetValue, variable.ContentType);
        }

        private IEnumerator AssertValueChangePersists(IVariable variable, object newValue, Type contentType)
        {
            yield return null;
            Assert.NotNull(variable, "Variable creation failed.");

            VariableRow row = GetRowFor(variable);
            Assert.NotNull(row, "Variable row could not be materialized.");

            object originalValue = variable.BoxedValue;

            ApplyValueThroughUi(row, newValue, contentType);
            yield return null;
            Assert.AreEqual(newValue, variable.BoxedValue, "Value change was not applied.");

            Undo.PerformUndo();
            Assert.AreEqual(originalValue, variable.BoxedValue, "Undo did not restore the original value.");

            Undo.PerformRedo();
            Assert.AreEqual(newValue, variable.BoxedValue, "Redo did not reapply the edited value.");
        }

        private VariableRow GetRowFor(IVariable variable)
        {
            _variableListView.ForceMaterializeAllRowsForTests();

            int index = FindVariableIndex(variable);
            Assert.GreaterOrEqual(index, 0, "Variable was not found in the list view.");

            return _variableListView.RowAtIndex(index);
        }

        private int FindVariableIndex(IVariable variable)
        {
            var vars = _variableListView.VarsToDisplay;
            for (int i = 0; i < vars.Count; i++)
            {
                if (ReferenceEquals(vars[i], variable))
                {
                    return i;
                }
            }

            return -1;
        }

        private static readonly IReadOnlyDictionary<Type, Action<VisualElement, object>> valueAppliers =
            new Dictionary<Type, Action<VisualElement, object>>
            {
                #region Numerics
                {
                    typeof(float),
                    (element, boxed) =>
                    {
                        if (element is not INotifyValueChanged<float> floatField)
                        {
                            throw new InvalidOperationException("ValueField is not a float field.");
                        }

                        floatField.value = (float)boxed;
                    }
                },
                
                {
                    typeof(int),
                    (element, boxed) =>
                    {
                        if (element is not INotifyValueChanged<int> intField)
                        {
                            throw new InvalidOperationException("ValueField is not an int field.");
                        }

                        intField.value = (int)boxed;
                    }
                },
                {
                    typeof(Vector2),
                    (element, boxed) =>
                    {
                        if (element is not INotifyValueChanged<Vector2> vector2Field)
                        {
                            throw new InvalidOperationException("ValueField is not a Vector2 field.");
                        }
                        vector2Field.value = (Vector2)boxed;
                    }
                },
                {
                    typeof(Vector3),
                    (element, boxed) =>
                    {
                        if (element is not INotifyValueChanged<Vector3> vector3Field)
                        {
                            throw new InvalidOperationException("ValueField is not a Vector3 field.");
                        }
                        vector3Field.value = (Vector3)boxed;
                    }
                },
                #endregion

                #region Graphics
                {
                    typeof(string),
                    (element, boxed) =>
                    {
                        if (element is not INotifyValueChanged<string> stringField)
                        {
                            throw new InvalidOperationException("ValueField is not a string field.");
                        }

                        stringField.value = (string)boxed;
                    }
                },
                {
                    typeof(Color),
                    (element, boxed) =>
                    {
                        if (element is not INotifyValueChanged<Color> colorField)
                        {
                            throw new InvalidOperationException("ValueField is not a color field.");
                        }

                        colorField.value = (Color)boxed;
                    }
                },
                {
                    typeof(Sprite),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(Sprite))
                        {
                            throw new InvalidOperationException("ValueField is not a Sprite field.");
                        }
                        objField.value = (Sprite)boxed;
                    }
                },
                {
                    typeof(Texture),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(Texture))
                        {
                            throw new InvalidOperationException("ValueField is not a Texture field.");
                        }
                        objField.value = (Texture)boxed;
                    }
                },
                {
                    typeof(Material),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(Material))
                        {
                            throw new InvalidOperationException("ValueField is not a Material field.");
                        }
                        objField.value = (Material)boxed;
                    }
                },
                {
                    typeof(Animator),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(Animator))
                        {
                            throw new InvalidOperationException("ValueField is not an Animator field.");
                        }
                        objField.value = (Animator)boxed;
                    }
                },
                #endregion

                #region Audio
                {
                    typeof(AudioClip),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(AudioClip))
                        {
                            throw new InvalidOperationException("ValueField is not an AudioClip field.");
                        }
                        objField.value = (AudioClip)boxed;
                    }
                },
                {
                    typeof(AudioSource),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(AudioSource))
                        {
                            throw new InvalidOperationException("ValueField is not an AudioSource field.");
                        }
                        objField.value = (AudioSource)boxed;
                    }
                },
                {
                    typeof(AudioMixer),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(AudioMixer))
                        {
                            throw new InvalidOperationException("ValueField is not an AudioMixer field.");
                        }
                        objField.value = (AudioMixer)boxed;
                    }
                },
                #endregion

                #region Physics
                {
                    typeof(Collider),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(Collider))
                        {
                            throw new InvalidOperationException("ValueField is not a Collider field.");
                        }
                        objField.value = (Collider)boxed;
                    }
                },
                {
                    typeof(Rigidbody),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(Rigidbody))
                        {
                            throw new InvalidOperationException("ValueField is not a Rigidbody field.");
                        }
                        objField.value = (Rigidbody)boxed;
                    }
                },
                {
                    typeof(Collider2D),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(Collider2D))
                        {
                            throw new InvalidOperationException("ValueField is not a Collider2D field.");
                        }
                        objField.value = (Collider2D)boxed;
                    }
                },
                {
                    typeof(Rigidbody2D),
                    (element, boxed) =>
                    {
                        ObjectField objField = element as ObjectField;
                        if (objField == null || objField.objectType != typeof(Rigidbody2D))
                        {
                            throw new InvalidOperationException("ValueField is not a Rigidbody2D field.");
                        }
                        objField.value = (Rigidbody2D)boxed;
                    }
                }
                #endregion
            };

        private static void ApplyValueThroughUi(VariableRow row, object newValue, Type contentType)
        {
            VisualElement valueElement = row.RootElement?.Q("ValueField");
            Assert.NotNull(valueElement, "Variable row is missing a ValueField element.");

            if (!valueAppliers.TryGetValue(contentType, out var applyValue))
            {
                Assert.Fail($"Unsupported value field type '{valueElement.GetType().Name}' for " +
                    $"content type '{contentType.Name}'.");
                return;
            }

            applyValue(valueElement, newValue);
        }

        private IDictionary<Type, Type> BuildHandlerLookup()
        {
            return new Dictionary<Type, Type>
            {
                #region Numeric
                { typeof(float), typeof(FloatRowVisualHandler) },
                { typeof(Color), typeof(ColorRowVisualHandler) },
                { typeof(int), typeof(IntRowVisualHandler) },
                { typeof(Vector2), typeof(VectorTwoRowVisualHandler) },
                { typeof(Vector3), typeof(VectorThreeRowVisualHandler) },
                #endregion

                #region Audio
                { typeof(AudioClip), typeof(AudioClipRowVisualHandler) },
                { typeof(AudioSource), typeof(AudioSourceRowVisualHandler) },
                { typeof(AudioMixer), typeof(AudioMixerRowVisualHandler) },
                #endregion

                #region Graphics
                { typeof(string), typeof(StringRowVisualHandler) },
                { typeof(Texture), typeof(TextureRowVisualHandler) },
                { typeof(Material), typeof(MaterialRowVisualHandler) },
                { typeof(Sprite), typeof(SpriteRowVisualHandler) },
                { typeof(Animator), typeof(AnimatorRowVisualHandler) },
                #endregion

                #region Physics
                { typeof(Rigidbody), typeof(RigidbodyThreeDRowVisualHandler) }, 
                { typeof(Collider), typeof(ColliderThreeDRowVisualHandler) },
                { typeof(Rigidbody2D), typeof(RigidbodyTwoDRowVisualHandler) },
                { typeof(Collider2D), typeof(ColliderTwoDRowVisualHandler) },
                #endregion

                #region Etc
                #endregion

            };
        }

        private static IEnumerable<VariableRowTestCase> VariableRowCases()
        {
            ShadowDatabase db = AmanitaManager.ShadowDB; // We will need this for some cases
            var gameObjects = db.GetAssetsOfType<GameObject>(AssetType.Prefab);

            #region Numeric
            yield return new VariableRowTestCase(
                "FloatVariable",
                fc => fc.AddNewMuscariable<float, FloatMuscariable>("FloatVar", 1f),
                12.5f);

            yield return new VariableRowTestCase(
                "IntVariable",
                fc => fc.AddNewMuscariable<int, IntMuscariable>("IntVar", 10),
                42);

            yield return new VariableRowTestCase(
                "VectorTwoVariable",
                fc => fc.AddNewMuscariable<Vector2, VectorTwoMuscariable>("Vector2Var", new Vector2(1, 2)),
                new Vector2(3, 4));

            yield return new VariableRowTestCase(
                "VectorThreeVariable",
                fc => fc.AddNewMuscariable<Vector3, VectorThreeMuscariable>("Vector3Var", new Vector3(1, 2, 3)),
                new Vector3(4, 5, 6));
            #endregion

            #region Graphics
            yield return new VariableRowTestCase(
                "StringVariable",
                fc => fc.AddNewMuscariable<string, StringMuscariable>("StringVar", "Hello"),
                "World");

            yield return new VariableRowTestCase(
                "ColorVariable",
                fc => fc.AddNewMuscariable<Color, ColorMuscariable>("ColorVar", Color.red),
                new Color(0.1f, 0.4f, 0.9f, 0.5f));

            var sprites = db.GetAssetsOfType<Sprite>(AssetType.Sprite);
            Sprite testSprite = sprites.Count > 0 ?
                sprites[0] :
                null;
            yield return new VariableRowTestCase(
                "SpriteVariable",
                fc => fc.AddNewMuscariable<Sprite, SpriteMuscariable>("SpriteVar", testSprite),
                testSprite);

            var textures = db.GetAssetsOfType<Texture>(AssetType.Texture);
            Texture testTexture = textures.Count > 0 ?
                textures[0] :
                null;
            yield return new VariableRowTestCase(
                "TextureVariable",
                fc => fc.AddNewMuscariable<Texture, TextureMuscariable>("TextureVar", testTexture),
                testTexture);

            var materials = db.GetAssetsOfType<Material>(AssetType.Material);
            Material testMaterial = materials.Count > 0 ?
                materials[0] :
                null;
            yield return new VariableRowTestCase(
                "MaterialVariable",
                fc => fc.AddNewMuscariable<Material, MaterialMuscariable>("MaterialVar", testMaterial),
                testMaterial);

            var animators = db.GetAssetsOfType<Animator>(AssetType.AnimatorController);
            Animator testAnimator = animators.Count > 0 ?
                animators[0] :
                null;
            yield return new VariableRowTestCase(
                "AnimatorVariable",
                fc => fc.AddNewMuscariable<Animator, AnimatorMuscariable>("AnimatorVar", testAnimator),
                testAnimator);
            #endregion

            #region Audio
            var audioClips = db.GetAssetsOfType<AudioClip>(AssetType.AudioClip);
            AudioClip testClip = audioClips.Count > 0 ? 
                audioClips[0] : 
                null;
            yield return new VariableRowTestCase(
                "AudioClipVariable",
                fc => fc.AddNewMuscariable<AudioClip, AudioClipMuscariable>("AudioClipVar", testClip),
                testClip);

            var audioSources = gameObjects.Select((elem) => elem.GetComponent<AudioSource>())
                .Where((elem) => elem != null)
                .ToArray();
            AudioSource testSource = audioSources.Any() ?
                audioSources.First().GetComponent<AudioSource>() :
                null;
            yield return new VariableRowTestCase(
                "AudioSourceVariable",
                fc => fc.AddNewMuscariable<AudioSource, AudioSourceMuscariable>("AudioSourceVar", testSource),
                testSource);

            var audioMixers = db.GetAssetsOfType<AudioMixer>(AssetType.AudioMixer);
            AudioMixer testMixer = audioMixers.Count > 0 ?
                audioMixers[0] :
                null;
            yield return new VariableRowTestCase(
                "AudioMixerVariable",
                fc => fc.AddNewMuscariable<AudioMixer, AudioMixerMuscariable>("AudioMixerVar", testMixer),
                testMixer);
            #endregion

            #region Physics
            var rigidbodies = gameObjects.Where((elem) => elem.GetComponent<Rigidbody>() != null)
                .Select((elem) => elem.GetComponent<Rigidbody>())
                .ToArray();
            Rigidbody testRigidbody = rigidbodies.Length > 0 ?
                rigidbodies[0] :
                null;
            yield return new VariableRowTestCase(
                "RigidbodyVariable",
                fc => fc.AddNewMuscariable<Rigidbody, RigidbodyThreeDMuscariable>("RigidbodyVar", testRigidbody),
                testRigidbody);
            var colliders = gameObjects.Where((elem) => elem.GetComponent<Collider>() != null)
                .Select((elem) => elem.GetComponent<Collider>())
                .ToArray();
            Collider testCollider = colliders.Length > 0 ?
                colliders[0] :
                null;
            yield return new VariableRowTestCase(
                "ColliderVariable",
                fc => fc.AddNewMuscariable<Collider, ColliderThreeDMuscariable>("ColliderVar", testCollider),
                testCollider);
            #endregion



        }

        public sealed class VariableRowTestCase
        {
            public VariableRowTestCase(string name, Func<Flowchart, IVariable> createVariable, object targetValue)
            {
                Name = name;
                CreateVariable = createVariable;
                TargetValue = targetValue;
            }

            public string Name { get; }
            public Func<Flowchart, IVariable> CreateVariable { get; }
            public object TargetValue { get; }
            public override string ToString() => Name;
            public Type ContentType => TargetValue.GetType();
        }

        private sealed class HandlerAwareVisualTreeAsset : VisualTreeAsset
        {
            public Type HandlerType;
        }

        private sealed class TestRowVisualElementBuilder : IRowVisualElementBuilder
        {
            public RowVisualElements Build(VisualTreeAsset template)
            {
                var handlerAware = template as HandlerAwareVisualTreeAsset;

                var root = new VisualElement();
                var keyField = new TextField { isDelayed = true, multiline = false, name = "KeyInput" };
                var valueHolder = new VisualElement { name = "ValueFieldHolder" };
                var scopeField = new EnumField(VariableScope.Private) { name = "Scope" };
                var removeButton = new Button { name = "RemoveButton" };

                IBindable valueField = CreateValueField(handlerAware?.HandlerType);
                if (valueField is VisualElement valueElement)
                {
                    valueElement.name = "ValueField";
                    valueHolder.Add(valueElement);
                }

                root.Add(keyField);
                root.Add(valueHolder);
                root.Add(scopeField);
                root.Add(removeButton);

                return new RowVisualElements(root, keyField, valueHolder, valueField, scopeField, removeButton);
            }

            private static IBindable CreateValueField(Type handlerType)
            {
                if (handlerType == typeof(ColorRowVisualHandler))
                {
                    return new ColorField();
                }

                if (handlerType == typeof(FloatRowVisualHandler))
                {
                    return new FloatField();
                }

                return new TextField();
            }
        }
    }
}