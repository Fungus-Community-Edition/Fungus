using UnityEngine;
using UnityEditor;

namespace Amanita.EditorUtils
{
    public class ExportPackageMenuItems : MonoBehaviour
    {
        [MenuItem("Tools/Fungus/Utilities/Export Fungus Package")]
        static void ExportFungusPackageFull()
        {
            ExportFungusPackage( new string[] {"Assets/Fungus", "Assets/FungusExamples" });
        }

        [MenuItem("Tools/Fungus/Utilities/Export Fungus Package - Lite")]
        static void ExportFungusPackageLite()
        {
            ExportFungusPackage(new string[] { "Assets/Fungus" });
        }

        static void ExportFungusPackage(string[] folders)
        {
            string path = EditorUtility.SaveFilePanel("Export Fungus Package", "", "Fungus", "unitypackage");
            if (path.Length == 0)
            {
                return;
            }

            AssetDatabase.ExportPackage(folders, path, ExportPackageOptions.Recurse);
        }
    }
}
