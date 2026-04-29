using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AtMycelia.AmaniTween
{
    public class TweenManager : MonoBehaviour
    {
        public static TweenManager S
        {
            get
            {
                return _s;
            }
            set
            {
                _s = value;
            }
        }
        protected static TweenManager _s;

        protected Dictionary<string, ITween> _activeTweens = new();

        protected virtual void Awake()
        {
            if (_s != null && _s != this)
            {
                Debug.LogWarning("Multiple TweenManagers detected. Destroying the new one.");
                Destroy(this.gameObject);
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

        public virtual void RemoveTween(string id)
        {
            _activeTweens.Remove(id);
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