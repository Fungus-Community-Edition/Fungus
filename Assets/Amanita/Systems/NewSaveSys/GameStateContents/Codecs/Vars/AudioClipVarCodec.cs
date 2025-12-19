using Amanita.VScripting;
using System.Linq;
using System;
using UnityEngine;
using FullSerializer;
using Amanita.FSExt;
using Lorekeeper;
using UnityObj = UnityEngine.Object;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding Vector2 and Vector3 data types.
    /// </summary>
    [Serializable]
    [VarCodec(true, typeof(AudioClipVariable), typeof(AudioClipMuscariable))]
    public class AudioClipVarCodec : IVarCodec, IVarStateApplier<VariableSaveData>, IVarStateApplier<string>
    {
        public virtual bool CanHandle(IVariable variable) => variable is IVariable<AudioClip>;

        public virtual bool CanHandle(string typeName)
        {
            bool result = TypeNameFitsWhatWeSupport(typeName);
            return result;
        }

        private static bool TypeNameFitsWhatWeSupport(string typeName)
        {
            bool result = supportedVarTypes.Any(supported => supported.Name.Equals(typeName,
                StringComparison.OrdinalIgnoreCase));
            return result;
        }

        protected static Type[] supportedVarTypes = new Type[]
        {
            typeof(AudioClipVariable),
            typeof(AudioClipMuscariable)
        };

        public virtual bool CanHandle(VariableSaveData saveData)
        {
            bool result = TypeNameFitsWhatWeSupport(saveData.VarTypeName);
            return result;
        }
        
        public virtual string EncodeToString(IVariable variable)
        {
            if (variable is not IVariable<AudioClip> audioClipVar)
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for " +
                    $"encoding in {this.GetType().Name}.");
                return string.Empty;
            }
            lock (Serializer)
            {
                AudioClipState audioClipState = From(audioClipVar.Value);
                return Serializer.ToJson(audioClipState);
            }
        }

        public virtual void ApplyState(IVariable toApplyTo, object data)
        {
            if (data is string strData)
            {
                ApplyState(toApplyTo, strData);
            }
            else if (data is VariableSaveData saveData)
            {
                ApplyState(toApplyTo, saveData);
            }
            else
            {
                Debug.LogError($"Data type {data.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }

        public virtual void ApplyState(IVariable variable, string stringData)
        {
            if (variable is not IVariable<AudioClip> audioClipVar)
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding " +
                    $"in {this.GetType().Name}.");
                return;
            }

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

        public virtual void ApplyState(IVariable variable, VariableSaveData saveData)
        {
            bool validVarType = variable is IVariable<AudioClip>;
            if (!validVarType)
            {
                Debug.LogError($"Variable type {saveData.VarTypeName} is not supported for " +
                    $"decoding in {this.GetType().Name}.");
                return;
            }
            ApplyState(variable, saveData.Value);
        }

        public virtual VariableSaveData EncodeToSave(IVariable variable)
        {
            string data = EncodeToString(variable);
            if (string.IsNullOrEmpty(data))
            {
                Debug.LogError($"Failed to encode variable {variable} in {this.GetType().Name}.");
                return VariableSaveData.Null;
            }

            VariableSaveData result = new()
            {
                VarTypeName = variable.GetType().Name,
                ItemId = variable.ItemId,
                Key = variable.Key,
                Value = data,
            };

            return result;
        }

        public virtual T DecodeTo<T>(string data)
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

        private fsSerializer Serializer => AmanitaManager.DefaultSerializer;
    }
}