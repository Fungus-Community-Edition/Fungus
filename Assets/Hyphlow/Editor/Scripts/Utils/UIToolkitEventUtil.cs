using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class UIToolkitEventUtil
    {
        private static readonly MethodInfo registerMethod =
            typeof(CallbackEventHandler)
                .GetMethod(nameof(CallbackEventHandler.RegisterCallback),
                           flags,
                           null,
                           new[] { typeof(EventCallback<>), typeof(TrickleDown) },
                           null);

        public static void ToggleValueChangeDynamic(VisualElement element, Delegate callback, bool on)
        {
            if (element == null || callback == null) return;

            var interfacesImplemented = element.GetType().GetInterfaces();
            var notifyInterface = Array.Find(interfacesImplemented, IsINotifyValueChanged);
            static bool IsINotifyValueChanged(Type elem)
            {
                return elem.IsGenericType &&
                       elem.GetGenericTypeDefinition() == typeof(INotifyValueChanged<>);
            }
            if (notifyInterface == null)
                return; // Not a value field

            var valueType = notifyInterface.GetGenericArguments()[0];
            var changeEventType = typeof(ChangeEvent<>).MakeGenericType(valueType);

            // Build the correct EventCallback<ChangeEvent<T>> type
            var eventCallbackType = typeof(EventCallback<>).MakeGenericType(changeEventType);

            // Convert the provided delegate to the right type
            var typedCallback = Delegate.CreateDelegate(eventCallbackType, callback.Target, callback.Method);

            // Get RegisterCallback<T> or UnregisterCallback<T> based on whether we're
            // toggling it on or off
            var methodName = on ? nameof(CallbackEventHandler.RegisterCallback)
                                : nameof(CallbackEventHandler.UnregisterCallback);

            var firstValidMethod = typeof(CallbackEventHandler)
                .GetMethods(flags)
                .First(elem => elem.Name == methodName &&
                            elem.IsGenericMethod &&
                            elem.GetParameters().Length >= 1);

            if (firstValidMethod == null)
            {
                return;
            }
            var method = firstValidMethod.MakeGenericMethod(changeEventType);

            // Invoke it
            method.Invoke(element, new object[] { typedCallback, TrickleDown.NoTrickleDown });
        }

        private static BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    }
}