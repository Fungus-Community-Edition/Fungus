using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Amanita.SaveSys;
using Fungus;
using System.Collections.Generic;
using System;
using UnityObject = UnityEngine.Object;
using UnityEngine.TestTools;
using Amanita.Myceliaudio;

namespace Amanita.SaveSystemTests
{
    public class MyceliaudioSaveTests
    {
        protected string toVarStateTests = "ScenePrefabs/VarStateTests";

        [SetUp]
        public virtual void DoSetUp()
        {
            PrepScene();

        }

        protected virtual void PrepScene()
        {
            varStateTestPrefab = Resources.Load<GameObject>(toVarStateTests);
            varStateTestScene = UnityObject.Instantiate(varStateTestPrefab);
            playAudioArgsSO = Resources.Load<PlayAudioArgsSO>(pathToAudioArgsSO);
            audioSys = AudioSystem.S;
            applier = ScriptableObject.CreateInstance<MyceliaudioApplier>();
        }

        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;


        protected PlayAudioArgsSO playAudioArgsSO;
        protected string pathToAudioArgsSO = "testClip";

        protected PlayAudioArgs audioArgs;
        protected AudioSystem audioSys;
        protected MyceliaudioApplier applier;

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [UnityTest]
        public virtual IEnumerator PlaysCorrectClip()
        {
            float quickWaitTime = 1f, waitTime = 3f;
            WaitForSeconds quickWait = new WaitForSeconds(quickWaitTime);
            WaitForSeconds wait = new WaitForSeconds(waitTime);
            audioSys.Play(playAudioArgsSO);
            MyceliaudioSaveData saveData = new MyceliaudioSaveData();
            yield return wait;

            audioSys.StopPlaying(TrackGroup.BGMusic, 0);
            yield return quickWait;
            applier.Apply(new MyceliaudioSaveData[] { saveData });
            yield return wait; ;
            AudioClip clipPlaying = audioSys.GetClipPlayingAt(TrackGroup.BGMusic, 0);
            bool playingCorrectClip = clipPlaying == playAudioArgsSO.MainClip;
            Assert.IsTrue(playingCorrectClip, "The clip playing is not the one we expected it to be.");

        }
    }
}