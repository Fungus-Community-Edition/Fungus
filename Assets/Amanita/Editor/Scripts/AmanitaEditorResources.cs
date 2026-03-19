using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AtMycelia.Amanita.EditorUtils
{
    [CustomEditor(typeof(AmanitaEditorResources))]
    public class AmanitaEditorResourcesInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
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
        private static readonly string subfolderLocation = "AtMycelia/Amanita/Editor"; // Relative to Resources folder

        public static AmanitaEditorResources Instance
        {
            get
            {
                if (instance == null)
                {
                    var guids = AssetDatabase.FindAssets(_searchFilter);

                    if (guids.Length == 0)
                    {
                        instance = SOUtils.EnsureSOExists<AmanitaEditorResources>(subfolderLocation, _assetName);
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

        private static readonly string _searchFilter = "t:AmanitaEditorResources";
        private static readonly string _assetName = "AmanitaEditorResources";

    }
}
