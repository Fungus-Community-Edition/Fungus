using System.Collections.Generic;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// In charge of handling the essential MonoBehaviours and GameObjects for the Hyphlow system,
    /// such as TweenManager.
    /// </summary>
    public sealed class HyphlowManager : MonoBehaviour
    {
        [SerializeField, HideInInspector] private GameObject _tweenAnchorHolder;

        public static HyphlowManager S
        {
            get => _s;
            private set => _s = value;
        }

        private static HyphlowManager _s;

        private readonly Dictionary<int, GameObject> _adapterAnchors = new Dictionary<int, GameObject>();

        private void Awake()
        {
            if (_s != null && _s != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _s = this;
            DontDestroyOnLoad(this.gameObject);
            ResetAnchors();
            EnsureTweenAnchorHolder();
            EventDispatcher = this.gameObject.GetOrAddComponent<EventDispatcher>();
        }

        public EventDispatcher EventDispatcher { get; private set; }

        private void ResetAnchors()
        {
            foreach (var kv in _adapterAnchors)
            {
                var anchorFound = kv.Value;
                if (anchorFound == null)
                {
                    continue;
                }

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

        private void EnsureTweenAnchorHolder()
        {
            if (_tweenAnchorHolder == null)
            {
                _tweenAnchorHolder = new GameObject("TweenAnchorHolder");
                _tweenAnchorHolder.transform.SetParent(this.transform, false);
#if UNITY_EDITOR
                _tweenAnchorHolder.hideFlags = HideFlags.HideAndDontSave;
#else
                tweenAnchorHolder.hideFlags = HideFlags.HideInInspector;
#endif
            }
        }

        private void OnDestroy()
        {
            if (_s == this)
            {
                _s = null;
            }

            ResetAnchors();
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

            if (_adapterAnchors.TryGetValue(key, out var existing) && existing != null)
            {
                return existing;
            }

            string anchorName = $"{unityObj.GetType().Name}_AdapterAnchor_{key}";
            Transform found = _tweenAnchorHolder.transform.Find(anchorName);
            if (found != null && found.gameObject != null)
            {
                _adapterAnchors[key] = found.gameObject;
                return found.gameObject;
            }

            GameObject anchor = new GameObject(anchorName);
            anchor.transform.SetParent(_tweenAnchorHolder.transform, false);

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
                {
                    Destroy(go);
                }
                else
                {
                    DestroyImmediate(go);
                }
#else
                Destroy(go);
#endif
            }

            _adapterAnchors.Remove(key);
        }
    }
}