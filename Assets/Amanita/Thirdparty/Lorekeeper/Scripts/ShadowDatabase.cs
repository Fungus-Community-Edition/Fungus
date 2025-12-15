#define LOREKEEPER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityObj = UnityEngine.Object;
#if UNITY_6000_0_OR_NEWER
using ThreeDPhysicsMaterial = UnityEngine.PhysicsMaterial;
#else
using ThreeDPhysicsMaterial = UnityEngine.PhysicMaterial;
#endif

namespace Lorekeeper
{
    public class ShadowDatabase : ScriptableObject
    {
        // Audio
        [SerializeField] protected List<AudioClip> _audioClips = new List<AudioClip>();
        [SerializeField] protected List<AudioMixer> _audioMixers = new List<AudioMixer>();

        // Graphics
        [SerializeField] protected List<Sprite> _sprites = new List<Sprite>();
        [SerializeField] protected List<Texture> _textures = new List<Texture>();
        [SerializeField] protected List<RenderTexture> _renderTextures = new List<RenderTexture>();
        [SerializeField] protected List<Cubemap> _cubemaps = new List<Cubemap>();
        [SerializeField] protected List<Material> _materials = new List<Material>();
        [SerializeField] protected List<Shader> _shaders = new List<Shader>();
        [SerializeField] protected List<ComputeShader> _computeShaders = new List<ComputeShader>();

        // Animation
        [SerializeField] protected List<AnimationClip> _animationClips = new List<AnimationClip>();
        [SerializeField] protected List<RuntimeAnimatorController> _animatorControllers = new List<RuntimeAnimatorController>();
        [SerializeField] protected List<Avatar> _avatars = new List<Avatar>();

        // Models & Prefabs
        [SerializeField] protected List<UnityObj> _models = new List<UnityObj>();
        [SerializeField] protected List<Mesh> _meshes = new List<Mesh>();
        [SerializeField] protected List<GameObject> _prefabs = new List<GameObject>();

        // UI & Fonts
        [SerializeField] protected List<Font> _fonts = new List<Font>();
        [SerializeField] protected List<TMPro.TMP_FontAsset> _tmpFontAssets = new List<TMPro.TMP_FontAsset>();

        // Data
        [SerializeField] protected List<ScriptableObject> _scriptableObjects = new List<ScriptableObject>();
        [SerializeField] protected List<TextAsset> _textAssets = new List<TextAsset>();

        // Physics

        [SerializeField] protected List<ThreeDPhysicsMaterial> _physicsMaterials = new List<ThreeDPhysicsMaterial>();
        [SerializeField] protected List<PhysicsMaterial2D> _physicsMaterials2D = new List<PhysicsMaterial2D>();

        [SerializeField] protected List<UnityObj> _other = new List<UnityObj>();

        public virtual void Refresh()
        {
            // We need this since Unity doesn't persist dictionaries in ScriptableObjects
            _assetDictionary[AssetType.AudioClip] = _audioClips;
            _assetDictionary[AssetType.AudioMixer] = _audioMixers;

            _assetDictionary[AssetType.Sprite] = _sprites;
            _assetDictionary[AssetType.Texture] = _textures;
            _assetDictionary[AssetType.RenderTexture] = _renderTextures;
            _assetDictionary[AssetType.Cubemap] = _cubemaps;
            _assetDictionary[AssetType.Material] = _materials;
            _assetDictionary[AssetType.Shader] = _shaders;
            _assetDictionary[AssetType.ComputeShader] = _computeShaders;

            _assetDictionary[AssetType.AnimationClip] = _animationClips;
            _assetDictionary[AssetType.AnimatorController] = _animatorControllers;
            _assetDictionary[AssetType.Avatar] = _avatars;

            _assetDictionary[AssetType.Model] = _models;
            _assetDictionary[AssetType.Mesh] = _meshes;
            _assetDictionary[AssetType.Prefab] = _prefabs;

            _assetDictionary[AssetType.Font] = _fonts;
            _assetDictionary[AssetType.TMPFontAsset] = _tmpFontAssets;

            _assetDictionary[AssetType.ScriptableObject] = _scriptableObjects;
            _assetDictionary[AssetType.TextAsset] = _textAssets;
            _assetDictionary[AssetType.PhysicsMaterial] = _physicsMaterials;
            _assetDictionary[AssetType.PhysicsMaterial2D] = _physicsMaterials2D;

            _assetDictionary[AssetType.Other] = _other;
        }

        protected IDictionary<AssetType, IList> _assetDictionary = new Dictionary<AssetType, IList>();
        // ^We go with object here so that we can properly tie this dictionary to the serialized lists above,
        // as opposed to copies of those lists.

        public virtual T GetAssetAt<T>(int index, AssetType assetType) where T : UnityObj
        {
            return GetAssetAt(index, assetType) as T;
        }

        public virtual UnityObj GetAssetAt(int index, AssetType assetType)
        {
            RefreshAsNeeded();
            IList listToGetFrom = _assetDictionary[assetType];
            if (index >= 0 && index < listToGetFrom.Count)
            {
                return listToGetFrom[index] as UnityObj;
            }
            return null;
        }


        protected virtual void RefreshAsNeeded()
        {
            bool needsRefresh = _assetDictionary.Count == 0;
            if (needsRefresh)
            {
                Refresh();
            }
        }

        public virtual int GetAssetCount(AssetType assetType)
        {
            RefreshAsNeeded();
            var dictToGetFrom = _assetDictionary[assetType] as IList<UnityObj>;
            return dictToGetFrom.Count;
        }

        public virtual void TryAdd(UnityObj toAdd, AssetType assetType, out bool wasAdded)
        {
            RefreshAsNeeded();
            wasAdded = false;
            var listToAddTo = _assetDictionary[assetType];
            // Let's check by reference, since two assets with the same name can exist.
            for (int i = 0; i < listToAddTo.Count; i++)
            {
                if (ReferenceEquals(listToAddTo[i], toAdd))
                {
                    return;
                }
            }
            listToAddTo.Add(toAdd);
            wasAdded = true;
        }

        public virtual void ClearAllAssets()
        {
            Debug.Log("[ShadowDatabase]: Clearing all assets from Shadow Database.");
            RefreshAsNeeded();

            foreach (var list in _assetDictionary.Values)
            {
                list.Clear();
            }
        }

        public static AssetType GetAssetTypeFor(UnityObj obj)
        {
            if (obj == null)
            {
                return AssetType.Null;
            }
            else if (obj is AudioClip)
            {
                return AssetType.AudioClip;
            }
            else if (obj is AudioMixer)
            {
                return AssetType.AudioMixer;
            }
            else if (obj is Sprite)
            {
                return AssetType.Sprite;
            }
            else if (obj is Texture && !(obj is RenderTexture) && !(obj is Cubemap))
            {
                return AssetType.Texture;
            }
            else if (obj is RenderTexture)
            {
                return AssetType.RenderTexture;
            }
            else if (obj is Cubemap)
            {
                return AssetType.Cubemap;
            }
            else if (obj is Material)
            {
                return AssetType.Material;
            }
            else if (obj is Shader)
            {
                return AssetType.Shader;
            }
            else if (obj is ComputeShader)
            {
                return AssetType.ComputeShader;
            }
            else if (obj is AnimationClip)
            {
                return AssetType.AnimationClip;
            }
            else if (obj is RuntimeAnimatorController)
            {
                return AssetType.AnimatorController;
            }
            else if (obj is Avatar)
            {
                return AssetType.Avatar;
            }
            else if (obj is Mesh)
            {
                return AssetType.Mesh;
            }
            else if (obj is GameObject go && go.scene.rootCount == 0)
            {
                return AssetType.Prefab;
            }
            else if (obj is Font)
            {
                return AssetType.Font;
            }
            else if (obj is TMPro.TMP_FontAsset)
            {
                return AssetType.TMPFontAsset;
            }
            else if (obj is ScriptableObject)
            {
                return AssetType.ScriptableObject;
            }
            else if (obj is TextAsset)
            {
                return AssetType.TextAsset;
            }
            else if (obj is ThreeDPhysicsMaterial)
            {
                return AssetType.PhysicsMaterial;
            }
            else if (obj is PhysicsMaterial2D)
            {
                return AssetType.PhysicsMaterial2D;
            }
            else
            {
                return AssetType.Other;
            }
        }

        public virtual IList<T> GetAssetsOfType<T>(AssetType assetType) where T : UnityObj
        {
            RefreshAsNeeded();
            IList<T> assets = new List<T>();
            IList listToGetFrom = _assetDictionary[assetType];
            foreach (var obj in listToGetFrom)
            {
                if (obj is T tObj)
                    assets.Add(tObj);
            }
            return assets;
        }


        public virtual T GetAssetWithName<T>(string name, AssetType assetType,
            StringComparison stringComp = StringComparison.OrdinalIgnoreCase) where T : UnityObj
        {
            RefreshAsNeeded();
            IList listToGetFrom = _assetDictionary[assetType];
            foreach (var obj in listToGetFrom)
            {
                T tObj = obj as T;
                bool rightType = tObj != null;
                if (!rightType)
                {
                    continue;
                }

                bool nameMatches = tObj.name.Equals(name, stringComp);
                if (nameMatches)
                {
                    return tObj;
                }
            }
            return null;
        }

        public virtual int TotalAssetCount
        {
            get
            {
                RefreshAsNeeded();
                int totalCount = 0;
                foreach (var list in _assetDictionary.Values)
                {
                    totalCount += list.Count;
                }
                return totalCount;
            }
        }

        public virtual int GetIndexFor(UnityObj obj, AssetType assetType)
        {
            if (obj == null)
            {
                Debug.LogWarning("[ShadowDatabase]: Attempted to get index for null object.");
                return -1;
            }

            RefreshAsNeeded();
            IList listToGetFrom = _assetDictionary[assetType];
            for (int i = 0; i < listToGetFrom.Count; i++)
            {
                if (ReferenceEquals(listToGetFrom[i], obj))
                    return i;
            }
            return -1;
        }
    }

}