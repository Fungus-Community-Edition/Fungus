using UnityEngine;
using Type = System.Type;
using System.Reflection;

namespace Amanita.SaveSys
{
    public static class SaveSysTypeUtils
    {
        public static string GetDisplayName(Type type)
        {
            var attr = type.GetCustomAttribute<SaveSysDisplayName>();
            if (attr != null)
            {
                return attr.DisplayName;
            }

            return $"{type.Name} ({type.Namespace})";
        }
    }
}