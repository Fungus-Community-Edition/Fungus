using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.EditorUtils
{
    public class DefaultEditorAssetResolver : IEditorAssetResolver
    {
        public IEnumerable<T> LoadAllFromResources<T>(string path) where T : UnityObj
        {
            return Resources.LoadAll<T>(path) ?? Enumerable.Empty<T>();
        }

        public IEnumerable<T> LoadAllAssetsAtPath<T>(string assetPath) where T : UnityObj
        {
            return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<T>();
        }

        public string GetAssetPath(UnityObj obj)
        {
            return AssetDatabase.GetAssetPath(obj);
        }

        public void StartAssetEditing() => AssetDatabase.StartAssetEditing();
        public void StopAssetEditing() => AssetDatabase.StopAssetEditing();
        public void RefreshAssets() => AssetDatabase.Refresh();

        public void AddObjectToAsset(UnityObj objToAdd, UnityObj asset) => AssetDatabase.AddObjectToAsset(objToAdd, asset);
    }
}