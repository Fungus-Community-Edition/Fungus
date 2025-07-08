using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Amanita.SaveSys;
using UnityEngine.TestTools;
using Amanita.Myceliaudio;
using System.Threading.Tasks;

namespace Amanita.SaveSystemTests
{
    public class MyceliaudioSaveTests : CommonTestFunctionality
    {

        public override void DoOneTimeSetUp()
        {
            base.DoOneTimeSetUp();
            quickWait = new WaitForSeconds(quickWaitTime);
            wait = new WaitForSeconds(waitTime);
        }

        protected float quickWaitTime = 0.5f, waitTime = 1.5f;
        protected WaitForSeconds quickWait, wait;

        [UnityTest]
        public virtual IEnumerator PlaysCorrectClip()
        {
            yield return CommonSetup();
            
            AudioSys.Play(playAudioArgsSO);
            MyceliaudioSaveData saveData = new MyceliaudioSaveData();
            yield return wait;

            AudioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;
            Task applyTask = audioApplier.ApplyMulti(new MyceliaudioSaveData[] { saveData });
            yield return WaitFor(applyTask);
            AudioClip clipPlaying = AudioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);
            bool playingCorrectClip = clipPlaying == playAudioArgsSO.MainClip;
            Assert.IsTrue(playingCorrectClip, "The clip playing is not the one we expected it to be.");

        }
    }
}