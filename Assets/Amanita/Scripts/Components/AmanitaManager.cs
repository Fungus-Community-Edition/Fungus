using Amanita.DialogueSys;
using Amanita.Myceliaudio;
using Amanita.SaveSys;
using Amanita.Tweening;
using Amanita.VScripting;
using FullSerializer;
using Lorekeeper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using Amanita.SaveSys.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Amanita
{
    /// <summary>
    /// Amanita manager singleton. Manages access to all Amanita singletons in a consistent manner.
    /// </summary>
    public sealed class AmanitaManager : MonoBehaviour
    {
        [SerializeField] private List<VariableSourceAsset> globalVariables = new List<VariableSourceAsset>();
        [SerializeField, HideInInspector] private GameObject tweenAnchorHolder;

        public static fsSerializer DefaultSerializer { get; } = new fsSerializer();
        public IList<IVariable> GlobalVariables
        {
            get
            {
                List<IVariable> result = new List<IVariable>();
                foreach (var src in globalVariables)
                {
                    if (src == null)
                    {
                        continue;
                    }
                    result.AddRange(src.Variables.Where(elem => elem != null));
                }
                return result;
            }
        }

        public IList<VariableSourceAsset> GlobalVariableSources
        {
            get => globalVariables.ToArray();
            set
            {
                globalVariables.Clear();
                globalVariables.AddRange(value);
            }
        }

        public static int GetNumericIdTiedTo(string guid)
        {
            var fcGuidRegistry = GetOrAddGuidRegistryFor<Flowchart>();
            fcGuidRegistry.Refresh();
            fcGuidRegistry.AddTypeStoredFor<Flowchart>();
            int result = fcGuidRegistry.GetNumericId(guid);
            if (result >= 0)
            {
                return result;
            }

            var vsaGuidRegistry = GetOrAddGuidRegistryFor<VariableSourceAsset>();
            vsaGuidRegistry.Refresh();
            vsaGuidRegistry.AddTypeStoredFor<VariableSourceAsset>();
            result = vsaGuidRegistry.GetNumericId(guid);
            return result;
        }

        public static GuidRegistry GetOrAddGuidRegistryFor<T>() where T: IHasUniqueID
        {
            bool gotOneReady = typeToRegistryMap.TryGetValue(typeof(T), out var existing);
            if (gotOneReady)
            {
                return existing;
            }

            string assetName = $"{typeof(T).Name}GuidRegistry";
            var result = SOUtils.EnsureSOExists<GuidRegistry>(whereGuidRegistriesGo, assetName);
            result.AddTypeStoredFor<T>();
            typeToRegistryMap[typeof(T)] = result;
            return result;
        }

        private static readonly string whereGuidRegistriesGo = "GuidRegistries"; // Relative to Resources folder

        private static readonly IDictionary<System.Type, GuidRegistry> typeToRegistryMap =
            new Dictionary<System.Type, GuidRegistry>(new TypeNameComparer())
        {
        };

        public static DefaultTweenAdapter DefaultTweener
        {
            get
            {
                EnsureDefaultTweenerAvailable();
                return _defaultTweener;
            }
        }

        private static void EnsureDefaultTweenerAvailable()
        {
            _defaultTweener = SOUtils.EnsureSOExists<DefaultTweenAdapter>(resourcesRootFolder, "DefaultTweenAdapter");
        }

        static DefaultTweenAdapter _defaultTweener;

        volatile static AmanitaManager _s;  // The keyword "volatile" is friendly to multi-threading.
        private static readonly object _ensureLock = new object();

        public static ShadowDatabase ShadowDB
        {
            get
            {
                EnsureShadowDbAvailable();
                return shadowDb;
            }
            private set
            {
                shadowDb = value;
            }
        }

        private static void EnsureShadowDbAvailable()
        {
            if (shadowDb != null)
            {
                return;
            }
            shadowDb = Resources.Load<ShadowDatabase>("ShadowDatabase"); // We expect Lorekeeper to have placed it here.
            if (shadowDb == null)
            {
                Debug.LogError("ShadowDatabase asset not found in Resources/ShadowDatabase.");
            }
        }
        private static readonly string resourcesRootFolder = ""; 
        // ^Relative to Resources folder, hence this being an empty string
        private static ShadowDatabase shadowDb;

        private static void EnsureGuidRegistriesAvailable()
        {
            GetOrAddGuidRegistryFor<Flowchart>();//
            GetOrAddGuidRegistryFor<VariableSourceAsset>();
        }

        /// <summary>
        /// Ensure a single AmanitaManager instance exists in the scene (robust to edit-mode and concurrent calls).
        /// When there are any Flowcharts in the scene editor, there should also be an AmanitaManager in that same scene.
        /// </summary>
        public static AmanitaManager EnsureExists()
        {
            // Fast path
            if (_s != null)
            {
                _s.Init();
                return _s;
            }

            lock (_ensureLock)
            {
                // Double-check after taking the lock
                if (_s != null)
                {
                    _s.Init();
                    return _s;
                }

                _s = FindFirstObjectByType<AmanitaManager>(FindObjectsInactive.Include);
                if (_s != null)
                {
                    _s.gameObject.SetActive(true);
                    _s.Init();
                    return _s;
                }

                bool needToCreateNewOne = _s == null;
                AmanitaManager newlyInstantiated = null;
                if (needToCreateNewOne)
                {
                    newlyInstantiated = CreateNewManager();
                }

                // After creating, re-scan to ensure we didn't race with another instantiation.
                if (!Application.isPlaying)
                {
#if UNITY_EDITOR
                    // Note: FindObjectsOfTypeAll includes stuff in the scene AND project files, even in edit mode.
                    var postAll = Resources.FindObjectsOfTypeAll<AmanitaManager>()
                        .Where((elem) => !EditorUtility.IsPersistent(elem.gameObject) && 
                        elem != newlyInstantiated && elem != null);
                    // ^This Where clause is so we skip project files. Apparently, FindFirstObjectByType can miss
                    // stuff in the scene.

                    // Prefer an existing one that is not the newly instantiated one
                    AmanitaManager keeper = postAll.FirstOrDefault();
                    if (keeper != null)
                    {
                        // Another instance won the race. Thus...
                        DestroyImmediate(newlyInstantiated.gameObject);
                        _s = keeper;
                        _s.Init();
                        return _s;
                    }
#endif
                }

                // Otherwise keep the instantiated one
                _s = newlyInstantiated;
                _s.Init();
                return _s;
            }
        }

        private static AmanitaManager CreateNewManager()
        {
            AmanitaManager prefab = Resources.Load<AmanitaManager>(AmanitaConstants.PathToAmanitaManagerPrefab);
            if (prefab == null)
            {
                Debug.LogError($"AmanitaManager prefab not found at Resources/{AmanitaConstants.PathToAmanitaManagerPrefab}.");
                return null;
            }

            // Resources.Load may call Awake on the prefab's script in some Unity versions,
            // so we null-check again after instantiation.
            AmanitaManager instantiated;
#if UNITY_EDITOR
            instantiated = PrefabUtility.InstantiatePrefab(prefab) as AmanitaManager;
#else
            instantiated = Instantiate(prefab);
#endif

            instantiated.gameObject.name = prefab.name; // We don't want "Clone" in the name
            return instantiated;
        }

        public void Init()
        {
            if (IsFullyInitted)
            {
                return;
            }

            // We do this in both inits since not all scenes will necessarily have a Flowchart that
            // will ensure an instance of this exists.
            bool thisIsDuplicate = S != this && S != null;
            if (thisIsDuplicate)
            {
                Debug.Log("AmanitaManager instance already exists. Destroying the new one.");
                Destroy(this.gameObject);
                return;
            }
            _s = this;

            EnsureShadowDbAvailable();
            EnsureGuidRegistriesAvailable();

            VariableRegistry = new VariableRegistry(this);

            ResetAnchors();
            void ResetAnchors()
            {
                // Destroy any existing anchors managed by this instance (defensive cleanup).
                if (_adapterAnchors != null)
                {
                    foreach (var kv in _adapterAnchors)
                    {
                        var anchorFound = kv.Value;
                        if (anchorFound == null) continue;

                        if (!Application.isPlaying)
                        {
                            DestroyImmediate(anchorFound);
                        }
                        else
                        {
                            Destroy(anchorFound);
                        }
                    }
                    _adapterAnchors.Clear();
                }
            }

            // So GetOrCreateAnchorFor can parent anchors.
            EnsureTweenAnchorHolder();
            PrepSubmodules();

        }

        public IReadOnlyList<Flowchart> FlowchartsInScene
        {
            get
            {
                var result = FlowchartRegistry.GetFlowcharts();
                return result;
            }
        }

        public static SaveMenuManager SaveMenu { get; private set; }

        public bool IsFullyInitted
        {
            get => (TweenManager != null && TweenManager.IsFullyInitted) &&
                (NarrativeLog != null && NarrativeLog.IsFullyInitted) &&
                (AudioSystem != null && AudioSystem.IsFullyInitted) &&
                (SaveSysInstaller != null && SaveSysInstaller.IsFullyInitted);
        }

        private void PrepSubmodules()
        {
            // We assume that these are each on separate GameObjects (for the sake of easier testing)
            this.gameObject.GetOrAddComponent<AmanitaState>();
            CameraManager = GetComponentInChildren<CameraManager>();
            EventDispatcher = GetComponentInChildren<EventDispatcher>();
            NarrativeLog = GetComponentInChildren<NarrativeLog>();
            AudioSystem = GetComponentInChildren<AudioSystem>();
            SaveSysInstaller = GetComponentInChildren<SaveSystemInstaller>();
            TweenManager = GetComponentInChildren<TweenManager>();
            SaveMenu = GetComponentInChildren<SaveMenuManager>();

            InitAll();
            void InitAll()
            {
                // The order here matters
                TweenManager.Init();
                NarrativeLog.Init();
                AudioSystem.Init();
                SaveSysInstaller.Init();
            }
        }

        private void EnsureTweenAnchorHolder()
        {
            if (tweenAnchorHolder == null)
            {
                tweenAnchorHolder = new GameObject("TweenAnchorHolder");
                tweenAnchorHolder.transform.SetParent(this.transform, false);
#if UNITY_EDITOR
                tweenAnchorHolder.hideFlags = HideFlags.HideAndDontSave;
#else
                tweenAnchorHolder.hideFlags = HideFlags.HideInInspector;
#endif
            }
        }

        private void Awake()
        {
            if (_s != null && _s != this)
            {
                CleanSelfUp();
                void CleanSelfUp()
                {
                    string logMessage = "AmanitaManager instance already exists. Destroying the new one.";
                    Debug.Log(logMessage);
                    if (!Application.isPlaying)
                    {
                        // Since DestroyImmediate doesn't call OnDestroy...
                        OnDestroy();
                        if (AudioSystem != null)
                        {
                            AudioSystem.OnDestroy();
                        }
                        DestroyImmediate(this.gameObject); // Prevents duplicates in edit mode
                    }
                    else
                    {
                        Destroy(this.gameObject);
                    }
                }
                return;
            }
            
            _s = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            Init();
        }

        private SaveSystemInstaller SaveSysInstaller { get; set; }

        private TweenManager TweenManager { get; set; }
        #region Public methods

        /// <summary>
        /// Gets the camera manager singleton instance.
        /// </summary>
        public CameraManager CameraManager { get; private set; }

        /// <summary>
        /// Gets the event dispatcher singleton instance.
        /// </summary>
        public EventDispatcher EventDispatcher { get; private set; }

        /// <summary>
        /// Gets the save manager singleton instance.
        /// </summary>
        public SaveManager SaveManager { get; private set; }

        /// <summary>
        /// Gets the history manager singleton instance.
        /// </summary>
        public NarrativeLog NarrativeLog { get; private set; }
        
        /// <summary>
        /// Gets the FungusManager singleton instance.
        /// </summary>
        public static AmanitaManager S
        {
            get => _s;
            set => _s = value;
        }

        #endregion

        public static void ResetStaticsForTest()
        {
            S = null;
        }

        public AudioSystem AudioSystem { get; private set; }

        private void OnDestroy()
        {
            if (_s == this)
            {
                // Clean up anchors we created
                if (_adapterAnchors != null)
                {
                    foreach (var kv in _adapterAnchors)
                    {
                        var go = kv.Value;
                        if (go == null) continue;
#if UNITY_EDITOR
                        if (Application.isPlaying)
                            Destroy(go);
                        else
                            DestroyImmediate(go);
#else
                        Destroy(go);
#endif
                    }
                    _adapterAnchors.Clear();
                }

                _s = null;

                SaveSystem.S = null;
            }
        }

        /// <summary>
        /// Return an existing anchor GameObject for the given adapter (Unity object),
        /// or create one as a child of the manager. Anchor lifetime follows the manager.
        /// </summary>
        public GameObject GetOrCreateAnchorFor(UnityObj unityObj)
        {
            if (unityObj == null) return null;

            EnsureTweenAnchorHolder();

            int key = unityObj.GetInstanceID();

            // Try dictionary first (fast path)
            if (_adapterAnchors.TryGetValue(key, out var existing) && existing != null)
                return existing;

            // Try to find by deterministic name (useful across domain reloads)
            string anchorName = $"{unityObj.GetType().Name}_AdapterAnchor_{key}";
            Transform found = tweenAnchorHolder.transform.Find(anchorName);
            if (found != null && found.gameObject != null)
            {
                _adapterAnchors[key] = found.gameObject;
                return found.gameObject;
            }

            // Create new anchor
            GameObject anchor = new GameObject(anchorName);
            anchor.transform.SetParent(tweenAnchorHolder.transform, false);

#if UNITY_EDITOR
            anchor.hideFlags = HideFlags.HideAndDontSave;
#else
            anchor.hideFlags = HideFlags.HideInInspector;
#endif

            _adapterAnchors[key] = anchor;
            return anchor;
        }

        /// <summary>
        /// Remove and destroy anchor for given adapter (if any).
        /// </summary>
        public void RemoveAnchorFor(UnityObj unityObj)
        {
            if (unityObj == null) return;
            int key = unityObj.GetInstanceID();
            if (_adapterAnchors.TryGetValue(key, out var go) && go != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(go);
                else
                    DestroyImmediate(go);
#else
                Destroy(go);
#endif
            }
            _adapterAnchors.Remove(key);
        }

        // replaced the old list with a dictionary keyed by adapter instance id
        private readonly Dictionary<int, GameObject> _adapterAnchors = new Dictionary<int, GameObject>();
        public VariableRegistry VariableRegistry { get; private set; }
        private void OnValidate()
        {
            // OnValidate gets called on the prefab in response to Resources.Load(), so...
            if (!this.gameObject.scene.IsValid() || (S != null && S != this))
            {
                return;
            }
            S = this;
            // Best make sure to log errors and such when this has any screwy fields
            if (globalVariables == null)
            {
                Debug.LogError("AmanitaManager has no globalVariables list assigned.");
            }
            else if (globalVariables.Any(elem => elem == null))
            {
                Debug.LogError("AmanitaManager has null global variable sources.");
            }

            EnsureVariableRegistryIsReady();

        }

        private void EnsureVariableRegistryIsReady()
        {
            if (VariableRegistry == null)
            {
                VariableRegistry = new VariableRegistry(this);
                var selected = Selection.activeGameObject;
                Flowchart currentFc = null;
                if (selected != null)
                {
                    selected.TryGetComponent(out currentFc);
                }
                VariableRegistry.Rebuild(currentFc);
            }
        }

        private void OnEnable()
        {
            EnsureVariableRegistryIsReady();
        }
    }
}