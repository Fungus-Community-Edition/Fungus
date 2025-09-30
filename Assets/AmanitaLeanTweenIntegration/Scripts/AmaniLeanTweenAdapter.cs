using System.Collections.Generic;
using Amanita.DentedPixel;
using Amanita.Myceliaudio;
using Amanita.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityObj = UnityEngine.Object;

namespace Amanita.LeanTweenIntegration
{
    [CreateAssetMenu(fileName = "NewAmanitaLeanTweenAdapter", menuName = "Amanita/LeanTween/TweenAdapter")]
    public class AmaniLeanTweenAdapter : ScriptableObject, ITransformTweenAdapter,
        IGraphicTweenAdapter, IAudioSourceTweenAdapter, ICameraTweenAdapter,
        ILightTweenAdapter, ICanvasGroupTweenAdapter, IRectTransformTweenAdapter,
        IMaterialTweenAdapter, IAudioFilterTweenAdapter, IGeneralTweenAdapter<float>,
        IGeneralTweenAdapter<int>, IGeneralTweenAdapter<Vector2>, IGeneralTweenAdapter<Vector3>,
        IMyceliaudioTweenAdapter, IOmniTweenKiller<GameObject>
    {
        [SerializeField] protected LeanTweenType ease = LeanTweenType.easeInOutQuad;

        public virtual LeanTweenType Ease
        {
            get => ease;
            set => ease = value;
        }

        // Lightweight wrapper around LTDescr so the rest of the codebase can use ITweenHandle.


        #region Helpers
        private GameObject FindOwnerForMaterial(Material material)
        {
            if (material == null) return null;

            // Use Unity's FindObjectsOfType (safe across versions). This is a little expensive;
            // caller should prefer passing an owner when possible.
            IList<Renderer> renderers;
#if UNITY_6000_0_OR_NEWER
            renderers = UnityObj.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
#else
            renderers = UnityObj.FindObjectsOfType<Renderer>();
#endif
            for (int i = 0; i < renderers.Count; i++)
            {
                var elem = renderers[i];
                // Check sharedMaterial first to avoid creating material instances.
                if (elem.sharedMaterial == material || elem.material == material)
                {
                    return elem.gameObject;
                }
            }

            return null;
        }

        private GameObject GetOrCreateManagerAnchor()
        {
            GameObject result = null;
            AmanitaManager manager = AmanitaManager.S;
            if (manager == null)
            {
                manager = AmanitaManager.EnsureExists();
            }

            if (manager != null)
            {
                result = manager.GetOrCreateAnchorFor(this);
            }
            else
            {
                Debug.LogError("AmaniLeanTweenAdapter: Unable to obtain AmanitaManager singleton instance. By " +
                    "extension, also an anchor.");
            }
            return result;
        }
#endregion


        #region Transform
        public ITweenHandle MoveTo(Transform target, Vector3 position, float duration)
        {
            var tween = LeanTween.move(target.gameObject, position, duration).setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle RotateTo(Transform target, Quaternion rotation, float duration)
        {
            // LeanTween uses Euler angles for rotate; convert quaternion to euler.
            var tween = LeanTween.rotate(target.gameObject, rotation.eulerAngles, duration).setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle ScaleTo(Transform target, Vector3 scale, float duration)
        {
            var tween = LeanTween.scale(target.gameObject, scale, duration).setEase(ease);
            return new LeanTweenHandle(tween);
        }
        #endregion

        #region Graphic
        public ITweenHandle FadeColor(Graphic target, Color endVal, float duration)
        {
            var start = target.color;
            var tween = LeanTween.value(target.gameObject, start, endVal, duration)
                .setOnUpdate((Color tweenedColor) => target.color = tweenedColor)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle FadeColor(SpriteRenderer target, Color endVal, float duration)
        {
            var start = target.color;
            var tween = LeanTween.value(target.gameObject, start, endVal, duration)
                .setOnUpdate((Color tweenedColor) => target.color = tweenedColor)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle FadeOpacity(Graphic target, float endVal, float duration)
        {
            float start = target.color.a;
            var tween = LeanTween.value(target.gameObject, start, endVal, duration)
                .setOnUpdate((float tweenedAlpha) =>
                {
                    var c = target.color;
                    c.a = tweenedAlpha;
                    target.color = c;
                })
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle FadeOpacity(SpriteRenderer target, float endVal, float duration)
        {
            float start = target.color.a;
            var tween = LeanTween.value(target.gameObject, start, endVal, duration)
                .setOnUpdate((float tweenedAlpha) =>
                {
                    var c = target.color;
                    c.a = tweenedAlpha;
                    target.color = c;
                })
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }


        #endregion

        #region AudioSource

        /// <summary>
        /// 0 for silent, 100 for max
        /// </summary>
        /// <returns></returns>
        public ITweenHandle FadeVolume(AudioSource target, float targVal, float duration)
        {
            return FadeVolume01(target, targVal / 100f, duration);
        }

        /// <summary>
        /// 0 for silent, 1 for max
        /// </summary>
        public ITweenHandle FadeVolume01(AudioSource target, float targVal, float duration)
        {
            targVal = Mathf.Clamp01(targVal);
            float start = target.volume;
            var tween = LeanTween.value(target.gameObject, start, targVal, duration)
                .setOnUpdate((float tweenedVol) => target.volume = tweenedVol)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        /// <summary>
        /// -300 for min, 300 for max. Normal pitch is 100
        /// </summary>
        public ITweenHandle FadePitch(AudioSource target, float targVal, float duration)
        {
            targVal /= 100f;
            targVal = Mathf.Clamp(targVal, -3, 3);
            float start = target.pitch;
            var tween = LeanTween.value(target.gameObject, start, targVal, duration)
                .setOnUpdate((float tweenedPitch) => target.pitch = tweenedPitch)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle FadePitchN33(AudioSource target, float targVal, float duration)
        {
            return FadePitch(target, targVal * 100f, duration);
        }

        #endregion

        #region Camera
        public ITweenHandle TweenFOV(Camera target, float targetVal, float duration)
        {
            float start = target.fieldOfView;
            var tween = LeanTween.value(start, targetVal, duration)
                .setOnUpdate((float tweenedFov) => target.fieldOfView = tweenedFov)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle TweenOrthoSize(Camera target, float targetVal, float duration)
        {
            float start = target.orthographicSize;
            var tween = LeanTween.value(start, targetVal, duration)
                .setOnUpdate((float tweenedSize) => target.orthographicSize = tweenedSize)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle FadeBackgroundColor(Camera target, Color targetVal, float duration)
        {
            Color start = target.backgroundColor;
            var tween = LeanTween.value(target.gameObject, start, targetVal, duration)
                .setOnUpdate((Color tweenedBackgroundColor) => target.backgroundColor = tweenedBackgroundColor)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }
        #endregion

        #region Light
        public ITweenHandle TweenIntensity(Light target, float targetVal, float duration)
        {
            float start = target.intensity;
            var tween = LeanTween.value(start, targetVal, duration)
                .setOnUpdate((float tweenedIntensity) => target.intensity = tweenedIntensity)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle FadeColor(Light target, Color targetVal, float duration)
        {
            Color start = target.color;
            var tween = LeanTween.value(target.gameObject, start, targetVal, duration)
                .setOnUpdate((Color tweenedColor) => target.color = tweenedColor)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle FadeColor(Light target, float targetVal, float duration)
        {
            float start = target.range;
            var tween = LeanTween.value(start, targetVal, duration)
                .setOnUpdate((float tweenedRange) => target.range = tweenedRange)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }
        #endregion

        #region CanvasGroup
        public ITweenHandle FadeOpacity(CanvasGroup target, float endVal, float duration)
        {
            var tween = LeanTween.alphaCanvas(target, endVal, duration).setEase(ease);
            return new LeanTweenHandle(tween);
        }
        #endregion

        #region RectTransform
        public ITweenHandle TweenAnchoredPosition(RectTransform target, Vector2 position, float duration)
        {
            Vector2 start = target.anchoredPosition;
            var tween = LeanTween.value(target.gameObject, new Vector3(start.x, start.y, 0f), new Vector3(position.x, position.y, 0f), duration)
                .setOnUpdate((Vector3 tweenedPosition) => target.anchoredPosition = (Vector2)tweenedPosition)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle TweenSizeDelta(RectTransform target, Vector2 size, float duration)
        {
            Vector2 start = target.sizeDelta;
            var tween = LeanTween.value(target.gameObject, new Vector3(start.x, start.y, 0f), new Vector3(size.x, size.y, 0f), duration)
                .setOnUpdate((Vector3 tweenedSizeVec) => target.sizeDelta = (Vector2)tweenedSizeVec)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle RotateTo(RectTransform target, Quaternion rotation, float duration)
        {
            var tween = LeanTween.rotate(target.gameObject, rotation.eulerAngles, duration).setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle ScaleTo(RectTransform target, Vector3 scale, float duration)
        {
            var tween = LeanTween.scale(target.gameObject, scale, duration).setEase(ease);
            return new LeanTweenHandle(tween);
        }
        #endregion

        #region Material

        // Owner-specific overloads (deterministic lifecycle)
        public ITweenHandle FadeColor(GameObject owner, Material target, Color targetVal, float duration)
        {
            if (target == null)
                return null;

            if (owner == null)
            {
                Debug.LogError("AmaniLeanTweenAdapter: ShiftColorTo received a null owner GameObject — tween aborted.");
                return null;
            }

            Color start = target.color;

            var tween = LeanTween.value(owner, start, targetVal, duration)
                .setOnUpdate((Color tweenedColor) => target.color = tweenedColor)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle TweenFloat(GameObject owner, Material target, string propertyName, float targetVal, float duration)
        {
            if (target == null)
            {
                Debug.LogError("AmaniLeanTweenAdapter: ShiftFloatTo received a null Material — tween aborted.");
                return null;
            }

            if (owner == null)
            {
                Debug.LogError("AmaniLeanTweenAdapter: ShiftFloatTo received a null owner GameObject — tween aborted.");
                return null;
            }

            float start = target.GetFloat(propertyName);

            var tween = LeanTween.value(owner, start, targetVal, duration)
                .setOnUpdate((float tweenedFloat) => target.SetFloat(propertyName, tweenedFloat))
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        // Backwards-compatible overloads that try to find a sensible owner, otherwise fall back to global tween.
        public ITweenHandle FadeColor(Material target, Color targetVal, float duration)
        {
            if (target == null)
            {
                Debug.LogError("AmaniLeanTweenAdapter: ShiftColorTo received a null Material — tween aborted.");
                return null;
            }

            GameObject owner = FindOwnerForMaterial(target);
            return FadeColor(owner, target, targetVal, duration);
        }

        public ITweenHandle TweenFloat(Material target, string propertyName, float targetVal, float duration)
        {
            if (target == null)
            {
                Debug.LogError("AmaniLeanTweenAdapter: ShiftFloatTo received a null Material — tween aborted.");
                return null;
            }

            GameObject owner = FindOwnerForMaterial(target);
            return TweenFloat(owner, target, propertyName, targetVal, duration);
        }
        #endregion

        #region Audio Filters
        public ITweenHandle FadeLowPassCutoff(AudioLowPassFilter target, float targetVal, float duration)
        {
            float start = target.cutoffFrequency;
            var tween = LeanTween.value(start, targetVal, duration)
                .setOnUpdate((float tweenedCutoff) => target.cutoffFrequency = tweenedCutoff)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle FadeReverbLevel(AudioReverbFilter target, float targetVal, float duration)
        {
            float start = target.reverbLevel;
            var tween = LeanTween.value(start, targetVal, duration)
                .setOnUpdate((float tweenedReverb) => target.reverbLevel = tweenedReverb)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle ShiftFillTo(Image target, float endVal, float duration)
        {
            float start = target.fillAmount;
            var tween = LeanTween.value(start, endVal, duration)
                .setOnUpdate((float tweenedFill) => target.fillAmount = tweenedFill)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }
        #endregion

        #region General
        public ITweenHandle TweenGeneral(Func<float> getter, Action<float> setter, float endVal,
            float duration, Action onComplete = null)
        {
            onComplete ??= delegate { };
            float start = getter();
            var tween = LeanTween.value(start, endVal, duration)
                .setOnUpdate((float tweenedVal) => setter(tweenedVal))
                .setOnComplete(() => onComplete())
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle TweenGeneral(Func<int> getter, Action<int> setter, int endVal,
            float duration, Action onComplete = null)
        {
            onComplete ??= delegate { };
            int startI = getter();
            // tween as float and cast on update
            var tween = LeanTween.value((float)startI, (float)endVal, duration)
                .setOnUpdate((float tweenedVal) => setter((int)Mathf.Round(tweenedVal)))
                .setOnComplete(() => onComplete())
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle TweenGeneral(Func<Vector2> getter, Action<Vector2> setter, Vector2 endVal,
            float duration, Action onComplete = null)
        {
            onComplete ??= delegate { };
            Vector2 start = getter();

            Vector3 startVec = new Vector3(start.x, start.y, 0f);
            Vector3 endVec = new Vector3(endVal.x, endVal.y, 0f);

            GameObject owner = GetOrCreateManagerAnchor();
            if (owner == null)
            {
                Debug.LogError("AmaniLeanTweenAdapter: Unable to obtain AmanitaManager anchor — tween aborted.");
                return null;
            }

            LTDescr descr = LeanTween.value(owner, startVec, endVec, duration)
                .setOnUpdate((Vector3 tweenedVec) => setter(new Vector2(tweenedVec.x, tweenedVec.y)))
                .setOnComplete(() => onComplete())
                .setEase(ease);

            return new LeanTweenHandle(descr);
        }

        public ITweenHandle TweenGeneral(Func<Vector3> getter, Action<Vector3> setter, Vector3 endVal,
            float duration, Action onComplete = null)
        {
            onComplete ??= delegate { };
            Vector3 start = getter();
            GameObject anchor = GetOrCreateManagerAnchor();
            var tween = LeanTween.value(anchor, start, endVal, duration)
                .setOnUpdate((Vector3 tweenedVal) => setter(tweenedVal))
                .setOnComplete(() => onComplete())
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        /// <summary>
        /// Scale of 0 to 100
        /// </summary>
        public ITweenHandle FadeVolume(IAudioTrack track, float targVal, float duration)
        {
            // track.BaseVolume is expected to be 0-100 scale in original API; keep that contract
            float start = track.BaseVolume;
            var tween = LeanTween.value(start, targVal, duration)
                .setOnUpdate((float tweenedVal) => track.BaseVolume = tweenedVal)
                .setEase(ease);
            return new LeanTweenHandle(tween);
        }

        public ITweenHandle FadeVolume01(IAudioTrack track, float targVal, float duration)
        {
            // targVal here is 0..1 — convert to 0..100 to match FadeVolume's 0..100 contract
            return FadeVolume(track, targVal * 100f, duration);
        }

        #endregion

        #region Bloodthirst
        public virtual void KillAll()
        {
            LeanTween.cancelAll();
        }

        public void KillAllOn(object target)
        {
            if (target is GameObject go)
            {
                KillAllOn(go);
            }
            else
            {
                Debug.LogWarning("AmaniLeanTweenAdapter: KillAllOn received an object that is not a GameObject");
            }
        }

        public void KillAllOn(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("AmaniLeanTweenAdapter: KillAllOn received a null GameObject; nothing to cancel.");
                return;
            }

            LeanTween.cancel(target);
        }
        #endregion

    }
}