using Amanita.Myceliaudio;
using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystemTests
{
    public class MyceliaudioSaveTests : CommonTestFunctionality
    {
        // Only need SaveSystem (for AudioSystem singleton); no scene or flowchart.
        protected override bool ReqSaveSystem => true;
        protected override bool ReqSceneLoad => false;
        protected override bool ReqFlowchart => false;

        protected MyceliaudioSaveCodec mycelSaveCodec;
        protected WaitForSeconds quickWait;
        protected WaitForSeconds wait;
        protected readonly float quickWaitTime = 0.5f;
        protected readonly float waitTime = 1.5f;

        [OneTimeSetUp]
        public override void DoOneTimeSetUp()
        {
            base.DoOneTimeSetUp();
            quickWait = new WaitForSeconds(quickWaitTime);
            wait = new WaitForSeconds(waitTime);
        }

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            // Base already creates audioApplier. Only create codec here.
            mycelSaveCodec = ScriptableObject.CreateInstance<MyceliaudioSaveCodec>();
            mycelSaveCodec.PreInstallInit();
            toDestroyInTearDown.Add(mycelSaveCodec);
        }

        [UnityTest]
        public IEnumerator PlaysCorrectClip()
        {
            yield return CommonSetup();

            AudioSys.Play(playAudioArgsSO);
            MyceliaudioSaveData saveData = mycelSaveCodec.EncodeToSave(AudioSystem.S);
            yield return wait;

            AudioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;

            Task applyTask = audioApplier.ApplyRange(new[] { saveData });
            yield return WaitFor(applyTask);

            AudioClip clipPlaying = AudioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);
            Assert.IsTrue(clipPlaying == playAudioArgsSO.MainClip,
                $"Expected {playAudioArgsSO.MainClip.name} but got {clipPlaying?.name}");
        }

        [UnityTest]
        public IEnumerator DoesNotPlayWhenNothingWasPlaying()
        {
            yield return CommonSetup();

            AudioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;

            var save = mycelSaveCodec.EncodeToSave(AudioSystem.S);
            var apply = audioApplier.ApplyRange(new[] { save });
            yield return WaitFor(apply);

            Assert.IsFalse(AudioSys.GetIsPlaying(TrackGroup.BGMusic, 0),
                "No BGM should be playing after applying a save captured with no playback.");
            Assert.AreEqual(-1, save.GetBgmIndex(0), "Expected invalid BGM index when nothing was playing.");
        }

        [UnityTest]
        public IEnumerator FallsBackToClipNameWhenIndexInvalid()
        {
            yield return CommonSetup();

            AudioSys.Play(playAudioArgsSO);
            var save = mycelSaveCodec.EncodeToSave(AudioSystem.S);
            yield return wait;

            AudioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;

            save.AddBgmIndex(0, -1); // Corrupt index; name should still resolve.

            var apply = audioApplier.ApplyRange(new[] { save });
            yield return WaitFor(apply);

            var clipPlaying = AudioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);
            Assert.IsNotNull(clipPlaying, "Clip should be playing via name fallback.");
            Assert.AreEqual(playAudioArgsSO.MainClip.name, clipPlaying.name,
                "Fallback by name did not play the expected clip.");
        }

        [UnityTest]
        public IEnumerator LogsWarningAndDoesNotPlayWhenClipNameMissing()
        {
            yield return CommonSetup();

            AudioClip fakeClip = AudioClip.Create("NonExistentClipName_X", 44100 * 2, 1, 44100, false);

            var save = new MyceliaudioSaveData
            {
                PlayAudioArgs = new PlayAudioArgs
                {
                    TrackGroup = TrackGroup.BGMusic,
                    Track = 0,
                    MainClip = fakeClip,
                    Loop = false,
                    OneShot = false
                }
            };
            save.AddBgmIndex(0, -1);

            LogAssert.Expect(LogType.Warning,
                $"[MyceliaudioApplier]: Could not find audio clip with name: {fakeClip.name}. Cannot play BGM upon application.");

            var apply = audioApplier.ApplyRange(new[] { save });
            yield return WaitFor(apply);

            Assert.IsFalse(AudioSys.GetIsPlaying(TrackGroup.BGMusic, 0),
                "Playback should not start when clip cannot be resolved.");
        }

        [Test]
        public void CodecCanHandleKnownTypes()
        {
            Assert.IsTrue(mycelSaveCodec.CanHandle(typeof(AudioSystem).FullName),
                "Codec should handle AudioSystem full type name.");
            Assert.IsTrue(mycelSaveCodec.CanHandle(typeof(MyceliaudioSaveData).Name),
                "Codec should handle MyceliaudioSaveData type name.");
            Assert.IsFalse(mycelSaveCodec.CanHandle("CompletelyRandomTypeName"),
                "Codec should reject unknown type names.");
        }

        [Test]
        public void BgmIndexesAreDefensivelyCopied()
        {
            var save = new MyceliaudioSaveData();
            save.AddBgmIndex(0, 7);

            var ext = save.BgmIndexes; // copy
            ext[0] = 99;

            Assert.AreEqual(7, save.GetBgmIndex(0),
                "External modifications to copy should not affect internal state.");
        }

        [UnityTest]
        public IEnumerator ApplyingSameSaveTwiceIsIdempotent()
        {
            yield return CommonSetup();
            
            AudioSys.Play(playAudioArgsSO);
            var save = mycelSaveCodec.EncodeToSave(AudioSystem.S);
            yield return wait;

            AudioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;

            var apply1 = audioApplier.ApplyRange(new[] { save });
            yield return WaitFor(apply1);
            var firstClip = AudioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);

            var apply2 = audioApplier.ApplyRange(new[] { save });
            yield return WaitFor(apply2);
            var secondClip = AudioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);

            Assert.IsNotNull(firstClip);
            Assert.IsNotNull(secondClip);
            Assert.AreSame(firstClip, secondClip,
                "Applying same save twice should result in same clip continuing to play.");
        }
    }
}