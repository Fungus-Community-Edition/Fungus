namespace Amanita
{
    public interface IAmanitaManagerSubmodule
    {
        void Init();
        int OrderIndex { get; }
    }
}