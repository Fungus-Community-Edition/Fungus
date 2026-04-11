using AtMycelia.SaveSys;
using AtMycelia.Hyphlow.Tweening;
using FullSerializer;
using Lorekeeper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;
#if ENABLE_INPUT_SYSTEM
using AtMycelia.Amanita.DialogueSys;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AtMycelia.Amanita
{
    /// <summary>
    /// Amanita manager singleton. Manages access to all Amanita singletons in a consistent manner.
    /// </summary>
    public sealed class AmanitaManager : MonoBehaviour, ITearDownResponder
    {
        [SerializeField, HideInInspector] private GameObject tweenAnchorHolder;

        public static fsSerializer DefaultSerializer { get; } = new fsSerializer();

        volatile static AmanitaManager _s;  // The keyword "volatile" is friendly to multi-threading.
        private static readonly object _ensureLock = new object();

        public static ShadowDatabase ShadowDB
        {
            get
            {
                EnsureShadowDbAvailable();
                return _shadowDb;
            }
            private set
            {
                _shadowDb = value;
            }
        }

        private static void EnsureShadowDbAvailable()
        {
            if (_shadowDb != null)
            {
                return;
            }
            _shadowDb = Resources.Load<ShadowDatabase>("ShadowDatabase"); // We expect Lorekeeper to have placed it here.
            if (_shadowDb == null)
            {
                Debug.LogError("ShadowDatabase asset not found in Resources/ShadowDatabase.");
            }
        }

        private static ShadowDatabase _shadowDb;

        /// <summary>
        /// Ensure a single AmanitaManager instance exists in the scene (robust to edit-mode and concurrent calls).
        /// When there are any Flowcharts in the scene editor, there should also be an AmanitaManager in that same scene.
        /// </summary>
        public static AmanitaManager EnsureExists()
        {
            // Fast path
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
                        .Where((elem) => !EditorUtility.IsPersistent(elem.gameObject) && elem != 
                        newlyInstantiated && elem != null);
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
            AmanitaManager prefab = Resources.Load<AmanitaManager>(_pathToPrefab);
            if (prefab == null)
            {
                string errorMessage = $"AmanitaManager prefab not found at Resources/{_pathToPrefab}. " +
                    $"Please ensure it exists and is located there.";
                Debug.LogError(errorMessage);
                return null;
            }

            AmanitaManager manager = Instantiate(prefab);
            return manager;
        }

        private static readonly string _pathToPrefab = "Prefabs/AmanitaManager"; // Relative to Resources

        public void Init()
        {
            if (IsFullyInitted || 
                this.gameObject.scene == default ||
                this.gameObject.scene.name == this.name) // <- This can happen when we're in prefab mode
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

        public bool IsFullyInitted
        {
            get => (NarrativeLog != null && NarrativeLog.IsFullyInitted);
        }

        private void PrepSubmodules()
        {
            // We assume that these are each on separate GameObjects (for the sake of easier testing)
            FetchSubmodules();
            void FetchSubmodules()
            {
                CameraManager = GetComponentInChildren<CameraManager>();
                EventDispatcher = GetComponentInChildren<EventDispatcher>();
                NarrativeLog = GetComponentInChildren<NarrativeLog>();
            }

            ApplySceneOverrides();

            List<IAmanitaManagerSubmodule> submodules = GetComponentsInChildren<IAmanitaManagerSubmodule>().ToList();
            // Lower order index, earlier execution
            submodules.Sort((first, second) => first.OrderIndex.CompareTo(second.OrderIndex));
            for (int i = 0; i < submodules.Count; i++)
            {
                var module = submodules[i];
                module.Init();
            }
        }

        public void ApplySceneOverrides()
        {
            if (CameraManager != null)
            {
                CameraManager.ApplyConfig(AmanitaConfigResolver.ResolveCameraManagerConfig());
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
            Init();

            if (Application.IsPlaying(this))
            {
                DontDestroyOnLoad(gameObject);
            }
        }

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

        private void OnDestroy()
        {
            if (_s == this)
            {
                _s = null;
                TweenManager.S = null;

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
        private void OnValidate()
        {
            // OnValidate gets called on the prefab in response to Resources.Load(), so...
            if (!this.gameObject.scene.IsValid() || (S != null && S != this))
            {
                return;
            }
            S = this;

        }

        private void OnEnable()
        {
            ToggleSubs(true);
        }

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                SaveSysSignals.SaveLoaded += OnSaveSlotLoaded;
            }
            else
            {
                SaveSysSignals.SaveLoaded -= OnSaveSlotLoaded;
            }
        }

        private void OnSaveSlotLoaded(CompositeSaveData saveData)
        {
        }

#if UNITY_EDITOR
        public void OnTearDown()
        {
            List<ITearDownResponder> responders = new List<ITearDownResponder>();
            foreach (var submodule in GetComponentsInChildren<IAmanitaManagerSubmodule>())
            {
                if (submodule is ITearDownResponder responder)
                {
                    responders.Add(responder);
                }
            }
            foreach (var responder in responders)
            {
                responder.OnTearDown();
            }
        }
#endif

        private void OnDisable()
        {
            ToggleSubs(false);
        }
    }
}