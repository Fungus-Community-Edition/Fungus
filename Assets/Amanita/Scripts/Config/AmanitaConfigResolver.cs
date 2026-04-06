using UnityEngine;
using AtMycelia;

namespace AtMycelia.Amanita
{
    public static class AmanitaConfigResolver
    {
        public const string ResourcesRoot = "AtMycelia/Amanita";
        private const string CameraManagerConfigAssetName = "CameraManagerConfig";

        public static CameraManagerConfig GetDefaultCameraManagerConfig()
        {
#if UNITY_EDITOR
            return SOUtils.EnsureSOExists<CameraManagerConfig>(ResourcesRoot, CameraManagerConfigAssetName);
#else
            string path = $"{ResourcesRoot}/{CameraManagerConfigAssetName}";
            CameraManagerConfig config = Resources.Load<CameraManagerConfig>(path);
            if (config == null)
            {
                Debug.LogError($"CameraManagerConfig not found at Resources/{path}.");
            }
            return config;
#endif
        }

        public static CameraManagerConfig ResolveCameraManagerConfig()
        {
            CameraManagerConfig defaultConfig = GetDefaultCameraManagerConfig();
            AmanitaSceneConfigOverrides overrides = Object.FindFirstObjectByType<AmanitaSceneConfigOverrides>(FindObjectsInactive.Include);
            if (overrides != null && overrides.CameraManagerConfigOverride != null)
            {
                return overrides.CameraManagerConfigOverride;
            }

            return defaultConfig;
        }
    }
}