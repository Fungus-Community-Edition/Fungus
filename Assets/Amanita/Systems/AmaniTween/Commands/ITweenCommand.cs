using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.Amanita.Tweening.VScripting
{
    public interface ITweenCommand
    {
        FloatData Duration { get; }
        ScriptableObject TweenerSO { get; }
        BooleanData StopPreviousTweens { get; }
        BooleanData WaitUntilFinished { get; }
        ITweenHandle CurrentTween { get; }
    }
}