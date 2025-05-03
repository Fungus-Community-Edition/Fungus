using UnityEngine;

namespace Amanita
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Like regular GetComponent, except that if the component is not found, an instance
        /// of it is added and then returned.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent(out T result))
            {
                result = gameObject.AddComponent<T>();
            }

            return result;
        }
    }
}