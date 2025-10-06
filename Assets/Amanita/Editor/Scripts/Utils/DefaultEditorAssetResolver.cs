using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// IEditorAssetResolver isolates UnityEditor asset APIs (e.g. AssetDatabase, Resources) behind a small testable interface.
    /// DefaultEditorAssetResolver is the concrete editor implementation that delegates to Resources and AssetDatabase.
    /// Tests should inject small fakes that implement IEditorAssetResolver to avoid touching the real asset DB.
    /// </summary>
    public class DefaultEditorAssetResolver : IEditorAssetResolver
    {
        /// <summary>
        /// Use the resolver when your code needs to:
        /// - Enumerate assets in Resources folders,
        /// - Inspect or enumerate sub-assets of a ScriptableObject,
        /// - Add or mutate sub-assets (holders) under an asset,
        /// - Read an object's asset path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public IEnumerable<T> LoadAllFromResources<T>(string path) where T : UnityObj
        {
            return Resources.LoadAll<T>(path) ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Direct calls to AssetDatabase are hard to test and can make edit-mode unit tests brittle or slow.
        /// The resolver enables fast, deterministic unit tests and keeps editor-only code isolated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public IEnumerable<T> LoadAllAssetsAtPath<T>(string assetPath) where T : UnityObj
        {
            return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<T>();
        }

        /// <summary>
        /// GetAssetPath(obj) must return the same key used by LoadAllAssetsAtPath(...) in test fakes.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetAssetPath(UnityObj obj)
        {
            return AssetDatabase.GetAssetPath(obj);
        }

        /// <summary>
        /// StartAssetEditing() / StopAssetEditing() are used as a paired hint during bulk editing; fakes may be no-ops.
        /// </summary>
        public void StartAssetEditing() => AssetDatabase.StartAssetEditing();
        public void StopAssetEditing() => AssetDatabase.StopAssetEditing();

        /// <summary>
        /// RefreshAssets() will refresh the AssetDatabase.
        /// </summary>
        public void RefreshAssets() => AssetDatabase.Refresh();

        /// <summary>
        /// AddObjectToAsset(obj, asset) must ensure LoadAllAssetsAtPath(GetAssetPath(asset)) returns the added object afterward.
        /// </summary>
        /// <param name="objToAdd"></param>
        /// <param name="asset"></param>
        public void AddObjectToAsset(UnityObj objToAdd, UnityObj asset) => AssetDatabase.AddObjectToAsset(objToAdd, asset);
    }
}