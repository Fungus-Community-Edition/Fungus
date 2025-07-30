using UnityEngine;
using UnityEditor;

namespace Amanita.EditorUtils
{
    public class ExportPackageMenuItems : MonoBehaviour
    {
        [MenuItem("Tools/Amanita/Utilities/Export Amanita Package")]
        static void ExportFungusPackageFull()
        {
            ExportAmanitaPackage( new string[] {"Assets/Amanita", "Assets/FungusExamples" });
        }

        [MenuItem("Tools/Amanita/Utilities/Export Fungus Package - Lite")]
        static void ExportFungusPackageLite()
        {
            ExportAmanitaPackage(new string[] { "Assets/Amanita" });
        }

        static void ExportAmanitaPackage(string[] folders)
        {
            string path = EditorUtility.SaveFilePanel("Export Amanita Package", "", "Amanita", "unitypackage");
            if (path.Length == 0)
            {
                return;
            }

            AssetDatabase.ExportPackage(folders, path, ExportPackageOptions.Recurse);
        }
    }
}
