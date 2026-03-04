using Type = System.Type;
using System.Reflection;

namespace AtMycelia.SaveSys
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

        public static string GetAssetName(Type type)
        {
            var attr = type.GetCustomAttribute<SaveSysAssetName>();
            if (attr != null)
            {
                return attr.AssetName;
            }
            return type.Name;
        }
    }
}