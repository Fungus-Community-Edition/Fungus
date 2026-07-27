using UnityEngine;
using System;
using System.Collections.Generic;
using AtMycelia.HyphaTween;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita
{
	/// <summary>
	/// Manager for main camera. Supports several types of camera transition including snap, pan & fade.
	/// </summary>
	public class CameraManager : MonoBehaviour, IAmanitaManagerSubmodule
	{
		[SerializeField] private int orderIndex = 0;
		[SerializeField] private CameraManagerConfig _config;

		[Tooltip("Camera to use when in swipe mode")]
		[FormerlySerializedAs("swipeCamera")]
		[SerializeField] protected Camera _swipeCamera;

		protected float fadeAlpha = 0f;
		// ^When this changes, OnGUI changes the fadedness of the screen.

		public int OrderIndex => orderIndex;
		public virtual float ScreenOpacity => fadeAlpha;

		// Swipe panning control
		protected bool swipePanActive;

		protected virtual float SwipeSpeedMultiplier
		{
			get
			{
				if (_config == null)
				{
					return 1f;
				}
				return _config.SwipeSpeedMultiplier;
			}
			set
			{
				if (_config == null)
				{
					return;
				}
				_config.SwipeSpeedMultiplier = Mathf.Max(0f, value);
			}
		}
		protected View _firstSwipePanView;
		protected View _secondSwipePanView;
		protected Vector3 _prevMousePos;
		
		protected class CameraView
		{
			public Vector3 cameraPos;
			public Quaternion cameraRot;
			public float cameraSize;
		};
		
		protected Dictionary<string, CameraView> storedViews = new Dictionary<string, CameraView>();
		
		public virtual void Init()
		{
			if (IsFullyInitted)
			{
				return;
			}

			if (_config == null)
			{
				string logMessage = "CameraManager config is null. If you see this message in a " +
					"non-dev build, please report it to the devs.";
				Debug.LogError(logMessage);
			}

			IsFullyInitted = true;
		}

		/// <summary>
		/// The settings this manager uses for things like the ScreenFadeTexture, 
		/// SwipePanIcon, and other camera-related settings.
		/// </summary>
		public CameraManagerConfig Config => _config;

		/// <summary>
		/// Full screen texture used for screen fade effect.
		/// </summary>
		public Texture2D ScreenFadeTexture
		{
			get
			{
				if (_config == null)
				{
					return null;
				}
				return _config.ScreenFadeTexture;
			}
			set
			{
				if (_config == null)
				{
					string logMessage = "CameraManager config is null. If you see this " +
						"message in a non-dev build, please report it to the devs.";
					Debug.LogWarning(logMessage);
					return;
				}

				_config.ScreenFadeTexture = value;
			}
		}

		protected Texture2D SwipePanIcon
		{
			get
			{
				if (_config == null)
				{
					return null;
				}
				return _config.SwipePanIcon;
			}
		}

		protected Vector2 SwipeIconPosition
		{
			get
			{
				if (_config == null)
				{
					return Vector2.zero;
				}
				return _config.SwipeIconPosition;
			}
		}

		protected float CameraZ
		{
			get
			{
				if (_config == null)
				{
					return _defaultCameraZ;
				}
				return _config.CameraZ;
			}
		}
		private static readonly float _defaultCameraZ = -10f;

		public virtual bool IsFullyInitted { get; protected set; } = false;

		protected virtual void OnGUI()
		{
			if (swipePanActive)
			{
				// Draw the swipe panning icon
				if (SwipePanIcon)
				{
					float x = Screen.width * SwipeIconPosition.x;
					float y = Screen.height * SwipeIconPosition.y;
					float width = SwipePanIcon.width;
					float height = SwipePanIcon.height;
					
					x = Mathf.Max(x, 0);
					y = Mathf.Max(y, 0);
					x = Mathf.Min(x, Screen.width - width);
					y = Mathf.Min(y, Screen.height - height);
					
					Rect rect = new Rect(x, y, width, height);
					GUI.DrawTexture(rect, SwipePanIcon);
				}
			}

			#region Draw full screen fade texture
			if (fadeAlpha > 0f && ScreenFadeTexture != null)
			{
				// 1 = scene fully visible
				// 0 = scene fully obscured
				GUI.color = new Color(1,1,1, fadeAlpha);    
				GUI.depth = -1000;
				GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), ScreenFadeTexture);
			}
			#endregion
		}


		protected virtual void DoApplyCamZTo(Camera camera)
		{
			if (!camera)
			{
				return;
			}

			if (camera == null)
			{
				Debug.LogWarning("Camera is null");
				return;
			}

			Transform camTrans = camera.transform;
			Vector3 camPosBeforeSet = camTrans.position;
			camPosBeforeSet.z = CameraZ;
			camTrans.position = camPosBeforeSet;
		}
		
		protected virtual void Update() 
		{
			if (!swipePanActive)
			{
				return;
			}

			if (_swipeCamera == null)
			{
				Debug.LogWarning("Camera is null");
				return;
			}

			Vector3 delta = Vector3.zero;

#if ENABLE_INPUT_SYSTEM
			delta = UnityEngine.InputSystem.Touchscreen.current?.primaryTouch?.delta.ReadValue() ?? Vector3.zero;

			if(UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame ?? false)
			{
				_prevMousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
			}
			else if(UnityEngine.InputSystem.Mouse.current?.leftButton.isPressed ?? false)
			{
				delta = UnityEngine.InputSystem.Mouse.current.position.ReadValue() - (Vector2)_prevMousePos;
				_prevMousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
			}
#else
			if (
		   Input.touchCount > 0)
			{
				if (Input.GetTouch(0).phase == TouchPhase.Moved)
				{
					delta = Input.GetTouch(0).deltaPosition;
				}
			}
			
			if (Input.GetMouseButtonDown(0))
			{
				previousMousePos = Input.mousePosition; 
			}
			else if (Input.GetMouseButton(0)) 
			{
				delta = Input.mousePosition - previousMousePos;
				previousMousePos = Input.mousePosition;
			}
#endif

			Vector3 cameraDelta = _swipeCamera.ScreenToViewportPoint(delta);
			cameraDelta.x *= -2f * SwipeSpeedMultiplier;
			cameraDelta.y *= -2f * SwipeSpeedMultiplier;
			cameraDelta.z = 0f;
			
			Vector3 cameraPos = _swipeCamera.transform.position;
			
			cameraPos += cameraDelta;
			
			_swipeCamera.transform.position = CalcCameraPosition(cameraPos, _firstSwipePanView, _secondSwipePanView);
			_swipeCamera.orthographicSize = CalcCameraSize(cameraPos, _firstSwipePanView, _secondSwipePanView); 
		}
		
		// Clamp camera position to region defined by the two views
		protected virtual Vector3 CalcCameraPosition(Vector3 pos, View viewA, View viewB)
		{
			Vector3 safePos = pos;
			
			// Clamp camera position to region defined by the two views
			safePos.x = Mathf.Max(safePos.x, Mathf.Min(viewA.transform.position.x, viewB.transform.position.x));
			safePos.x = Mathf.Min(safePos.x, Mathf.Max(viewA.transform.position.x, viewB.transform.position.x));
			safePos.y = Mathf.Max(safePos.y, Mathf.Min(viewA.transform.position.y, viewB.transform.position.y));
			safePos.y = Mathf.Min(safePos.y, Mathf.Max(viewA.transform.position.y, viewB.transform.position.y));
			
			return safePos;
		}
		
		// Smoothly interpolate camera orthographic size based on relative position to two views
		protected virtual float CalcCameraSize(Vector3 pos, View viewA, View viewB)
		{
			// Get ray and point in same space
			Vector3 toViewB = viewB.transform.position - viewA.transform.position;
			Vector3 localPos = pos - viewA.transform.position;
			
			// Normalize
			float distance = toViewB.magnitude;
			toViewB /= distance;
			localPos /= distance;
			
			// Project point onto ray
			float t = Vector3.Dot(toViewB, localPos);
			t = Mathf.Clamp01(t); // Not really necessary but no harm
			
			float cameraSize = Mathf.Lerp(viewA.ViewSize, viewB.ViewSize, t);
			
			return cameraSize;
		}

		#region Public members
		
		/// <summary>
		/// Creates a flat colored texture.
		/// </summary>
		public static Texture2D CreateColorTexture(Color color, int width, int height)
		{
			Color[] pixels = new Color[width * height];
			for (int i = 0; i < pixels.Length; i++) 
			{
				pixels[i] = color;
			}
			Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			texture.SetPixels(pixels);
			texture.Apply();

			return texture;     
		}
			
		
		/// <summary>
		/// Perform a fullscreen fade over a duration.
		/// </summary>
		public virtual void Fade(float targetAlpha, float fadeDuration, Action onComplete,
			IGeneralTweenAdapter<float> tweenAdapter = null)
		{
			tweenAdapter ??= DefaultTweener;
			bool finishInstantly = Mathf.Approximately(fadeDuration, 0);
			bool alreadyAtTarget = Mathf.Approximately(fadeAlpha, targetAlpha);
			if (finishInstantly || alreadyAtTarget)
			{
				fadeAlpha = targetAlpha;
				onComplete?.Invoke();
				return;
			}

			tweenAdapter.TweenGeneral(() => fadeAlpha, UpdateFadeAlpha, targetAlpha, fadeDuration, onComplete);
			
		}

		private DefaultTweenAdapter DefaultTweener => TweenManager.S.DefaultAdapter;

		protected Tween<float> _neoFadeTween;
		protected virtual void UpdateFadeAlpha(float newVal)
		{
			fadeAlpha = newVal;
		}

		/// <summary>
		/// Fade out, move camera to view and then fade back in.
		/// </summary>
		public virtual void FadeToView(Camera camera, View view, float fadeDuration, bool fadeOut, Action onComplete,
			IGeneralTweenAdapter<float> fadeTweener = null, ICameraTweenAdapter sizeTweener = null,
			ITransformTweenAdapter posTweener = null, ITransformTweenAdapter rotTweener = null)
		{
			swipePanActive = false;
			fadeAlpha = 0f;

			float outDuration;
			float inDuration;

			if (fadeOut)
			{
				outDuration = fadeDuration / 2f;
				inDuration = fadeDuration / 2f;
			}
			else
			{
				outDuration = 0;
				inDuration = fadeDuration;
			}

			// Fade out
			Fade(1f, outDuration, delegate
			{

				// Snap to new view
				PanToPosition(camera, view.transform.position, view.transform.rotation,
					view.ViewSize, 0f, null, sizeTweener, posTweener, rotTweener);

				// Fade in
				Fade(0f, inDuration, () =>
				{
					onComplete?.Invoke();
				}, fadeTweener);
			}, fadeTweener);
		}


		/// <summary>
		/// Stop all camera tweening.
		/// </summary>
		public virtual void Stop()
		{
			StopFadeTween();
			StopPosTweens();
		}

		protected void StopFadeTween()
		{
			_neoFadeTween?.FullKill();
		}

		protected void StopPosTweens()
		{
			_camOrthoSizeTween?.OnCompleteKill();
			_neoCamPosTween?.OnCompleteKill();
			_neoCamRotTween?.OnCompleteKill();
		}

		protected Tween<float> _camOrthoSizeTween;
		protected Tween<Vector3> _neoCamPosTween;
		protected Tween<Quaternion> _neoCamRotTween;
		public bool ApplyFixedCameraZ
		{
			get
			{
				if (_config == null)
				{
					return false;
				}
				return _config.ApplyFixedCamZ;
			}
		}

		/// <summary>
		/// Moves camera from current position to a target position over a period of time.
		/// </summary>
		public virtual void PanToPosition(Camera camera, Vector3 targetPosition,
			Quaternion targetRotation,
			float targetSize, float duration, Action onPanDone,
			ICameraTweenAdapter sizeTweener = null,
			ITransformTweenAdapter posTweener = null,
			ITransformTweenAdapter rotationTweener = null)
		{
			if (camera == null)
			{
				Debug.LogWarning("Camera is null");
				return;
			}

			if (ApplyFixedCameraZ)
			{
				targetPosition.z = camera.transform.position.z;
			}

			StopPosTweens();
			swipePanActive = false;

			if (Mathf.Approximately(duration, 0f))
			{
				// Move immediately
				camera.orthographicSize = targetSize;
				camera.transform.SetPositionAndRotation(targetPosition, targetRotation);
				DoApplyCamZTo(camera);
				onPanDone?.Invoke();
			}
			else
			{
				sizeTweener.TweenOrthoSize(camera, targetSize, duration)
					.SetOnComplete(OnOrthoSizeTweenDone);
				void OnOrthoSizeTweenDone()
				{
					camera.orthographicSize = targetSize;
					onPanDone?.Invoke();
					// ^The size, motion, and rotation tweens should be set to the same duration.
					// Thus, placing just one call of onPanDone in any of the end-of-tween 
					// callbacks should be okay.
				}

				posTweener.MoveTo(camera.transform, targetPosition, duration)
						.SetOnComplete(OnCamPosTweenDone);
				void OnCamPosTweenDone()
				{
					camera.transform.position = targetPosition;
					_neoCamPosTween = null;
				}

				Transform camTrans = camera.transform;
				rotationTweener.RotateTo(camera.transform, targetRotation, duration)
					.SetOnComplete(OnCamRotTweenDone);
				void OnCamRotTweenDone()
				{
					camTrans.rotation = targetRotation;
				}

			}
		}

		/// <summary>
		/// Activates swipe panning mode. The player can pan the camera within the area between viewA & viewB.
		/// </summary>
		public virtual void StartSwipePan(Camera camera, View viewA, View viewB,
			float duration, float speedMultiplier, Action arriveAction)
		{
			if (camera == null)
			{
				Debug.LogWarning("Camera is null");
				return;
			}

			_firstSwipePanView = viewA;
			_secondSwipePanView = viewB;
			SwipeSpeedMultiplier = speedMultiplier;

			Vector3 cameraPos = camera.transform.position;

			Vector3 targetPosition = CalcCameraPosition(cameraPos, _firstSwipePanView, _secondSwipePanView);
			float targetSize = CalcCameraSize(cameraPos, _firstSwipePanView, _secondSwipePanView); 

			PanToPosition(camera, targetPosition, Quaternion.identity, targetSize, duration, delegate {

				swipePanActive = true;
				_swipeCamera = camera;

				arriveAction?.Invoke();
			}); 
		}

		/// <summary>
		/// Deactivates swipe panning mode.
		/// </summary>
		public virtual void StopSwipePan()
		{
			swipePanActive = false;
			_firstSwipePanView = null;
			_secondSwipePanView = null;
			_swipeCamera = null;
		}

		#endregion
	}
}