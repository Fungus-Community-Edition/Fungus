// Assets/Editor/VariableRowManagerTestWindow.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using UITKLabel = UnityEngine.UIElements.Label;

// Optional: avoid pulling conflicting types into the global scope
using AV = Amanita.VScripting;


// Adjust these to your namespaces
using Amanita.VScripting;
using Amanita.Tests.Editor;
// using Amanita.VScripting.EditorUtils; // if you keep helpers here

namespace Amanita.VScripting.EditorUtils
{
    public class VariableRowManagerTestWindow : EditorWindow
    {
        [MenuItem("Amanita/Tests/Variable Row Manager Test")]
        public static void Open()
        {
            var wnd = GetWindow<VariableRowManagerTestWindow>();
            wnd.titleContent = new GUIContent("VRM Test");
            wnd.minSize = new Vector2(680, 300);
            wnd.Show();
        }

        // UI
        private VisualElement _root;
        private VisualElement _toolbar;
        private VisualElement _rowsRoot;
        private UITKLabel _status;

        // Data
        private Flowchart _flowchart;
        private GameObject _ownerGO; // hidden owner of test Flowchart
        private readonly System.Random _rng = new System.Random(1337);
        private bool _liveRefresh = true;
        private double _lastRefresh;
        private const double LiveRefreshInterval = 0.25;

        // Cached audio clips for seeding
        private List<AudioClip> _clips;

        // If your VariableRowManager is a class, you can keep a reference here.
        // Replace this with your real type/usage.
        // private VariableRowManager _vrm;

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EnsureTestFlowchart();

            // Listen to Flowchart events if available for instant UI updates
            TrySubscribeFlowchartEvents(_flowchart, subscribe: true);
        }

        private void OnDisable()
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

        private static void MarginRight(VisualElement e, float px) => e.style.marginRight = px;

        private void BuildToolbar()
        {
            _toolbar = new VisualElement { name = "toolbar" };
            _toolbar.style.flexDirection = FlexDirection.Row;
            _toolbar.style.paddingLeft = 6;
            _toolbar.style.paddingRight = 6;
            _toolbar.style.paddingTop = 4;
            _toolbar.style.paddingBottom = 4;

            Button Btn(string text, Action onClick)
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

        private void BuildRowsHost()
        {
            // Remove the outer ScrollView entirely
            _rowsRoot = new VisualElement { name = "rows-root" };
            _rowsRoot.style.flexDirection = FlexDirection.Column;
            _rowsRoot.style.flexGrow = 1;

            _root.Add(_rowsRoot);
        }

        private void OnEditorUpdate()
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

        private void OnPlayModeStateChanged(PlayModeStateChange change)
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

        private void EnsureTestFlowchart()
        {
            // Reuse if still around
            if (_flowchart != null) return;

            _ownerGO = GameObject.Find("__VRM_Test_Flowchart") ?? new GameObject("__VRM_Test_Flowchart");
            _ownerGO.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

            _flowchart = _ownerGO.GetComponent<Flowchart>();
            if (_flowchart == null)
            {
                Undo.RecordObject(_ownerGO, "Add Flowchart for VRM Test");
                _flowchart = Undo.AddComponent<Flowchart>(_ownerGO);
            }
        }

        private void TrySubscribeFlowchartEvents(Flowchart fc, bool subscribe)
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

        private void OnVariableAddedRemoved(IVariable _)
        {
            // Ensure UI reflects changes caused by code outside the window
            RefreshRows();
            UpdateStatus();
        }

        private void MountRowsUI()
        {
            // Clean previous
            _rowsRoot.Clear();
            _vRowManager?.Dispose();
            _vRowManager = null;

            // Load UXML
            const string pathToUxml = "_EditorResources/UIToolkitTemplates/VariableDisplayEditor";
            _variableTemplate ??= Resources.Load<VisualTreeAsset>(pathToUxml);
            if (_variableTemplate == null)
            {
                Debug.LogError($"VariableRowManagerTestWindow: Could not load UXML at Resources/{pathToUxml}.uxml");
                return;
            }

            // Clone and query parts
            var root = _variableTemplate.CloneTree();
            var listContainer = root.Q<ScrollView>("rowList");
            var countLabel = root.Q<UITKLabel>("varCountLabel");
            var addButton = root.Q<Button>("addVarButton");

            // Sanity checks (prevents silent nothingness)
            if (listContainer == null || countLabel == null || addButton == null)
            {
                Debug.LogError("VariableRowManagerTestWindow: UXML is missing required elements: rowList / varCountLabel / addVarButton");
                return;
            }

            // Build manager
            var args = new VRowManagerInitArgs
            {
                HoldsManager = _rowsRoot,   // where the manager should attach its root
                Root = root,        // the cloned root
                ListContainer = listContainer,
                CountLabel = countLabel,
                AddButton = addButton,
                Flowchart = _flowchart,
            };

            _vRowManager = new VariableRowManager(_resolver);
            _vRowManager.Init(args);

            // Attach the manager's root to the window (Init doesn't do this)
            _vRowManager.RegisterAndAddToRoot(_rowsRoot);

            // Optional: force a refresh (Init already calls Refresh if Flowchart is set)
            _vRowManager.Refresh();

            UpdateStatus();

        }

        private VariableRowManager _vRowManager;
        private SilentTestResolver _resolver = new SilentTestResolver();
        private VisualTreeAsset _variableTemplate;


        private void RefreshRows()
        {
            // If your manager supports lightweight refresh, call it here.
            // e.g., _vrm?.Refresh();
            UpdateStatus();
        }

        private void RebindRows()
        {
            // For safety, remount entirely — good for catching binding lifecycle issues
            MountRowsUI();
        }

        // ------------- Data ops -------------

        private void ClearVariables()
        {
            var list = GetVariables().ToList();
            _flowchart.ClearVariables();
            Undo.IncrementCurrentGroup();
            foreach (var v in list)
            {
                Undo.DestroyObjectImmediate(v as UnityEngine.Object);
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorUtility.SetDirty(_flowchart);
            UpdateStatus();
        }

        private void SeedVariables(int count, bool clearBefore)
        {
            if (clearBefore) ClearVariables();
            EnsureClipsCached();
            
            Undo.IncrementCurrentGroup();
            int varTypeCount = 6;
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
                        var clip = _clips.Count > 0 ? _clips[_rng.Next(_clips.Count)] : null;
                        var = AddVariableComponent<AudioClipVariable>(clip);
                        break;
                    case 5:
                    Vector2 toDisplay = new Vector2(_rng.Next(-100, 100),
                        _rng.Next(-100, 100));
                    var = AddVariableComponent<Vector2Variable>(toDisplay);
                    break;
                }

                // Assign a unique key via Flowchart helper if available
                if (var != null)
                {
                    var desired = $"var_{var.GetType().Name}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                    try
                    {
                        var.Key = _flowchart.GetUniqueVariableKey(desired, var);
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

        private void RandomMutate(int count)
        {
            var vars = GetVariables().ToList();
            if (vars.Count == 0) return;

            Undo.IncrementCurrentGroup();

            for (int i = 0; i < count; i++)
            {
                var v = vars[_rng.Next(vars.Count)];
                var uo = v as UnityEngine.Object;
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
                else if (v.ContentType != null && typeof(UnityEngine.Object).IsAssignableFrom(v.ContentType))
                {
                    MutateObjectRef(valueProp, so);
                }
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            UpdateStatus();
        }

        private void SimulateExternalChange()
        {
            var vars = GetVariables().ToList();
            if (vars.Count == 0) return;

            // Pick one and mutate via SerializedObject (simulates another inspector/editor change)
            var v = vars[_rng.Next(vars.Count)];
            var uo = v as UnityEngine.Object;
            if (uo == null) return;

            Undo.RecordObject(uo, "External Change Variable");

            var so = new SerializedObject(uo);
            var valueProp = FindValueProperty(so);

            if (v is FloatVariable) MutateFloat(valueProp, so);
            else if (v is IntegerVariable) MutateInt(valueProp, so);
            else if (v is BooleanVariable) MutateBool(valueProp, so);
            else if (v is StringVariable) MutateString(valueProp, so);
            else if (v.ContentType != null && typeof(UnityEngine.Object).IsAssignableFrom(v.ContentType)) MutateObjectRef(valueProp, so);

            UpdateStatus();
        }

        // ------------- Helpers -------------

        private IEnumerable<IVariable> GetVariables()
        {
            return _flowchart?.Variables?.Cast<IVariable>() ?? Enumerable.Empty<IVariable>();
        }

        private T AddVariableComponent<T>(object valueForInit) where T : Component, IVariable
        {
            var c = Undo.AddComponent<T>(_ownerGO);
            var uo = c as UnityEngine.Object;
            var so = new SerializedObject(uo);

            // Assign initial value through serialization
            var valueProp = FindValueProperty(so);
            so.Update();
            if (valueForInit is float f && valueProp != null) valueProp.floatValue = f;
            else if (valueForInit is int i && valueProp != null) valueProp.intValue = i;
            else if (valueForInit is bool b && valueProp != null) valueProp.boolValue = b;
            else if (valueForInit is string s && valueProp != null) valueProp.stringValue = s;
            else if (valueForInit is UnityEngine.Object obj && valueProp != null) valueProp.objectReferenceValue = obj;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(uo);
            return c;
        }

        private void MutateFloat(SerializedProperty p, SerializedObject so)
        {
            if (p != null)
            {
                so.Update();
                p.floatValue += UnityEngine.Random.Range(-5f, 5f);
                so.ApplyModifiedProperties();
            }
        }

        private void MutateInt(SerializedProperty p, SerializedObject so)
        {
            if (p != null)
            {
                so.Update();
                p.intValue += UnityEngine.Random.Range(-5, 6);
                so.ApplyModifiedProperties();
            }
        }

        private void MutateBool(SerializedProperty p, SerializedObject so)
        {
            if (p != null)
            {
                so.Update();
                p.boolValue = !p.boolValue;
                so.ApplyModifiedProperties();
            }
        }

        private void MutateString(SerializedProperty p, SerializedObject so)
        {
            if (p != null)
            {
                so.Update();
                p.stringValue = RandomString(6);
                so.ApplyModifiedProperties();
            }
        }

        private void MutateObjectRef(SerializedProperty p, SerializedObject so)
        {
            if (p == null) return;
            EnsureClipsCached();
            var newObj = _clips.Count > 0 ? _clips[_rng.Next(_clips.Count)] : null;

            so.Update();
            p.objectReferenceValue = newObj;
            so.ApplyModifiedProperties();
        }

        private SerializedProperty FindValueProperty(SerializedObject so)
        {
            string[] candidates = { "value", "baseVal", "baseValue", "m_Value" };
            foreach (var name in candidates)
            {
                var prop = so.FindProperty(name);
                if (prop != null) return prop;
            }

            // Fallback: prefer object refs, then any non m_Script visible property
            var it = so.GetIterator();
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

        private void EnsureClipsCached()
        {
            if (_clips != null) return;
            _clips = new List<AudioClip>();
            var guids = AssetDatabase.FindAssets("t:AudioClip");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) _clips.Add(clip);
            }
        }

        private string RandomString(int len)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Range(0, len).Select(_ => chars[_rng.Next(chars.Length)]).ToArray());
        }

        private float RandomFloat()
        {
            return (float)(_rng.NextDouble() * 200.0 - 100.0);
        }

        private void UpdateStatus()
        {
            if (_status == null) return;
            var count = GetVariables().Count();
            var play = EditorApplication.isPlaying ? "Play" : "Edit";
            _status.text = $"Vars: {count}  |  Mode: {play}  |  LiveRefresh: {(_liveRefresh ? "On" : "Off")}";
        }
    }
}