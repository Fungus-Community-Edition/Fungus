using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Amanita.Utils
{
    /// <summary>
    /// We need this so that things like codecs can execute stuff on the main thread.
    /// Such is usually needed for Unity API calls that aren't thread-safe.
    /// </summary>
    public class MainThreadDispatcher : MonoBehaviour
    {
        protected static readonly ConcurrentQueue<Action> _actions = new();

        public static MainThreadDispatcher S { get; private set; }

        protected virtual void Awake()
        {
            if (S == null)
            {
                S = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void Enqueue(Action action)
        {
            _actions.Enqueue(action);
        }

        protected virtual void Update()
        {
            while (_actions.TryDequeue(out var action))
            {
                action();
            }
        }
    }
}