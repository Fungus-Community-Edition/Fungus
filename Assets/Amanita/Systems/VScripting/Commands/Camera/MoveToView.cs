using UnityEngine;
using Amanita.Tweening;

namespace Amanita.VScripting
{
    /// <summary>
    /// Moves the camera to a location specified by a View object.
    /// </summary>
    [CommandInfo("Camera", 
                 "Move To View", 
                 "Moves the camera to a location specified by a View object.")]
    [AddComponentMenu("")]
    public class MoveToView : Command 
    {
        [Tooltip("Time for move effect to complete")]
        [SerializeField] protected float duration = 1;

        [Tooltip("View to transition to when move is complete")]
        [SerializeField] protected View targetView;
        public virtual View TargetView { get { return targetView; } }

        [Tooltip("Wait until the fade has finished before executing next command")]
        [SerializeField] protected bool waitUntilFinished = true;

        [Tooltip("Camera to use for the pan. Will use main camera if set to none.")]
        [SerializeField] protected Camera targetCamera;

        [SerializeField] protected ScriptableObject orthoSizeTweener, posTweener, rotTweener;

        protected virtual void Awake()
        {
            ValidateTweeners();
        }

        protected virtual void ValidateTweeners()
        {
            SetNullsToDefaults();
            void SetNullsToDefaults()
            {
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

            CheckAssignedTweeners();
            void CheckAssignedTweeners()
            {
                doOrthoSizeTween = orthoSizeTweener as ICameraTweenAdapter;
                doPosTween = posTweener as ITransformTweenAdapter;
                doRotTween = rotTweener as ITransformTweenAdapter;

                if (doOrthoSizeTween == null)
                {
                    Debug.LogWarning($"Ortho size tweener passed is invalid. It does not implement " +
                        $"ICameraTweenAdapter. Reverting to the default.");
                    doOrthoSizeTween = AmanitaManager.DefaultTweener;
                }

                if (doPosTween == null)
                {
                    Debug.LogWarning($"Pos tweener passed is invalid. It does not implement " +
                        $"ITransformTweenAdapter. Reverting to the default.");
                    doPosTween = AmanitaManager.DefaultTweener;
                }

                if (doRotTween == null)
                {
                    Debug.LogWarning($"Rot tweener passed is invalid. It does not implement " +
                        $"ITransformTweenAdapter. Reverting to the default.");
                    doRotTween = AmanitaManager.DefaultTweener;
                }
            }

        }

        protected ICameraTweenAdapter doOrthoSizeTween;
        protected ITransformTweenAdapter doPosTween, doRotTween;

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

        public virtual void Start()
        {
            AcquireCamera();
        }

        #region Public members

        public override void OnEnter()
        {
            AcquireCamera();
            if (targetCamera == null || targetView == null)
            {
                Continue();
                return;
            }

            var cameraManager = AmanitaManager.S.CameraManager;

            Vector3 targetPosition = targetView.transform.position;
            Quaternion targetRotation = targetView.transform.rotation;
            float targetSize = targetView.ViewSize;

            cameraManager.PanToPosition(targetCamera, targetPosition, targetRotation, targetSize,
                duration, OnMovementDone, doOrthoSizeTween, doPosTween, doRotTween);

            void OnMovementDone()
            {
                if (waitUntilFinished)
                {
                    Continue();
                }
            }
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

        
    }
}
