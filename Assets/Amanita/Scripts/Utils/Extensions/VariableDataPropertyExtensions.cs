using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    public static class VariableDataPropertyExtensions
    {
        /// <summary>
        /// Assigns a chosen variable or literal to a VariableData's varRef property,
        /// wrapping UnityObj variables in a VariablePointer<T> if needed.
        /// NOTE: This overload sets the SerializedProperty directly and DOES NOT call VariableData.VarRef setter.
        /// Prefer the overload that takes the owning VariableData to ensure its setter runs.
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
                        // Keep legacy behavior for Object-backed variables
                        WrapVarIntoPointer();
                        void WrapVarIntoPointer()
                        {
                            var pointerType = typeof(VariablePointer<>).MakeGenericType(contentType);

                            var ctor = pointerType.GetConstructor(new[] { typeof(UnityObj) });

                            object wrapper;
                            if (ctor != null)
                            {
                                wrapper = ctor.Invoke(new object[] { unityObj });
                            }
                            else
                            {
                                wrapper = Activator.CreateInstance(pointerType);
                                var field = pointerType.GetField("_component", BindingFlags.NonPublic | BindingFlags.Instance);
                                field?.SetValue(wrapper, unityObj);
                            }

                            varRefProp.managedReferenceValue = wrapper;
                        }
                    }
                    else
                    {
                        // Pure CLR type (like the Muscariables themselves); assign directly
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