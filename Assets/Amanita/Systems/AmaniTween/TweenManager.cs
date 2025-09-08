using Amanita.Myceliaudio;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityObj = UnityEngine.Object;

namespace Amanita.Tweening
{
    public class TweenManager : MonoBehaviour, ITransformTweenAdapter, IGeneralTweenAdapter<Vector2>,
        IGeneralTweenAdapter<Vector3>, IGeneralTweenAdapter<float>, IGeneralTweenAdapter<int>,
        IGraphicTweenAdapter, ICameraTweenAdapter, IAudioSourceTweenAdapter, IMyceliaudioTweenAdapter,
        IAudioFilterTweenAdapter, ILightTweenAdapter, IRectTransformTweenAdapter, IMaterialTweenAdapter
    {
        public static DefaultTweenAdapter TweenAdapter
        {
            get
            {
                if (_adapter == null)
                {

#if UNITY_EDITOR
                    _adapter = Resources.Load<DefaultTweenAdapter>(pathToAdapter);

                    if (_adapter == null)
                    {
                        Debug.LogWarning($"No TweenAdapter found at Resources/{pathToAdapter}. Creating a new one.");
                        _adapter = TweenAdapterUtility.GetOrCreateDefaultAdapter();
                    }
#else
                    _adapter = ScriptableObject.CreateInstance<DefaultTweenAdapter>();
#endif
                    }

                return _adapter;
            }
        }

        protected static DefaultTweenAdapter _adapter;
        protected static string pathToAdapter = "DefaultTweenAdapter";

        protected static TweenManager _s;
        public static TweenManager S
        {
            get
            {
                return _s;
            }
        }

        protected Dictionary<string, ITween> _activeTweens = new();

        protected virtual void Awake()
        {
            if (_s != null && _s != this)
            {
                Debug.LogWarning("Multiple TweenManagers detected. Destroying the new one.");
                Destroy(this);
                return;
            }

            _s = this;
        }

        protected virtual void OnDestroy()
        {
            if (_s == this)
            {
                _s = null;
            }
        }

        public void AddTween<T>(Tween<T> toAdd)
        {
            if (_activeTweens.ContainsKey(toAdd.ID))
            {
                _activeTweens[toAdd.ID].OnCompleteKill();
                // ^Since the client may be trying to modify the same property on the same game object
                // as another tween. Thus, we need to do this to avoid issues
            }

            _activeTweens[toAdd.ID] = toAdd;
        }

        protected virtual void Update()
        {
            foreach (var pair in _activeTweens.ToList())
            {
                ITween tween = pair.Value;
                tween.Update();

                if (tween.IsComplete && !tween.WasKilled)
                {
                    var onCompleteToCall = tween.OnComplete;
                    tween.OnComplete();
                    tween.OnComplete = delegate { };

                    var tweenAfterOnComplete = _activeTweens[pair.Key];
                    bool replacedTheTween = tweenAfterOnComplete != tween;
                    // ^Like for when OnComplete involves applying a tween of the same type
                    // on the same target as the one the OnComplete belongs to

                    if (!replacedTheTween)
                    {
                        RemoveTween(pair.Key);
                    }
                    
                }

                if (tween.WasKilled)
                {
                    RemoveTween(pair.Key);
                }
            }
        }

        public static Tween<float> TweenSpriteAlpha(GameObject gameObject, float startAlpha, float endAlpha, float duration)
        {
            SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
            return TweenSpriteAlpha(renderer, startAlpha, endAlpha, duration);
        }

        public static Tween<float> TweenSpriteAlpha(SpriteRenderer renderer, float startAlpha, float endAlpha, float duration)
        {
            string id = $"SpriteRenderer_{renderer.GetInstanceID()}_Alpha";

            Tween<float> result = new Tween<float>(renderer.gameObject, id, startAlpha,
                endAlpha, duration, val =>
                {
                    Color color = renderer.color;
                    color.a = val;
                    renderer.color = color;
                });
            return result;
        }

        public static Tween<Color> TweenSpriteColor(SpriteRenderer renderer, Color startCol, Color endCol, float duration, Action onComplete = null)
        {
            onComplete += delegate { };
            string id = $"SpriteRenderer_{renderer.GetInstanceID()}_Color";

            Tween<Color> result = new Tween<Color>(renderer.gameObject, id, startCol,
                endCol, duration, val =>
                {
                    renderer.color = val;
                })
                .SetOnComplete(onComplete);
            return result;
        }

        public static Tween<float> TweenGraphicAlpha(Graphic graphic, float startAlpha, float endAlpha, float duration,
            Action onComplete = null)
        {
            onComplete += delegate { };
            string id = $"Graphic_{graphic.GetInstanceID()}_Alpha";

            Tween<float> result = new Tween<float>(graphic.gameObject, id, startAlpha,
                endAlpha, duration, val =>
                {
                    Color color = graphic.color;
                    color.a = val;
                    graphic.color = color;
                })
                .SetOnComplete(onComplete);
            return result;
        }

        public static Tween<Color> TweenGraphicColor(Graphic graphic, Color startCol, Color endCol, float duration,
            Action onComplete = null)
        {
            onComplete += delegate { };
            string id = $"Graphic_{graphic.GetInstanceID()}_Color";

            Tween<Color> result = new Tween<Color>(graphic.gameObject, id, startCol,
                endCol, duration, val =>
                {
                    graphic.color = val;
                })
                .SetOnComplete(onComplete);
            return result;
        }

        public static Tween<float> TweenFloat(Func<float> getFloatToTween, Action<float> setFloatToTween,
            float endValue, float duration, Action onComplete = null)
        {
            onComplete += delegate { };
            string id = $"{getFloatToTween.Target.GetHashCode()}_Float";
            object target = getFloatToTween.Target;
            float startVal = getFloatToTween();

            Tween<float> result = new Tween<float>(target, id, startVal, endValue, duration, value =>
            {
                setFloatToTween(value);
            })
            .SetOnComplete(onComplete);

            return result;
        }

        /// <summary>
        /// For tweening basic primitives and vectors.
        /// </summary>
        public static Tween<T> TweenBasic<T>(Func<T> getValToTween, Action<T> setValToTween,
            T endValue, float duration, Action onComplete = null)
        {
            onComplete += delegate { };
            string id = $"{getValToTween.Target.GetHashCode()}_{typeof(T).FullName}";
            object target = getValToTween.Target;
            T startVal = getValToTween();

            Tween<T> result = new Tween<T>(target, id, startVal, endValue, duration, value =>
            {
                setValToTween(value);
            })
            .SetOnComplete(onComplete);

            return result;
        }

        public static Tween<Vector3> TweenScale(GameObject gameObject, Vector3 startScale, Vector3 endScale, float duration)
        {
            return TweenScale(gameObject.transform, startScale, endScale, duration);
        }

        public static Tween<Vector3> TweenScale(Transform transform, Vector3 startScale, Vector3 endScale, float duration)
        {
            string id = $"Transform_{transform.GetInstanceID()}_Scale";
            Tween<Vector3> result = new Tween<Vector3>(transform, id, startScale,
                endScale, duration, value =>
                {
                    transform.localScale = value;
                });

            return result;
        }

        public static Tween<Vector3> TweenPosition(Transform transform, Vector3 startPos, Vector3 endPos, float duration)
        {
            string id = $"Transform_{transform.GetInstanceID()}_Pos";
            Tween<Vector3> result = new Tween<Vector3>(transform, id, startPos,
                endPos, duration, value =>
                {
                    transform.position = value;
                });

            return result;
        }
        
        public static Tween<Quaternion> TweenRotation(Transform toRotate, Quaternion startRot,
            Quaternion endRot, float duration)
        {
            string id = $"Transform_{toRotate.GetInstanceID()}_Rot";
            void UpdateRot(Quaternion newRot)
            {
                toRotate.rotation = newRot;
            }
            Tween<Quaternion> result = new Tween<Quaternion>(toRotate, id, startRot,
                endRot, duration, UpdateRot);

            return result;
        }

        public virtual void RemoveTween(string id)
        {
            _activeTweens.Remove(id);
        }

        public static Tween<float> TweenAudioSourcePitch(AudioTweenArgs args)
        {
            void OnComplete()
            {
                args.OnComplete(args);
            }
            return TweenAudioSourcePitch(args.Target, args.BaseValue, args.TargetValue, args.HowLongToTake, OnComplete);
        }

        /// <summary>
        /// Uses a scale of -300 for min to 300 for max. Normal pitch = 100
        /// </summary>
        /// <returns></returns>
        public static Tween<float> TweenAudioSourcePitch(AudioSource source, float startPitch,
            float endPitch, float duration, Action onComplete = null)
        {
            return TweenAudioSourcePitchN33(source, startPitch / 100, endPitch / 100, duration, onComplete);
        }

        /// <summary>
        /// Uses a scale of -3 to 3. Normal pitch = 1
        /// </summary>
        public static Tween<float> TweenAudioSourcePitchN33(AudioSource source, float startPitch,
            float endPitch, float duration, Action onComplete = null)
        {
            startPitch = Mathf.Clamp(startPitch, -3, 3);
            endPitch = Mathf.Clamp(endPitch, -3, 3);

            onComplete += delegate { };
            string id = $"AudioSource_{source.GetInstanceID()}_Pitch";
            void UpdateThePitch(float val)
            {
                source.pitch = val;
            }

            Tween<float> result = new Tween<float>(source, id, startPitch, endPitch, duration, UpdateThePitch)
            .SetOnComplete(onComplete);
            
            return result;
        }

        public static Tween<float> TweenAudioSourceVolume(AudioTweenArgs args)
        {
            void ApplyOnComplete()
            {
                args.OnComplete(args);
            }
            return TweenAudioSourceVolume(args.Target, args.BaseValue, args.TargetValue, args.HowLongToTake, ApplyOnComplete);
        }

        /// <summary>
        /// Uses a scale of 0 to 100
        /// </summary>
        /// <returns></returns>
        public static Tween<float> TweenAudioSourceVolume(AudioSource source, float startVol,
            float endVol, float duration, Action onComplete = null)
        {
            return TweenAudioSourceVolume01(source, startVol / 100, endVol / 100, duration, onComplete);
        }

        /// <summary>
        /// Uses a scale of 0 to 1
        /// </summary>
        public static Tween<float> TweenAudioSourceVolume01(AudioSource source, float startVol,
            float endVol, float duration, Action onComplete = null)
        {
            startVol = Mathf.Clamp01(startVol);
            endVol = Mathf.Clamp01(endVol);

            string id = $"AudioSource_{source.GetInstanceID()}_Volume";
            void UpdateTheVol(float newVol)
            {
                source.volume = newVol;
            }
            Tween<float> result = new Tween<float>(source, id, startVol, endVol, duration, UpdateTheVol)
                .SetOnComplete(onComplete);

            return result;
        }


        public static Tween<float> TweenCanvasGroupAlpha(CanvasGroup group, float startAlpha, float targAlpha,
            float duration, Action onComplete = null)
        {
            onComplete += delegate { };
            string id = $"CanvasGroup_{group.GetInstanceID()}_Alpha";

            void UpdateTheAlpha(float newAlpha)
            {
                group.alpha = newAlpha;
            }
            Tween<float> result = new Tween<float>(group, id, startAlpha, targAlpha, duration, UpdateTheAlpha)
            .SetOnComplete(onComplete);

            return result;
        }

        /// <summary>
        /// Kills all tween targeting the specified target
        /// </summary>
        public virtual void KillAllOn(object target, bool callOnComplete = true)
        {
            IList<ITween> toCancel = (from elem in _activeTweens.Values
                                      where elem.Target == target
                                      select elem).ToList();
            foreach (var elem in toCancel)
            {
                if (callOnComplete)
                {
                    elem.OnComplete();
                }

                elem.OnCompleteKill();
            }
        }

        public virtual bool IsTweeningOn(object target)
        {
            bool result = (from elem in _activeTweens.Values
                           where elem.Target == target
                           select elem).Count() > 0;

            return result;
        }

        public ITweenHandle MoveTo(Transform target, Vector3 position, float duration)
        {
            var tweenPos = TweenPosition(target, target.position, position, duration);
            return DefaultTweenHandle.From(tweenPos);
        }

        public ITweenHandle ScaleTo(Transform target, Vector3 scale, float duration)
        {
            var tweenScale = TweenScale(target, target.localScale, scale, duration);
            return DefaultTweenHandle.From(tweenScale);
        }

        public ITweenHandle RotateTo(Transform target, Quaternion rotation, float duration)
        {
            var tweenRot = TweenRotation(target, target.rotation, rotation, duration);
            return DefaultTweenHandle.From(tweenRot);
        }

        public ITweenHandle TweenGeneral(Func<Vector2> getter, Action<Vector2> setter, Vector2 endVal,
            float duration, Action onComplete = null)
        {
            var tween = TweenBasic(getter, setter, endVal, duration, onComplete);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle TweenGeneral(Func<Vector3> getter, Action<Vector3> setter, Vector3 endVal,
            float duration, Action onComplete = null)
        {
            var tween = TweenBasic(getter, setter, endVal, duration, onComplete);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle TweenGeneral(Func<int> getter, Action<int> setter, int endVal,
            float duration, Action onComplete = null)
        {
            var tween = TweenBasic(getter, setter, endVal, duration, onComplete);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle TweenGeneral(Func<float> getter, Action<float> setter, float endVal,
            float duration, Action onComplete = null)
        {
            var tween = TweenBasic(getter, setter, endVal, duration, onComplete);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftColorTo(Graphic target, Color endVal, float duration)
        {
            var tween = TweenGraphicColor(target, target.color, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeTo(Graphic target, float endVal, float duration)
        {
            var tween = TweenGraphicAlpha(target, target.color.a, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftColorTo(SpriteRenderer target, Color endVal, float duration)
        {
            var tween = TweenSpriteColor(target, target.color, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeTo(SpriteRenderer target, float endVal, float duration)
        {
            var tween = TweenSpriteAlpha(target, target.color.a, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeTo(CanvasGroup target, float endVal, float duration)
        {
            var tween = TweenCanvasGroupAlpha(target, target.alpha, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftFillTo(Image target, float endVal, float duration)
        {
            var tween = TweenImageFill(target, target.fillAmount, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public static Tween<float> TweenImageFill(Image img, float startVal, float endVal, float duration)
        {
            string id = $"Image_{img.GetInstanceID()}_Fill";

            Tween<float> result = new Tween<float>(img.gameObject, id, startVal,
                endVal, duration, val =>
                {
                    img.fillAmount = val;
                });
            return result;
        }

        public ITweenHandle ShiftFieldOfViewTo(Camera target, float targetVal, float duration)
        {
            var tween = TweenCameraFOC(target, target.fieldOfView, targetVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public static Tween<float> TweenCameraFOC(Camera target, float startVal, float targetVal, float duration)
        {
            string id = $"Camera_{target.GetInstanceID()}_ShiftFieldOfView";
            Tween<float> result = new Tween<float>(target.gameObject, id, target.fieldOfView,
                targetVal, duration, val =>
                {
                    target.fieldOfView = val;
                });
            return result;
        }

        public ITweenHandle ShiftOrthographicSizeTo(Camera target, float targetVal, float duration)
        {
            var tween = TweenCameraOrthoSize(target, target.orthographicSize, targetVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public static Tween<float> TweenCameraOrthoSize(Camera target, float startSize, float endSize, float duration)
        {
            string id = $"Camera_{target.GetInstanceID()}_CameraOrthoSize";
            Tween<float> result = new Tween<float>(target.gameObject, id, target.orthographicSize,
                endSize, duration, val =>
                {
                    target.orthographicSize = val;
                });
            return result;
        }

        public ITweenHandle ShiftBackgroundColorTo(Camera target, Color targetVal, float duration)
        {
            var tween = TweenCameraBGColor(target, target.backgroundColor, targetVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public Tween<Color> TweenCameraBGColor(Camera target, Color startVal, Color targetVal, float duration)
        {
            string id = $"Camera_{target.GetInstanceID()}_CameraBGColor";
            Tween<Color> result = new Tween<Color>(target, id, target.backgroundColor, targetVal, duration,
                val =>
                {
                    target.backgroundColor = val;
                });
            return result;
        }

        /// <summary>
        /// Scaleof 0 for silent, 100 for max
        /// </summary>
        public ITweenHandle ShiftVolumeTo(AudioSource target, float targVal, float duration)
        {
            var tween = TweenAudioSourceVolume(target, target.volume * 100, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Scale of 0 for silent, 1 for max
        /// </summary>
        public ITweenHandle ShiftVolume01To(AudioSource target, float targVal, float duration)
        {
            targVal = Mathf.Clamp01(targVal);
            var tween = TweenAudioSourceVolume01(target, target.volume, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Scale of -300 for min, 300 for max. Normal pitch = 100
        /// </summary>
        public ITweenHandle ShiftPitchTo(AudioSource target, float targVal, float duration)
        {
            var tween = TweenAudioSourcePitch(target, target.pitch, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftPitchN33To(AudioSource target, float targVal, float duration)
        {
            var tween = TweenAudioSourcePitchN33(target, target.pitch, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Scale of 0 for silent to 100 for max.
        /// </summary>
        public ITweenHandle ShiftVolumeTo(IAudioTrack track, float targVal, float duration)
        {
            string id = $"GameObject_{track.GameObject.GetInstanceID()}_MyceliaudioShiftVolume";
            void UpdateTheVol(float newVol)
            {
                track.BaseVolume = newVol;
            }
            Tween<float> tween = new Tween<float>(track.GameObject, id, track.BaseVolume,
                targVal, duration, UpdateTheVol);

            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftVolume01To(IAudioTrack track, int targVal, float duration)
        {
            return ShiftVolumeTo(track, targVal / 100f, duration);
        }

        public ITweenHandle ShiftLowPassCutoffTo(AudioLowPassFilter target, float targetVal, float duration)
        {
            string id = GenIDFor(target, "ShiftLowPassCutoff");
            void UpdateCutoff(float newCutoff)
            {
                target.cutoffFrequency = newCutoff;
            }
            Tween<float> tween = new Tween<float>(target.gameObject, id, target.cutoffFrequency,
                targetVal, duration, UpdateCutoff);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftReverbLevelTo(AudioReverbFilter target, float targetVal, float duration)
        {
            string id = GenIDFor(target, "ReverbLevel");
            void UpdateReverbLevel(float newReverbLevel)
            {
                target.reverbLevel = newReverbLevel;
            }
            Tween<float> tween = new Tween<float>(target, id, target.reverbLevel, targetVal, duration, UpdateReverbLevel);
            return DefaultTweenHandle.From(tween);
        }

        protected virtual string GenIDFor(UnityObj unityObj, string aspectName)
        {
            string result = $"{unityObj.name}_{unityObj.GetInstanceID()}_{aspectName}";
            return result;
        }

        public ITweenHandle ShiftIntensityTo(Light target, float targetVal, float duration)
        {
            string id = GenIDFor(target, "ShiftIntensity");
            void UpdateIntensity(float newIntensity)
            {
                target.intensity = newIntensity;
            }
            Tween<float> tween = new Tween<float>(target, id, target.intensity, targetVal, duration, UpdateIntensity);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftColorTo(Light target, Color targetVal, float duration)
        {
            string id = GenIDFor(target, "ShiftColor");
            void UpdateColor(Color newCol)
            {
                target.color = newCol;
            }
            Tween<Color> newTween = new Tween<Color>(target, id, target.color, targetVal, duration, UpdateColor);
            return DefaultTweenHandle.From(newTween);
        }

        public ITweenHandle ShiftRangeTo(Light target, float targetVal, float duration)
        {
            string id = GenIDFor(target, "ShiftRange");
            void UpdateRange(float newRange)
            {
                target.range = newRange;
            }
            Tween<float> tween = new Tween<float>(target, id, target.range, targetVal, duration, UpdateRange);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftAnchoredPositionTo(RectTransform target, Vector2 position, float duration)
        {
            string id = GenIDFor(target, "AnchoredPosition");
            void UpdatePos(Vector2 newPos)
            {
                target.anchoredPosition = newPos;
            }
            Tween<Vector2> tween = new Tween<Vector2>(target, id, target.anchoredPosition, position, duration, UpdatePos);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftSizeDeltaTo(RectTransform target, Vector2 size, float duration)
        {
            string id = GenIDFor(target, "SizeDelta");
            void UpdateSize(Vector2 newSize)
            {
                target.sizeDelta = newSize;
            }
            Tween<Vector2> tween = new Tween<Vector2>(target, id, target.sizeDelta, size, duration, UpdateSize);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle RotateTo(RectTransform target, Quaternion rotation, float duration)
        {
            string id = GenIDFor(target, "Rotation");
            void UpdateRot(Quaternion newRot)
            {
                target.rotation = newRot;
            }
            Tween<Quaternion> tween = new Tween<Quaternion>(target, id, target.rotation, rotation, duration, UpdateRot);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ScaleTo(RectTransform target, Vector3 scale, float duration)
        {
            string id = GenIDFor(target, "Scale");
            void UpdateScale(Vector3 newScale)
            {
                target.localScale = newScale;
            }
            Tween<Vector3> tween = new Tween<Vector3>(target, id, target.localScale, scale, duration, UpdateScale);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle ShiftColorTo(Material target, Color targetVal, float duration)
        {
            string id = GenIDFor(target, "Color");
            void UpdateColor(Color newCol)
            {
                target.color = newCol;
            }
            Tween<Color> newTween = new Tween<Color>(target, id, target.color, targetVal, duration, UpdateColor);
            return DefaultTweenHandle.From(newTween);
        }

        public ITweenHandle ShiftFloatTo(Material target, string propertyName, float targetVal, float duration)
        {
            string id = GenIDFor(target, propertyName);
            void UpdateFloat(float newFloat)
            {
                target.SetFloat(propertyName, newFloat);
            }
            float startVal = target.GetFloat(propertyName);
            Tween<float> tween = new Tween<float>(target, id, startVal, targetVal, duration, UpdateFloat);
            return DefaultTweenHandle.From(tween);
        }
    }

}