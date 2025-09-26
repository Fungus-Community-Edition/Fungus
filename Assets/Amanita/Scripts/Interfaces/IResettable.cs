namespace Amanita
{
    public interface IResettable
    {
        /// <summary>
        /// Prepare for reuse without reloading static templates or attributes.
        /// </summary>
        void Reset();
    }

}