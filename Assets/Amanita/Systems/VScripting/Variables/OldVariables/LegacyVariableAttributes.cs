using System;
using System.Linq;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Attribute class for variables. This helps decide how they're presented as an option
    /// when selecting a variable to add to a Flowchart.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class VariableInfoAttribute : System.Attribute
    {
        // Note do not use "isPreviewedOnly:true", it causes the script to fail to load without errors shown
        public VariableInfoAttribute(string category, string optionDisplayName, Type contentType,
            bool isLegacy = true, int order = 0)
        {
            this.Category = category;
            this.OptionDisplayName = optionDisplayName;
            this.ContentType = contentType;
            this.IsLegacy = isLegacy;

            this.Order = order;
        }

        public string Category { get; set; }
        public string OptionDisplayName { get; set; }
        public Type ContentType { get; set; }
        public int Order { get; set; }
        public bool IsLegacy { get; set; }
        public bool IsPreviewedOnly { get; set; }
    }

    /// <summary>
    /// Attribute class for variable properties. We use these so that fields in Commands
    /// that should ONLY take variable inputs accept the intended variable types.
    /// </summary>
    public sealed class VariablePropertyAttribute : PropertyAttribute
    {
        public VariablePropertyAttribute()
        {
            this.VariableTypes = VariableTypeRegistry.AllLegacyTypes.ToArray();
        }
        
        public VariablePropertyAttribute(params System.Type[] variableTypes)
        {
            this.VariableTypes = variableTypes;
        }

        public VariablePropertyAttribute(string defaultText, params System.Type[] variableTypes)
        {
            this.defaultText = defaultText;
            this.VariableTypes = variableTypes;
        }

        public string defaultText = "<None>";
        public string compatibleVariableName = string.Empty;

        public System.Type[] VariableTypes { get; set; }
    }

}