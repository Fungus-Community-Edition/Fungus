using Amanita.Myceliaudio;
using Amanita.Myceliaudio.VScripting;
using Amanita.DOTweenIntegration;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Type = System.Type;
using System.Reflection;

namespace CommandCompat
{
    public class MA_FadeVolumeCompatTests : CommandTestBase<MA_FadeVolume>
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

        protected TrackGroup currentGroup = TrackGroup.BGMusic; // default
        protected int currentIndex;

        protected override void ConfigureCommand(MA_FadeVolume cmd)
        {
            // Set all group vols to max to avoid scaling in reported values
            AudioSystem.S.SetTrackGroupVol(TrackGroup.Master, 100);
            AudioSystem.S.SetTrackGroupVol(TrackGroup.BGMusic, 100);
            AudioSystem.S.SetTrackGroupVol(TrackGroup.SoundFX, 100);
            AudioSystem.S.SetTrackGroupVol(TrackGroup.Voice, 100);

            // Assign protected fields via reflection
            Type cmdType = cmd.GetType();
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            cmdType.GetField("trackGroup", flags)
                .SetValue(cmd, currentGroup);
            cmdType.GetField("track", flags)
                .SetValue(cmd, new IntegerData(currentIndex));
            cmdType.GetField("targetVol", flags)
                .SetValue(cmd, new FloatData(targetVolume));
            cmdType.GetField("duration", flags)
                .SetValue(cmd, new FloatData(Duration));
            cmdType.GetField("waitUntilFinished", flags)
                .SetValue(cmd, new BooleanData(true));
            cmdType.GetField("doFade", flags)
                .SetValue(cmd, adapter);
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