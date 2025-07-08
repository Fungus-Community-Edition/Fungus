using Amanita.Myceliaudio;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "NewMyceliaudioApplier", menuName = "Amanita/SaveSys/MyceliaudioApplier")]
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
                PlayAudioArgs audioArgs = saveData.PlayAudioArgs;

                string mainClipName = saveData.PlayAudioArgs.MainClipName;

                IList<AudioClip> allAudioClips = Resources.LoadAll<AudioClip>("Audio/BGM");
                AudioClip toPlay = (from elem in allAudioClips
                                    where elem.name == mainClipName
                                    select elem).FirstOrDefault();

                audioArgs.MainClip = toPlay;

                if (toPlay != null)
                {
                    audioSys.Play(audioArgs);
                }
            }
        
            return Task.CompletedTask;
        }

        public override Task Apply(SaveData saveData)
        {
            return Apply(saveData as MyceliaudioSaveData);
        }

        public override async Task ApplyMulti(IList<MyceliaudioSaveData> saveData)
        {
            foreach (var elem in saveData)
            {
                await Apply(elem);
            }
        }
    }
}