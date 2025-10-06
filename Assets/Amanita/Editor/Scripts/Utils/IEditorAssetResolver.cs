using System.Collections.Generic;
using UnityObj = UnityEngine.Object;

namespace Amanita.EditorUtils
{
    public interface IEditorAssetResolver
    {
        IEnumerable<T> LoadAllFromResources<T>(string path) where T : UnityObj;
        IEnumerable<T> LoadAllAssetsAtPath<T>(string assetPath) where T : UnityObj;
        string GetAssetPath(UnityObj obj);
        void StartAssetEditing();
        void StopAssetEditing();
        void RefreshAssets();
        void AddObjectToAsset(UnityObj objToAdd, UnityObj asset);
    }
}