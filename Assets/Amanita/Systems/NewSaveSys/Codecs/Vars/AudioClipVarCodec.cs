using AtMycelia.Amanita.VScripting;
using System;
using UnityEngine;
using FullSerializer;
using AtMycelia.FSExt;
using Lorekeeper;
using AtMycelia.SaveSys;
using System.Collections.Generic;

namespace AtMycelia.Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding Vector2 and Vector3 data types.
    /// </summary>
    [Serializable]
    [VarCodec(true, typeof(AudioClipVariable), typeof(AudioClipMuscariable))]
    public class AudioClipVarCodec : VarCodec, IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        protected override IReadOnlyList<Type> SupportedContentTypes => (IReadOnlyList<Type>)_supportedContentTypes;
        private static readonly IList<Type> _supportedContentTypes = new Type[]
        {
            typeof(AudioClip)
        };

        public override string EncodeToString(IVariable variable)
        {
            if (!CanHandle(variable))
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for " +
                    $"encoding in {this.GetType().Name}.");
                return string.Empty;
            }
            IVariable<AudioClip> audioClipVar = variable as IVariable<AudioClip>;
            lock (Serializer)
            {
                AudioClipState audioClipState = From(audioClipVar.Value);
                return Serializer.ToJson(audioClipState);
            }
        }

        public override void ApplyState(IVariable variable, string stringData)
        {
            if (!CanHandle(variable))
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding " +
                    $"in {this.GetType().Name}.");
                return;
            }

            IVariable<AudioClip> audioClipVar = variable as IVariable<AudioClip>;
            lock (Serializer)
            {
                // We assume that the string is a AudioClipState serialized as JSON.
                AudioClipState audioClipState = Serializer.FromJson<AudioClipState>(stringData);
                audioClipVar.Value = ToAudioClip(audioClipState);
            }
        }

        private static AudioClip ToAudioClip(AudioClipState state)
        {
            AudioClip clip = null;
            if (state.IsValid) 
            {
                // Note that it being valid won't guarantee we'll find anything. For all we know, the
                // asset could have been removed from the database. Or perhaps the DB was mishandled.
                clip = ShadowDb.GetAssetAt<AudioClip>(state.lorekeeperIndex, AssetType.AudioClip);
                if (clip == null)
                {
                    clip = ShadowDb.GetAssetWithName<AudioClip>(state.clipName, AssetType.AudioClip);
                }
            }
            
            return clip;
        }

        private static ShadowDatabase ShadowDb => AmanitaManager.ShadowDB;

        private static AudioClipState From(AudioClip clip)
        {
            AudioClipState result = new AudioClipState();
            if (clip == null)
            {
                result.lorekeeperIndex = -1;
                result.clipName = string.Empty;
            }
            else
            {
                int index = ShadowDb.GetIndexFor(clip, AssetType.AudioClip);
                string name = string.Empty;
                if (index >= 0)
                {
                    name = clip.name;
                }
                result.lorekeeperIndex = index;
                result.clipName = name;
            }
            return result;
        }

        public override void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            bool validVarType = CanHandle(variable);
            if (!validVarType)
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for " +
                    $"decoding in {this.GetType().Name}.");
                return;
            }
            ApplyState(variable, saveData.Value);
        }

        public override T DecodeTo<T>(string data)
        {
            // Again, we assume that the data is a AudioClipState serialized as JSON.
            T result = default;
            lock (Serializer)
            {
                if (typeof(T) == typeof(AudioClip))
                {
                    AudioClipState audioClipState = Serializer.FromJson<AudioClipState>(data);
                    AudioClip clip = ShadowDb.GetAssetAt<AudioClip>(audioClipState.lorekeeperIndex, AssetType.AudioClip);
                    if (clip == null)
                    {
                        clip = ShadowDb.GetAssetWithName<AudioClip>(audioClipState.clipName, AssetType.AudioClip);
                    }
                    result = (T)(object)clip;
                }
            }

            return result;
        }

        private fsSerializer Serializer => SaveSystem.DefaultSerializer;
    }
}