using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Fungus
{
    public class NeoTweenManager : MonoBehaviour
    {
        protected static NeoTweenManager _s;
        public static NeoTweenManager S
        {
            get
            {
                if (_s == null)
                {
                    GameObject holder = new GameObject("FungusTweenManager");
                    _s = holder.AddComponent<NeoTweenManager>();

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
                    tween.OnComplete();
                    tween.OnComplete = delegate { };
                    RemoveTween(pair.Key);
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

        public static Tween<float> TweenFloat(Func<float> getFloatToTween, Action<float> setFloatToTween,
            float endValue, float duration)
        {
            string id = $"{getFloatToTween.Target.GetHashCode()}_Float";
            object target = getFloatToTween.Target;
            float startVal = getFloatToTween();

            Tween<float> result = new Tween<float>(target, id, startVal, endValue, duration, value =>
            {
                setFloatToTween(value);
            });

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
            return TweenAudioSourceVolume(args.Target, args.BaseValue, args.TargetValue, args.HowLongToTake);
        }

        /// <summary>
        /// Uses a scale of 0 to 100
        /// </summary>
        /// <returns></returns>
        public static Tween<float> TweenAudioSourceVolume(AudioSource source, float startVol, float endVol, float duration)
        {
            return TweenAudioSourceVolume01(source, startVol / 100, endVol / 100, duration);
        }

        /// <summary>
        /// Uses a scale of 0 to 2
        /// </summary>
        public static Tween<float> TweenAudioSourceVolume01(AudioSource source, float startVol, float endVol, float duration)
        {

            string id = $"AudioSource_{source.GetInstanceID()}_Volume";
            Tween<float> result = new Tween<float>(source, id, startVol, endVol, duration, val =>
            {
                source.volume = val;
            });

            return result;
        }

    }
}