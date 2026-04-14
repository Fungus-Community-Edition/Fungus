using System;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public sealed class HyphlowEditorSysAssets : ScriptableObject
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

        [SerializeField] private EditorTexture add;
        [SerializeField] private EditorTexture add_small;
        [SerializeField] private EditorTexture delete;
        [SerializeField] private EditorTexture down;
        [SerializeField] private EditorTexture duplicate;
        [SerializeField] private EditorTexture fungus_mushroom;
        [SerializeField] private EditorTexture up;
        [SerializeField] private EditorTexture bullet_point;
        [SerializeField] private EditorTexture choice_node_off;
        [SerializeField] private EditorTexture choice_node_on;
        [SerializeField] private EditorTexture command_background;
        [SerializeField] private EditorTexture event_node_off;
        [SerializeField] private EditorTexture event_node_on;
        [SerializeField] private EditorTexture play_big;
        [SerializeField] private EditorTexture play_small;
        [SerializeField] private EditorTexture process_node_off;
        [SerializeField] private EditorTexture process_node_on;
        [SerializeField] private FlowchartWindowConfig _fcwConfig;

        private static HyphlowEditorSysAssets instance;
        private static readonly string subfolderLocation = "AtMycelia/Editor/Hyphlow"; // Relative to Resources folder
        private static readonly string _searchFilter = "t:HyphlowEditorSysAssets";
        private static readonly string _assetName = "HyphlowEditorSysAssets";

        public static HyphlowEditorSysAssets S
        {
            get
            {
                if (instance == null)
                {
                    string[] guids = AssetDatabase.FindAssets(_searchFilter);

                    if (guids.Length == 0)
                    {
                        instance = SOUtils.EnsureSOExists<HyphlowEditorSysAssets>(subfolderLocation, _assetName);
                    }
                    else
                    {
                        if (guids.Length > 1)
                        {
                            Debug.LogError("Multiple HyphlowEditorSysAssets assets found!");
                        }

                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        instance = AssetDatabase.LoadAssetAtPath(path, typeof(HyphlowEditorSysAssets)) as HyphlowEditorSysAssets;
                    }
                }

                return instance;
            }
        }

        public static Texture2D Add { get { return S.add.Texture2D; } }
        public static Texture2D AddSmall { get { return S.add_small.Texture2D; } }
        public static Texture2D Delete { get { return S.delete.Texture2D; } }
        public static Texture2D Down { get { return S.down.Texture2D; } }
        public static Texture2D Duplicate { get { return S.duplicate.Texture2D; } }
        public static Texture2D FungusMushroom { get { return S.fungus_mushroom.Texture2D; } }
        public static Texture2D Up { get { return S.up.Texture2D; } }
        public static Texture2D BulletPoint { get { return S.bullet_point.Texture2D; } }
        public static Texture2D ChoiceNodeOff { get { return S.choice_node_off.Texture2D; } }
        public static Texture2D ChoiceNodeOn { get { return S.choice_node_on.Texture2D; } }
        public static Texture2D CommandBackground { get { return S.command_background.Texture2D; } }
        public static Texture2D EventNodeOff { get { return S.event_node_off.Texture2D; } }
        public static Texture2D EventNodeOn { get { return S.event_node_on.Texture2D; } }
        public static Texture2D PlayBig { get { return S.play_big.Texture2D; } }
        public static Texture2D PlaySmall { get { return S.play_small.Texture2D; } }
        public static Texture2D ProcessNodeOff { get { return S.process_node_off.Texture2D; } }
        public static Texture2D ProcessNodeOn { get { return S.process_node_on.Texture2D; } }
        public static FlowchartWindowConfig FcwConfig { get { return S._fcwConfig; } }
    }
}
