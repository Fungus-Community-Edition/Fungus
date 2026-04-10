using UnityEngine;
using UnityEditor;
using System;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomEditor(typeof(HyphlowEditorResources))]
    public class HyphlowEditorResourcesInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }

    public partial class HyphlowEditorResources : ScriptableObject
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

        private static HyphlowEditorResources instance;
        private static readonly string subfolderLocation = "AtMycelia/Amanita/Editor"; // Relative to Resources folder

        public static HyphlowEditorResources Instance
        {
            get
            {
                if (instance == null)
                {
                    var guids = AssetDatabase.FindAssets(_searchFilter);

                    if (guids.Length == 0)
                    {
                        instance = SOUtils.EnsureSOExists<HyphlowEditorResources>(subfolderLocation, _assetName);
                    }
                    else 
                    {
                        if (guids.Length > 1)
                        {
                            Debug.LogError("Multiple HyphlowEditorResources assets found!");
                        }

                        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        instance = AssetDatabase.LoadAssetAtPath(path, typeof(HyphlowEditorResources)) as HyphlowEditorResources;
                    }
                }

                return instance;
            }
        }

        private static readonly string _searchFilter = "t:HyphlowEditorResources";
        private static readonly string _assetName = "HyphlowEditorResources";

    }
}
