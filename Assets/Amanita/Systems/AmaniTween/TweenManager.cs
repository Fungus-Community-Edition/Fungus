using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Amanita.Tweening
{
    public class TweenManager : MonoBehaviour, ITransformTweenAdapter, IGeneralTweenAdapter<Vector2>,
        IGeneralTweenAdapter<Vector3>, IGeneralTweenAdapter<float>, IGeneralTweenAdapter<int>,
        IGraphicTweenAdapter, ICameraTweenAdapter
    {
        public static DefaultTweenAdapter TweenAdapter { get; protected set; } =
            ScriptableObject.CreateInstance<DefaultTweenAdapter>();

        protected static TweenManager _s;
        public static TweenManager S
        {
            get
            {
                if (_s == null)
                {
                    GameObject holder = new GameObject("FungusTweenManager");
                    _s = holder.AddComponent<TweenManager>();

                }

                return _s;
            }
        }

        protected Dictionary<string, ITween> _activeTweens = new();

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
        
        public static Tween<Quaternion> TweenRotation(Transform transform, Quaternion startRot, Quaternion endRot, float duration)
        {
            string id = $"Transform_{transform.GetInstanceID()}_Rot";
            Tween<Quaternion> result = new Tween<Quaternion>(transform, id, startRot,
                endRot, duration, value =>
                {
                    transform.rotation = value;
                });

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
        /// Uses a scale of 0 to 200
        /// </summary>
        /// <returns></returns>
        public static Tween<float> TweenAudioSourcePitch(AudioSource source, float startPitch,
            float endPitch, float duration, Action onComplete = null)
        {
            return TweenAudioSourcePitch02(source, startPitch / 100, endPitch / 100, duration, onComplete);
        }

        /// <summary>
        /// Uses a scale of 0 to 2
        /// </summary>
        public static Tween<float> TweenAudioSourcePitch02(AudioSource source, float startPitch,
            float endPitch, float duration, Action onComplete = null)
        {
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
        /// Uses a scale of 0 to 2
        /// </summary>
        public static Tween<float> TweenAudioSourceVolume01(AudioSource source, float startVol,
            float endVol, float duration, Action onComplete = null)
        {

            string id = $"AudioSource_{source.GetInstanceID()}_Volume";
            Tween<float> result = new Tween<float>(source, id, startVol, endVol, duration, val =>
            {
                source.volume = val;
            })
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
            var tweenRot = TweenRotation(transform, target.rotation, rotation, duration);
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
    }

    public class DefaultTweenHandle : ITweenHandle
    {
        public static DefaultTweenHandle From(ITween tween)
        {
            var result = new DefaultTweenHandle(tween);
            return result;
        }

        public DefaultTweenHandle(ITween tween)
        {
            Tween = tween;
        }

        public virtual void Kill()
        {
            Tween?.FullKill();
        }

        public virtual ITween Tween { get; set; }

        public virtual bool IsPlaying => Tween != null && !Tween.IsPaused;

        public virtual ITweenHandle SetOnComplete(Action arg)
        {
            OnComplete = arg;
            return this;
        }

        public virtual Action OnComplete
        {
            get
            {
                Action result = delegate { };
                if (Tween != null)
                {
                    result = () => Tween.OnComplete();
                }

                return result;
            }
            set
            {
                if (Tween != null)
                {
                    Tween.OnComplete = value;
                }
            }
        }
    }

    public class DefaultTweenAdapter : ScriptableObject, ITransformTweenAdapter, IGeneralTweenAdapter<Vector2>,
        IGeneralTweenAdapter<Vector3>, IGeneralTweenAdapter<float>, IGeneralTweenAdapter<int>,
        IGraphicTweenAdapter, ICameraTweenAdapter
    {
        public ITweenHandle FadeTo(Graphic target, float endVal, float duration)
        {
            return TweenManager.S.FadeTo(target, endVal, duration);
        }

        public ITweenHandle FadeTo(SpriteRenderer target, float endVal, float duration)
        {
            return TweenManager.S.FadeTo(target, endVal, duration);
        }

        public ITweenHandle FadeTo(CanvasGroup target, float endVal, float duration)
        {
            return TweenManager.S.FadeTo(target, endVal, duration);
        }

        public ITweenHandle MoveTo(Transform target, Vector3 position, float duration)
        {
            return TweenManager.S.MoveTo(target, position, duration);
        }

        public ITweenHandle RotateTo(Transform target, Quaternion rotation, float duration)
        {
            return TweenManager.S.RotateTo(target, rotation, duration);
        }

        public ITweenHandle ScaleTo(Transform target, Vector3 scale, float duration)
        {
            return TweenManager.S.ScaleTo(target, scale, duration);
        }

        public ITweenHandle ShiftBackgroundColorTo(Camera target, Color targetVal, float duration)
        {
            throw new NotImplementedException();
        }

        public ITweenHandle ShiftColorTo(Graphic target, Color endVal, float duration)
        {
            throw new NotImplementedException();
        }

        public ITweenHandle ShiftColorTo(SpriteRenderer target, Color endVal, float duration)
        {
            throw new NotImplementedException();
        }

        public ITweenHandle ShiftFieldOfViewTo(Camera target, float targetVal, float duration)
        {
            throw new NotImplementedException();
        }

        public ITweenHandle ShiftFillTo(Image target, float endVal, float duration)
        {
            throw new NotImplementedException();
        }

        public ITweenHandle ShiftOrthographicSizeTo(Camera target, float targetVal, float duration)
        {
            throw new NotImplementedException();
        }

        public ITweenHandle TweenGeneral(Func<Vector2> getter, Action<Vector2> setter, Vector2 endVal,
            float duration, Action onComplete = null)
        {
            return TweenManager.S.TweenGeneral(getter, setter, endVal, duration, onComplete);
        }

        public ITweenHandle TweenGeneral(Func<Vector3> getter, Action<Vector3> setter, Vector3 endVal,
            float duration, Action onComplete = null)
        {
            return TweenManager.S.TweenGeneral(getter, setter, endVal, duration, onComplete);
        }

        public ITweenHandle TweenGeneral(Func<float> getter, Action<float> setter, float endVal,
            float duration, Action onComplete = null)
        {
            return TweenManager.S.TweenGeneral(getter, setter, endVal, duration, onComplete);
        }

        public ITweenHandle TweenGeneral(Func<int> getter, Action<int> setter, int endVal,
            float duration, Action onComplete = null)
        {
            return TweenManager.S.TweenGeneral(getter, setter, endVal, duration, onComplete);
        }
    }
}