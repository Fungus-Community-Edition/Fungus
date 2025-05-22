using Amanita.Myceliaudio;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Amanita.SaveSys
{
    public class MyceliaudioApplier : SaveDataApplier<MyceliaudioSaveData>
    {
        public override void Apply(IList<MyceliaudioSaveData> saveData)
        {
            AudioSystem audioSys = AudioSystem.S;
            MyceliaudioSaveData firstSaveData = saveData[0];
            ApplyAudioSettings();
            void ApplyAudioSettings()
            {
                VolumeSettings volSettings = firstSaveData.VolumeSettings;
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
                // We are assuming that the clip name is unique and that it is
                // in a Resources/Audio/BGM folder. Of course, for flexibility's sake,
                // we will also search by the clip's last saved path.
                PlayAudioArgs audioArgs = firstSaveData.PlayAudioArgs;

                string mainClipName = firstSaveData.PlayAudioArgs.MainClipName;

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

        }
    }
}