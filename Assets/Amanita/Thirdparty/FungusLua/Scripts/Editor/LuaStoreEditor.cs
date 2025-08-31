using UnityEngine;
using UnityEditor;
using MoonSharp.Interpreter.Serialization;

namespace Amanita.Lua
{
    [CustomEditor(typeof(LuaStore))]
    public class LuaStoreEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Use the Serialization extension to display the contents of the prime table.
            LuaStore luaStore = target as LuaStore;
            if (luaStore.PrimeTable != null)
            {
                EditorGUILayout.PrefixLabel(new GUIContent("Inspect Table", "Displays the contents of the fungus.store prime table."));
                string serialized = luaStore.PrimeTable.Serialize();
                EditorGUILayout.SelectableLabel(serialized, GUILayout.ExpandHeight(true));
            }
        }
    }
}
