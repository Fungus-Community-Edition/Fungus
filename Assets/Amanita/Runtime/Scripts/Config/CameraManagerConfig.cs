using UnityEngine;

namespace AtMycelia.Amanita
{
    [CreateAssetMenu(fileName = "CameraManagerConfig", menuName = "Atelier Mycelia/Amanita/Config/CameraManager")]
    public class CameraManagerConfig : ScriptableObject
    {
        [Tooltip("Full screen texture used for screen fade effect.")]
        [SerializeField] private Texture2D _screenFadeTexture;

        [Tooltip("Icon to display when swipe pan mode is active.")]
        [SerializeField] private Texture2D _swipePanIcon;

        [Tooltip("Position of continue and swipe icons in normalized screen space coords. " +
            "(0,0) = top left, (1,1) = bottom right")]
        [SerializeField] private Vector2 _swipeIconPosition = new Vector2(1, 0);

        /// <summary>
        /// Whether to apply a fixed Z coordinate to the main camera. This is
        /// useful for 2D games where you want to ensure the camera stays
        /// at a specific depth.
        /// </summary>
        [Tooltip("Whether to apply a fixed Z coordinate to the main camera.")]
        [SerializeField] private bool _applyFixedCamZ = true;

        [Tooltip("Fixed Z coordinate of main camera.")]
        [SerializeField] private float _cameraZ = -10f;

        [Tooltip("Multiplier applied to swipe movement when panning.")]
        [SerializeField] private float _swipeSpeedMultiplier = 1f;

        public Texture2D ScreenFadeTexture
        {
            get => _screenFadeTexture;
            set => _screenFadeTexture = value;
        }
        public Texture2D SwipePanIcon => _swipePanIcon;
        public Vector2 SwipeIconPosition => _swipeIconPosition;
        public bool ApplyFixedCamZ => _applyFixedCamZ;
        public float CameraZ => _cameraZ;
        public float SwipeSpeedMultiplier
        {
            get => _swipeSpeedMultiplier;
            set => _swipeSpeedMultiplier = Mathf.Max(0f, value); 
            // ^We don't want it going into the negatives
        }
    }
}