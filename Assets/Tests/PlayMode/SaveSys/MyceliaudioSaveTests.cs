using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Amanita.SaveSys;
using UnityEngine.TestTools;
using Amanita.Myceliaudio;
using System.Threading.Tasks;

namespace SaveSystemTests
{
    public class MyceliaudioSaveTests : CommonTestFunctionality
    {
        [OneTimeSetUp]
        public override void DoOneTimeSetUp()
        {
            base.DoOneTimeSetUp();
            quickWait = new WaitForSeconds(quickWaitTime);
            wait = new WaitForSeconds(waitTime);
        }

        protected float quickWaitTime = 0.5f, waitTime = 1.5f;
        protected WaitForSeconds quickWait, wait;

        [SetUp]
        public override void DoSetUp()
        {
            base.DoSetUp();
            audioApplier = ScriptableObject.CreateInstance<MyceliaudioApplier>();
            mycelSaveCodec = ScriptableObject.CreateInstance<MyceliaudioSaveCodec>();
            toDestroyInTearDown.Add(audioApplier);
            toDestroyInTearDown.Add(mycelSaveCodec);
        }

        protected MyceliaudioSaveCodec mycelSaveCodec;

        [UnityTest]
        public virtual IEnumerator PlaysCorrectClip()
        {
            yield return CommonSetup();
            
            AudioSys.Play(playAudioArgsSO);
            MyceliaudioSaveData saveData = mycelSaveCodec.EncodeToSave(AudioSystem.S);
            yield return wait;

            AudioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;
            Task applyTask = audioApplier.ApplyRange(new MyceliaudioSaveData[] { saveData });
            yield return WaitFor(applyTask);
            AudioClip clipPlaying = AudioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);
            bool playingCorrectClip = clipPlaying == playAudioArgsSO.MainClip;
            Assert.IsTrue(playingCorrectClip, $"The clip playing is not the one we expected it to be. We expected " +
                $"{playAudioArgsSO.MainClip.name} but instead got {clipPlaying?.name}");

        }

        [UnityTest]
        public IEnumerator DoesNotPlayWhenNothingWasPlaying()
        {
            yield return CommonSetup();

            // Ensure nothing is playing, then encode.
            AudioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;

            var save = mycelSaveCodec.EncodeToSave(AudioSystem.S);

            // Apply and ensure nothing starts playing.
            var apply = audioApplier.ApplyRange(new[] { save });
            yield return WaitFor(apply);

            Assert.IsFalse(AudioSys.GetIsPlaying(TrackGroup.BGMusic, 0), "No BGM should be playing after " +
                "applying a save captured with no playback.");
            Assert.AreEqual(-1, save.GetBgmIndex(0), "Expected no valid BGM index when nothing was playing.");
        }

        [UnityTest]
        public IEnumerator FallsBackToClipNameWhenIndexInvalid()
        {
            yield return CommonSetup();

            // Play a known clip and capture save data.
            AudioSys.Play(playAudioArgsSO);
            var save = mycelSaveCodec.EncodeToSave(AudioSystem.S);
            yield return wait;

            // Stop current playback to force applier to re-play from save.
            AudioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;

            // Corrupt the saved index but keep a valid name to test name fallback.
            save.AddBgmIndex(0, -1);

            // Here, we let the inner PlayAudioArgs keep the reference to the clip
            // since it registers the name from that. 

            //save.PlayAudioArgs.MainClip = null;
            //save.PlayAudioArgs.MainClipName = playAudioArgsSO.MainClip.name;

            var apply = audioApplier.ApplyRange(new[] { save });
            yield return WaitFor(apply);

            var clipPlaying = AudioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);
            Assert.IsNotNull(clipPlaying, "Expected a clip to be playing after applying with name fallback.");
            Assert.AreEqual(playAudioArgsSO.MainClip.name, clipPlaying.name, "Fallback by name should resolve and play the correct clip.");
        }

        [UnityTest]
        public IEnumerator LogsWarningAndDoesNotPlayWhenClipNameMissing()
        {
            yield return CommonSetup();

            // Build a save that requests playback but has invalid index and non-existent name among what's
            // registered in ShadowDatabase
            AudioClip fakeClip = AudioClip.Create("T4fuiy5tg7iwt57i46rt26t", 44100 * 2, 1, 44100, false);

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

            // Expect warning from MyceliaudioApplier about missing clip by name.
            LogAssert.Expect(LogType.Warning, 
                $"[MyceliaudioApplier]: Could not find audio clip with name: {fakeClip.name}. Cannot play BGM upon application.");

            var apply = audioApplier.ApplyRange(new[] { save });
            yield return WaitFor(apply);

            Assert.IsFalse(AudioSys.GetIsPlaying(TrackGroup.BGMusic, 0), "Applier should not start playback when it cannot resolve the clip by index or name.");
        }

        [Test]
        public void CodecCanHandleKnownTypes()
        {
            Assert.IsTrue(mycelSaveCodec.CanHandle(typeof(AudioSystem).FullName),
                "Codec should handle AudioSystem full type name.");

            Assert.IsTrue(mycelSaveCodec.CanHandle(typeof(MyceliaudioSaveData).Name),
                "Codec should handle MyceliaudioSaveData type name.");

            Assert.IsFalse(mycelSaveCodec.CanHandle("CompletelyRandomTypeName"),
                "Codec should not handle unknown type names.");
        }

        [Test]
        public void BgmIndexesAreDefensivelyCopied()
        {
            var save = new MyceliaudioSaveData();
            save.AddBgmIndex(0, 7);

            var ext = save.BgmIndexes; // Should be a copy
            ext[0] = 99;

            Assert.AreEqual(7, save.GetBgmIndex(0), 
                "External modifications to BgmIndexes copy should not affect internal state.");
        }

        [UnityTest]
        public IEnumerator ApplyingSameSaveTwiceIsIdempotent()
        {
            yield return CommonSetup();

            AudioSys.Play(playAudioArgsSO);
            var save = mycelSaveCodec.EncodeToSave(AudioSystem.S);
            yield return wait;

            // Stop, then apply twice.
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
                "Applying the same save twice should result in the same clip continuing to play.");
        }
    }
}