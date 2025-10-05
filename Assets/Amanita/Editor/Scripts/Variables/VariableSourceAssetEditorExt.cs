using System.Collections.Generic;
using System.Linq;

namespace Amanita.VScripting.EditorUtils
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

        public static MuscariableHolder GetHolderFor(this VariableSourceAsset thisAsset, IVariable variable)
        {
            // Prefer using the injected/global resolver when available so editor code is testable.
            string pathToThis;
            try
            {
                var resolver = VariableSourceAssetMaintenance.AssetResolver;
                if (resolver != null)
                {
                    pathToThis = resolver.GetAssetPath(thisAsset);
                    IList<MuscariableHolder> holders = resolver.LoadAllAssetsAtPath<MuscariableHolder>(pathToThis)
                        .OfType<MuscariableHolder>().ToList();
                    return holders.FirstOrDefault(holder => holder.ItemID == variable.ItemID && holder.ContentType.Equals(variable.ContentType));
                }
            }
            catch
            {
                // Fall back to AssetDatabase if resolver is not available or throws.
            }

            // Fallback: original behavior using AssetDatabase
            pathToThis = UnityEditor.AssetDatabase.GetAssetPath(thisAsset);
            IList<MuscariableHolder> fallbackHolders = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(pathToThis)
                .OfType<MuscariableHolder>().ToList();
            return fallbackHolders.FirstOrDefault(holder => holder.ItemID == variable.ItemID && holder.ContentType.Equals(variable.ContentType));
        }

        public static void RefreshHolders(this VariableSourceAsset thisAsset)
        {
            var resolver = VariableSourceAssetMaintenance.AssetResolver;
            if (resolver != null)
            {
                string pathToThis = resolver.GetAssetPath(thisAsset);
                IList<MuscariableHolder> holders = resolver.LoadAllAssetsAtPath<MuscariableHolder>(pathToThis)
                    .OfType<MuscariableHolder>().ToList();

                foreach (var elem in holders)
                {
                    elem.Refresh();
                }

            }
        }
    }
}