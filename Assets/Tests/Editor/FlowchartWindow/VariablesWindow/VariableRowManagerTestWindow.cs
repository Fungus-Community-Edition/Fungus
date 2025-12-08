using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObj = UnityEngine.Object;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityRandom = UnityEngine.Random;
using Amanita.VScripting;

// Optional: avoid pulling conflicting types into the global scope
using Amanita.EditorUtils;
using Collections;
using Amanita.VScripting.EditorUtils;
using Amanita; // if you keep helpers here

namespace VScriptingTests.VariableOperations
{
    public class VariableRowManagerTestWindow : EditorWindow
    {
        [MenuItem("Tools/Amanita/Tests/Variable Row Manager Test")]
        public static void Open()
        {
            var wnd = GetWindow<VariableRowManagerTestWindow>();
            wnd.titleContent = new GUIContent("VRM Test");
            wnd.minSize = new Vector2(680, 300);
            wnd.Show();
        }

        // UI
        protected VisualElement _root;
        protected VisualElement _toolbar;
        protected VisualElement _holdsManager;
        protected UITKLabel _status;

        // Data
        protected Flowchart _flowchart;
        protected GameObject _ownerGO; // hidden owner of test Flowchart
        protected readonly System.Random _rng = new System.Random(1337);
        protected bool _liveRefresh = true;
        protected double _lastRefresh;
        protected const double LiveRefreshInterval = 0.25;

        // Cached values for seeding
        protected List<AudioClip> _audioClips = new List<AudioClip>();
        protected List<GameObject> _gameObjects = new List<GameObject>();
        protected List<Sprite> _sprites = new List<Sprite>();

        // If your VariableRowManager is a class, you can keep a reference here.
        // Replace this with your real type/usage.
        // protected VariableRowManager _vrm;

        protected void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EnsureTestFlowchart();

            // Listen to Flowchart events if available for instant UI updates
            TrySubscribeFlowchartEvents(_flowchart, subscribe: true);
        }

        protected void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            TrySubscribeFlowchartEvents(_flowchart, subscribe: false);

            _vRowManager?.Dispose();
            _vRowManager = null;

            // Cleanup hidden GO if we created it
            if (_ownerGO != null)
            {
                DestroyImmediate(_ownerGO);
                _ownerGO = null;
            }
        }

        public void CreateGUI()
        {
            _root = rootVisualElement;
            _root.style.flexDirection = FlexDirection.Column;

            BuildToolbar();
            BuildRowsHost();

            // Initial UI mount
            MountRowsUI();
            UpdateStatus();
        }

        protected static void MarginRight(VisualElement e, float px) => e.style.marginRight = px;

        protected void BuildToolbar()
        {
            _toolbar = new VisualElement { name = "toolbar" };
            _toolbar.style.flexDirection = FlexDirection.Row;
            _toolbar.style.paddingLeft = 6;
            _toolbar.style.paddingRight = 6;
            _toolbar.style.paddingTop = 4;
            _toolbar.style.paddingBottom = 4;

            static Button Btn(string text, Action onClick)
            {
                var b = new Button(onClick) { text = text };
                b.style.flexShrink = 0;
                MarginRight(b, 4);
                return b;
            }

            _toolbar.Add(Btn("Seed 10", () => { SeedVariables(10, clearBefore: true); RebindRows(); }));
            _toolbar.Add(Btn("Seed 50", () => { SeedVariables(50, clearBefore: true); RebindRows(); }));
            _toolbar.Add(Btn("Add 25", () => { SeedVariables(25, clearBefore: false); RebindRows(); }));
            _toolbar.Add(Btn("Clear", () => { ClearVariables(); RebindRows(); }));

            _toolbar.Add(Btn("Random Mutate 10", () => { RandomMutate(10); RebindRows(); }));
            _toolbar.Add(Btn("External Change", () => { SimulateExternalChange(); RebindRows(); }));

            var liveToggle = new Toggle("Live refresh") { value = _liveRefresh };
            liveToggle.RegisterValueChangedCallback(e => _liveRefresh = e.newValue);
            liveToggle.style.marginLeft = 8;
            _toolbar.Add(liveToggle);

            _toolbar.Add(Btn("Rebind UI", RebindRows));

            _status = new UITKLabel();
            _status.style.marginLeft = 10;
            _status.style.unityTextAlign = TextAnchor.MiddleLeft;
            _status.style.flexGrow = 1;
            _toolbar.Add(_status);

            _root.Add(_toolbar);
        }

        protected void BuildRowsHost()
        {
            // Remove the outer ListView entirely
            _holdsManager = new VisualElement { name = "rows-root" };

            _holdsManager.style.flexDirection = FlexDirection.Column;
            _holdsManager.style.flexGrow = 1;

            _root.Add(_holdsManager);
        }

        protected void OnEditorUpdate()
        {
            if (!_liveRefresh) return;

            var now = EditorApplication.timeSinceStartup;
            if (now - _lastRefresh >= LiveRefreshInterval)
            {
                _lastRefresh = now;
                // If external code modified variables, reflect it
                RefreshRows();
            }
        }

        protected void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            // Keep UI and data view sane across domain reloads/runtime copies
            if (change == PlayModeStateChange.EnteredPlayMode ||
                change == PlayModeStateChange.EnteredEditMode ||
                change == PlayModeStateChange.ExitingPlayMode ||
                change == PlayModeStateChange.ExitingEditMode)
            {
                RebindRows();
            }
        }

        protected void EnsureTestFlowchart()
        {
            // Reuse if still around
            if (_flowchart != null) return;

            _ownerGO = GameObject.Find("__VRM_Test_Flowchart");

            if (_ownerGO == null)
            {
                _ownerGO = new GameObject("__VRM_Test_Flowchart");
            }

            _ownerGO.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

            _flowchart = _ownerGO.GetComponent<Flowchart>();
            if (_flowchart == null)
            {
                Undo.RecordObject(_ownerGO, "Add Flowchart for VRM Test");
                _flowchart = Undo.AddComponent<Flowchart>(_ownerGO);
            }
        }

        protected void TrySubscribeFlowchartEvents(Flowchart fc, bool subscribe)
        {
            if (fc == null) return;
            // If your Flowchart exposes VariableAdded/VariableRemoved, hook them for instant updates.
            // Wrap in try to remain resilient if events aren't present yet.
            try
            {
                if (subscribe)
                {
                    fc.VariableAdded += OnVariableAddedRemoved;
                    fc.VariableRemoved += OnVariableAddedRemoved;
                }
                else
                {
                    fc.VariableAdded -= OnVariableAddedRemoved;
                    fc.VariableRemoved -= OnVariableAddedRemoved;
                }
            }
            catch { /* no-op if events differ */ }
        }

        protected void OnVariableAddedRemoved(IVariable _)
        {
            // Ensure UI reflects changes caused by code outside the window
            RefreshRows();
            UpdateStatus();
        }

        protected void MountRowsUI()
        {
            // Clean previous
            _holdsManager.Clear();
            _vRowManager?.Dispose();
            _vRowManager = null;

            // Load UXML
            const string pathToUxml = AmanitaConstants.PathToAmanitaVariableDisplayEditorUxml;
            if (_variableTemplate == null)
            {
                _variableTemplate = Resources.Load<VisualTreeAsset>(pathToUxml);
            }

            bool stillNothing = _variableTemplate == null;
            if (stillNothing)
            {
                Debug.LogError($"VariableRowManagerTestWindow: Could not load UXML at Resources/{pathToUxml}.uxml");
                return;
            }

            // Clone and query parts
            _root = _variableTemplate.CloneTree();
            var listContainer = _root.Q<ListView>("rowList");
            var countLabel = _root.Q<UITKLabel>("varCountLabel");
            var addButton = _root.Q<Button>("addVarButton");

            // Sanity checks (prevents silent nothingness)
            if (listContainer == null || countLabel == null || addButton == null)
            {
                Debug.LogError("VariableRowManagerTestWindow: UXML is missing required elements: rowList / varCountLabel / addVarButton");
                return;
            }
            var visualHandlerLookup = RowVisualHandlerRegistry.VisualHandlerLookup;
            var handlerPool = new RowVisualHandlerPool(_resolver, visualHandlerLookup);
            var factoryInitArgs = new VariableRowFactoryInitArgs()
            {
                RowPool = new VariableRowPool(),
                HandlerPool = handlerPool,
                Holder = _holdsManager,
            };
            var varRowFactory = new VariableRowFactory();
            varRowFactory.Init(factoryInitArgs);

            var listViewArgs = new VariableListViewInitArgs()
            {
                List = listContainer,
                CountLabel = countLabel,
                RowFactory = varRowFactory,
                VariableSource = _flowchart,
                AssetResolver = new DefaultEditorAssetResolver(),
            };

            var view = new VariableListView(listViewArgs);
            
            // Build manager
            var args = new VRowManagerInitArgs
            {
                HoldsManager = _holdsManager,
                Root = _root,
                AddButton = addButton,
                VariableSource = _flowchart,
                VariableListView = view,
            };

            _vRowManager = new VariableRowManager();
            _vRowManager.Init(args);

            // Attach the manager's root to the window (Init doesn't do this)
            RegisterAndAddToRoot(_holdsManager);

            // Optional: force a refresh (Init already calls Refresh if Flowchart is set)
            _vRowManager.Refresh();

            UpdateStatus();

        }

        public virtual void RegisterAndAddToRoot(VisualElement toHoldManager)
        {
            if ((_holdsManager != null && _holdsManager != toHoldManager) &&
                _root != null && _root.parent != null)
            {
                _holdsManager.Remove(_root);
            }

            _holdsManager = toHoldManager;
            if (_root != null && !_holdsManager.Contains(_root))
                _holdsManager.Add(_root);
        }

        protected VariableRowManager _vRowManager;
        protected SilentTestResolver _resolver = new SilentTestResolver();
        protected VisualTreeAsset _variableTemplate;


        protected void RefreshRows()
        {
            // If your manager supports lightweight refresh, call it here.
            // e.g., _vrm?.Refresh();
            UpdateStatus();
        }

        protected void RebindRows()
        {
            // For safety, remount entirely — good for catching binding lifecycle issues
            MountRowsUI();
        }

        protected void ClearVariables()
        {
            var list = GetVariables().ToList();
            _flowchart.ClearVariables();
            Undo.IncrementCurrentGroup();
            foreach (var v in list)
            {
                if (v != null)
                {
                    Undo.DestroyObjectImmediate(v as UnityObj);
                }
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorUtility.SetDirty(_flowchart);
            UpdateStatus();
        }

        protected void SeedVariables(int count, bool clearBefore)
        {
            if (clearBefore) ClearVariables();
            EnsureCacheForAssets(_audioClips);
            EnsureCacheForAssets(_gameObjects);
            EnsureCollidersCached();
            EnsureCacheForAssets(_textures);
            EnsureCacheForAssets(_materials);
            EnsureCacheForAssets(_sprites);
            EnsureRigidbodiesCached();
            EnsureUnityObjsCached();
            
            Undo.IncrementCurrentGroup();
            int varTypeCount = _supportedTypes.Count;
            // For when we find stuff
            for (int i = 0; i < count; i++)
            {
                var typeIndex = i % varTypeCount;
                IVariable var = null;

                switch (typeIndex)
                {
                    case 0: var = AddVariableComponent<FloatVariable>(RandomFloat()); break;
                    case 1: var = AddVariableComponent<IntegerVariable>(_rng.Next(-100, 100)); break;
                    case 2: var = AddVariableComponent<BooleanVariable>(_rng.NextDouble() > 0.5); break;
                    case 3: var = AddVariableComponent<StringVariable>(RandomString(6)); break;
                    case 4:
                        var clip = _audioClips.Count > 0 ? _audioClips[_rng.Next(_audioClips.Count)] : null;
                        var = AddVariableComponent<AudioClipVariable>(clip);
                        break;
                    case 5:
                        Vector2 toDisplay = new Vector2(RandomInt(),
                        RandomInt());
                        var = AddVariableComponent<Vector2Variable>(toDisplay);
                        break;
                    case 6:
                        Vector3 vec3 = new Vector3(RandomInt(), RandomInt(), RandomInt());
                        var = AddVariableComponent<Vector3Variable>(vec3);
                        break;
                    case 7:
                        var go = _gameObjects.Count > 0 ? _gameObjects[_rng.Next(_gameObjects.Count)] : null;
                        var = AddVariableComponent<GameObjectVariable>(go);
                        break;
                    case 8:
                        var hasTrans = _gameObjects.Count > 0 ? _gameObjects[_rng.Next(_gameObjects.Count)] : null;
                        Transform trans = null;
                        if (hasTrans != null)
                        {
                            trans = hasTrans.transform;
                        }
                        var = AddVariableComponent<TransformVariable>(trans);
                        break;
                    case 9:
                        var theObj = _UnityObjs.Count > 0 ? _UnityObjs[_rng.Next(_UnityObjs.Count)] : null;
                        var = AddVariableComponent<ObjectVariable>(theObj);
                        break;
                    case 10:
                        int r = UnityRandom.Range(0, 100), g = UnityRandom.Range(0, 100), b = UnityRandom.Range(0, 100);
                        var theCol = new Color(r, g, b);  
                        var = AddVariableComponent<ColorVariable>(theCol);
                        break;
                    case 11:
                        if (_textures.Count > 0)
                        {
                            var texVal = _textures.GetRandom();
                            var = AddVariableComponent<TextureVariable>(texVal);
                        }
                        break;
                    case 12:
                        if (_materials.Count > 0)
                        {
                            var matVal = _materials.GetRandom();
                            var = AddVariableComponent<MaterialVariable>(matVal);
                        }
                        break;
                    case 13:
                        if (_sprites.Count > 0)
                        {
                            var spriteVal = _sprites.GetRandom();
                            var = AddVariableComponent<SpriteVariable>(spriteVal);
                        }
                        break;

                }

                // Assign a unique key via Flowchart helper if available
                if (var != null)
                {
                    var desired = $"var_{var.GetType().Name}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                    try
                    {
                        var.Key = UniqueKeyGenerator.GetUniqueKeyFor(desired, (IList<IVariable>)_flowchart.Variables);
                    }
                    catch
                    {
                        var.Key = desired; // fallback
                    }

                    _flowchart.AddVariable(var);
                }


            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorUtility.SetDirty(_flowchart);
            UpdateStatus();
        }

        protected static IList<Type> _supportedTypes = new List<Type>()
        {
            typeof(int),
            typeof(float),
            typeof(string),
            typeof(bool),
            typeof(Color),
            typeof(GameObject),
            typeof(Transform),
            typeof(UnityObj),
            typeof(Vector3),
            typeof(Vector2),
            typeof(Collider2D),
            typeof(Collider),
            typeof(AudioClip),
            typeof(Texture),
            typeof(Material),
            typeof(Sprite),
            typeof(Rigidbody),
            typeof(Rigidbody2D)
            
        };

        protected void RandomMutate(int count)
        {
            var vars = GetVariables().ToList();
            if (vars.Count == 0) return;

            Undo.IncrementCurrentGroup();

            for (int i = 0; i < count; i++)
            {
                var v = vars[_rng.Next(vars.Count)];
                var uo = v as UnityObj;
                if (uo == null) continue;

                var so = new SerializedObject(uo);
                var valueProp = FindValueProperty(so);

                if (v is FloatVariable)
                {
                    MutateFloat(valueProp, so);
                }
                else if (v is IntegerVariable)
                {
                    MutateInt(valueProp, so);
                }
                else if (v is BooleanVariable)
                {
                    MutateBool(valueProp, so);
                }
                else if (v is StringVariable)
                {
                    MutateString(valueProp, so);
                }
                else if (v.ContentType != null && typeof(UnityObj).IsAssignableFrom(v.ContentType))
                {
                    MutateObjectRef(valueProp, so);
                }
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            UpdateStatus();
        }

        protected void SimulateExternalChange()
        {
            var vars = GetVariables().ToList();
            if (vars.Count == 0) return;

            // Pick one and mutate via SerializedObject (simulates another inspector/editor change)
            var v = vars[_rng.Next(vars.Count)];
            var uo = v as UnityObj;
            if (uo == null) return;

            Undo.RecordObject(uo, "External Change Variable");

            var so = new SerializedObject(uo);
            var valueProp = FindValueProperty(so);

            if (v is FloatVariable) MutateFloat(valueProp, so);
            else if (v is IntegerVariable) MutateInt(valueProp, so);
            else if (v is BooleanVariable) MutateBool(valueProp, so);
            else if (v is StringVariable) MutateString(valueProp, so);
            else if (v.ContentType != null && typeof(UnityObj).IsAssignableFrom(v.ContentType)) MutateObjectRef(valueProp, so);

            UpdateStatus();
        }

        // ------------- Helpers -------------

        protected IEnumerable<IVariable> GetVariables()
        {
            IEnumerable<IVariable> result = null;

            if (_flowchart != null)
            {
                result = _flowchart.Variables.Cast<IVariable>();
            }

            result ??= Enumerable.Empty<IVariable>();

            return result;
        }

        protected TVarType AddVariableComponent<TVarType>(object valueForInit)
    where TVarType : Component, IVariable
        {
            var varComponent = Undo.AddComponent<TVarType>(_ownerGO);
            var unityObj = varComponent as UnityObj;
            var serializedObj = new SerializedObject(unityObj);

            var valueProp = FindValueProperty(serializedObj);
            serializedObj.Update();

            if (valueForInit is float floatVal && valueProp != null) valueProp.floatValue = floatVal;
            else if (valueForInit is int intVal && valueProp != null) valueProp.intValue = intVal;
            else if (valueForInit is bool boolVal && valueProp != null) valueProp.boolValue = boolVal;
            else if (valueForInit is string stringVal && valueProp != null) valueProp.stringValue = stringVal;
            else if (valueForInit is Vector2 vecTwoVal && valueProp != null) valueProp.vector2Value = vecTwoVal;
            else if (valueForInit is Vector3 vecThreeVal && valueProp != null) valueProp.vector3Value = vecThreeVal;
            else if (valueForInit is Color colorVal && valueProp != null)
            {
                valueProp.colorValue = colorVal;
            }
            else if (valueForInit is UnityObj UnityObj && valueProp != null) valueProp.objectReferenceValue = UnityObj;

            serializedObj.ApplyModifiedProperties();

            EditorUtility.SetDirty(unityObj);

            return varComponent;
        }

        protected void MutateFloat(SerializedProperty prop, SerializedObject serializedObj)
        {
            if (prop != null)
            {
                serializedObj.Update();
                prop.floatValue += UnityEngine.Random.Range(-5f, 5f);
                serializedObj.ApplyModifiedProperties();
            }
        }

        protected void MutateInt(SerializedProperty prop, SerializedObject serializedObj)
        {
            if (prop != null)
            {
                serializedObj.Update();
                prop.intValue += UnityEngine.Random.Range(-5, 6);
                serializedObj.ApplyModifiedProperties();
            }
        }

        protected void MutateBool(SerializedProperty prop, SerializedObject serializedObj)
        {
            if (prop != null)
            {
                serializedObj.Update();
                prop.boolValue = !prop.boolValue;
                serializedObj.ApplyModifiedProperties();
            }
        }

        protected void MutateString(SerializedProperty prop, SerializedObject serializedObj)
        {
            if (prop != null)
            {
                serializedObj.Update();
                prop.stringValue = RandomString(6);
                serializedObj.ApplyModifiedProperties();
            }
        }

        protected void MutateObjectRef(SerializedProperty prop, SerializedObject serializedObj)
        {
            if (prop == null) return;
            EnsureCacheForAssets(_audioClips);
            var newObj = _audioClips.Count > 0 ? _audioClips[_rng.Next(_audioClips.Count)] : null;

            serializedObj.Update();
            prop.objectReferenceValue = newObj;
            serializedObj.ApplyModifiedProperties();
        }

        protected SerializedProperty FindValueProperty(SerializedObject serializedObj)
        {
            string[] candidates = { "value", "baseVal", "baseValue", "m_Value" };
            foreach (var name in candidates)
            {
                var prop = serializedObj.FindProperty(name);
                if (prop != null) return prop;
            }

            // Fallback: prefer object refs, then any non m_Script visible property
            var it = serializedObj.GetIterator();
            bool enterChildren = true;
            SerializedProperty firstNonScript = null;
            while (it.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (it.name == "m_Script") continue;
                firstNonScript ??= it.Copy();
                if (it.propertyType == SerializedPropertyType.ObjectReference) return it.Copy();
            }
            return firstNonScript;
        }

        protected virtual void EnsureCollidersCached()
        {
            bool needRefreshThreeD = _colliderThreeDObjects.Count < _cacheCapacity || _colliderThreeDObjects.Contains(null);
            bool needRefreshTwoD = _colliderTwoDObjects.Count < _cacheCapacity || _colliderTwoDObjects.Contains(null);
            if (!needRefreshThreeD && !needRefreshTwoD)
            {
                return;
            }

            _colliderThreeDObjects.Clear();
            _colliderTwoDObjects.Clear();

            foreach (GameObject go in _gameObjects)
            {
                if (_colliderThreeDObjects.Count < _cacheCapacity)
                {
                    var colliderThreeDsFound = go.GetComponentsInChildren<Collider>();
                    _colliderThreeDObjects.AddRange(colliderThreeDsFound, _cacheCapacity);
                }

                if (_colliderTwoDObjects.Count < _cacheCapacity)
                {
                    var colliderTwoDsFound = go.GetComponentsInChildren<Collider2D>();
                    _colliderTwoDObjects.AddRange(colliderTwoDsFound, _cacheCapacity);
                }

                if (_colliderThreeDObjects.Count >= _cacheCapacity && 
                    _colliderTwoDObjects.Count >= _cacheCapacity)
                {
                    break;
                }

            }
        }

        protected static int _cacheCapacity = 10;

        protected virtual void EnsureRigidbodiesCached()
        {
            // We assume that the GameObject cache is ready by this point
            bool shouldRefreshThreeDs = _rigidbodyThreeDs.Count < _cacheCapacity || _rigidbodyThreeDs.Contains(null);

            int howManyToGoThrough = _cacheCapacity;
            if (shouldRefreshThreeDs)
            {
                _rigidbodyThreeDs.Clear();
                IList<Rigidbody> toAdd = GetComponentsFrom<Rigidbody>(_gameObjects);
                _rigidbodyThreeDs.AddRange(toAdd);
            }

            IList<T> GetComponentsFrom<T>(IList<GameObject> gameObjects, int countLimit = 10) where T: Component
            {
                IList<T> result = new List<T>();
                var hasWhatWeWant = (from elem in _gameObjects
                                     where elem.GetComponent<T>() != null
                                     select elem.GetComponent<T>()).ToList();

                howManyToGoThrough = Mathf.Min(_cacheCapacity, hasWhatWeWant.Count);

                for (int i = 0; i < howManyToGoThrough; i++)
                {
                    var currentRb = hasWhatWeWant[i];
                    result.Add(currentRb);
                }
                return result;
            }

            bool shouldRefreshTwoDs = _rigidbodyTwoDs.Count < _cacheCapacity || _rigidbodyTwoDs.Contains(null);

            if (shouldRefreshTwoDs)
            {
                _rigidbodyTwoDs.Clear();
                IList<Rigidbody2D> toAdd = GetComponentsFrom<Rigidbody2D>(_gameObjects);
                _rigidbodyTwoDs.AddRange(toAdd);
            }
        }

        protected List<Rigidbody> _rigidbodyThreeDs = new List<Rigidbody>();
        protected List<Rigidbody2D> _rigidbodyTwoDs = new List<Rigidbody2D>();

        protected virtual void EnsureUnityObjsCached()
        {
            // We want there to be a variety, hence why we're not populating by checking guids
            // like we did with the other asset types
            if (_UnityObjs.Count >= _cacheCapacity && !_UnityObjs.Contains(null)) return;

            _UnityObjs.Clear();
            int amountAdded = 0;
            foreach (var go in _gameObjects)
            {
                _UnityObjs.Add(go);
                amountAdded++;
                if (amountAdded > 2)
                    break;
            }

            amountAdded = 0;
            foreach (var clip in _audioClips) 
            {
                _UnityObjs.Add(clip);
                amountAdded++;
                if (amountAdded > 2)
                    break;
            }

            amountAdded = 0;
            foreach (var coll in _colliderThreeDObjects)
            {
                _UnityObjs.Add(coll);
                amountAdded++;
                if (amountAdded > 2)
                    break;
            }

            amountAdded = 0;
            foreach (var coll in _colliderTwoDObjects)
            {
                _UnityObjs.Add(coll);
                amountAdded++;
                if (amountAdded > 2)
                    break;
            }

            // We won't need to add more stuff than this
        }

        protected virtual void EnsureCacheForAssets<T>(IList<T> cacheInvolved) where T: UnityObj
        {
            if (cacheInvolved.Count >= _cacheCapacity && !cacheInvolved.Contains(default))
            {
                return;
            }

            cacheInvolved.Clear();
            string query = $"t:{typeof(T).Name}";
            var guids = AssetDatabase.FindAssets(query);
            int howManyToGoThrough = Mathf.Min(_cacheCapacity, guids.Length);

            for (int i = 0; i < howManyToGoThrough; i++)
            {
                var guidEl = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guidEl);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) cacheInvolved.Add(asset);
            }

        }

        protected IList<UnityObj> _UnityObjs = new List<UnityObj>();
        protected IList<Collider> _colliderThreeDObjects = new List<Collider>();
        protected IList<Collider2D> _colliderTwoDObjects = new List<Collider2D>();
        protected IList<Texture> _textures = new List<Texture>();
        protected IList<Material> _materials = new List<Material>();

        protected string RandomString(int len)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Range(0, len).Select(_ => chars[_rng.Next(chars.Length)]).ToArray());
        }

        protected float RandomFloat()
        {
            return (float)(_rng.NextDouble() * 200.0 - 100.0);
        }

        protected int RandomInt()
        {
            return _rng.Next(-100, 100);
        }

        protected void UpdateStatus()
        {
            if (_status == null) return;
            var count = GetVariables().Count();
            var play = EditorApplication.isPlaying ? "Play" : "Edit";
            _status.text = $"Vars: {count}  |  Mode: {play}  |  LiveRefresh: {(_liveRefresh ? "On" : "Off")}";
        }
    }

    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _start;   // index of oldest element
        private int _count;

        public int Capacity => _buffer.Length;
        public int Count => _count;

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _buffer = new T[capacity];
            _start = 0;
            _count = 0;
        }

        public void Add(T item)
        {
            int index = (_start + _count) % Capacity;
            _buffer[index] = item;

            if (_count == Capacity)
            {
                // Overwrite oldest
                int firstNullIndex = FirstIndexOfNull();

                _start = (_start + 1) % Capacity;
            }
            else
            {
                _count++;
            }
        }

        protected virtual int FirstIndexOfNull()
        {
            for (int i = 0; i < _buffer.Length; i++)
            {
                var item = _buffer[i];
                if (item == null)
                {
                    return i;
                }
            }

            return -1;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));
                return _buffer[(_start + index) % Capacity];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return this[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}