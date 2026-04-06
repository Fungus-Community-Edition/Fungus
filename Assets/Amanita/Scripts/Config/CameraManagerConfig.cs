using UnityEngine;

namespace AtMycelia.Amanita
{
    [CreateAssetMenu(fileName = "CameraManagerConfig", menuName = "Atelier Mycelia/Amanita/Config/CameraManager")]
    public class CameraManagerConfig : ScriptableObject
    {
        [Tooltip("Full screen texture used for screen fade effect.")]
        [SerializeField] private Texture2D screenFadeTexture;

        [Tooltip("Icon to display when swipe pan mode is active.")]
        [SerializeField] private Texture2D swipePanIcon;

        [Tooltip("Position of continue and swipe icons in normalized screen space coords. (0,0) = top left, (1,1) = bottom right")]
        [SerializeField] private Vector2 swipeIconPosition = new Vector2(1, 0);

        [Tooltip("Set the camera z coordinate to a fixed value every frame.")]
        [SerializeField] private bool setCameraZ = true;

        [Tooltip("Fixed Z coordinate of main camera.")]
        [SerializeField] private float cameraZ = -10f;

        [Tooltip("Multiplier applied to swipe movement when panning.")]
        [SerializeField] private float swipeSpeedMultiplier = 1f;

        public Texture2D ScreenFadeTexture => screenFadeTexture;
        public Texture2D SwipePanIcon => swipePanIcon;
        public Vector2 SwipeIconPosition => swipeIconPosition;
        public bool SetCameraZ => setCameraZ;
        public float CameraZ => cameraZ;
        public float SwipeSpeedMultiplier => swipeSpeedMultiplier;
    }
}