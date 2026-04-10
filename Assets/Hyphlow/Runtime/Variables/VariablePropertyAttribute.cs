using UnityEngine;
using Type = System.Type;
using System.Collections.Generic;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Attribute class for variable properties. We use these so that fields in Commands
    /// that should ONLY take variable inputs accept the intended variable types.
    /// </summary>
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public sealed class VariablePropertyAttribute : PropertyAttribute
    {
        public VariablePropertyAttribute()
        {
            this.VariableTypes.AddRange(VariableTypeRegistry.AllLegacyTypes);
            this.VariableTypes.AddRange(VariableTypeRegistry.AllMuscariableTypes);
        }

        public VariablePropertyAttribute(params Type[] variableTypes)
        {
            this.VariableTypes.AddRange(variableTypes);
        }

        public VariablePropertyAttribute(string defaultText, params Type[] variableTypes)
        {
            this.defaultText = defaultText;
            this.VariableTypes.AddRange(variableTypes);
        }

        public string defaultText = "<None>";
        public string compatibleVariableName = string.Empty;

        public List<Type> VariableTypes { get; set; } = new List<Type>();
    }

}