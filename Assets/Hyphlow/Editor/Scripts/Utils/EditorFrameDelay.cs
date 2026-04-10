using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class EditorFrameDelay
    {
        public static void AfterFrames(int frameCount, Action action)
        {
            if (action == null || frameCount <= 0)
            {
                action?.Invoke();
                return;
            }

            int remaining = frameCount;
            void Tick()
            {
                remaining--;
                if (remaining <= 0)
                {
                    action();
                }
                else
                {
                    EditorApplication.delayCall += Tick;
                }
            }

            EditorApplication.delayCall += Tick;
        }

        public static void AfterFrames(VisualElement element, int frameCount, Action action)
        {
            if (element == null || action == null || frameCount <= 0)
            {
                action?.Invoke();
                return;
            }

            int remaining = frameCount;
            void Tick()
            {
                remaining--;
                if (remaining <= 0)
                {
                    action();
                }
                else
                {
                    element.schedule.Execute(Tick);
                }
            }

            element.schedule.Execute(Tick);
        }
    }
}