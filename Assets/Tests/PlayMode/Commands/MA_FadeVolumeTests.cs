using Amanita.Myceliaudio;
using Amanita.Myceliaudio.VScripting;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Amanita;

namespace VScriptingTests.Commands
{
    public class MA_FadeVolumeTests : CommandTestBase<MA_FadeVolume>
    {
        protected const float startVolume = 1f;
        protected const float targetVolume = 25f;

        // All track groups except Null and Master
        protected static readonly TrackGroup[] TestTrackGroups =
        {
            TrackGroup.BGMusic,
            TrackGroup.SoundFX,
            TrackGroup.Voice
        };

        // Track indexes 0–2
        protected static readonly int[] TestTrackIndexes = { 0, 1, 2 };

        // Cartesian product of groups × indexes
        protected static readonly object[] GroupIndexCases = BuildCases();
        protected static object[] BuildCases()
        {
            var list = new System.Collections.Generic.List<object>();
            foreach (var group in TestTrackGroups)
            {
                foreach (var index in TestTrackIndexes)
                {
                    list.Add(new object[] { group, index });
                }
            }
            return list.ToArray();
        }

        protected TrackGroup currentGroup = TrackGroup.BGMusic; // Defaulting to this due to when ConfigureCommand gets called
        protected int currentIndex;

        protected override void ConfigureCommand(MA_FadeVolume cmd)
        {
            // Setting all the group vols to max due to how reported stuff gets scaled
            AudioSystem.S.SetTrackGroupVol(TrackGroup.Master, 100);
            AudioSystem.S.SetTrackGroupVol(TrackGroup.BGMusic, 100);
            AudioSystem.S.SetTrackGroupVol(TrackGroup.SoundFX, 100);
            AudioSystem.S.SetTrackGroupVol(TrackGroup.Voice, 100);

            // Assign protected fields via reflection
            typeof(MA_FadeVolume).GetField("trackGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, currentGroup);
            typeof(MA_FadeVolume).GetField("track", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, new IntegerData(currentIndex));
            typeof(MA_FadeVolume).GetField("targetVol", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, new FloatData(targetVolume));
            typeof(MA_FadeVolume).GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, new FloatData(Duration));
            typeof(MA_FadeVolume).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, new BooleanData(true));
            typeof(MA_FadeVolume).GetField("fadeTween", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cmd, AmanitaManager.DefaultTweener); // default adapter
        }

        protected override void AssertFinalState()
        {
            float actual = AudioSystem.S.GetTrackVol(currentGroup, currentIndex);
            Assert.AreEqual(targetVolume, actual, Epsilon,
                $"Track volume mismatch for {currentGroup} track {currentIndex}");
        }

        [UnityTest]
        public IEnumerator WaitUntilFinished_FadesVolume(
            [ValueSource(nameof(GroupIndexCases))] object[] caseData)
        {
            ConfigFor(caseData);
            AudioSystem.S.SetTrackVol(currentGroup, currentIndex, startVolume);

            yield return RunBlockAndWait();
            AssertFinalState();
        }

        protected virtual void ConfigFor(object[] caseData)
        {
            currentGroup = (TrackGroup)caseData[0];
            currentIndex = (int)caseData[1];

            typeof(MA_FadeVolume).GetField("trackGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(command, currentGroup);
            typeof(MA_FadeVolume).GetField("track", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(command, new IntegerData(currentIndex));

            AudioSystem.S.SetTrackVol(currentGroup, currentIndex, startVolume);
        }

        [UnityTest]
        public IEnumerator NoWait_ContinuesImmediately_AndFadesVolume(
            [ValueSource(nameof(GroupIndexCases))] object[] caseData)
        {
            ConfigFor(caseData);
            
            typeof(MA_FadeVolume).GetField("waitUntilFinished", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(command, new BooleanData(false));

            bool continued = false;
            command.StartedContinue += _ => continued = true;

            flowchart.ExecuteBlock(block);

            Assert.IsTrue(continued,
                $"Continue() should be called immediately when waitUntilFinished is false for {currentGroup} track {currentIndex}");

            yield return new WaitForSeconds(Duration + 0.05f);
            AssertFinalState();
        }
    }
}