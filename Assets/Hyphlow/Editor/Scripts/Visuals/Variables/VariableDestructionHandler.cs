using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Centralized destruction logic for all variable types, with full Undo/Redo support.
    /// Handles both legacy UnityEngine.Object variables and MuscariableHolders.
    /// </summary>
    public static class VariableDestructionHandler
    {
        public static void DestroyWithUndo(Object target, Object undoContext, string undoLabel = "Remove Variable")
        {
            if (target == null)
                return;

            Undo.RegisterCompleteObjectUndo(undoContext, undoLabel);
            Undo.DestroyObjectImmediate(target);
            EditorUtility.SetDirty(undoContext);
        }
    }


}