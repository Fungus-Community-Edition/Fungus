using System;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RowVisualHandlerAttribute : Attribute
    {
        public string MenuName { get; }
        public Type ContentType { get; }
        public string TypeDisplayName { get; }

        /// <summary>
        /// Relative to any Resources folder
        /// </summary>
        public string PathToTemplate { get; }
        
        public RowVisualHandlerAttribute(string menuName, Type contentType, string typeDisplayName, string pathToTemplate)
        {
            bool isValid = Validate();
            bool Validate()
            {
                bool isValid = true;
                bool anyInputsAreInvalid = string.IsNullOrEmpty(menuName) ||
                    contentType == null ||
                    string.IsNullOrEmpty(typeDisplayName) ||
                    string.IsNullOrEmpty(pathToTemplate);
                if (anyInputsAreInvalid)
                {
                    string errorMessage = "Must make sure all inputs for RowVisualHandlerAttribute are neither null or empty.";
                    Debug.LogError(errorMessage);
                    isValid = false;
                }
                return isValid;
            }

            if (!isValid)
            {
                return;
            }

            MenuName = menuName;
            ContentType = contentType;
            TypeDisplayName = typeDisplayName;
            PathToTemplate = pathToTemplate;
        }
    }
}