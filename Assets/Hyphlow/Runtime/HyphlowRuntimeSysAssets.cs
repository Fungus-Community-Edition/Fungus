using AtMycelia.AmaniTween;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AtMycelia.Hyphlow.Sys
{
    /// <summary>
    /// This is basically a settings file for resources that Hyphlow's runtime systems rely on.
    /// These are meant to be assigned by devs in the Inspector. If you're a user, you're free
    /// to mess with this as well, but be aware that changing these at runtime may cause
    /// unintended consequences if you don't know what you're doing. 
    /// </summary>
    public sealed class HyphlowRuntimeSysAssets : ScriptableObject
    {
        [SerializeField] private DefaultTweenAdapter _tweenAdapter;
        [SerializeField] private VariableRegistryConfig _variableRegistryConfig;

        public DefaultTweenAdapter TweenAdapter
        {
            get => _tweenAdapter;
            set
            {
                if (Application.isPlaying)
                {
                    ShowWarningAboutPlayModeMutations();
                    return;
                }

                if (_tweenAdapter == value)
                {
                    return;
                }

                _tweenAdapter = value;
                this.MarkDirtyAndSave();
            }
        }

        public VariableRegistryConfig VariableRegistryConfig
        {
            get => _variableRegistryConfig;
            set
            {
                if (Application.isPlaying)
                {
                    ShowWarningAboutPlayModeMutations();
                    return;
                }

                if (_variableRegistryConfig == value)
                {
                    return;
                }

                _variableRegistryConfig = value;
                this.MarkDirtyAndSave();
            }
        }

        private static void ShowWarningAboutPlayModeMutations()
        {
            string errorMessage =
                $"Cannot set the contents of a HyphlowRuntimeSysAssets in Play Mode! Ignoring attempt.";
            //Debug.LogWarning(errorMessage);
        }

        private void Awake()
        {
            if (S != null && S != this)
            {
                string errorMessage = $"Multiple instances of HyphlowRuntimeSysAssets detected! This is not intended. " +
                                      $"Destroying the new instance. Existing instance: {S.name} at " +
                                      $"{AssetDatabase.GetAssetPath(S)}. New instance: {name} at " +
                                      $"{AssetDatabase.GetAssetPath(this)}.";
                Debug.LogError(errorMessage);
                Destroy(this);
                return;
            }

            S = this;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (S == null)
            {
                S = this; // Can happen between domain reloads
            }
#endif
        }

        // Singleton
        public static HyphlowRuntimeSysAssets S { get; set; }

        private void OnDestroy()
        {
            if (S == this)
            {
                S = null;
            }
        }
    }
}