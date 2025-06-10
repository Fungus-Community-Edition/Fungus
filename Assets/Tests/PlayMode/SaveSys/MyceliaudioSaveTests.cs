using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Amanita.SaveSys;
using UnityObject = UnityEngine.Object;
using UnityEngine.TestTools;
using Amanita.Myceliaudio;

namespace Amanita.SaveSystemTests
{
    public class MyceliaudioSaveTests : CommonTestFunctionality
    {

        [UnityTest]
        public virtual IEnumerator PlaysCorrectClip()
        {
            float quickWaitTime = 1f, waitTime = 3f;
            WaitForSeconds quickWait = new WaitForSeconds(quickWaitTime);
            WaitForSeconds wait = new WaitForSeconds(waitTime);
            AudioSys.Play(playAudioArgsSO);
            MyceliaudioSaveData saveData = new MyceliaudioSaveData();
            yield return wait;

            AudioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;
            audioApplier.ApplyMulti(new MyceliaudioSaveData[] { saveData });
            yield return wait; ;
            AudioClip clipPlaying = AudioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);
            bool playingCorrectClip = clipPlaying == playAudioArgsSO.MainClip;
            Assert.IsTrue(playingCorrectClip, "The clip playing is not the one we expected it to be.");

        }
    }
}