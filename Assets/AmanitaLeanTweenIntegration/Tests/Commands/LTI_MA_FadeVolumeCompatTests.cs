using Amanita.Myceliaudio;
using Amanita.Myceliaudio.VScripting;
using Amanita.VScripting;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using System.Collections.Generic;
using Type = System.Type;

namespace CommandCompat
{
    public class LTI_MA_FadeVolumeCompatTests : LTI_CommandTestBase<MA_FadeVolume>
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

        // Track indexes 0..2
        protected static readonly int[] TestTrackIndexes = { 0, 1, 2 };

        // Cartesian product of groups & indexes
        protected static readonly object[] GroupIndexCases = BuildCases();
        protected static object[] BuildCases()
        {
            var list = new List<object>();
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
            CmdType.GetField("trackGroup", flags)
                .SetValue(cmd, currentGroup);
            CmdType.GetField("track", flags)
                .SetValue(cmd, new IntegerData(currentIndex));
            CmdType.GetField("targetVol", flags)
                .SetValue(cmd, new FloatData(targetVolume));
            CmdType.GetField("duration", flags)
                .SetValue(cmd, new FloatData(Duration));
            CmdType.GetField("waitUntilFinished", flags)
                .SetValue(cmd, new BooleanData(true));
            CmdType.GetField("doFade", flags)
                .SetValue(cmd, adapter);
        }

        protected override void AssertFinalState()
        {
            float actual = AudioSystem.S.GetTrackVol(currentGroup, currentIndex);
            Assert.AreEqual(targetVolume, actual, Epsilon, "Track volume mismatch");
        }

        [UnityTest]
        public IEnumerator WaitUntilFinished_FadesTrackGroup([ValueSource(nameof(GroupIndexCases))] object[] caseData)
        {
            ConfigFor(caseData);

            yield return RunBlockAndWait();
            AssertFinalState();
        }

        [UnityTest]
        public IEnumerator NoWait_ContinuesImmediately_AndFades([ValueSource(nameof(GroupIndexCases))] object[] caseData)
        {
            ConfigFor(caseData);

            CmdType.GetField("waitUntilFinished", flags)
                .SetValue(command, new BooleanData(false));

            bool continued = false;
            command.StartedContinue += _ => continued = true;

            flowchart.StartCoroutine(block.Execute());

            Assert.IsTrue(continued, "Continue() should be called immediately when waitUntilFinished is false.");

            yield return new WaitForSeconds(Duration + 0.05f);
            AssertFinalState();
        }

        protected virtual void ConfigFor(object[] caseData)
        {
            currentGroup = (TrackGroup)caseData[0];
            currentIndex = (int)caseData[1];

            // We want to make sure that the Command has the right inputs before we run it
            CmdType.GetField("trackGroup", flags)
                .SetValue(command, currentGroup);
            CmdType.GetField("track", flags)
                .SetValue(command, new IntegerData(currentIndex));

            AudioSystem.S.SetTrackVol(currentGroup, currentIndex, startVolume);
        }
    }
}