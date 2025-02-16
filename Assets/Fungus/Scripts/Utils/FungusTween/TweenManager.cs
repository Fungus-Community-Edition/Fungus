using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Fungus
{
    public class TweenManager : MonoBehaviour
    {
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