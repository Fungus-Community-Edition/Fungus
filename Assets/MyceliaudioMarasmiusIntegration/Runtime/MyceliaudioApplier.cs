using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AtMycelia.SaveSys;
using Lorekeeper;
using AtMycelia.Myceliaudio;

namespace AtMycelia.Myceliasmius
{
    [SaveSysDisplayName("Myceliaudio Applier (Default)")]
    [SaveSysAssetName("DefMyceliaudioApplier")]
    public class MyceliaudioApplier : SaveDataApplier<MyceliaudioSaveData>
    {
        public override void Apply(MyceliaudioSaveData saveData)
        {
            AudioSystem audioSys = AudioSystem.S;
            ApplyAudioSettings();
            void ApplyAudioSettings()
            {
                VolumeSettings volSettings = saveData.VolumeSettings;
                if (volSettings == null)
                {
                    Debug.LogWarning("Volume settings are null.");
                    return;
                }

                audioSys.Apply(volSettings);
            }

            PlayTheCorrectClip();
            void PlayTheCorrectClip()
            {
                // We can't serialize the audio clips themselves (that'd make the
                // save data waaaay too big), and thus we need to fetch them based
                // on the clip name. 
                
                IList<AudioClip> allAudioClips = _shadowDb.GetAssetsOfType<AudioClip>(AssetType.AudioClip);
                PlayAudioArgs audioArgs = saveData.PlayAudioArgs;

                AudioClip toPlay = FindTheCorrectClip();
                AudioClip FindTheCorrectClip()
                {
                    AudioClip result = null;
                    const int theOneBgmTrackWeCareAbout = 0;
                    int assetIndex = saveData.GetBgmIndex(theOneBgmTrackWeCareAbout);
                    bool validIndex = assetIndex >= 0 && assetIndex < allAudioClips.Count;
                    string mainClipName = saveData.PlayAudioArgs.MainClip.name;
                    bool canUseNameAsFallback = mainClipName.Length > 0;
                    if (validIndex)
                    {
                        result = allAudioClips[assetIndex];
                    }
                    else if (canUseNameAsFallback)
                    {
                        result = (from elem in allAudioClips
                                  where elem.name.Equals(mainClipName, System.StringComparison.OrdinalIgnoreCase)
                                  select elem).FirstOrDefault();
                        if (result == null)
                        {
                            Debug.LogWarning($"[MyceliaudioApplier]: Could not find audio clip with name: {mainClipName}. " +
                                $"Cannot play BGM upon application.");
                        }
                    }

                    return result;
                }
                
                if (toPlay != null)
                {
                    audioArgs.MainClip = toPlay;
                    audioSys.Play(audioArgs);
                }
            }
        
        }

        protected ShadowDatabase ShadowDb
        {
            get
            {
                if (_shadowDb == null)
                {
                    _shadowDb = Resources.LoadAll<ShadowDatabase>("").FirstOrDefault();
                }
                return _shadowDb;
            }
        }
        protected ShadowDatabase _shadowDb;

        public override void Apply(SaveData saveData, System.Action onComplete)
        {
            Apply(saveData as MyceliaudioSaveData);
            onComplete?.Invoke();
        }

    }
}