using System;

namespace Amanita.VScripting
{
    /// <summary>
    /// Optional attribute for overriding display name and category in the registry.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class VariableDataAttribute : Attribute
    {
        public string DisplayName { get; }
        public string Category { get; }

        public VariableDataAttribute(string displayName, string category)
        {
            DisplayName = displayName;
            Category = category;
        }
    }
}