using System.Linq;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Attribute class for variables. This helps decide how they're presented as an option
    /// when selecting a variable to add to a Flowchart.
    /// </summary>
    public sealed class VariableInfoAttribute : System.Attribute
    {
        // Note do not use "isPreviewedOnly:true", it causes the script to fail to load without errors shown
        public VariableInfoAttribute(string category, string variableType, string uniqueId = "", int order = 0)
        {
            this.Category = category;
            this.VariableType = variableType;
            this.UniqueID = uniqueId;

            if (string.IsNullOrEmpty(uniqueId))
            {
                this.UniqueID = this.VariableType;
            }

            this.Order = order;
        }

        public string Category { get; set; }
        public string VariableType { get; set; }
        public int Order { get; set; }
        public string UniqueID { get; set; }
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
            this.VariableTypes = VariableTypeRegistry.AllTypes.ToArray();
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