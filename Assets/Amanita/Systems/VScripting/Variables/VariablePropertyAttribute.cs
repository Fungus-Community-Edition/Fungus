using System.Linq;
using UnityEngine;

namespace Amanita.VScripting
{
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