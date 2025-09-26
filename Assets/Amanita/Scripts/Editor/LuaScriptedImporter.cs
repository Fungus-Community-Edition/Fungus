using UnityEngine;
using System.IO;

#if UNITY_2020_2_OR_NEWER
	using UnityEditor.AssetImporters;
#else
	using UnityEditor.Experimental.AssetImporters;
#endif

namespace Amanita.EditorUtils
{
#if UNITY_2018_4_OR_NEWER
    [ScriptedImporter(1, "lua")]
	public class LuaScriptedImporter : ScriptedImporter
	{
	    public override void OnImportAsset(AssetImportContext ctx)
	    {
	        TextAsset lua = new TextAsset(File.ReadAllText(ctx.assetPath));
	        ctx.AddObjectToAsset("main", lua);
	        ctx.SetMainObject(lua);
	    }
	}
#endif
}