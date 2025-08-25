using System;

namespace Amanita.VScripting.EditorUtils
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class VariableDataAttribute : Attribute
    {
        public string DisplayName { get; }
        public string Category { get; }

        public VariableDataAttribute(string displayName = null, string category = null)
        {
            DisplayName = displayName;
            Category = category;
        }
    }
}