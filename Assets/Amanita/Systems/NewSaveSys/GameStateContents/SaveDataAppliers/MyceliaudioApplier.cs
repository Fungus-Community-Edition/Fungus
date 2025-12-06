using Amanita.Myceliaudio;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Lorekeeper;

namespace Amanita.SaveSys
{
    [SaveSysDisplayName("Myceliaudio Applier (Amanita Default)")]
    public class MyceliaudioApplier : SaveDataApplier<MyceliaudioSaveData>
    {
        public override Task Apply(MyceliaudioSaveData saveData)
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
                
                var shadowDb = AmanitaManager.ShadowDB;
                IList<AudioClip> allAudioClips = shadowDb.GetAssetsOfType<AudioClip>(AssetType.AudioClip);
                PlayAudioArgs audioArgs = saveData.PlayAudioArgs;

                AudioClip toPlay = FindTheCorrectClip();
                AudioClip FindTheCorrectClip()
                {
                    AudioClip result = null;
                    const int theOneBgmTrackWeCareAbout = 0;
                    int assetIndex = saveData.GetBgmIndex(theOneBgmTrackWeCareAbout);
                    bool validIndex = assetIndex >= 0 && assetIndex < allAudioClips.Count;
                    string mainClipName = saveData.PlayAudioArgs.MainClipName;
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
        
            return Task.CompletedTask;
        }


        public override Task Apply(SaveData saveData)
        {
            return Apply(saveData as MyceliaudioSaveData);
        }

    }
}