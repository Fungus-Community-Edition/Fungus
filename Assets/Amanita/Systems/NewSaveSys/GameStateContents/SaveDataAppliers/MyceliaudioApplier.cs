using Amanita.Myceliaudio;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Lorekeeper;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "NewMyceliaudioApplier", menuName = "Amanita/SaveSys/Appliers/MyceliaudioApplier")]
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
                    bool invalidIndex = assetIndex < 0 || assetIndex >= allAudioClips.Count;
                    bool weHaveANameToFallBackOn = !string.IsNullOrEmpty(saveData.PlayAudioArgs.MainClipName);
                    if (invalidIndex && weHaveANameToFallBackOn)
                    {
                        string mainClipName = saveData.PlayAudioArgs.MainClipName;
                        result = (from elem in allAudioClips
                                  where elem.name.Equals(mainClipName, System.StringComparison.OrdinalIgnoreCase)
                                  select elem).FirstOrDefault();
                        if (result == null)
                        {
                            Debug.LogWarning($"[MyceliaudioApplier]: Could not find audio clip with name: {mainClipName}. " +
                                $"Cannot play BGM upon application.");
                        }
                    }
                    else
                    {
                        result = allAudioClips[assetIndex];
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