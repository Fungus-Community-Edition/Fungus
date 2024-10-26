using System;
using UnityEngine;
using UnityEngine.Events;

namespace Fungus
{
    public class FloatTweenArgs : TweenArgs<float>
    {
        public UnityAction<FloatTweenArgs> OnComplete = delegate { };
    }
}