using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

namespace Amanita
{
    public class TweenManager : MonoBehaviour
    {
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
    }
}