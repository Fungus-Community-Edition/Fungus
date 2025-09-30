using Amanita.Myceliaudio;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityObj = UnityEngine.Object;

namespace Amanita.Tweening
{
    public class DefaultTweenAdapter : ScriptableObject, ITransformTweenAdapter, IGeneralTweenAdapter<Vector2>,
        IGeneralTweenAdapter<Vector3>, IGeneralTweenAdapter<float>, IGeneralTweenAdapter<int>,
        IGraphicTweenAdapter, ICameraTweenAdapter, IAudioSourceTweenAdapter, IMyceliaudioTweenAdapter,
        IMaterialTweenAdapter, IRectTransformTweenAdapter, IAudioFilterTweenAdapter, ILightTweenAdapter
    {

        #region Transform and RectTransform
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

        protected virtual string GenIDFor(UnityObj unityObj, string aspectName)
        {
            string typeName = unityObj.GetType().Name;
            string result = $"{typeName}_{unityObj.name}_{unityObj.GetInstanceID()}_{aspectName}";
            return result;
        }

        public ITweenHandle TweenAnchoredPosition(RectTransform target, Vector2 position, float duration)
        {
            string id = GenIDFor(target, "AnchoredPosition");
            void UpdatePos(Vector2 newPos)
            {
                target.anchoredPosition = newPos;
            }
            Tween<Vector2> tween = new Tween<Vector2>(target, id, target.anchoredPosition, position, duration, UpdatePos);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle TweenSizeDelta(RectTransform target, Vector2 size, float duration)
        {
            string id = GenIDFor(target, "SizeDelta");
            void UpdateSize(Vector2 newSize)
            {
                target.sizeDelta = newSize;
            }
            Tween<Vector2> tween = new Tween<Vector2>(target, id, target.sizeDelta, size, duration, UpdateSize);
            return DefaultTweenHandle.From(tween);
        }

        
        #endregion

        #region Graphics

        public ITweenHandle FadeColor(Graphic target, Color endVal, float duration)
        {
            var tween = TweenGraphicColor(target, target.color, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeColor(SpriteRenderer target, Color endVal, float duration)
        {
            var tween = TweenSpriteColor(target, target.color, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeColor(GameObject owner, Material target, Color targetVal, float duration)
        {
            string id = GenIDFor(owner, "Color");
            void UpdateColor(Color newCol)
            {
                target.color = newCol;
            }
            Tween<Color> newTween = new Tween<Color>(owner, id, target.color, targetVal, duration, UpdateColor);
            return DefaultTweenHandle.From(newTween);
        }

        public ITweenHandle TweenFloat(GameObject owner, Material target, string propertyName,
            float targetVal, float duration)
        {
            string id = GenIDFor(owner, propertyName);
            void UpdateFloat(float newFloat)
            {
                target.SetFloat(propertyName, newFloat);
            }
            float startVal = target.GetFloat(propertyName);
            Tween<float> tween = new Tween<float>(owner, id, startVal, targetVal, duration, UpdateFloat);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeColor(Material target, Color targetVal, float duration)
        {
            string id = GenIDFor(target, "Color");
            void UpdateColor(Color newCol)
            {
                target.color = newCol;
            }
            Tween<Color> newTween = new Tween<Color>(target, id, target.color, targetVal, duration, UpdateColor);
            return DefaultTweenHandle.From(newTween);
        }

        public Tween<float> TweenSpriteAlpha(SpriteRenderer renderer, float startAlpha, float endAlpha, float duration)
        {
            string id = GenIDFor(renderer, "Alpha");

            Tween<float> result = new Tween<float>(renderer.gameObject, id, startAlpha,
                endAlpha, duration, val =>
                {
                    Color color = renderer.color;
                    color.a = val;
                    renderer.color = color;
                });
            return result;
        }

        public Tween<Color> TweenSpriteColor(SpriteRenderer renderer, Color startCol, Color endCol, float duration, Action onComplete = null)
        {
            onComplete += delegate { };
            string id = GenIDFor(renderer, "Color");

            Tween<Color> result = new Tween<Color>(renderer.gameObject, id, startCol,
                endCol, duration, val =>
                {
                    renderer.color = val;
                })
                .SetOnComplete(onComplete);
            return result;
        }

        public Tween<float> TweenGraphicAlpha(Graphic graphic, float startAlpha, float endAlpha, float duration,
            Action onComplete = null)
        {
            onComplete += delegate { };
            string id = GenIDFor(graphic, "Alpha");

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

        public Tween<Color> TweenGraphicColor(Graphic graphic, Color startCol, Color endCol, float duration,
            Action onComplete = null)
        {
            onComplete += delegate { };
            string id = GenIDFor(graphic, "Color");

            Tween<Color> result = new Tween<Color>(graphic.gameObject, id, startCol,
                endCol, duration, val =>
                {
                    graphic.color = val;
                })
                .SetOnComplete(onComplete);
            return result;
        }


        public ITweenHandle FadeOpacity(Graphic target, float endVal, float duration)
        {
            var tween = TweenGraphicAlpha(target, target.color.a, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeOpacity(SpriteRenderer target, float endVal, float duration)
        {
            var tween = TweenSpriteAlpha(target, target.color.a, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeOpacity(CanvasGroup target, float endVal, float duration)
        {
            var tween = TweenCanvasGroupAlpha(target, target.alpha, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public Tween<float> TweenCanvasGroupAlpha(CanvasGroup group, float startAlpha, float targAlpha,
            float duration, Action onComplete = null)
        {
            onComplete += delegate { };
            string id = GenIDFor(group, "Alpha");

            void UpdateTheAlpha(float newAlpha)
            {
                group.alpha = newAlpha;
            }
            Tween<float> result = new Tween<float>(group, id, startAlpha, targAlpha, duration, UpdateTheAlpha)
            .SetOnComplete(onComplete);

            return result;
        }
        #endregion

        public Tween<float> TweenFloat(Func<float> getFloatToTween, Action<float> setFloatToTween,
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
        public Tween<T> TweenBasic<T>(Func<T> getValToTween, Action<T> setValToTween,
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

        public Tween<Vector3> TweenScale(GameObject gameObject, Vector3 startScale, Vector3 endScale, float duration)
        {
            return TweenScale(gameObject.transform, startScale, endScale, duration);
        }

        public Tween<Vector3> TweenScale(Transform transform, Vector3 startScale, Vector3 endScale, float duration)
        {
            string id = GenIDFor(transform, "Scale");
            Tween<Vector3> result = new Tween<Vector3>(transform, id, startScale,
                endScale, duration, value =>
                {
                    transform.localScale = value;
                });

            return result;
        }

        public Tween<Vector3> TweenPosition(Transform transform, Vector3 startPos, Vector3 endPos, float duration)
        {
            string id = GenIDFor(transform, "Position");
            Tween<Vector3> result = new Tween<Vector3>(transform, id, startPos,
                endPos, duration, value =>
                {
                    transform.position = value;
                });

            return result;
        }

        public Tween<Quaternion> TweenRotation(Transform toRotate, Quaternion startRot,
            Quaternion endRot, float duration)
        {
            string id = GenIDFor(toRotate, "Rotation");
            void UpdateRot(Quaternion newRot)
            {
                toRotate.rotation = newRot;
            }
            Tween<Quaternion> result = new Tween<Quaternion>(toRotate, id, startRot,
                endRot, duration, UpdateRot);

            return result;
        }


        
        public ITweenHandle FadeBackgroundColor(Camera target, Color targetVal, float duration)
        {
            var tween = TweenCameraBGColor(target, target.backgroundColor, targetVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public Tween<Color> TweenCameraBGColor(Camera target, Color startVal, Color targetVal, float duration)
        {
            string id = GenIDFor(target, "BackgroundColor");
            Tween<Color> result = new Tween<Color>(target, id, startVal, targetVal, duration,
                val =>
                {
                    target.backgroundColor = val;
                });
            return result;
        }

        public ITweenHandle FadeColor(Light target, Color targetVal, float duration)
        {
            string id = GenIDFor(target, "ShiftColor");
            void UpdateColor(Color newCol)
            {
                target.color = newCol;
            }
            Tween<Color> newTween = new Tween<Color>(target, id, target.color, targetVal, duration, UpdateColor);
            return DefaultTweenHandle.From(newTween);
        }

        public ITweenHandle TweenFOV(Camera target, float targetVal, float duration)
        {
            var tween = TweenCameraFOC(target, target.fieldOfView, targetVal, duration);
            return DefaultTweenHandle.From(tween);
        }


        public Tween<float> TweenCameraFOC(Camera target, float startVal, float targetVal, float duration)
        {
            string id = GenIDFor(target, "FieldOfView");
            Tween<float> result = new Tween<float>(target.gameObject, id, startVal,
                targetVal, duration, val =>
                {
                    target.fieldOfView = val;
                });
            return result;
        }

        public ITweenHandle ShiftFillTo(Image target, float endVal, float duration)
        {
            var tween = TweenImageFill(target, target.fillAmount, endVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public Tween<float> TweenImageFill(Image img, float startVal, float endVal, float duration)
        {
            string id = GenIDFor(img, "FillAmount");

            Tween<float> result = new Tween<float>(img.gameObject, id, startVal,
                endVal, duration, val =>
                {
                    img.fillAmount = val;
                });
            return result;
        }

        public ITweenHandle TweenFloat(Material target, string propertyName, float targetVal, float duration)
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

        public ITweenHandle TweenIntensity(Light target, float targetVal, float duration)
        {
            string id = GenIDFor(target, "ShiftIntensity");
            void UpdateIntensity(float newIntensity)
            {
                target.intensity = newIntensity;
            }
            Tween<float> tween = new Tween<float>(target, id, target.intensity, targetVal, duration, UpdateIntensity);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeLowPassCutoff(AudioLowPassFilter target, float targetVal, float duration)
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

        public ITweenHandle TweenOrthoSize(Camera target, float targetVal, float duration)
        {
            var tween = TweenCameraOrthoSize(target, target.orthographicSize, targetVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        public Tween<float> TweenCameraOrthoSize(Camera target, float startSize, float endSize, float duration)
        {
            string id = GenIDFor(target, "OrthographicSize");
            Tween<float> result = new Tween<float>(target.gameObject, id, startSize,
                endSize, duration, val =>
                {
                    target.orthographicSize = val;
                });
            return result;
        }

        public ITweenHandle FadePitchN33(AudioSource target, float targVal, float duration)
        {
            var tween = TweenAudioSourcePitchN33(target, target.pitch, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Uses a scale of -3 to 3. Normal pitch = 1
        /// </summary>
        public Tween<float> TweenAudioSourcePitchN33(AudioSource source, float startPitch,
            float endPitch, float duration, Action onComplete = null)
        {
            startPitch = Mathf.Clamp(startPitch, -3, 3);
            endPitch = Mathf.Clamp(endPitch, -3, 3);

            onComplete += delegate { };
            string id = GenIDFor(source, "Pitch");
            void UpdateThePitch(float val)
            {
                source.pitch = val;
            }

            Tween<float> result = new Tween<float>(source, id, startPitch, endPitch, duration, UpdateThePitch)
            .SetOnComplete(onComplete);

            return result;
        }

        public ITweenHandle ShiftPitchTo(AudioTweenArgs tweenArgs)
        {
            return FadePitch(tweenArgs.Target, tweenArgs.TargetValue, tweenArgs.HowLongToTake);
        }

        public ITweenHandle TweenAudioSourceVolume(AudioTweenArgs tweenArgs)
        {
            return FadeVolume(tweenArgs.Target, tweenArgs.TargetValue, tweenArgs.HowLongToTake);
        }

        public ITweenHandle FadePitch(AudioSource target, float targVal, float duration)
        {
            var tween = TweenAudioSourcePitch(target, target.pitch, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Uses a scale of -300 for min to 300 for max. Normal pitch = 100
        /// </summary>
        /// <returns></returns>
        public Tween<float> TweenAudioSourcePitch(AudioSource source, float startPitch,
            float endPitch, float duration, Action onComplete = null)
        {
            return TweenAudioSourcePitchN33(source, startPitch / 100, endPitch / 100, duration, onComplete);
        }

        public ITweenHandle FadeColor(Light target, float targetVal, float duration)
        {
            string id = GenIDFor(target, "ShiftRange");
            void UpdateRange(float newRange)
            {
                target.range = newRange;
            }
            Tween<float> tween = new Tween<float>(target, id, target.range, targetVal, duration, UpdateRange);
            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadeReverbLevel(AudioReverbFilter target, float targetVal, float duration)
        {
            string id = GenIDFor(target, "ReverbLevel");
            void UpdateReverbLevel(float newReverbLevel)
            {
                target.reverbLevel = newReverbLevel;
            }
            Tween<float> tween = new Tween<float>(target, id, target.reverbLevel, targetVal, duration, UpdateReverbLevel);
            return DefaultTweenHandle.From(tween);
        }

        

        public ITweenHandle FadeVolume01(AudioSource target, float targVal, float duration)
        {
            targVal = Mathf.Clamp01(targVal);
            var tween = TweenAudioSourceVolume01(target, target.volume, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Uses a scale of 0 to 1
        /// </summary>
        public Tween<float> TweenAudioSourceVolume01(AudioSource source, float startVol,
            float endVol, float duration, Action onComplete = null)
        {
            startVol = Mathf.Clamp01(startVol);
            endVol = Mathf.Clamp01(endVol);

            string id = GenIDFor(source, "Volume");
            void UpdateTheVol(float newVol)
            {
                source.volume = newVol;
            }
            Tween<float> result = new Tween<float>(source, id, startVol, endVol, duration, UpdateTheVol)
                .SetOnComplete(onComplete);

            return result;
        }

        /// <summary>
        /// Scale of 0 to 100
        /// </summary>
        public ITweenHandle FadeVolume(AudioSource target, float targVal, float duration)
        {
            var tween = TweenAudioSourceVolume(target, target.volume, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Uses a scale of 0 to 100
        /// </summary>
        /// <returns></returns>
        public Tween<float> TweenAudioSourceVolume(AudioSource source, float startVol,
            float endVol, float duration, Action onComplete = null)
        {
            return TweenAudioSourceVolume01(source, startVol / 100, endVol / 100, duration, onComplete);
        }

        public ITweenHandle FadeVolume01(IAudioTrack track, float targVal, float duration)
        {
            return FadeVolume(track, targVal * 100f, duration);
        }

        /// <summary>
        /// Scale of 0 to 100
        /// </summary>
        public ITweenHandle FadeVolume(IAudioTrack track, float targVal, float duration)
        {
            string id = GenIDFor(track.GameObject, "MyceliaudioVolume");
            void UpdateTheVol(float newVol)
            {
                track.BaseVolume = newVol;
            }
            Tween<float> tween = new Tween<float>(track.GameObject, id, track.BaseVolume,
                targVal, duration, UpdateTheVol);

            return DefaultTweenHandle.From(tween);
        }

        #region General
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

        public ITweenHandle TweenGeneral(Func<float> getter, Action<float> setter, float endVal,
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
        #endregion


    }

}