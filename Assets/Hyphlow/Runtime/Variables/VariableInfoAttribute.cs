using System;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Attribute class for variables. This helps decide how they're presented as an option
    /// when selecting a variable to add to a Flowchart.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public sealed class VariableInfoAttribute : Attribute
    {
        // Note do not use "isPreviewedOnly:true", it causes the script to fail to load without errors shown
        public VariableInfoAttribute(string category, string optionDisplayName, Type contentType,
            bool showInMenu = true, int order = 0, bool isTest = false)
        {
            this.Category = category;
            this.OptionDisplayName = optionDisplayName;
            this.ContentType = contentType;
            this.ShowInMenu = showInMenu;
            this.Order = order;
            this.IsTest = isTest;
        }

        public string Category { get; set; }
        public string OptionDisplayName { get; set; }
        public Type ContentType { get; set; }
        public bool ShowInMenu { get; set; }
        public int Order { get; set; }
        public bool IsTest { get; set; }
        public bool IsPreviewedOnly { get; set; }
    }

}