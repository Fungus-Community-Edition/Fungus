namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Extension methods for VariableSourceAsset to be used in the editor.
    /// </summary>
    public static class VariableSourceAssetEditorExt
    {
        public static void RemoveVariableAt(this VariableSourceAsset source, int index)
        {
            if (source == null || index < 0 || index >= source.Variables.Count) return;
        }

    }
}