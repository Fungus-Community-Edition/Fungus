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

        /// <summary>
        /// Assigns a chosen variable or literal via VariableData.VarRef setter to ensure
        /// any associated identity/rehydration fields on VariableData are updated,
        /// then mirrors the assignment into the SerializedProperty for UI consistency.
        /// Will not record undo or mark dirty if the assignment is effectively a no-op
        /// (same variable identity already assigned).
        /// </summary>
        public static void AssignVarRef(this SerializedProperty varRefProp, VariableData ownerVarData, 
            object chosen, Type contentType)
        {
            if (varRefProp == null)
            {
                throw new ArgumentNullException(nameof(varRefProp));
            }
            if (ownerVarData == null)
            {
                Debug.LogError($"AssignVarRef: ownerVarData is null for {varRefProp.propertyPath}");
                return;
            }

            // Identity check: skip work if assigning the same variable
            var current = ownerVarData.VarRef; // May be a Unity Object (legacy) or a muscariable (managed ref)
            var chosenVar = chosen as IVariable;

            if (IsSameReference(current, chosen))
            {
                // Nothing to change: avoid Undo/dirty churn
                return;
            }

            var so = varRefProp.serializedObject;

            // Record undo for all targets
            Undo.RecordObjects(so.targetObjects, "Assign Variable Reference");

            // 1) Use the property setter so VariableData can capture identifiers
            try
            {
                ownerVarData.VarRef = chosenVar; // null is fine (means <Value>)
            }
            catch (Exception e)
            {
                Debug.LogError($"AssignVarRef: failed to set VariableData.VarRef on {varRefProp.propertyPath}. " +
                    $"Exception: {e}");
            }

            // 2) Mirror into SerializedProperty so the inspector reflects the selection immediately
            try
            {
                if (varRefProp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    varRefProp.objectReferenceValue = chosen as UnityObj;
                }
                else // ManagedReference or Generic (SerializeReference)
                {
                    // If user selected <Value>, force managed ref null regardless of what the getter returns.
                    if (chosen == null)
                    {
                        varRefProp.managedReferenceValue = null;
                    }
                    else if (chosen is UnityObj unityObj && chosen is IVariable)
                    {
                        // Wrap Unity Object-backed variables for managed reference fields
                        varRefProp.managedReferenceValue = CreatePointerWrapper(unityObj, contentType);
                    }
                    else
                    {
                        // If VariableData translates the selection, prefer what it exposes after set
                        var currentAfterSet = ownerVarData.VarRef;

                        if (currentAfterSet is UnityObj currentUnityObj)
                        {
                            // Still a Unity Object-backed variable: wrap it
                            varRefProp.managedReferenceValue = CreatePointerWrapper(currentUnityObj, contentType);
                        }
                        else
                        {
                            // Pure managed IVariable
                            varRefProp.managedReferenceValue = currentAfterSet;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string warningMessage = $"AssignVarRef: Failed to mirror into SerializedProperty for " +
                    $"{varRefProp.propertyPath}. Exception: {e}";
                Debug.LogWarning(warningMessage);
            }

            // 3) Apply and mark dirty to persist across reloads
            so.ApplyModifiedProperties();
            foreach (var t in so.targetObjects)
            {
                if (t != null)
                {
                    EditorUtility.SetDirty(t);
                }
            }

            static bool IsSameReference(IVariable current, object chosenObj)
            {
                if (chosenObj == null)
                    return current == null;

                // If both are UnityEngine.Object-backed variables
                if (chosenObj is UnityObj chosenUnityObj && chosenObj is IVariable chosenVar)
                {
                    if (current is UnityObj currentUnityObj)
                    {
                        // Unity's == handles destroyed objs correctly
                        if (currentUnityObj == chosenUnityObj)
                            return true;
                    }

                    // Fall back to identity by (OwnerId, ItemId) when available
                    if (current != null)
                    {
                        try
                        {
                            return current.ItemId == chosenVar.ItemId &&
                                   current.OwnerIdIndex == chosenVar.OwnerIdIndex;
                        }
                        catch
                        {
                            // Ignore if any accessor throws
                        }
                    }

                    return false;
                }

                // Pure CLR IVariable (muscariable)
                if (chosenObj is IVariable chosenManaged)
                {
                    if (ReferenceEquals(current, chosenManaged))
                        return true;

                    if (current != null)
                    {
                        try
                        {
                            return current.ItemId == chosenManaged.ItemId &&
                                   current.OwnerIdIndex == chosenManaged.OwnerIdIndex;
                        }
                        catch
                        {
                            // Ignore if any accessor throws
                        }
                    }
                }

                return false;
            }

            static object CreatePointerWrapper(UnityObj unityObj, Type contentType)
            {
                var pointerType = typeof(VariablePointer<>).MakeGenericType(contentType);
                var ctor = pointerType.GetConstructor(new[] { typeof(UnityObj) });

                if (ctor != null)
                {
                    return ctor.Invoke(new object[] { unityObj });
                }

                var wrapper = Activator.CreateInstance(pointerType);
                var field = pointerType.GetField("_component", BindingFlags.NonPublic | BindingFlags.Instance);
                field?.SetValue(wrapper, unityObj);
                return wrapper;
            }
        }
    }
}