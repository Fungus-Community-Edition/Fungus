using System;

namespace Amanita.VScripting
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MuscariableAttribute : Attribute
    {
        public string MenuName { get; }
        public Type ContentType { get; }
        public string TypeDisplayName { get; }

        public MuscariableAttribute(string menuName, Type contentType, string typeDisplayName)
        {
            MenuName = menuName;
            ContentType = contentType;
            TypeDisplayName = typeDisplayName;
        }
    }
}