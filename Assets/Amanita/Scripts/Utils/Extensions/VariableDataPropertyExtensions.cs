using System;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class VariableDataPropertyExtensions
    {
        /// <summary>
        /// Assigns a chosen variable or literal to a VariableData's varRef property,
        /// wrapping UnityEngine.Object variables in a VariablePointer<T> if needed.
        /// </summary>
        public static void AssignVarRef(this SerializedProperty varRefProp, object chosen, Type contentType)
        {
            if (varRefProp == null)
                throw new ArgumentNullException(nameof(varRefProp));

            switch (varRefProp.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    varRefProp.objectReferenceValue = chosen as UnityEngine.Object;
                    break;

                case SerializedPropertyType.ManagedReference:
                case SerializedPropertyType.Generic: // Unity sometimes reports SerializeReference as Generic
                    if (chosen == null)
                    {
                        varRefProp.managedReferenceValue = null;
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(contentType))
                    {
                        // Wrap the UnityEngine.Object in a VariablePointer<T>
                        if (chosen is UnityEngine.Object uo)
                        {
                            var pointerType = typeof(VariablePointer<>).MakeGenericType(contentType);
                            var wrapper = (IVariablePointer)Activator.CreateInstance(pointerType, uo);
                            wrapper.Component = uo;
                            varRefProp.managedReferenceValue = wrapper;
                        }
                        else
                        {
                            Debug.LogWarning($"AssignVarRef: Expected UnityEngine.Object for {contentType}, got {chosen.GetType()}");
                            varRefProp.managedReferenceValue = null;
                        }
                    }
                    else
                    {
                        // Pure CLR type — assign directly
                        varRefProp.managedReferenceValue = chosen;
                    }
                    break;

                default:
                    Debug.LogWarning($"AssignVarRef: Unsupported propertyType {varRefProp.propertyType} for {varRefProp.propertyPath}");
                    break;
            }
        }
    }
}