using Amanita.VScripting;
using NUnit.Framework;
using System;
using UnityEngine;

namespace Amanita.MuscariableTests.DataOnly
{
    public class AudioMuscariableTests
    {
        [SetUp]
        public void Setup()
        {
            string pathToBgm = "Audio/BGM/01 Main Theme",
                pathToSfx = "Audio/BGM/UI Select";
            // Create two distinct AudioClip instances
            clipA = Resources.Load<AudioClip>(pathToBgm);
            clipB = Resources.Load<AudioClip>(pathToSfx);

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
            audioVar.ItemID = 100;
            Assert.DoesNotThrow(() => audioVar.Init());
        }

        [Test]
        public void AudioClip_ValueAssignmentAndEvent()
        {
            var clipVar = new AudioClipMuscariable { Key = "clip", ItemID = 101 };
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
            var clipVar = new AudioClipMuscariable { Key = "clip", ItemID = 102 };
            clipVar.Init();

            Assert.DoesNotThrow(() => clipVar.Value = null);
            Assert.IsNull(clipVar.Value);
        }

        [Test]
        public void AudioClip_WrongTypeAssignment_Throws()
        {
            var audioVar = new AudioClipMuscariable { Key = "clip", ItemID = 103 };
            audioVar.Init();
            Muscariable baseVar = audioVar;

            Assert.Throws<ArgumentException>(() => baseVar.Value = "not a clip");
        }

        [Test]
        public void AudioClip_EqualityOperatorsAndEvaluate()
        {
            var firstClipVar = new AudioClipMuscariable { Key = "a", ItemID = 104, Value = clipA };
            var secondClipVar = new AudioClipMuscariable { Key = "b", ItemID = 105, Value = clipA };
            var thirdClipVar = new AudioClipMuscariable { Key = "c", ItemID = 106, Value = clipB };

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
            var v = new AudioSourceMuscariable { Key = "src", ItemID = 107 };
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
            var v = new AudioSourceMuscariable { Key = "src", ItemID = 108 };
            v.Init();

            Assert.DoesNotThrow(() => v.Value = null);
            Assert.IsNull(v.Value);
        }

        [Test]
        public void AudioSource_WrongTypeAssignment_Throws()
        {
            var audioVar = new AudioSourceMuscariable { Key = "src", ItemID = 109 };
            audioVar.Init();
            Muscariable baseVar = audioVar;

            Assert.Throws<ArgumentException>(() => baseVar.Value = 123);
        }

        [Test]
        public void AudioSource_EqualityAndEvaluate()
        {
            var a = new AudioSourceMuscariable { Key = "a", ItemID = 110, Value = sourceA };
            var b = new AudioSourceMuscariable { Key = "b", ItemID = 111, Value = sourceA };
            var c = new AudioSourceMuscariable { Key = "c", ItemID = 112, Value = sourceB };

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
