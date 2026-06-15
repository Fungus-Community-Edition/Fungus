using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorExt;
using UnityObject = UnityEngine.Object;
using AtMycelia.Amanita;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.Audio;
using Lorekeeper;
using System.Linq;
using UnityEditor.SceneManagement;

namespace VScriptingTests.VariableRows
{
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
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Undo.ClearAll();
            Flowchart.ResetStaticsForTest();

            _objectsToDestroy = new List<UnityObject>();

            _originalTemplateProvider = RowVisualTemplateProviderRegistry.Current;
            _originalElementBuilder = RowVisualElementBuilderRegistry.Current;

            RowVisualTemplateProviderRegistry.Current = new CachedRowVisualTemplateProvider();
            RowVisualElementBuilderRegistry.Current = new DefaultRowVisualElementBuilder();

            _objectsToDestroy.Add(_amanitaManager.gameObject);

            var flowchartGO = new GameObject("TestFlowchart");
            _objectsToDestroy.Add(flowchartGO);
            _flowchart = flowchartGO.AddComponent<Flowchart>();
            _flowchart.AlwaysKeepGuid = false;
            _flowchart.Refresh();

            _uiHost = ScriptableObject.CreateInstance<TestHostWindow>();//
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

            _variableListView.SetSource(_flowchart);

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

                        SetValueAndSendChange(floatField, (float)boxed);
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

                        SetValueAndSendChange(intField, (int)boxed);
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
                        SetValueAndSendChange(vector2Field, (Vector2)boxed);
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
                        SetValueAndSendChange(vector3Field, (Vector3)boxed);
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

                        SetValueAndSendChange(stringField, (string)boxed);
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

                        SetValueAndSendChange(colorField, (Color)boxed);
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
                        SetObjectFieldValue(objField, (Sprite)boxed);
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
                        SetObjectFieldValue(objField, (Texture)boxed);
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
                        SetObjectFieldValue(objField, (Material)boxed);
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
                        SetObjectFieldValue(objField, (Animator)boxed);
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
                        SetObjectFieldValue(objField, (AudioClip)boxed);
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
                        SetObjectFieldValue(objField, (AudioSource)boxed);
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
                        SetObjectFieldValue(objField, (AudioMixer)boxed);
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
                        SetObjectFieldValue(objField, (Collider)boxed);
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
                        SetObjectFieldValue(objField, (Rigidbody)boxed);
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
                        SetObjectFieldValue(objField, (Collider2D)boxed);
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
                        SetObjectFieldValue(objField, (Rigidbody2D)boxed);
                    }
                }
                #endregion
            };

        private static void SetValueAndSendChange<T>(INotifyValueChanged<T> field, T value)
        {
            if (field is not VisualElement element)
            {
                return;
            }

            T previousValue = field.value;
            field.SetValueWithoutNotify(value);

            using (ChangeEvent<T> evt = ChangeEvent<T>.GetPooled(previousValue, value))
            {
                evt.target = element;
                element.SendEvent(evt);
            }
        }

        private static void SetObjectFieldValue(ObjectField field, UnityEngine.Object value)
        {
            if (field == null)
            {
                return;
            }

            UnityEngine.Object previousValue = field.value;
            field.SetValueWithoutNotify(value);

            using (ChangeEvent<UnityEngine.Object> evt = ChangeEvent<UnityEngine.Object>.GetPooled(previousValue, value))
            {
                evt.target = field;
                field.SendEvent(evt);
            }
        }

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
                #endregion

                #region Graphics
                { typeof(string), typeof(StringRowVisualHandler) },
                { typeof(Texture), typeof(TextureRowVisualHandler) },
                { typeof(Material), typeof(MaterialRowVisualHandler) },
                { typeof(Sprite), typeof(SpriteRowVisualHandler) },
                { typeof(Animator), typeof(AnimatorRowVisualHandler) },
                #endregion

                #region Physics
                //{ typeof(Rigidbody), typeof(RigidbodyThreeDRowVisualHandler) }, 
                { typeof(Collider), typeof(ColliderThreeDRowVisualHandler) },
                //{ typeof(Rigidbody2D), typeof(RigidbodyTwoDRowVisualHandler) },
                { typeof(Collider2D), typeof(ColliderTwoDRowVisualHandler) },
                #endregion

                #region Etc
                #endregion

            };
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

    }
}