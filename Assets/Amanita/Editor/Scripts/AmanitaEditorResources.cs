using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Amanita.EditorUtils
{
    [CustomEditor(typeof(AmanitaEditorResources))]
    public class AmanitaEditorResourcesInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (serializedObject.FindProperty("updateOnReloadScripts").boolValue)
            {
                GUILayout.Label("Updating...");
            }
            else
            {
                if (GUILayout.Button("Sync with EditorResources folder"))
                {
                    AmanitaEditorResources.GenerateResourcesScript();
                }

                DrawDefaultInspector();
            }
        }
    }

    // Handle reimporting all assets
    public class EditorResourcesPostProcessor : AssetPostprocessor 
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] _, string[] __, string[] ___) 
        {
            foreach (var path in importedAssets)
            {
                if (path.EndsWith("AmanitaEditorResources.asset"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath(path, typeof(AmanitaEditorResources)) as AmanitaEditorResources;
                    if (asset != null)
                    {
                        AmanitaEditorResources.UpdateTextureReferences(asset);
                        AssetDatabase.SaveAssets();
                        return;
                    }
                }
            }
        }
    }

    public partial class AmanitaEditorResources : ScriptableObject
    {
        [Serializable]
        public class EditorTexture
        {
            [SerializeField] private Texture2D free;
            [SerializeField] private Texture2D pro;

            public Texture2D Texture2D
            {
                get { return EditorGUIUtility.isProSkin && pro != null ? pro : free; }
            }

            public EditorTexture(Texture2D free, Texture2D pro)
            {
                this.free = free;
                this.pro = pro;
            }
        }

        private static AmanitaEditorResources instance;
        private static readonly string editorResourcesFolderName = "\"_EditorResources\"";
        private static readonly string PartialEditorResourcesPath = System.IO.Path.Combine("Amanita", "Resources", "_EditorResources");
        [SerializeField] [HideInInspector] private bool updateOnReloadScripts = false;

        public static AmanitaEditorResources Instance
        {
            get
            {
                if (instance == null)
                {
                    var guids = AssetDatabase.FindAssets("AmanitaEditorResources t:AmanitaEditorResources");

                    if (guids.Length == 0)
                    {
                        instance = ScriptableObject.CreateInstance(typeof(AmanitaEditorResources)) as AmanitaEditorResources;
                        AssetDatabase.CreateAsset(instance, GetRootFolder() + "/AmanitaEditorResources.asset");
                    }
                    else 
                    {
                        if (guids.Length > 1)
                        {
                            Debug.LogError("Multiple AmanitaEditorResources assets found!");
                        }

                        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        instance = AssetDatabase.LoadAssetAtPath(path, typeof(AmanitaEditorResources)) as AmanitaEditorResources;
                    }
                }

                return instance;
            }
        }

        private static string GetRootFolder()
        {
            var res = AssetDatabase.FindAssets(editorResourcesFolderName);

            foreach (var item in res)
            {
                var path = AssetDatabase.GUIDToAssetPath(item);
                var safePath = System.IO.Path.GetFullPath(path);
                if (safePath.IndexOf(PartialEditorResourcesPath) != -1)
                    return path;
            }

            return string.Empty;
        }

        public static void GenerateResourcesScript()
        {
            // Get all unique filenames
            var textureNames = new HashSet<string>();
            var guids = AssetDatabase.FindAssets("t:Texture2D", new [] { GetRootFolder() });
            var paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            
            foreach (var path in paths)
            {
                textureNames.Add(Path.GetFileNameWithoutExtension(path));
            }

            var scriptGuid = AssetDatabase.FindAssets("AmanitaEditorResources t:MonoScript")[0];
            var relativePath = AssetDatabase.GUIDToAssetPath(scriptGuid).Replace("AmanitaEditorResources.cs", "AmanitaEditorResourcesGenerated.cs");
            var absolutePath = Application.dataPath + relativePath.Substring("Assets".Length);
            
            using (var writer = new StreamWriter(absolutePath))
            {			
                writer.WriteLine("#pragma warning disable 0649");
                writer.WriteLine("");
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("");
                writer.WriteLine("namespace Amanita.EditorUtils");
                writer.WriteLine("{");
                writer.WriteLine("    public partial class AmanitaEditorResources : ScriptableObject");
                writer.WriteLine("    {");
                
                foreach (var name in textureNames)
                {
                    writer.WriteLine("        [SerializeField] private EditorTexture " + name + ";");
                }

                writer.WriteLine("");

                foreach (var name in textureNames)
                {
                    var pascalCase = string.Join("", name.Split(new [] { '_' }, StringSplitOptions.RemoveEmptyEntries).Select(
                        s => s.Substring(0, 1).ToUpper() + s.Substring(1)).ToArray()
                    );
                    writer.WriteLine("        public static Texture2D " + pascalCase + " { get { return Instance." + name + ".Texture2D; } }");
                }

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            Instance.updateOnReloadScripts = true;
            AssetDatabase.ImportAsset(relativePath);
        }

        [DidReloadScripts]
        private static void OnDidReloadScripts()
        {
            if (Instance.updateOnReloadScripts)
            {                
                UpdateTextureReferences(Instance);
            }
        }

        public static void UpdateTextureReferences(AmanitaEditorResources instance)
        {
            // Iterate through all fields in instance and set texture references
            var serializedObject = new SerializedObject(instance);
            var prop = serializedObject.GetIterator();
            var rootFolder = new [] { GetRootFolder() };

            prop.NextVisible(true);
            while (prop.NextVisible(false))
            {
                if (prop.propertyType == SerializedPropertyType.Generic)
                {
                    var guids = AssetDatabase.FindAssets(prop.name + " t:Texture2D", rootFolder);
                    var paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Where(
                        path => path.Contains(prop.name + ".")
                    );

                    foreach (var path in paths)
                    {
                        var texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                        if (path.ToLower().Contains("/pro/"))
                        {
                            prop.FindPropertyRelative("pro").objectReferenceValue = texture;
                        }
                        else
                        {
                            prop.FindPropertyRelative("free").objectReferenceValue = texture;
                        }
                    }       
                }
            }

            serializedObject.FindProperty("updateOnReloadScripts").boolValue = false;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
