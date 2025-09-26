using System;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using System.Reflection;

namespace Amanita.VScripting
{
    public static class VariableDataPropertyExtensions
    {
        /// <summary>
        /// Assigns a chosen variable or literal to a VariableData's varRef property,
        /// wrapping UnityObj variables in a VariablePointer<T> if needed.
        /// </summary>
        public static void AssignVarRef(this SerializedProperty varRefProp, object chosen, Type contentType)
        {
            if (varRefProp == null)
                throw new ArgumentNullException(nameof(varRefProp));

            switch (varRefProp.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    varRefProp.objectReferenceValue = chosen as UnityObj;
                    break;

                case SerializedPropertyType.ManagedReference:
                case SerializedPropertyType.Generic: // Unity sometimes reports SerializeReference as Generic
                    if (chosen == null)
                    {
                        varRefProp.managedReferenceValue = null;
                    }
                    else if (chosen is UnityObj unityObj && chosen is IVariable)
                    {
                        WrapVarIntoPointer();
                        void WrapVarIntoPointer()
                        {
                            var pointerType = typeof(VariablePointer<>).MakeGenericType(contentType);

                            // Look for a ctor that takes a UnityObj
                            var ctor = pointerType.GetConstructor(new[] { typeof(UnityObj) });

                            object wrapper;
                            if (ctor != null)
                            {
                                // Preferred: construct with the component already set
                                wrapper = ctor.Invoke(new object[] { unityObj });
                            }
                            else
                            {
                                // Fallback: default-construct, then set _component via reflection
                                wrapper = Activator.CreateInstance(pointerType);
                                var field = pointerType.GetField("_component", BindingFlags.NonPublic | BindingFlags.Instance);
                                field?.SetValue(wrapper, unityObj);
                            }

                            varRefProp.managedReferenceValue = wrapper;
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