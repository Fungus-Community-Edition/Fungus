using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Fungus
{
    public class FadeTweenManager : MonoBehaviour
    {
        public static FadeTweenManager S
        {
            get
            {
                bool alreadyExists = _s != null;
                if (alreadyExists)
                {
                    return _s;
                }

                GameObject managerGO = new GameObject("FungusFadeTweenManager");
                _s = managerGO.AddComponent<FadeTweenManager>();
                return _s;
            }
        }

        protected static FadeTweenManager _s;

        protected virtual void Awake()
        {
            IDictionary<AudioSource, IEnumerator> volumeTweens = new Dictionary<AudioSource, IEnumerator>();
            IDictionary<AudioSource, IEnumerator> pitchTweens = new Dictionary<AudioSource, IEnumerator>();

            TweenHolders[AudioTweenType.Volume] = volumeTweens;
            TweenHolders[AudioTweenType.Pitch] = pitchTweens;
        }

        protected virtual IDictionary<AudioTweenType, IDictionary<AudioSource, IEnumerator>> TweenHolders
        { get; set; } = new Dictionary<AudioTweenType, IDictionary<AudioSource, IEnumerator>>();

        protected WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    }
}