using Amanita.DialogueSys;
using Amanita.Myceliaudio;
using Amanita.SaveSys;
using Amanita.Tweening;
using Amanita.VScripting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita
{
    /// <summary>
    /// Amanita manager singleton. Manages access to all Amanita singletons in a consistent manner.
    /// </summary>
    public sealed class AmanitaManager : MonoBehaviour
    {
        [SerializeField] private List<VariableSourceAsset> globalVariables;
        [SerializeField, HideInInspector] private GameObject tweenAnchorHolder;

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
                    result.AddRange(src.Variables.Where(v => v != null));
                }
                return result;
            }
        }

        public IList<VariableSourceAsset> GlobalVariableSources
        {
            get => globalVariables;
            set
            {
                globalVariables.Clear();
                globalVariables.AddRange(value);
            }
        }

        public static DefaultTweenAdapter DefaultTweener
        {
            get
            {
                if (_defaultTweener == null)
                {
                    _defaultTweener = Resources.Load<DefaultTweenAdapter>(pathToAdapter);
#if UNITY_EDITOR
                    if (_defaultTweener == null)
                    {
                        Debug.LogWarning($"No TweenAdapter found at Resources/{pathToAdapter}. Creating a new one.");
                        _defaultTweener = TweenAdapterUtility.GetOrCreateDefaultAdapter();
                    }
#else
                    if (_adapter == null)
                    {
                        _adapter = ScriptableObject.CreateInstance<DefaultTweenAdapter>();
                    }
#endif
                }

                return _defaultTweener;
            }
        }

        static DefaultTweenAdapter _defaultTweener;
        static string pathToAdapter = "DefaultTweenAdapter";

        volatile static AmanitaManager _s;  // The keyword "volatile" is friendly to the multi-thread.
        private static readonly object _ensureLock = new object();

        /// <summary>
        /// Ensure a single AmanitaManager instance exists in the scene (robust to edit-mode and concurrent calls).
        /// </summary>
        public static AmanitaManager EnsureExists()
        {
            // Fast path
            if (_s != null)
            {
                return _s;
            }

            lock (_ensureLock)
            {
                // Double-check after taking the lock
                if (_s != null)
                    return _s;

#if UNITY_EDITOR
                // In the editor, include inactive scene objects but skip prefab assets in Resources
                List<AmanitaManager> allManagers;

#if UNITY_6000_0_OR_NEWER
                allManagers = UnityObj.FindObjectsByType<AmanitaManager>(FindObjectsSortMode.None).ToList();
#else
                allManagers = UnityObj.FindObjectsOfType<AmanitaManager>(true);
#endif
                AmanitaManager sceneInstance = allManagers.FirstOrDefault();
                if (sceneInstance != null)
                {
                    _s = sceneInstance;
                    _s.Init();
                    CleanUpOtherInstances();
                    void CleanUpOtherInstances()
                    {
                        allManagers.Remove(sceneInstance);
                        foreach (var dup in allManagers)
                        {
                            if (dup != null && dup != _s)
                            {
                                Debug.Log("AmanitaManager instance already exists. Destroying the new one.");
                                UnityObj.DestroyImmediate(dup.gameObject);
                            }
                        }
                        allManagers.Clear(); // Help GC
                    }
                    return _s;
                }
#else
                // Runtime: regular lookup (active scene)
                var existing = UnityObj.FindObjectOfType<AmanitaManager>();
                if (existing != null)
                {
                    _s = existing;
                    _s.Init();
                    return _s;
                }
#endif

                // None found -> try to instantiate from prefab
                AmanitaManager prefab = Resources.Load<AmanitaManager>(AmanitaConstants.PathToAmanitaManagerPrefab);
                if (prefab == null)
                {
                    Debug.LogError($"AmanitaManager prefab not found at Resources/{AmanitaConstants.PathToAmanitaManagerPrefab}.");
                    return null;
                }

                // Instantiate the prefab. Resources.Load may call Awake on the prefab's script in some Unity versions,
                // so we null-check again after instantiation.
                AmanitaManager instantiated = Instantiate(prefab);
                instantiated.gameObject.name = prefab.name; // remove "(Clone)"

                // After creating, re-scan to ensure we didn't race with another instantiation.
#if UNITY_EDITOR
                var postAll = Resources.FindObjectsOfTypeAll<AmanitaManager>();
                AmanitaManager keeper = null;
                foreach (var elem in postAll)
                {
                    // Skip assets (prefabs)
                    #if UNITY_EDITOR
                    if (UnityEditor.EditorUtility.IsPersistent(elem.gameObject))
                        continue;
                    #endif
                    // Prefer an existing one that is not the newly instantiated one
                    if (keeper == null)
                        keeper = elem;
                }

                if (keeper != null && keeper != instantiated)
                {
                    // Another instance won the race. Destroy the one we just created.
                    DestroyImmediate(instantiated.gameObject);
                    _s = keeper;
                    _s.Init();
                    return _s;
                }
#endif

                // Otherwise keep the instantiated one
                _s = instantiated;
                _s.Init();
                return _s;
            }
        }

        public void Init()
        {
            if (IsInitted)
            {
                return;
            }

            bool thisIsDuplicate = S != this && S != null;
            if (thisIsDuplicate)
            {
                Debug.Log("AmanitaManager instance already exists. Destroying the new one.");
                Destroy(this.gameObject);
                return;
            }

            _s = this;

            if (S == null)
            {
                Debug.LogError($"AmanitaManager's claim to the S field was ignored.");
            }

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
#if UNITY_EDITOR
                        DestroyImmediate(anchorFound);
#else
                        Destroy(anchorFound);
#endif
                    }
                    _adapterAnchors.Clear();
                }

                // Ensure a tween anchor holder exists so GetOrCreateAnchorFor can parent anchors.
                EnsureTweenAnchorHolder();
                
            }
            PrepSubmodules();

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(_s.gameObject);
            }
            IsInitted = true;

        }

        private void PrepSubmodules()
        {
            // We assume that these are each on separate GameObjects (for the sake of easier testing)
            CameraManager = GetComponentInChildren<CameraManager>();
            EventDispatcher = GetComponentInChildren<EventDispatcher>();
            MainAudioMixer = GetComponentInChildren<MainAudioMixer>();
            NarrativeLog = GetComponentInChildren<NarrativeLog>();
            AudioSystem = GetComponentInChildren<AudioSystem>();
            SaveSysInstaller = GetComponentInChildren<SaveSystemInstaller>();
            TweenManager = GetComponentInChildren<TweenManager>();

            InitAll();
            void InitAll()
            {
                // The order here matters
                TweenManager.Init();
                NarrativeLog.Init();
                MainAudioMixer.Init();
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
            if (_s == null)
            {
                _s = this;
                if (Application.isPlaying)
                    DontDestroyOnLoad(gameObject);

                Init();
            }
            else if (_s != this)
            {
                if (!Application.isPlaying)
                {
                    // Since DestroyImmediate doesn't call OnDestroy...
                    OnDestroy();
                    if (AudioSystem != null)
                    {
                        AudioSystem.OnDestroy();
                    }
                    DestroyImmediate(gameObject); // Prevents duplicates in edit mode
                }
                else
                    Destroy(gameObject);
                return;
            }
            else if (IsInitted == false)
            {
                Init();
            }
        }

        public bool IsInitted { get; private set; } = false;

        private SaveSystemInstaller SaveSysInstaller { get; set; }

        private TweenManager TweenManager { get; set; }
        #region Public methods

        /// <summary>
        /// Gets the camera manager singleton instance.
        /// </summary>
        public CameraManager CameraManager { get; private set; }

        /// <summary>
        /// Gets the music manager singleton instance.
        /// </summary>
        public MusicManager MusicManager { get; private set; }

        /// <summary>
        /// Gets the event dispatcher singleton instance.
        /// </summary>
        public EventDispatcher EventDispatcher { get; private set; }

        public MainAudioMixer MainAudioMixer { get; private set; }

#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Gets the save manager singleton instance.
        /// </summary>
        public SaveManager SaveManager { get; private set; }
        
        /// <summary>
        /// Gets the history manager singleton instance.
        /// </summary>
        public NarrativeLog NarrativeLog { get; private set; }
        
#endif

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
                IsInitted = false;

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

        private readonly static string anchorNameSuffix = "_TweenAnchor";

        // replaced the old list with a dictionary keyed by adapter instance id
        private readonly Dictionary<int, GameObject> _adapterAnchors = new Dictionary<int, GameObject>();
    }
}