using UnityEngine;

namespace AtMycelia.Amanita
{
    [DisallowMultipleComponent]
    public class AmanitaSceneConfigOverrides : MonoBehaviour
    {
        [SerializeField] private CameraManagerConfig cameraManagerConfigOverride;

        public CameraManagerConfig CameraManagerConfigOverride => cameraManagerConfigOverride;
    }
}