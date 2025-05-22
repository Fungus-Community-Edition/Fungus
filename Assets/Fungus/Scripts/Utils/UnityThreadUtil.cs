using System.Threading;
using UnityEngine;

namespace Amanita.Utils
{
    [DefaultExecutionOrder(-9999)]
    public static class UnityThreadUtil
    {
        private static int _mainThreadId;
        private static bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _initialized = true;
        }

        public static bool IsMainThread
        {
            get
            {
                if (!_initialized)
                {
                    // Fallback: assume main thread if not initialized
                    return true;
                }
                return Thread.CurrentThread.ManagedThreadId == _mainThreadId;
            }
        }
    }
}