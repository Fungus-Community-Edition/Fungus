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

        /// <summary>
        /// This assumes a scene is running. If not (and you're calling from outside the main thread),
        /// this will cause an error.
        /// </summary>
        public static void RunOnMainThread(System.Action action)
        {
            if (IsMainThread)
            {
                action();
            }
            else
            {
                // Schedule the action to run on the main thread
                using (var countdown = new CountdownEvent(1))
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        action();
                        countdown.Signal(); // Signal that we're done
                    });
                    countdown.Wait(); // Wait for the main thread to finish
                }
            }
        }
    }
}