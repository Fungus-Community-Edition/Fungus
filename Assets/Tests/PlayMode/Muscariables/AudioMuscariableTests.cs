using Amanita;
using Amanita.VScripting;
using Lorekeeper;
using NUnit.Framework;
using System;
using UnityEngine;

namespace VScriptingTests.MuscariableTests.DataOnly
{
    public class AudioMuscariableTests
    {
        [SetUp]
        public void Setup()
        {
            GetClipsFromLorekeeper();
            void GetClipsFromLorekeeper()
            {
                ShadowDatabase database = AmanitaManager.ShadowDB;
                var audioClips = database.GetAssetsOfType<AudioClip>(AssetType.AudioClip);
                clipA = audioClips[0];
                clipB = audioClips[1];
            }

            Assert.IsNotNull(clipA, "Clip A is null");
            Assert.IsNotNull(clipB, "Clip B is null");

            // Create two AudioSource components on separate GameObjects
            go = new GameObject("TestGO");
            sourceA = go.AddComponent<AudioSource>();
            sourceB = new GameObject("OtherGO").AddComponent<AudioSource>();
        }

        protected AudioClip clipA;
        protected AudioClip clipB;
        protected GameObject go;
        protected AudioSource sourceA;
        protected AudioSource sourceB;

        [TearDown]
        public void Teardown()
        {
            clipA = clipB = null;
            UnityEngine.Object.DestroyImmediate(sourceA.gameObject);
            UnityEngine.Object.DestroyImmediate(sourceB.gameObject);
        }

        [Test]
        public void AudioClip_Init_RequiresKeyAndID()
        {
            var audioVar = new AudioClipMuscariable();
            var ex = Assert.Throws<Exception>(() => audioVar.Init());
            StringAssert.Contains("needs a valid key", ex.Message);
            StringAssert.Contains("needs a valid ID", ex.Message);

            audioVar.Key = "clip";
            audioVar.ItemId = 100;
            Assert.DoesNotThrow(() => audioVar.Init());
        }

        [Test]
        public void AudioClip_ValueAssignmentAndEvent()
        {
            var clipVar = new AudioClipMuscariable { Key = "clip", ItemId = 101 };
            clipVar.Init();

            AudioClip captured = null;
            clipVar.OnValueChanged += c => captured = c;

            clipVar.Value = clipA;
            Assert.AreEqual(clipA, clipVar.Value);
            Assert.AreEqual(clipA, captured);
        }

        [Test]
        public void AudioClip_NullAssignmentAllowed()
        {
            var clipVar = new AudioClipMuscariable { Key = "clip", ItemId = 102 };
            clipVar.Init();

            Assert.DoesNotThrow(() => clipVar.Value = null);
            Assert.IsNull(clipVar.Value);
        }

        [Test]
        public void AudioClip_WrongTypeAssignment_Throws()
        {
            var audioVar = new AudioClipMuscariable { Key = "clip", ItemId = 103 };
            audioVar.Init();
            Muscariable baseVar = audioVar;

            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = "not a clip");
        }

        [Test]
        public void AudioClip_EqualityOperatorsAndEvaluate()
        {
            var firstClipVar = new AudioClipMuscariable     { Key = "a", ItemId = 104, BoxedValue = clipA };
            var secondClipVar = new AudioClipMuscariable    { Key = "b", ItemId = 105, BoxedValue = clipA };
            var thirdClipVar = new AudioClipMuscariable     { Key = "c", ItemId = 106, BoxedValue = clipB };

            // operator==
            Assert.IsTrue(firstClipVar == secondClipVar);
            Assert.IsFalse(firstClipVar != secondClipVar);
            Assert.IsFalse(firstClipVar == thirdClipVar);
            Assert.IsTrue(firstClipVar != thirdClipVar);

            // Evaluate via CompareOperator
            Assert.IsTrue(firstClipVar.Evaluate(CompareOperator.Equals, clipA));
            Assert.IsFalse(firstClipVar.Evaluate(CompareOperator.Equals, clipB));

            // Unsupported relational operator
            Assert.Throws<ArgumentException>(
                () => firstClipVar.Evaluate(CompareOperator.GreaterThan, clipA)
            );
        }

        [Test]
        public void AudioSource_ValueAssignmentAndEvent()
        {
            var v = new AudioSourceMuscariable { Key = "src", ItemId = 107 };
            v.Init();

            AudioSource captured = null;
            v.OnValueChanged += s => captured = s;

            v.Value = sourceA;
            Assert.AreEqual(sourceA, v.Value);
            Assert.AreEqual(sourceA, captured);
        }

        [Test]
        public void AudioSource_NullAssignmentAllowed()
        {
            var v = new AudioSourceMuscariable { Key = "src", ItemId = 108 };
            v.Init();

            Assert.DoesNotThrow(() => v.Value = null);
            Assert.IsNull(v.Value);
        }

        [Test]
        public void AudioSource_WrongTypeAssignment_Throws()
        {
            var audioVar = new AudioSourceMuscariable { Key = "src", ItemId = 109 };
            audioVar.Init();
            Muscariable baseVar = audioVar;

            Assert.Throws<ArgumentException>(() => baseVar.BoxedValue = 123);
        }

        [Test]
        public void AudioSource_EqualityAndEvaluate()
        {
            var a = new AudioSourceMuscariable { Key = "a", ItemId = 110, Value = sourceA };
            var b = new AudioSourceMuscariable { Key = "b", ItemId = 111, Value = sourceA };
            var c = new AudioSourceMuscariable { Key = "c", ItemId = 112, Value = sourceB };

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);

            Assert.IsTrue(a.Evaluate(CompareOperator.Equals, sourceA));
            Assert.IsFalse(a.Evaluate(CompareOperator.Equals, sourceB));

            Assert.Throws<ArgumentException>(
                () => a.Evaluate(CompareOperator.LessThan, sourceA)
            );
        }
    }


}
