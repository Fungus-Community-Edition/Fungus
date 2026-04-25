namespace AtMycelia.AmaniTween
{
    /// <summary>
    /// For classes that can kill all active tweens on a particular target.
    /// </summary>
    public interface IOmniTweenKiller
    {
        void KillAll();
        void KillAllOn(System.Object target);
    }

    public interface IOmniTweenKiller<T> : IOmniTweenKiller
    {
        void KillAllOn(T target);
    }
    
}