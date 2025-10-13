using Amanita.Tweening;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Fades the camera out and in again at a position specified by a View object.
    /// </summary>
    [CommandInfo("Camera", 
                 "Fade To View", 
                 "Fades the camera out and in again at a position specified by a View object.")]
    [AddComponentMenu("")]
    public class FadeToView : Command 
    {
        [Tooltip("Time for fade effect to complete")]
        [SerializeField] protected float duration = 1f;

        [Tooltip("Fade from fully visible to opaque at start of fade")]
        [SerializeField] protected bool fadeOut = true;

        [Tooltip("View to transition to when Fade is complete")]
        [SerializeField] protected View targetView;

        [Tooltip("Wait until the fade has finished before executing next command")]
        [SerializeField] protected bool waitUntilFinished = true;

        [Tooltip("Color to render fullscreen fade texture with when screen is obscured.")]
        [SerializeField] protected Color fadeColor = Color.black;

        [Tooltip("Optional texture to use when rendering the fullscreen fade effect.")]
        [SerializeField] protected Texture2D fadeTexture;

        [Tooltip("Camera to use for the fade. Will use main camera if set to none.")]
        [SerializeField] protected Camera targetCamera;

        [SerializeField] protected ScriptableObject fadeTweener, orthoSizeTweener, posTweener, rotTweener;

        protected virtual void Awake()
        {
            ValidateTweeners();
        }

        protected virtual void Start()
        {
            AcquireCamera();
        }

        protected virtual void AcquireCamera()
        {
            if (targetCamera != null)
            {
                return;
            }

            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = GameObject.FindFirstObjectByType<Camera>();
            }
        }

        #region Public members

        /// <summary>
        /// View to transition to when Fade is complete
        /// </summary>
        public virtual View TargetView { get { return targetView; } }

        public override void OnEnter()
        {
            AcquireCamera();
            if (targetCamera == null ||
                targetView == null)
            {
                Continue();
                return;
            }

            var cameraManager = AmanitaManager.S.CameraManager;

            if (fadeTexture)
            {
                cameraManager.ScreenFadeTexture = fadeTexture;
            }
            else
            {
                cameraManager.ScreenFadeTexture = CameraManager.CreateColorTexture(fadeColor, 32, 32);
            }

            cameraManager.FadeToView(targetCamera, targetView, duration, fadeOut, delegate { 
                if (waitUntilFinished)
                {
                    Continue();
                }
            }, doFadeTween, doOrthoSizeTween, doPosTween, doPosTween);

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override void OnStopExecuting()
        {
            var cameraManager = AmanitaManager.S.CameraManager;

            cameraManager.Stop();
        }

        public override string GetSummary()
        {
            if (targetView == null)
            {
                return "Error: No view selected";
            }
            else
            {
                return targetView.name;
            }
        }

        public override Color GetButtonColor()
        {
            return new Color32(216, 228, 170, 255);
        }

        #endregion

        public override void OnValidate()
        {
            base.OnValidate();
            ValidateTweeners();
            
        }

        protected virtual void ValidateTweeners()
        {
            ApplyDefaultsAsInits();
            void ApplyDefaultsAsInits()
            {
                if (fadeTweener == null)
                {
                    fadeTweener = AmanitaManager.DefaultTweener;
                }

                if (orthoSizeTweener == null)
                {
                    orthoSizeTweener = AmanitaManager.DefaultTweener;
                }

                if (posTweener == null)
                {
                    posTweener = AmanitaManager.DefaultTweener;
                }

                if (rotTweener == null)
                {
                    rotTweener = AmanitaManager.DefaultTweener;
                    return;
                }
            }

            CheckCurrentlyAppliedTweeners();
            void CheckCurrentlyAppliedTweeners()
            {
                doFadeTween = fadeTweener as IGeneralTweenAdapter<float>;
                doOrthoSizeTween = orthoSizeTweener as ICameraTweenAdapter;
                doPosTween = posTweener as ITransformTweenAdapter;
                doRotTween = rotTweener as ITransformTweenAdapter;

                string logMessage = "";
                if (doFadeTween == null)
                {
                    logMessage = "Fade tweener assigned is invalid. It needs to implement IGeneralTweenAdapter<float>.\n";
                    doFadeTween = AmanitaManager.DefaultTweener;
                }

                if (doOrthoSizeTween == null)
                {
                    logMessage += "Ortho size tweener assigned is invalid. It needs to implement ICameraTweenAdapter.\n";
                    doOrthoSizeTween = AmanitaManager.DefaultTweener;
                }

                if (doPosTween == null)
                {
                    logMessage += "Pos tweener assigned is invalid. I needs to implement ITransformTweenAdapter.\n";
                    doPosTween = AmanitaManager.DefaultTweener;
                }

                if (doRotTween == null)
                {
                    logMessage = "Rotation tweener assigned is invalid. It needs to implement ITransformTweenAdapter.\n";
                    doRotTween = AmanitaManager.DefaultTweener;
                }

                if (!string.IsNullOrEmpty(logMessage))
                {
                    logMessage += "Reverting to defaults.";
                    Debug.LogWarning(logMessage);
                }
            }
        }

        protected IGeneralTweenAdapter<float> doFadeTween;
        protected ICameraTweenAdapter doOrthoSizeTween;
        protected ITransformTweenAdapter doPosTween, doRotTween;
    }
}
