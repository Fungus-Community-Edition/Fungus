using System;
using System.Collections.Generic;
using UnityEngine;
using Lorekeeper;

namespace Lorekeeper.EditorCode
{
    [Serializable]
    public class LorekeeperSettings
    {
        #region Serializable Fields and their Properties
        [SerializeField] protected List<string> blacklist = new List<string>() { "/Lorekeeper" };

        [SerializeField] protected bool hasAssetTypeSettings = true;

        [SerializeField] protected bool trackAudioClips = true;
        [SerializeField] protected bool trackAudioMixers = true;

        [SerializeField] protected bool trackSprites = true;
        [SerializeField] protected bool trackTextures = true;
        [SerializeField] protected bool trackRenderTextures = false;
        [SerializeField] protected bool trackCubemaps = false;
        [SerializeField] protected bool trackMaterials = false;
        [SerializeField] protected bool trackShaders = false;
        [SerializeField] protected bool trackComputeShaders = false;

        [SerializeField] protected bool trackAnimationClips = true;
        [SerializeField] protected bool trackAnimatorControllers = false;
        [SerializeField] protected bool trackAvatars = false;

        [SerializeField] protected bool trackModels = false;
        [SerializeField] protected bool trackMeshes = false;
        [SerializeField] protected bool trackPrefabs = false;

        [SerializeField] protected bool trackFonts = false;
        [SerializeField] protected bool trackTmpFontAssets = false;

        [SerializeField] protected bool trackScriptableObjects = false;
        [SerializeField] protected bool trackTextAssets = false;

        [SerializeField] protected bool trackPhysicsMaterials = false;
        [SerializeField] protected bool trackPhysicsMaterials2D = false;

        [SerializeField] protected bool trackOther = false;

        /// <summary>
        /// For paths relative to the Assets folder. The setter replaces the list's contents 
        /// but keeps the list reference.
        /// </summary>
        public virtual IList<string> Blacklist
        {
            get { return blacklist.AsReadOnly(); }
            set
            {
                blacklist.Clear();
                if (value == null)
                {
                    return;
                }

                for (int i = 0; i < value.Count; i++)
                {
                    AddExclusion(value[i]);
                }
            }
        }

        public virtual bool TrackAudioClips { get { return trackAudioClips; } set { trackAudioClips = value; } }
        public virtual bool TrackAudioMixers { get { return trackAudioMixers; } set { trackAudioMixers = value; } }

        public virtual bool TrackSprites { get { return trackSprites; } set { trackSprites = value; } }
        public virtual bool TrackTextures { get { return trackTextures; } set { trackTextures = value; } }
        public virtual bool TrackRenderTextures { get { return trackRenderTextures; } set { trackRenderTextures = value; } }
        public virtual bool TrackCubemaps { get { return trackCubemaps; } set { trackCubemaps = value; } }
        public virtual bool TrackMaterials { get { return trackMaterials; } set { trackMaterials = value; } }
        public virtual bool TrackShaders { get { return trackShaders; } set { trackShaders = value; } }
        public virtual bool TrackComputeShaders { get { return trackComputeShaders; } set { trackComputeShaders = value; } }

        public virtual bool TrackAnimationClips { get { return trackAnimationClips; } set { trackAnimationClips = value; } }
        public virtual bool TrackAnimatorControllers { get { return trackAnimatorControllers; } set { trackAnimatorControllers = value; } }
        public virtual bool TrackAvatars { get { return trackAvatars; } set { trackAvatars = value; } }

        public virtual bool TrackModels { get { return trackModels; } set { trackModels = value; } }
        public virtual bool TrackMeshes { get { return trackMeshes; } set { trackMeshes = value; } }
        public virtual bool TrackPrefabs { get { return trackPrefabs; } set { trackPrefabs = value; } }

        public virtual bool TrackFonts { get { return trackFonts; } set { trackFonts = value; } }
        public virtual bool TrackTmpFontAssets { get { return trackTmpFontAssets; } set { trackTmpFontAssets = value; } }

        public virtual bool TrackScriptableObjects { get { return trackScriptableObjects; } set { trackScriptableObjects = value; } }
        public virtual bool TrackTextAssets { get { return trackTextAssets; } set { trackTextAssets = value; } }

        public virtual bool TrackPhysicsMaterials { get { return trackPhysicsMaterials; } set { trackPhysicsMaterials = value; } }
        public virtual bool TrackPhysicsMaterials2D { get { return trackPhysicsMaterials2D; } set { trackPhysicsMaterials2D = value; } }

        public virtual bool TrackOther { get { return trackOther; } set { trackOther = value; } }
        #endregion

        #region Asset Folder Exclusion Methods
        public virtual void AddExclusion(string path)
        {
            path = LKUtils.EnsureForwardSlashAtStart(path);
            blacklist.Add(path);
        }

        public virtual void ChangeExclusion(int index, string newPath)
        {
            if (index < 0 || index >= blacklist.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range.");
            }
            newPath = LKUtils.EnsureForwardSlashAtStart(newPath);
            blacklist[index] = newPath;
        }

        public virtual void RemoveExclusion(string path)
        {
            path = LKUtils.EnsureForwardSlashAtStart(path);
            blacklist.Remove(path);
        }

        public virtual void RemoveExclusionByIndex(int index)
        {
            if (index < 0 || index >= blacklist.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            blacklist.RemoveAt(index);
        }
        #endregion

        public virtual void Clear()
        {
            blacklist.Clear();
        }

        public virtual void OnDeserialize()
        {
            for (int i = 0; i < blacklist.Count; i++)
            {
                blacklist[i] = LKUtils.EnsureForwardSlashAtStart(blacklist[i]);
            }

            EnsureAssetTypeDefaults();
        }

        public virtual bool IsAssetTypeEnabled(AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.AudioClip:
                    return TrackAudioClips;
                case AssetType.AudioMixer:
                    return TrackAudioMixers;
                case AssetType.Sprite:
                    return TrackSprites;
                case AssetType.Texture:
                    return TrackTextures;
                case AssetType.RenderTexture:
                    return TrackRenderTextures;
                case AssetType.Cubemap:
                    return TrackCubemaps;
                case AssetType.Material:
                    return TrackMaterials;
                case AssetType.Shader:
                    return TrackShaders;
                case AssetType.ComputeShader:
                    return TrackComputeShaders;
                case AssetType.AnimationClip:
                    return TrackAnimationClips;
                case AssetType.AnimatorController:
                    return TrackAnimatorControllers;
                case AssetType.Avatar:
                    return TrackAvatars;
                case AssetType.Model:
                    return TrackModels;
                case AssetType.Mesh:
                    return TrackMeshes;
                case AssetType.Prefab:
                    return TrackPrefabs;
                case AssetType.Font:
                    return TrackFonts;
                case AssetType.TMPFontAsset:
                    return TrackTmpFontAssets;
                case AssetType.ScriptableObject:
                    return TrackScriptableObjects;
                case AssetType.TextAsset:
                    return TrackTextAssets;
                case AssetType.PhysicsMaterial:
                    return TrackPhysicsMaterials;
                case AssetType.PhysicsMaterial2D:
                    return TrackPhysicsMaterials2D;
                case AssetType.Other:
                    return TrackOther;
                default:
                    return false;
            }
        }

        protected virtual void EnsureAssetTypeDefaults()
        {
            if (hasAssetTypeSettings)
            {
                return;
            }

            trackAudioClips = true;
            trackAudioMixers = true;

            trackSprites = true;
            trackTextures = true;
            trackRenderTextures = true;
            trackCubemaps = true;
            trackMaterials = true;
            trackShaders = true;
            trackComputeShaders = true;

            trackAnimationClips = true;
            trackAnimatorControllers = true;
            trackAvatars = true;

            trackModels = true;
            trackMeshes = true;
            trackPrefabs = true;

            trackFonts = true;
            trackTmpFontAssets = true;

            trackScriptableObjects = true;
            trackTextAssets = true;

            trackPhysicsMaterials = true;
            trackPhysicsMaterials2D = true;

            trackOther = true;

            hasAssetTypeSettings = true;
        }
    }
}